VERSION=preview7
RUNTIME_DEST=runtime/$(VERSION)

NIGHTLY:=runtime/nightly

HERE := $(dir $(abspath "."))

tools/dotnet-install.sh:
	curl -o $@ -L https://dot.net/v1/dotnet-install.sh
	chmod +x $@

_dotnet7/clean:
	-rm -r $(RUNTIME_DEST)

dotnet7: tools/dotnet-install.sh
	$< -i $(RUNTIME_DEST) -v latest -q daily --channel 7.0

dotnet7/nightly:
	$(MAKE) _dotnet7/clean dotnet7 dotnet7/install/workload/nightly RUNTIME_DEST=$(NIGHTLY)

dotnet7/install/template:
	$(RUNTIME_DEST)/dotnet new install packages/$(VERSION)/Microsoft.NET.Runtime.WebAssembly.Templates.7.0.0-dev.nupkg

dotnet7/install/template/nightly:
	$(RUNTIME_DEST)/dotnet new install --force nightly/runtime/src/mono/wasm/templates

dotnet7/uninstall/template:
	$(RUNTIME_DEST)/dotnet new uninstall Microsoft.NET.Runtime.WebAssembly.Templates

dotnet7/uninstall/template/nightly:
	$(RUNTIME_DEST)/dotnet new uninstall nightly/runtime/src/mono/wasm/templates

dotnet7/install/workload:
	$(RUNTIME_DEST)/dotnet workload install --skip-manifest-update --no-cache --configfile tools/Nuget.$(VERSION).config wasm-tools

dotnet7/install/workload/nightly:
	$(RUNTIME_DEST)/dotnet workload install --skip-manifest-update --no-cache wasm-tools -s https://pkgs.dev.azure.com/dnceng/public/_packaging/dotnet7/nuget/v3/index.json

nightly/runtime:
	git clone --single-branch --branch main https://github.com/dotnet/runtime.git nightly/runtime

nightly/runtime/checkout: nightly/runtime
	cd nightly/runtime; git fetch -n -p origin main
	cd nightly/runtime; git checkout $(NIGHTLY_COMMIT)

nightly/install/template:
	$(MAKE) nightly/runtime/checkout NIGHTLY_COMMIT=$$(cat runtime/nightly/packs/Microsoft.NETCore.App.Runtime.Mono.browser-wasm/*/Microsoft.NETCore.App.versions.txt | head -n 1)
	$(MAKE) dotnet7/install/template/nightly RUNTIME_DEST=$(NIGHTLY)

init/workspace:
	cat try-net7-dotnet-webassembly.code-workspace.template | sed -E "s:<!-- CWD -->:$(HERE):" > try-net7-dotnet-webassembly.code-workspace
