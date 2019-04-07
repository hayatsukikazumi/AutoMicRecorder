# AutoMicRecorder

## 何のソフトなの？

このプログラムは、Unity（ゲームエンジン）において、マイクからの音声を
音量レベルに応じて自動的にAudioClipに切り出すためのアセットです。
サンプルとして、自動的に切り出した音声を復唱し、Google Cloud Speech APIで
テキスト化する機能のシーンを用意しました。
Google Cloud Speech APIを使うには、以下の作業も必要です。

* Google Developer Consoleを使うためのユーザ登録
* Google Developer Console内でプロジェクトの作成
* Google Cloud Speech APIの有効化
* APIキー（認証情報）の作成

## 動作条件

Unity5以上をインストールしたマシン（Unity2017.1で動作確認）
Google Cloud Speech APIを使うためには、Unity5.4以上が必要です。

## 変更履歴
* 2017/10/15 Ver. 1.0.0を公開
* 2017/10/17 本ドキュメントの修正
* 2017/10/30 Ver. 1.0.1を公開
    * 録音音声が途切れる不具合の修正
    * GooglePreRecordedSpeechToTextServiceにMax Alternativesパラメータを追加
* 2017/11/04 Ver. 1.0.2を公開
    * AutoMicRecorderクラスのの一部プロパティをpublic変数に変更
    * AutoMicRecorderのSensor On Keep (Sensor On Keep)プロパティの内容を
    最小録音（停止）時間からセンサー不感時間に変更
    * AutoMicRecorder.RecorderStatusにSENSOR_SURPRESSINGステータスを追加
* 2018/05/20 Ver. 2.0.0を公開
    * Speech-to-textアセットの公開終了に伴うサンプルの修正
    （AutoMicRecorderに変更はありません）
* 2018/07/08 Ver. 2.1.0を公開
    * AutoMicRecorderに以下の機能を追加
        * 自動しきい値調整の自動動作（感度指定あり）
        * Microphoneのデバイス名指定
        * Start時のマイク起動指定選択
        * マイク初期化終了イベント
    * メソッド名変更 ResetMicrophone → StartMicrophone
    * 以下の各イベントをdelegateからUnityEvent継承クラスに変更
        * 録音開始
        * 録音終了
        * 録音タイムアウト
        * 自動しきい値調整終了
        * マイク初期化終了
    * AutoMicRecorderのプロパティ追加
        * playOnStart
        * autoCalibration
        * sensivity
        * micDeviceName
        * smoothedSoundLevel
    * 自動録音開始・終了の基準となる音量レベルをsmoothedSoundLevelに変更
* 2018/07/14 Ver. 2.2.0を公開
    * バグ修正
    * AutoMicRecorderに以下の機能を追加
        * マイク初期化終了最大時間指定
        * マイク初期化終了失敗イベント
    * CalibrateThresholdメソッドの修正
        * 戻り値をvoidに変更 しきい値決定アルゴリズムの修正
    * AutoMicRecorderのプロパティ追加
        * maxInitTime

--------
Copyright(c) はやつきかづみ（早月加積） 2017-2018

zzzszoo200@gmail.com
