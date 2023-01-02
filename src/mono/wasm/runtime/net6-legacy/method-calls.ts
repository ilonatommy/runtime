// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

import { get_js_obj, mono_wasm_get_jsobj_from_js_handle } from "../gc-handles";
import { Module, runtimeHelpers, INTERNAL } from "../imports";
import { wrap_error_root, wrap_no_error_root } from "../invoke-js";
import { _release_temp_frame, setU16 } from "../memory";
import { mono_wasm_new_external_root, mono_wasm_new_root } from "../roots";
import { find_entry_point } from "../run";
import { conv_string_root, js_string_to_mono_string_root } from "../strings";
import { JSHandle, MonoStringRef, MonoObjectRef, MonoArray, MonoString, MonoObject, is_nullish, mono_assert, WasmRoot } from "../types";
import { Int32Ptr, VoidPtr } from "../types/emscripten";
import { mono_array_root_to_js_array, unbox_mono_obj_root } from "./cs-to-js";
import { js_array_to_mono_array, js_to_mono_obj_root } from "./js-to-cs";
import { Converter, BoundMethodToken, mono_method_resolve, mono_method_get_call_signature_ref, mono_bind_method } from "./method-binding";

const boundMethodsByFqn: Map<string, Function> = new Map();

export function _teardown_after_call(
    converter: Converter | undefined, token: BoundMethodToken | null,
    buffer: VoidPtr,
    resultRoot: WasmRoot<any>,
    exceptionRoot: WasmRoot<any>,
    thisArgRoot: WasmRoot<MonoObject>,
    sp: VoidPtr
): void {
    _release_temp_frame();
    Module.stackRestore(sp);

    if (typeof (resultRoot) === "object") {
        resultRoot.clear();
        if ((token !== null) && (token.scratchResultRoot === null))
            token.scratchResultRoot = resultRoot;
        else
            resultRoot.release();
    }
    if (typeof (exceptionRoot) === "object") {
        exceptionRoot.clear();
        if ((token !== null) && (token.scratchExceptionRoot === null))
            token.scratchExceptionRoot = exceptionRoot;
        else
            exceptionRoot.release();
    }
    if (typeof (thisArgRoot) === "object") {
        thisArgRoot.clear();
        if ((token !== null) && (token.scratchThisArgRoot === null))
            token.scratchThisArgRoot = thisArgRoot;
        else
            thisArgRoot.release();
    }
}

export function mono_bind_static_method(fqn: string, signature?: string/*ArgsMarshalString*/): Function {
    mono_assert(runtimeHelpers.mono_wasm_bindings_is_ready, "The runtime must be initialized.");

    const key = `${fqn}-${signature}`;
    let js_method = boundMethodsByFqn.get(key);
    if (js_method === undefined) {
        const method = mono_method_resolve(fqn);

        if (typeof signature === "undefined")
            signature = mono_method_get_call_signature_ref(method, undefined);

        js_method = mono_bind_method(method, signature!, false, fqn);
        boundMethodsByFqn.set(key, js_method);
    }
    return js_method;
}

export function mono_bind_assembly_entry_point(assembly: string, signature?: string/*ArgsMarshalString*/): Function {
    const method = find_entry_point(assembly);
    if (typeof (signature) !== "string")
        signature = mono_method_get_call_signature_ref(method, undefined);

    const js_method = mono_bind_method(method, signature!, false, "_" + assembly + "__entrypoint");

    return async function (...args: any[]) {
        if (args.length > 0 && Array.isArray(args[0]))
            args[0] = js_array_to_mono_array(args[0], true, false);
        return js_method(...args);
    };
}

export function mono_call_assembly_entry_point(assembly: string, args?: any[], signature?: string/*ArgsMarshalString*/): number {
    mono_assert(runtimeHelpers.mono_wasm_bindings_is_ready, "The runtime must be initialized.");
    if (!args) {
        args = [[]];
    }
    return mono_bind_assembly_entry_point(assembly, signature)(...args);
}

