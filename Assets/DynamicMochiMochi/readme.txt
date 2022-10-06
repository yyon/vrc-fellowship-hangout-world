つかいかた

VRCSDKをインポートしてからprefabを配置してください。
各prefabは共存できません。どれか一つを配置してください。
MochiRootというprefabはパンケーキ、ゼリー、プリンの切り替えが可能です。
ゼリーを使用する場合は、影付きのDirectional Lightが必要です（空のレイヤーを対象としたDirectional Lightでもかまいません）。また、Pickupではない背景のオブジェクトをEnvironmentレイヤーに指定しておくと綺麗に見えるようになります。
いまのところ正方形のみをサポートしています。それ以外の形にする場合はMask（白黒画像）で設定してください。
MaskはMochiMochiMainMatとMochiMochiDynamicMatの両方のマテリアルで設定する必要があります。
このシェーダーを適応したオブジェクトに作用させたい物体はPickupレイヤーに設定してください。（Prefab内のCameraのCulling Maskの設定を変えれば他のレイヤーのオブジェクトも作用させられます）

MochiMochiDynamicMatにHeightテクスチャを設定することで高さの初期値を設定することができます。（黒が高さ最大、白が高さ最小です）
また、高さの最大値（MaxHeight）と物理演算の設定（Stiffness, Elasticity, Damping）もMochiMochiDynamicMatの方で変更できます。
Stiffness: 変形しにくさです。大きくするとへこませた時へこむ範囲が広くなります。
Elasticity: 元の形に戻る力の強さです。
Damping: 減衰です。全体的に動きが緩やかになります。

・サイズ変更の仕方（パンケーキまたはプリン）　（縦横0.5倍、高さ0.75倍にする場合）
1. ルートオブジェクトの子のMochiMochiのScaleのXとZに0.5、Yに0.75を掛ける。この場合スケールは(X, Y, Z) = (0.25, 0.75, 0.25)となります。
2. CameraのSizeに、1.でXとZのScaleに掛けた数(0.5)を掛ける
3. CameraのClipping PlanesのFarに、1.でYのScaleに掛けた数(0.75)を掛ける

・サイズ変更の仕方（ゼリー）　（縦横0.5倍、高さ0.75倍にする場合）
1. ルートオブジェクトの子のCubeのScaleのXとZに0.5、Yに0.75を掛ける。この場合スケールは(X, Y, Z) = (2.5, 0.975, 2.5)となります。
2. ルートオブジェクトの子のCubeのPositionのYにScaleのYの1/2の値を入れる。この場合値は0.4875となります。
3. CameraのSizeに、1.でXとZのScaleに掛けた数(0.5)を掛ける
4. CameraのClipping PlanesのFarに、1.でYのScaleに掛けた数(0.75)を掛ける

サンプルワールド（VRChat）
https://vrchat.com/home/world/wrld_ec038630-8729-4145-8872-aea9cbe6db3a

このシェーダーはWmup様のSimple Snow Shader（https://booth.pm/ja/items/1677650）を元に作られています。

ライセンスはMITです。
Copyright 2021 OmochiNobiru

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

更新履歴:
2021/12/15 レイトレーシングによる屈折をサポートしたマテリアルを追加。半透明の場合に光沢が薄くなってしまう問題を修正。マットキャップの適用強度設定を追加。
2021/11/17 半透明とMatCapに対応。法線の計算を修正。
2021/08/06 Unity2019でクラッシュする問題を修正
2021/07/23 MochiMochiDynamicMatのMaskとHeightMapが左右反転していた問題を修正。全体を回転させたときに形がおかしくなる問題を修正。MochiMochiDynamicMatのMaskをアルファ値参照から白黒画像に変更。サイズ変更の手順の追加。
2021/07/17 リリース