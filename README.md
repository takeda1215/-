# 指の半拘束状態におけるボタンの押下感呈示システム

## 概要
本プロジェクトは、仮想環境(VR)において、物理的なボタンの「押した感覚」を低コストで再現する力触覚呈示システムです。指の動きを部分的に制限した「半拘束」状態でロードセルセンサを用いて押下力を計測し、その力に応じてUnityで作成した3Dオブジェクトが沈み込む視覚フィードバックと、押下音による聴覚フィードバックをユーザーに提供します。

## デモ (Demonstration)
## 特徴 (Features)
- 高価な専用デバイスを使わずに、ロードセルセンサと視覚・聴覚フィードバックを組み合わせてボタンの押下感を再現。
- キーボード、リモコン、マウスボタンなど、特性の異なる5種類の機械式ボタンの挙動を、物理パラメータ（硬さ、しきい値、変位量）を変更することでシミュレート。
- [脇田らの研究](https://www.jstage.jst.go.jp/article/ieejeiss/136/8/136_1092/_article/-char/ja/)で計測された実物のボタンのパラメータを利用し、リアルな押下感の再現を追求。

## システム構成 (System Architecture)
#### ハードウェア
- Arduino UNO
- ロードセルセンサ
- HX711 コンバータ

#### ソフトウェア
- Unity (バージョン: 2022.3.22f1)
- Arduino IDE

## セットアップ方法 (Setup & Usage)
1. **ハードウェアの接続:**
   2. **Arduinoの準備:**
   1. `/Arduino` フォルダ内のスケッチをArduino IDEで開きます。
   2. スケッチをArduinoボードに書き込みます。
3. **Unityの実行:**
   1. `/UnityProject` フォルダをUnity Hubから開きます。
   2. `MainScene` (※シーン名はご自身のものに修正) を開きます。
   3. 実行（Play）ボタンを押します。

## 参考文献 & クレジット (References & Credits)
- 本研究内容は、電子情報通信学会 2025年3月知覚情報研究会（論文番号: PI-25-015）にて発表しました。
- 本研究は、広島市立大学の脇田 航先生の指導のもとで行われました。

#### 使用アセット
- **Female hand:** [Deep3dstudio](https://sketchfab.com/3d-models/female-hand-3e9b8ad1942048e3a267d92fb1124d46) 
- **Keyboard:** [hslj](https://sketchfab.com/3d-models/keyboard-c7c32c29f3544cbcb5a54a5dc6299016) 
- **Remote Control:** [lucky95](https://www.cgtrader.com/free-3d-models/electronics/other/remote-control-af1f9bc1-1e84-447b-a310-b3d448bd8ba3) 
- **Mouse:** [anshirk](https://www.cgtrader.com/free-3d-models/electronics/computer/logitech-g304-wireless-mouse) 
- **Sponge:** [Aullwen](https://sketchfab.com/3d-models/sponge-97ace64cdf0c4521b82416f3b30c61ce) 
- **iPad Pro:** [Lumberjack](https://www.cgtrader.com/free-3d-models/electronics/computer/ipad-pro-2020-dd8daf25-2618-41db-a3b1-09d77664709a)