export function mono_wasm_invoke_js_with_args_ref(js_handle: JSHandle, method_name: MonoStringRef, args: MonoObjectRef, is_exception: Int32Ptr, result_address: MonoObjectRef): any {
    const argsRoot = mono_wasm_new_external_root<MonoArray>(args),
        nameRoot = mono_wasm_new_external_root<MonoString>(method_name),
        resultRoot = mono_wasm_new_external_root<MonoObject>(result_address);
    try {
        const js_name = conv_string_root(nameRoot);
        if (!js_name || (typeof (js_name) !== "string")) {
            wrap_error_root(is_exception, "ERR12: Invalid method name object @" + nameRoot.value, resultRoot);
            return;
        }

        const obj = get_js_obj(js_handle);
        if (is_nullish(obj)) {
            wrap_error_root(is_exception, "ERR13: Invalid JS object handle '" + js_handle + "' while invoking '" + js_name + "'", resultRoot);
            return;
        }

        const js_args = mono_array_root_to_js_array(argsRoot);

        try {
            const m = obj[js_name];
            if (typeof m === "undefined")
                throw new Error("Method: '" + js_name + "' not found for: '" + Object.prototype.toString.call(obj) + "'");
            const res = m.apply(obj, js_args);

            js_to_mono_obj_root(res, resultRoot, true);
            wrap_no_error_root(is_exception);
        } catch (ex) {
            wrap_error_root(is_exception, ex, resultRoot);
        }
    } finally {
        argsRoot.release();
        nameRoot.release();
        resultRoot.release();
    }
}

export function mono_wasm_get_object_property_ref(js_handle: JSHandle, property_name: MonoStringRef, is_exception: Int32Ptr, result_address: MonoObjectRef): void {
    const nameRoot = mono_wasm_new_external_root<MonoString>(property_name),
        resultRoot = mono_wasm_new_external_root<MonoObject>(result_address);
    try {
        const js_name = conv_string_root(nameRoot);
        if (!js_name) {
            wrap_error_root(is_exception, "Invalid property name object '" + nameRoot.value + "'", resultRoot);
            return;
        }

        const obj = mono_wasm_get_jsobj_from_js_handle(js_handle);
        if (is_nullish(obj)) {
            wrap_error_root(is_exception, "ERR01: Invalid JS object handle '" + js_handle + "' while geting '" + js_name + "'", resultRoot);
            return;
        }

        const m = obj[js_name];
        js_to_mono_obj_root(m, resultRoot, true);
        wrap_no_error_root(is_exception);
    } catch (ex) {
        wrap_error_root(is_exception, ex, resultRoot);
    } finally {
        resultRoot.release();
        nameRoot.release();
    }
}

export function mono_wasm_set_object_property_ref(js_handle: JSHandle, property_name: MonoStringRef, value: MonoObjectRef, createIfNotExist: boolean, hasOwnProperty: boolean, is_exception: Int32Ptr, result_address: MonoObjectRef): void {
    const valueRoot = mono_wasm_new_external_root<MonoObject>(value),
        nameRoot = mono_wasm_new_external_root<MonoString>(property_name),
        resultRoot = mono_wasm_new_external_root<MonoObject>(result_address);
    try {

        const property = conv_string_root(nameRoot);
        if (!property) {
            wrap_error_root(is_exception, "Invalid property name object '" + property_name + "'", resultRoot);
            return;
        }

        const js_obj = mono_wasm_get_jsobj_from_js_handle(js_handle);
        if (is_nullish(js_obj)) {
            wrap_error_root(is_exception, "ERR02: Invalid JS object handle '" + js_handle + "' while setting '" + property + "'", resultRoot);
            return;
        }

        const js_value = unbox_mono_obj_root(valueRoot);

        if (createIfNotExist) {
            js_obj[property] = js_value;
        }
        else {
            if (!createIfNotExist) {
                if (!Object.prototype.hasOwnProperty.call(js_obj, property)) {
                    return;
                }
            }
            if (hasOwnProperty === true) {
                if (Object.prototype.hasOwnProperty.call(js_obj, property)) {
                    js_obj[property] = js_value;
                }
            }
            else {
                js_obj[property] = js_value;
            }
        }
        wrap_no_error_root(is_exception, resultRoot);
    } catch (ex) {
        wrap_error_root(is_exception, ex, resultRoot);
    } finally {
        resultRoot.release();
        nameRoot.release();
        valueRoot.release();
    }
}

