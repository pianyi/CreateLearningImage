# CreateLearningImage
C#(WPF) で 動画から機械学習用の顔イメージデータを作成する

# 動機
機械学習(DeepLearning)の勉強を進めるにあたり、学習用データを作るのが止めも面倒だという事が分かった。
画像サイズの統一、正方形、等合わせるのがメンドクサイ。
探すとPython を使って作成するアプリがいくつか合った。
それを「C#で真似して見よう」というお話です。

# ビルド環境
- Visual Studio 2022 Community
- .net6.0-windows

# 概要
- 動画 及び youtube の動画から、OpenCVの顔認証を利用した画像を抜き出し、ディープラーニング用の学習フォルダ振り分けを行えます。
- 対応したXMLファイルを変更することで何でも検出できるはずです。

# 操作
1. Learning または Test を選択します。
2. 画像のキャプチャタイミング(OpenCVが認識用に利用する画像)を変更します。
3. 対応するXMLファイル及び出力先を指定します。
4. 保存する画像のサイズを指定します。
5. 再生ボタンを押下します。
6. 別ダイアログにOpenCVが認識した画像が表示されます。
　（動画を再生せずに「Distribution」ボタンを押下する事で再振り分けが可能です）

# DISTRIBUTION ダイアログの操作方法
- 基本はテンキーを利用して操作します。
- 最初の1回目はボタンを押下しないとキーボードが動きません【不具合】
- キー操作：
  - +：次の画像へ（移動時に自動保存）
  - -：前の画像へ（移動時に自動保存）
  - *：フォルダの追加
  - .：画像の削除（保存済みのデータはゴミ箱に入ります）
  - 0～9：フォルダ選択（0はOthers専用・10個以上はコンボボックスから選択）

# 出典
動画から顔検出＋保存：
https://qiita.com/hxbdy625/items/0ac47cecc9e6fae2ce51#%E5%AD%A6%E7%BF%92%E7%94%A8%E3%83%87%E3%83%BC%E3%82%BF%E3%81%AE%E4%BD%9C%E6%88%90
https://qiita.com/bianca26neve/items/19085841c9ac6209fe91#%E3%83%A9%E3%82%A4%E3%83%96%E3%82%A2%E3%83%8B%E3%83%A1%E3%83%BC%E3%82%B7%E3%83%A7%E3%83%B3%E3%81%8B%E3%82%89%E9%A1%94%E7%94%BB%E5%83%8F%E3%82%92%E9%9B%86%E3%82%81%E3%81%A6%E3%83%A9%E3%83%99%E3%83%AB%E3%82%92%E3%81%A4%E3%81%91%E3%82%8B

OpenCVによるアニメ顔検出
https://github.com/nagadomi/lbpcascade_animeface

OpenCVのプリインストール検出
https://github.com/opencv/opencv/tree/master/data/haarcascades

# License(ライセンス)
[MIT license](https://en.wikipedia.org/wiki/MIT_License).

日本語：(https://licenses.opensource.jp/MIT/MIT.html)
