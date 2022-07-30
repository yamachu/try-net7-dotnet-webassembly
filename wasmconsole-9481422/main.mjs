import { App } from "./app-support.mjs";

App.main = async function (applicationArguments) {
    App.IMPORTS.node = {
        process: {
            version: () => globalThis.process.version,
        },
    };

    const exports = await App.MONO.mono_wasm_get_assembly_exports("wasmconsole.dll");
    const text = exports.MyClass.Greeting();
    console.log(text);

    exports.MyClass.DoFunc((csharpPassedText) => {
        console.log(`This is from C#: ${csharpPassedText}`);
    });
    console.log(text);
    const fn = exports.MyClass.GreetingFn();
    console.log(fn());

    return await App.MONO.mono_run_main("wasmconsole.dll", applicationArguments);
};