export function mono_wasm_get_by_index_ref(js_handle: JSHandle, property_index: number, is_exception: Int32Ptr, result_address: MonoObjectRef): void {
    const resultRoot = mono_wasm_new_external_root<MonoObject>(result_address);
    try {
        const obj = mono_wasm_get_jsobj_from_js_handle(js_handle);
        if (is_nullish(obj)) {
            wrap_error_root(is_exception, "ERR03: Invalid JS object handle '" + js_handle + "' while getting [" + property_index + "]", resultRoot);
            return;
        }

        const m = obj[property_index];
        js_to_mono_obj_root(m, resultRoot, true);
        wrap_no_error_root(is_exception);
    } catch (ex) {
        wrap_error_root(is_exception, ex, resultRoot);
    } finally {
        resultRoot.release();
    }
}

export function mono_wasm_set_by_index_ref(js_handle: JSHandle, property_index: number, value: MonoObjectRef, is_exception: Int32Ptr, result_address: MonoObjectRef): void {
    const valueRoot = mono_wasm_new_external_root<MonoObject>(value),
        resultRoot = mono_wasm_new_external_root<MonoObject>(result_address);
    try {
        const obj = mono_wasm_get_jsobj_from_js_handle(js_handle);
        if (is_nullish(obj)) {
            wrap_error_root(is_exception, "ERR04: Invalid JS object handle '" + js_handle + "' while setting [" + property_index + "]", resultRoot);
            return;
        }

        const js_value = unbox_mono_obj_root(valueRoot);
        obj[property_index] = js_value;
        wrap_no_error_root(is_exception, resultRoot);
    } catch (ex) {
        wrap_error_root(is_exception, ex, resultRoot);
    } finally {
        resultRoot.release();
        valueRoot.release();
    }
}

export function mono_wasm_get_global_object_ref(global_name: MonoStringRef, is_exception: Int32Ptr, result_address: MonoObjectRef): void {
    const nameRoot = mono_wasm_new_external_root<MonoString>(global_name),
        resultRoot = mono_wasm_new_external_root(result_address);
    try {
        const js_name = conv_string_root(nameRoot);

        let globalObj;

        if (!js_name) {
            globalObj = globalThis;
        }
        else if (js_name == "Module") {
            globalObj = Module;
        }
        else if (js_name == "INTERNAL") {
            globalObj = INTERNAL;
        }
        else {
            globalObj = (<any>globalThis)[js_name];
        }

        // TODO returning null may be useful when probing for browser features
        if (globalObj === null || typeof globalObj === undefined) {
            wrap_error_root(is_exception, "Global object '" + js_name + "' not found.", resultRoot);
            return;
        }

        js_to_mono_obj_root(globalObj, resultRoot, true);
        wrap_no_error_root(is_exception);
    } catch (ex) {
        wrap_error_root(is_exception, ex, resultRoot);
    } finally {
        resultRoot.release();
        nameRoot.release();
    }
}

