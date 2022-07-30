DOTNET=$$(which dotnet)
NODE=$$(which node)

HERE := $(dir $(abspath "."))

publish:
	$(DOTNET) publish

run:
	cd $(HERE)/bin/Debug/net7.0/browser-wasm/AppBundle; $(NODE) main.cjs
