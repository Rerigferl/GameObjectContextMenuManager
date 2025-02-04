# GameObject ContextMenu Manager
GameObject右クリックしたときに表示される項目を調整できるツール

![image](https://github.com/user-attachments/assets/bb69482a-de2c-4a1b-8461-ed1df32d6ed2)

## Install
- [VPM](https://rerigferl.github.io/vpm)

## How to use

このツールをインストールした直後から、すべての項目が `Others` に移動します。

### Load built-in preset

設定を一から作ることもできますが、プリセットが同梱されています。

適当な GameObject を右クリックし、`Manage Items Edit on GUI..` を使用すると管理用ウィンドウが表示されるので、一番下の `Load from .json` ボタンを使用することでプリセットをロードすることが可能です。

UnityProject の `Packages` 配下にある、このツールのフォルダから、 `BuiltInConfigurations` のフォルダの中に同梱されたプレセットが存在し、それぞれこのようなプリセットです。

#### UnityEmbededOnly-MenuManagerConfiguration.json

Unity組み込みの MenuItem (厳密にはもともと、上部に置かれていた) 物のみを並べ、それ以外を `Others` に表示するプリセットです。

#### UnityEmbededAndVPMToolsOnly-MenuManagerConfiguration.json

開発環境に存在した VPM系ツールと Unity組み込みの MenuItem が並べられ、それ以外はすべて `Others` に表示するプリセットです。

README 最初に表示される画像はこのプリセットです。

#### ReinaSakiria's-MenuManagerConfiguration.json

ReinaSakiria の使用頻度が高いメニューアイテムをいい感じに並べ、ReinaSakiria にとって「呼ぶ頻度が非常に低い」または「一度も呼んだことがない」ものを `Others` に表示するプリセットです。

## Create preset

基本動作は、上から順に `IncludeStatPath` に先頭一致する MenuItem を収集し、`IncludeStatPath` に先頭から一致する部分までを `Path` に置き換えることで、順序制御や再配置を行います。

そして、上で先頭一致で使用された MenuItem は他の設定で先頭一致したとしても、それら設定の影響を受けません。

簡易的なショートカットとして、ルートに存在する MenuItem は一覧表示され、 `Add` ボタンを押すことで追加することができます。

![image](https://github.com/user-attachments/assets/78cb071f-d884-446d-a19b-63265f234f56)


先頭から一致する部分までを `Path` に置き換えるという仕様は、階層移動も可能にするので  `Light Limit Changer/Setup` を `Setup Light Limit Changer` に移動させることも可能です。

`ThisIsSeparator` を有効化することで、その場所に セパレーターを追加することが可能です。
`ThisIsSeparator` が有効な場合は、`IncludeStatPath` は完全に無視され、`Path` を正しく指定しないと、階層の深い場所にセパレーターを置きたい場合は正しく表示されないのでご注意ください。

![image](https://github.com/user-attachments/assets/e115d158-986e-4b78-82bb-8942ad51e7d9)

最後に、 `Save to .json` を使うことで、そのプリセットを json にエンコードし、別のプロジェクトに適用したり、人に配布したりすることも可能です。

![image](https://github.com/user-attachments/assets/c6847c4b-342c-4de6-a3df-dae37f798f9e)