// Blazor specific custom routine
// eslint-disable-next-line @typescript-eslint/explicit-module-boundary-types
export function mono_wasm_invoke_js_blazor(exceptionMessage: Int32Ptr, callInfo: any, arg0: any, arg1: any, arg2: any): void | number {
    try {
        const blazorExports = (<any>globalThis).Blazor;
        if (!blazorExports) {
            throw new Error("The blazor.webassembly.js library is not loaded.");
        }

        return blazorExports._internal.invokeJSFromDotNet(callInfo, arg0, arg1, arg2);
    } catch (ex: any) {
        pass_exception_details(ex, exceptionMessage);
        return 0;
    }
}

// ----------------------Hybrid Globalization functions, to be moved later -----------------------------
export function mono_wasm_change_case_invariant(exceptionMessage: Int32Ptr, src: number, srcLength: number, dst: number, dstLength: number, toUpper: boolean) : void{
    try{
        const input = get_uft16_string(src, srcLength);
        let result = toUpper ? input.toUpperCase() : input.toLowerCase();
        // Unicode defines some codepoints which expand into multiple codepoints,
        // originally we do not support this expansion
        if (result.length > dstLength)
            result = input;

        for (let i = 0; i < result.length; i++)
            setU16(dst + i*2, result.charCodeAt(i));
    }
    catch (ex: any) {
        pass_exception_details(ex, exceptionMessage);
    }
}

export function mono_wasm_change_case(exceptionMessage: Int32Ptr, culture: MonoStringRef, src: number, srcLength: number, dst: number, destLength: number, toUpper: boolean) : void{
    const cultureRoot = mono_wasm_new_external_root<MonoString>(culture);
    try{
        const cultureName = conv_string_root(cultureRoot);
        if (!cultureName)
            throw new Error("Cannot change case, the culture name is null.");
        const input = get_uft16_string(src, srcLength);
        let result = toUpper ? input.toLocaleUpperCase(cultureName) : input.toLocaleLowerCase(cultureName);
        if (result.length > destLength)
            result = input;

        for (let i = 0; i < destLength; i++)
            setU16(dst + i*2, result.charCodeAt(i));
    }
    catch (ex: any) {
        pass_exception_details(ex, exceptionMessage);
    }
    finally {
        cultureRoot.release();
    }
}

