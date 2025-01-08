# AutoPenGimmick

[![サンプル動画](http://img.youtube.com/vi/jUtED8EdFs0/0.jpg)](https://www.youtube.com/watch?v=jUtED8EdFs0)  
https://www.youtube.com/watch?v=jUtED8EdFs0

自動でペンを動かすギミックです。  
事前に登録した文字や絵を描くことができます。

現在は `QvPen` (Package Shop @aivrc, ureishi, https://booth.pm/ja/items/1555789) のみ対応しています。



# できること


#### 1. プリセットに登録してある絵や文字を描画できます (描画サイズの変更もできます)
<img height=300 src=res/images/3.png>


#### 2. キーボード型UIから かな、英数字、記号 などをタイプできます (文字サイズ、文字間隔、行間隔の変更もできます)
<img height=300 src=res/images/4.png>


#### 3. 登録してある絵や文字の編集、新規登録ができます
<img height=300 src=res/images/5.png>


#### 4. プレイ中にUIからデータを直接入力して描画することができます  (ワールドを訪れたユーザーが自作のデータで描画することができます)

<img height=300 src=res/images/8.png>

#### 4. 入力したテキストを描画することができます

<img height=300 src=res/images/9.png>


# インストール


### QvPen の導入
まず一番最初に `QvPen` (https://booth.pm/ja/items/1555789) を導入してください。


### AutoPen の導入
`AutoPen.unitypackage` から導入してください。

### フォントの設定

導入直後はUIが文字化けしている可能性があります。  
これは`TextMesh Pro`のフォントが日本語に対応していないためとなります。  
以下の手順でフォールバックフォントに日本語対応フォントを設定します。  

1. 「Edit > Project Settings...」を選択します。  
<img height=450 src=res/images/30.jpg>

2. 「TextMesh Pro > Settings」を選択します。  
<img height=300 src=res/images/31.jpg>

3. Fallback Font Assets List」に「NotoSansJP..._fallback」を選択します。  
これでまだ日本語が表示されない場合は別のシーンを開いて再度元のシーンを開きます。  
<img height=300 src=res/images/32.jpg>


# 使い方


## サンプルシーン

`Scenes`にサンプルシーンが入っています。

<img height=100 src=res/images/1.png>


- AutoPenSample  
サンプルシーンです。  
ギミックを実際に試したい場合はこのシーンを使用してください。
Unity上での実行や、ローカルワールドでの実行、VRCへのアップロードも可能です。

- CharaDataEditor  
- DrawDataEditor  
絵、文字データの編集、登録が行えるシーンです。


シーンを開いたときに「TMP Importer」が表示された場合は「Import TMP Essentials」を押します。

<img height=200 src=res/images/12.png>

## AutoPen

パネル内のボタン(①)を押すとそこに登録されたデータで描画を行います。  
②のスライダーで描画サイズの変更を行えます。  
③のボタンを押すとテキストボックス(③)が出現します。  
③にデータを直接入力して下部のボタンを押すとそのデータで描画を行うことができます。  

描画の向きはパネルの向きと一致します。上部の赤い三角の位置から描画を開始します。

<img height=250 src=res/images/10.png><img height=250 src=res/images/3.png>



## AutoPen Typewriter

キーボード型のUIのキーをクリックすることで文字を描くことができます。  
ひらがな、カタカナ、アルファベット、数字に対応しています。  

Spaceキーで1字送り、Enterキーで改行が行えます。  

<img height=300 src=res/images/11.png>  

<br>

キーのクリックの代わりにテキストを入力することでも文字の描画が可能です。

<img height=300 src=res/images/9.png>  


<hr>

歯車マークのボタンから設定画面を開くことができます。  
設定画面では文字の大きさ、文字の間隔、行の間隔を設定できます。  
「Auto」の上のマークが緑になっている状態では文字の大きさ以外の数値が自動で設定されます。  
個別に設定したい場合は緑のマークをクリックして解除します。  

<img height=300 src=res/images/13.png>  

<br>

文字の描画はカーソルの位置から始まります。  
パネルを移動してもカーソルは移動しません。  
「Reset Cursor」ボタンを押すとカーソルがパネルの初期位置に戻ります。  
カーソルはPickUpが設定されていて掴んで動かすこともできます。  

<img height=300 src=res/images/14.png>  


カーソルは一定時間経過後に非表示になります。  
非表示になっていても掴むことができます。  
非表示状態のカーソルを掴んだり、「Reset Cursor」ボタンや「Show Cursor」ボタンを押すと再度表示されます。



## ペンの紐づけ
ギミックを使用するにはペンと紐づけを行う必要があります。

#### ・QvPen
`AutoPenManager`オブジェクトをクリックして`Inspector`の`AutoPenManager`欄で「QvPen 自動検索」を押します。  
手動で行う場合は `Pen Managers`、`Pens`、`Pen Pickups`にオブジェクトを割り当てて下さい。  

<img height=350 src=res/images/6.png>

登録したくないペンがある場合はそれを選択した状態で左下の「-」ボタンを押して削除してください。

<img height=350 src=res/images/7.png>

<br>

カラーパレットはゲーム実行時に自動で登録されたペンから色を取得するため編集不要です。  
<img height=200 src=res/images/25.png>  



# データの登録

## AutoPen用データ

`DrawDataEditor`シーンから描画データの登録が行えます。  

`DrawData_...`で始まる名前のオブジェクトそれぞれに各描画データが入っています。  

<img height=300 src=res/images/15.png>  

<br>

`Scene`ビュー上部の`Gizmo`を有効にするとUnityを再生しなくても`Scene`ビューに描画線のプレビューを表示できます。  

<img height=200 src=res/images/16.png>  


<hr>



描画データオブジェクトを選択して`Inspector`にて「データ生成」をクリックすると`Draw Data String`にデータが生成されます。  
これをコピーして`AutoPenButtonInputUI`オブジェクトの`Draw Data String List`のいずれかのスロットに貼り付けることで反映されます。  

<img height=350 src=res/images/17.png>

<img height=500 src=res/images/18.png>  


<hr>


描画データは以下のように複数のオブジェクトから構成されます。  

<img height=500 src=res/images/19.png>  

線オブジェクト1つが1本の線となります。  
線オブジェクトの子には線の描画に必要な点情報を決める点オブジェクトが配置されます。

線グループオブジェクトは任意で線オブジェクトをまとめることができ、データを管理しやすくします。

<hr>

線オブジェクトは名前に線の色や種類の情報を持つことも可能です。  
以下のように指定します。  

`{ペンの番号},{線の種類},{任意の文字}`  

`{ペンの番号}`: `AutoPenManager`オブジェクトに登録されているペンの番号を指定できます。(最初のペンの場合 → 0)
`{線の種類}`: 0 → 直線, 1 → Spline曲線, のように線の種類を指定できます。  
`{任意の文字}`: 作成者が分かりやすいような文字を設定できます。


<hr>

「Draw Data Generator」には「トランスフォームをリセット (単体データ用)」、「トランスフォームを最小領域でクリップ」ボタンがあります。  
それぞれ以下の機能になっています。

- トランスフォームをリセット (単体データ用)  
pointオブジェクトの位置はそのままに、各オブジェクトの位置を0に、サイズを1に整えます。  
オブジェクトの名前も上から順に数字を振りなおします。  
これを行っても行わなくても生成されるデータに変化はありません。  

- トランスフォームを最小領域でクリップ  
pointオブジェクトの位置はそのままに、  
線グループオブジェクト、線オブジェクトの位置を、それぞれの持つ子を囲む最小領域の左下に合わせます。  
子オブジェクトと親のオブジェクトの位置が近くなるため編集が行いやすくなります。  

<img height=250 src=res/images/33.png>  

<hr>

作成したデータのプレビューの表示非表示は`Gizmos`アイコンから行えますが、  
`Draw Data Visualizer`の「Show Line」のチェックを変更することでも切り替えが可能です。  

<img height=200 src=res/images/34.png>  

## AutoPenWriter用データ

`CharaDataEditor`シーンから文字データの登録が行えます。  
`CharaData`オブジェクトに文字データが入っています。  

描画データオブジェクトを選択して`Inspector`にて「データ生成」をクリックすると`Chara Data String`にデータが生成されます。  
これをコピーして`AutoPenWriter`オブジェクトの`Chara Data String`に貼り付けることで反映されます。  


<img height=300 src=res/images/28.png>  

<img height=300 src=res/images/29.png>  

<hr>


`CharaData`オブジェクトの下にはAutoPen用データで使用した描画データと同じ形式のオブジェクトが格納されています。  

<img height=300 src=res/images/20.png>  

<br>

描画データオブジェクトの名前は以下のように指定します。  

`{文字ID},{文字アクション},{任意の文字}`  

`{文字ID}`: 任意の数値を設定します。他の文字と重複しないものにします。桁数が多すぎるエラーとなります。(intの範囲)  
`{文字アクション}`: 空文字 or 0 → なし, 1 → 描画後に1文字戻る (濁点、半濁点など前の文字に重ねたい場合に1にします)  
`{任意の文字}`: 作成者が分かりやすいような文字を設定できます。


<hr>

同じく`Scene`ビュー上部の`Gizmo`を有効にするとUnityを再生しなくても`Scene`ビューに描画線のプレビューを表示できます。  
文字データは色データを見ないため全て黒線で表示されます。  

<img height=200 src=res/images/16.png>  

<br>

黒線の後ろの白文字はGuidオブジェクトに入っているテキストです。  
文字データを作る際に参考にしたものです。  

<img height=200 src=res/images/21.png>  


# ギミックが動作しない場合
動作しない場合は以下を参考にして下さい。

## ペンが登録されていない

`AutoPenManager`オブジェクトの「Pen Managers」、「Pens」、「Pen Pickups」にQvPenが登録されていない可能性があります。  
右の数字が「15」などになっていても、参照データが`NULL`や`missing`になっている場合があるため開いて確認してみてください。  
自動設定する場合は「QvPen自動検索」ボタンを押してください。  

<img height=350 src=res/images/22.png>  

## `AutoPen`の参照が切れている

`AutoPenButtonInputUI`や`AutoPenWriter`は`AutoPen`というオブジェクトを参照しています。  
`AutoPen`オブジェクトは`AutoPenManager`オブジェクトの下存在するので、こちらと紐づけてください。  

<img height=350 src=res/images/24.png>  

## 複数のギミックが存在していてそれぞれ同じ`AutoPen`を参照している

`AutoPen`オブジェクトは動かす機能を持っていますが、複数のギミックから同時に命令が送られると競合してしまいます。  
`AutoPenButtonInputUI`と`AutoPenWriter`など複数同時に使用する場合は`AutoPen`オブジェクトをコピペして増やし参照を別にして下さい。  


<img height=350 src=res/images/23.png>  


## 押せないボタンがある、カラーパレットの端の方が押せない
ボタンがキャンバスの範囲をはみ出ていると押せません。  
`Gizmos`表示を有効にした状態で`Canvas`の`Rect Transform`と`Box Collider`が全てのUIを覆うように位置とサイズを調整してください。

<img height=350 src=res/images/26.png>  

<br>

カラーパレットの端が押せない場合はペンの数が想定より多いためカラーパレットがキャンバスからはみ出ている可能性があります。  
上記の手順でキャンバスサイズを調整しても解決しますが、以下の`Colors`オブジェクトの位置をずらすことでも位置調整可能です。  
`Colors`オブジェクトの下には2つのカラーパレットがありますが、この2つの間隔が実行時にカラーパレットが並ぶ間隔になります。    
<img height=200 src=res/images/27.png>  



##

#### 注意点
以下は検証が十分に行えていないため動作が不安定な可能性があります
- ギミックのボタンを複数人で同時押しした場合
- 複数のギミックを同時に使用した場合
- 複数人で同時に長時間使用した場合

※ QvPenが使用できなくなった、同期されなくなったなどはプレイヤーの通信環境による可能性もあります
