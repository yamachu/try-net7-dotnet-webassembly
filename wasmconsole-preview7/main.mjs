import { App } from './app-support.mjs'

App.main = async function (applicationArguments) {
    await App.MONO.mono_run_main("wasmconsole.dll", applicationArguments);
}