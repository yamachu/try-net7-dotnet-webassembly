VERSION=preview7
RUNTIME_DEST=runtime/$(VERSION)

HERE := $(dir $(abspath "."))

tools/dotnet-install.sh:
	curl -o $@ -L https://dot.net/v1/dotnet-install.sh
	chmod +x $@

dotnet7: tools/dotnet-install.sh
	$< -i $(RUNTIME_DEST) -v latest -q daily --channel 7.0

dotnet7/install/template:
	$(RUNTIME_DEST)/dotnet new install packages/$(VERSION)/Microsoft.NET.Runtime.WebAssembly.Templates.7.0.0-dev.nupkg

dotnet7/uninstall/template:
	$(RUNTIME_DEST)/dotnet new uninstall Microsoft.NET.Runtime.WebAssembly.Templates

dotnet7/install/workload:
	$(RUNTIME_DEST)/dotnet workload install --skip-manifest-update --no-cache --configfile tools/Nuget.$(VERSION).config wasm-tools

init/workspace:
	cat try-net7-dotnet-webassembly.code-workspace.template | sed -E "s:<!-- CWD -->:$(HERE):" > try-net7-dotnet-webassembly.code-workspace
