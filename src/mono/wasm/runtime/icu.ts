// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

import cwraps from "./cwraps";
import { Module, runtimeHelpers } from "./imports";
import { IcuDictionary, MonoConfig } from "./types";
import { VoidPtr } from "./types/emscripten";

let num_icu_assets_loaded_successfully = 0;

// @offset must be the address of an ICU data archive in the native heap.
// returns true on success.
export function mono_wasm_load_icu_data(offset: VoidPtr, type: number): boolean {
    const ok = (cwraps.mono_wasm_load_icu_data(offset, type)) === 1;
    if (ok)
        num_icu_assets_loaded_successfully++;
    return ok;
}

// Get icudt.dat exact filename that matches given culture, examples:
//   "ja" -> "icudt_CJK.dat"
//   "en_US" (or "en-US" or just "en") -> "icudt_EFIGS.dat"
// etc, see "mono_wasm_get_icudt_name" implementation in pal_icushim_static.c
export function mono_wasm_get_icudt_name(culture: string): string {
    return cwraps.mono_wasm_get_icudt_name(culture);
}

// Performs setup for globalization.
// @globalization_mode is one of "icu", "invariant", or "auto".
// "auto" will use "icu" if any ICU data archives have been loaded,
//  otherwise "invariant".
export function mono_wasm_globalization_init(): void {
    const config = Module.config as MonoConfig;
    let invariantMode = false;
    if (!config.globalization_mode)
        config.globalization_mode = "auto";
    if (config.globalization_mode === "invariant")
        invariantMode = true;

    if (!invariantMode) {
        if (num_icu_assets_loaded_successfully > 0) {
            if (runtimeHelpers.diagnostic_tracing) {
                console.debug("MONO_WASM: ICU data archive(s) loaded, disabling invariant mode");
            }
        } else if (config.globalization_mode !== "icu") {
            if (runtimeHelpers.diagnostic_tracing) {
                console.debug("MONO_WASM: ICU data archive(s) not loaded, using invariant globalization mode");
            }
            invariantMode = true;
        } else {
            const msg = "invariant globalization mode is inactive and no ICU data archives were loaded";
            Module.printErr(`MONO_WASM: ERROR: ${msg}`);
            throw new Error(msg);
        }
    }

    if (invariantMode)
        cwraps.mono_wasm_setenv("DOTNET_SYSTEM_GLOBALIZATION_INVARIANT", "1");

    // Set globalization mode to PredefinedCulturesOnly
    cwraps.mono_wasm_setenv("DOTNET_SYSTEM_GLOBALIZATION_PREDEFINED_CULTURES_ONLY", "1");
}

export function _get_shard_name(shards: {[key: string]: any}, culture: string): string {
    // Get shard name that culture belongs to
    const parent_culture = culture.includes("-") ? culture.split("-")[0] : culture;
    for (const name in shards) {
        if (parent_culture.match(shards[name]))
            return name;
    }
    return "";
}

export function _get_list_of_icu_files(
    dictionary: IcuDictionary,
    culture: string,
    feature_shards=true,
    features = "") : any  {
    console.log("[ILONA] _get_list_of_icu_files 1");
    let icu_files = [];
    if (dictionary === undefined)
        return null;
    console.log("[ILONA] _get_list_of_icu_files 2");
    if (culture === undefined || culture.length < 2 ) {
        console.log("[ILONA] _get_list_of_icu_files 3");
        icu_files = [dictionary.packs.full];
    } else {
        console.log("[ILONA] _get_list_of_icu_files 4");
        const shard_name = _get_shard_name(dictionary.shards, culture);
        const packs = dictionary.packs;
        const files = packs[shard_name];
        if (!feature_shards) {
            icu_files = files.full;
        } else {
            // Get base files first
            for (const feature in packs[files.extends]) {
                icu_files.push(...packs[files.extends][feature]);
            }
            // Adding shard specific core files such as collation and locales
            icu_files.push(...files["core"]);

            //	Add any additional features
            if (features != "") {
                features.split(",").forEach(feat => {
                    icu_files.push(...files[feat]);
                });
            }
        }
    }
    const icu_assets: any = [];
    icu_files.forEach((file: any) => {
        const type = "common";
        // if (file.includes("locales"))
        // 	type = "app";
        icu_assets.push({
            "behavior": "icu",
            "name": file,
            "load_remote": false,
            "data_type": type
        });
    });
    return icu_assets;
}

