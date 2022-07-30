DOTNET=$$(which dotnet)
NODE=$$(which node)

HERE := $(dir $(abspath "."))
CONFIGURATION=Debug

publish:
	$(DOTNET) publish -c $(CONFIGURATION)

run:
	cd $(HERE)/bin/$(CONFIGURATION)/net7.0/browser-wasm/AppBundle; $(NODE) main.cjs
