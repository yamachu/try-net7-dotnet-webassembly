const { App } = require("./app-support.cjs");

App.init = async function () {
    await App.MONO.mono_run_main_and_exit("wasmconsole.dll", App.processedArguments.applicationArgs);
};
