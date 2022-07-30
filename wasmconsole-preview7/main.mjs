import { App } from "./app-support.mjs";

App.main = async function (applicationArguments) {
    App.IMPORTS.node = {
        process: {
            version: () => globalThis.process.version,
        },
    };

    const exports = await App.MONO.mono_wasm_get_assembly_exports("wasmconsole.dll");
    const callFromJS = exports.SampleClass.CallFromJS;
    console.log(callFromJS());
    const add42 = exports.SampleClass.Add42;
    console.log(add42(100));

    await App.MONO.mono_run_main("wasmconsole.dll", applicationArguments);
};