export function mono_wasm_compare_string(culture: MonoStringRef, str1: number, str1Length: number, str2: number, str2Length: number, options: number): number{
    const cultureRoot = mono_wasm_new_external_root<MonoString>(culture);
    try{
        const cultureName = conv_string_root(cultureRoot);
        const ignoreKana = (options & 0x8) == 0x8;
        const ignoreWidth = (options & 0x10) == 0x10;
        const string1 = get_uft16_string_for_comparison(str1, str1Length, ignoreWidth, ignoreKana);
        const string2 = get_uft16_string_for_comparison(str2, str2Length, ignoreWidth, ignoreKana);
        const locale = (cultureName && cultureName?.trim()) ? cultureName : undefined;
        const casePicker = (options & 0x1f) % 8;
        switch (casePicker)
        {
            case 0:
                // 0: None - default algorithm for the platform OR StringSort - since .Net 5 it gives the same result as None, even for hyphen etc.
                // 8: IgnoreKanaType
                // 16: IgnoreWidth
                // 24: IgnoreKanaType | IgnoreWidth
                return string1.localeCompare(string2, locale); // a ≠ b, a ≠ á, a ≠ A
            case 1:
                // 1: IgnoreCase
                // 9: IgnoreKanaType | IgnoreCase
                // 17: IgnoreWidth | IgnoreCase
                // 25: IgnoreKanaType | IgnoreWidth | IgnoreCase
                return string1.localeCompare(string2, locale, { sensitivity: "accent" }); // a ≠ b, a ≠ á, a = A
            case 2:
                // 2: IgnoreNonSpace
                // 10: IgnoreKanaType | IgnoreNonSpace
                // 18: IgnoreWidth | IgnoreNonSpace
                // 26: IgnoreKanaType | IgnoreWidth | IgnoreNonSpace
                return string1.localeCompare(string2, locale, { sensitivity: "case" }); // a ≠ b, a = á, a ≠ A
            case 3:
                // 3: IgnoreNonSpace | IgnoreCase
                // 11: IgnoreKanaType | IgnoreNonSpace | IgnoreCase
                // 19: IgnoreWidth | IgnoreNonSpace | IgnoreCase
                // 27: IgnoreKanaType | IgnoreWidth | IgnoreNonSpace | IgnoreCase
                return string1.localeCompare(string2, locale, { sensitivity: "base" }); // a ≠ b, a = á, a ≠ A
            case 4:
                // 4: IgnoreSymbols - does not ignore currency symbols
                // 12: IgnoreKanaType | IgnoreSymbols
                // 20: IgnoreWidth | IgnoreSymbols
                // 28: IgnoreKanaType | IgnoreWidth | IgnoreSymbols
                return string1.localeCompare(string2, locale, { ignorePunctuation: true }); // by default ignorePunctuation: false
            case 5:
                // 5: IgnoreSymbols | IgnoreCase
                // 13: IgnoreKanaType | IgnoreSymbols | IgnoreCase
                // 21: IgnoreWidth | IgnoreSymbols | IgnoreCase
                // 29: IgnoreKanaType | IgnoreWidth | IgnoreSymbols | IgnoreCase
                return string1.localeCompare(string2, locale, { sensitivity: "accent", ignorePunctuation: true });
            case 6:
                // 6: IgnoreSymbols | IgnoreNonSpace
                // 14: IgnoreKanaType | IgnoreSymbols | IgnoreNonSpace
                // 22: IgnoreWidth | IgnoreSymbols | IgnoreNonSpace
                // 29: IgnoreKanaType | IgnoreWidth | IgnoreSymbols | IgnoreNonSpace
                return string1.localeCompare(string2, locale, { sensitivity: "case", ignorePunctuation: true });
            case 7:
                // 7: IgnoreSymbols | IgnoreNonSpace | IgnoreCase
                // 15: IgnoreKanaType | IgnoreSymbols | IgnoreNonSpace | IgnoreCase
                // 23: IgnoreWidth | IgnoreSymbols | IgnoreNonSpace | IgnoreCase
                // 29: IgnoreKanaType | IgnoreWidth | IgnoreSymbols | IgnoreNonSpace | IgnoreCase
                return string1.localeCompare(string2, locale, { sensitivity: "base", ignorePunctuation: true });
            default:
                throw new Error(`${options} is an invalid comparison option.`);
        }
    }
    catch (ex: any) {
        throw new Error(`${ex}`);
    }
    finally {
        cultureRoot.release();
    }
}

export function get_uft16_string(ptr: number, length: number): string{
    const view = new Uint16Array(Module.HEAPU16.buffer, ptr, length);
    let string = "";
    for (let i = 0; i < length; i++)
        string += String.fromCharCode(view[i]);
    return string;
}

export function get_uft16_string_for_comparison(ptr: number, length: number, isIgnoreWidth: boolean, isIgnoreKana: boolean): string{
    const view = new Uint16Array(Module.HEAPU16.buffer, ptr, length);
    let string = "";
    for (let i = 0; i < length; i++)
    {
        let code = view[i];
        // check if we are in hiragana range:   [0x3040; 0x309F]
        // if so, shift it to katakana:         [0x30A0; 0x30FF]
        if (isIgnoreKana && code <= 12447 && code >= 12352)
        {
            code += 96;
        }
        if (isIgnoreWidth)
        {
            // change all to lower for consistency
            code = map_higher_char_to_lower(code);
        }

        string += String.fromCharCode(code);
    }
    return string;
}

