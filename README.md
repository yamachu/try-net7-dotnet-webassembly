# Try .NET 7 .NET WebAssembly

C100にて頒布予定の技術同人誌の内容を簡単に試せるようにするリポジトリ

https://github.com/dotnet/runtime で開発が行われている [wasm](https://github.com/dotnet/runtime/tree/main/src/mono/wasm) に関する技術を各リリースだったり、大幅な機能追加のタイミングで試せるようにする予定

## Directory structure

- wasmconsole-${shorthash or release}
  - 各リリースやコミットの段階のテンプレートを反映し、そのタイミングで追加された機能を使ってみている砂場
- tools
  - Dockerfile
    - HASH_OR_BRANCH ARGを指定してビルドすると、いい感じにそのタイミングの dotnet/runtime がビルドできるDockerfile
    - https://hub.docker.com/r/yamachu/dotnet-wasm-tools で公開中
  - Nuget.template.config
    - dotnet/runtime をビルドして得られたartifactsを参照する際に使用するconfig
- runtime
  - ${shorthash or release}
    - 環境を分離するために dotnet-install scripts でダウンロードした.NET SDKを配置する箇所
    - ex: preview7
- packages
  - ${shorthash or release}
    - dotnet/runtime をビルドして得られた artifacts/packages/Release/Shipping 以下を展開する場所
    - ex: preview7/*.nupkg

## How to try

1. https://github.com/dotnet/runtime/tree/main/docs/workflow/requirements を見て環境構築する
2. dotnet/runtime をビルドする

は大変なので、Dockerfileを使用しビルドするか、Docker Hubで配布しているImageを使用しビルドする
