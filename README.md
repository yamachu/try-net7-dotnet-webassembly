# Try .NET 7 .NET WebAssembly

C100にて頒布予定の技術同人誌の内容を簡単に試せるようにするリポジトリ

https://www.youtube.com/watch?v=3o91I6lD-Bo で .NET WebAssembly without Blazor UI として紹介されていた、
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
  - nightly
    - nightlyなパッケージをインストールする砂場
- packages
  - ${shorthash or release}
    - dotnet/runtime をビルドして得られた artifacts/packages/Release/Shipping 以下を展開する場所
    - ex: preview7/*.nupkg
- nightly
  - runtime
    - dotnet/runtime リポジトリのclone先

## How to try

### Easy (Recommended)

#### Docker

- tools/Dockerfileを使ってBuild

or 

- Docker HubからPull
  - https://hub.docker.com/r/yamachu/dotnet-wasm-tools

#### Use pre-built packages

```sh
# 初期インストール
$ make dotnet7/nightly nightly/install/template
# 試し終わったらTemplate削除した方が良い
$ make dotnet7/uninstall/template/nightly
```

### Hard

1. https://github.com/dotnet/runtime/tree/main/docs/workflow/requirements を見て環境構築する
2. dotnet/runtime をビルドする

### memo

```sh
$ cat ./nightly/runtime/.git/config
[core]
	repositoryformatversion = 0
	filemode = true
	bare = false
	logallrefupdates = true
	ignorecase = true
	precomposeunicode = true
[remote "origin"]
	url = https://github.com/dotnet/runtime.git
	fetch = +refs/heads/main:refs/remotes/origin/main
	fetch = +refs/heads/release/7.0*:refs/remotes/origin/release/7.0*
[branch "main"]
	remote = origin
	merge = refs/heads/main
```

package.json
```json
{
  "dependencies": {
    "@microsoft/dotnet-runtime": "link:./bin/Debug/net7.0/browser-wasm"
  }
}
```

```sh
$ runtime/nightly/dotnet new wasmconsole -n wasmconsole-nightly
```