export function map_higher_char_to_lower(code: number): number
{
    // check if mapping is allowed
    if (is_half_full_higher_symbol(code)) // ToDo: use it to fix IgnoreSymbols
        return code;

    // for some ranges, chars are just shifted by a constant value between higherChar and lowerChar
    if (0xff01 <= code && code <= 0xff5e && code != 0xff3c) // [65281; 65339] U [65374; 65374]
        return code - 65248;
    if (0xffa1 <= code && code <= 0xffbe) // [65441; 65470]
        return code - 52848;
    if (0xffd2 <= code && code <= 0xffd7) // [65490; 65495]
        return code - 52855;

    // for not shifted chars, use O(1) HashMap
    const lowerChar = higherCharsMappedToLower[code];
    return lowerChar === undefined ? code : lowerChar;
}

export function pass_exception_details(ex: any, exceptionMessage: Int32Ptr){
    const exceptionJsString = ex.message + "\n" + ex.stack;
    const exceptionRoot = mono_wasm_new_root<MonoString>();
    js_string_to_mono_string_root(exceptionJsString, exceptionRoot);
    exceptionRoot.copy_to_address(<any>exceptionMessage);
    exceptionRoot.release();
}

export function is_half_full_higher_symbol(charCode: number)
{
    // some half full chars are considered symbols:
    // [65504;65510] U [65377;65381]
    return (0xffe0 <= charCode && charCode <= 0xffe6)
        || (0xff61 <= charCode && charCode <= 0xff65);
}

// based on static data from pal_collation.c:
const higherCharsMappedToLower: { [name: number]: number; } = {
    0xff66: 0x30f2,
    0xff67: 0x30a1,
    0xff68: 0x30a3,
    0xff69: 0x30a5,
    0xff6a: 0x30a7,
    0xff6b: 0x30a9,
    0xff6c: 0x30e3,
    0xff6d: 0x30e5,
    0xff6e: 0x30e7,
    0xff6f: 0x30c3,
    0xff71: 0x30a2,
    0xff72: 0x30a4,
    0xff73: 0x30a6,
    0xff74: 0x30a8,
    0xff75: 0x30aa,
    0xff76: 0x30ab,
    0xff77: 0x30ad,
    0xff78: 0x30af,
    0xff79: 0x30b1,
    0xff7a: 0x30b3,
    0xff7b: 0x30b5,
    0xff7c: 0x30b7,
    0xff7d: 0x30b9,
    0xff7e: 0x30bb,
    0xff7f: 0x30bd,
    0xff80: 0x30bf,
    0xff81: 0x30c1,
    0xff82: 0x30c4,
    0xff83: 0x30c6,
    0xff84: 0x30c8,
    0xff85: 0x30ca,
    0xff86: 0x30cb,
    0xff87: 0x30cc,
    0xff88: 0x30cd,
    0xff89: 0x30ce,
    0xff8a: 0x30cf,
    0xff8b: 0x30d2,
    0xff8c: 0x30d5,
    0xff8d: 0x30d8,
    0xff8e: 0x30db,
    0xff8f: 0x30de,
    0xff90: 0x30df,
    0xff91: 0x30e0,
    0xff92: 0x30e1,
    0xff93: 0x30e2,
    0xff94: 0x30e4,
    0xff95: 0x30e6,
    0xff96: 0x30e8,
    0xff97: 0x30e9,
    0xff98: 0x30ea,
    0xff99: 0x30eb,
    0xff9a: 0x30ec,
    0xff9b: 0x30ed,
    0xff9c: 0x30ef,
    0xff9d: 0x30f3,
    0xffa0: 0x313f,
    0xffc2: 0x314f,
    0xffc3: 0x3150,
    0xffc4: 0x3151,
    0xffc5: 0x3152,
    0xffc6: 0x3153,
    0xffc7: 0x3154,
    0xffca: 0x3155,
    0xffcb: 0x3156,
    0xffcc: 0x3157,
    0xffcd: 0x3158,
    0xffce: 0x3159,
    0xffcf: 0x315a,
    0xffda: 0x3161,
    0xffdb: 0x3162,
    0xffdc: 0x3163
};
