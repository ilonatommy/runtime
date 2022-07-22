TOP=../../../../..
CULTURE=en-us

include ../wasm.mk

ifneq ($(AOT),)
override MSBUILD_ARGS+=/p:RunAOTCompilation=true
endif

ifneq ($(V),)
DOTNET_MONO_LOG_LEVEL=--setenv=MONO_LOG_LEVEL=debug
endif

ifneq ($(SHARD),)
override MSBUILD_ARGS+=/p:EnableSharding=true /p:ICUDefaultCulture=$(CULTURE)
endif

PROJECT_NAME=Wasm.Console.V8.ES6.Sample.csproj
CONSOLE_DLL=Wasm.Console.V8.ES6.Sample.dll
MAIN_JS=v8shim.cjs

run: run-console
