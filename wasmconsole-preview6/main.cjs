const { App } = require("./app-support.cjs");

App.init = async function () {
    const callFromJS = App.BINDING.bind_static_method("[wasmconsole] SampleClass:CallFromJS");
    console.log(callFromJS());
    const add42 = App.BINDING.bind_static_method("[wasmconsole] SampleClass:Add42");
    console.log(add42(100));

    await App.MONO.mono_run_main_and_exit("wasmconsole.dll", App.processedArguments.applicationArgs);
};
