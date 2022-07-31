FROM debian:bullseye-slim as build-phase

RUN apt-get update && apt-get install -y cmake llvm-9 clang \
    build-essential python3 curl git lldb liblldb-dev \
    libunwind8 libunwind8-dev gettext libicu-dev liblttng-ust-dev \
    libssl-dev libnuma-dev libkrb5-dev zlib1g-dev ninja-build locales locales-all && \
    locale-gen en_US.UTF-8

WORKDIR /work

ARG HASH_OR_BRANCH=9481422

RUN git clone https://github.com/dotnet/runtime.git && \
    cd runtime && \
    git checkout ${HASH_OR_BRANCH} && \
    make -C src/mono/wasm provision-wasm build-all

FROM debian:bullseye-slim as install-phase

RUN apt-get update && apt-get install curl libicu-dev python3 -y

WORKDIR /work

COPY --from=build-phase /work/runtime/artifacts/packages/Release/Shipping /tmp/packages
COPY --from=build-phase /work/runtime/NuGet.config /tmp/BaseNuget.config

RUN cat /tmp/BaseNuget.config | sed -E "s|<\!-- TEST_RESTORE_SOURCES_INSERTION_LINE -->|<add key=\"nuget-local\" value=\"/tmp/packages\" />|" > /tmp/NuGet.config

RUN mkdir -p /usr/local
RUN curl -fsSL https://dot.net/v1/dotnet-install.sh | /bin/bash -s -- -i /usr/local/dotnet -v latest -q daily --channel 7.0

ENV PATH $PATH:/usr/local/dotnet

RUN /usr/local/dotnet/dotnet workload install --skip-manifest-update --no-cache --configfile /tmp/NuGet.config wasm-tools && \
    /usr/local/dotnet/dotnet new install /tmp/packages/Microsoft.NET.Runtime.WebAssembly.Templates.7.0.0-dev.nupkg

CMD /usr/local/dotnet/dotnet --help
