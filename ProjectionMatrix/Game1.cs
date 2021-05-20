using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace ProjectionMatrix
{
	/// <summary>
	/// ゲームメインクラス
	/// </summary>
	public class Game1 : Game
	{
    /// <summary>
    /// グラフィックデバイス管理クラス
    /// </summary>
    private readonly GraphicsDeviceManager _graphics = null;

    /// <summary>
    /// スプライトのバッチ化クラス
    /// </summary>
    private SpriteBatch _spriteBatch = null;

    /// <summary>
    /// スプライトでテキストを描画するためのフォント
    /// </summary>
    private SpriteFont _font = null;

    /// <summary>
    /// モデル
    /// </summary>
    private Model _model = null;

    /// <summary>
    /// 視野角(radian)
    /// </summary>
    private float _angle = 45.0f;

    /// <summary>
    /// アスペクト比
    /// </summary>
    private float _aspect = 1.0f;

    /// <summary>
    /// 手前のクリッピング位置
    /// </summary>
    private float _nearLimit = 1.0f;

    /// <summary>
    /// 奥のクリッピング位置
    /// </summary>
    private float _farLimit = 100.0f;

    /// <summary>
    /// 自動動作フラグ
    /// </summary>
    private int _autoMode = 0;

    /// <summary>
    /// マウスボタン押下フラグ
    /// </summary>
    private bool _isMousePressed = false;


    /// <summary>
    /// GameMain コンストラクタ
    /// </summary>
    public Game1()
    {
      // グラフィックデバイス管理クラスの作成
      _graphics = new GraphicsDeviceManager(this);

      // ゲームコンテンツのルートディレクトリを設定
      Content.RootDirectory = "Content";

      // マウスカーソルを表示
      IsMouseVisible = true;
    }

    /// <summary>
    /// ゲームが始まる前の初期化処理を行うメソッド
    /// グラフィック以外のデータの読み込み、コンポーネントの初期化を行う
    /// </summary>
    protected override void Initialize()
    {
      // TODO: ここに初期化ロジックを書いてください

      // コンポーネントの初期化などを行います
      base.Initialize();
    }

    /// <summary>
    /// ゲームが始まるときに一回だけ呼ばれ
    /// すべてのゲームコンテンツを読み込みます
    /// </summary>
    protected override void LoadContent()
    {
      // テクスチャーを描画するためのスプライトバッチクラスを作成します
      _spriteBatch = new SpriteBatch(GraphicsDevice);

      // フォントをコンテンツパイプラインから読み込む
      _font = Content.Load<SpriteFont>("Font");

      // モデルを作成
      _model = Content.Load<Model>("Model");

      foreach (ModelMesh mesh in _model.Meshes)
      {
        foreach (BasicEffect effect in mesh.Effects)
        {
          // デフォルトのライト適用
          effect.EnableDefaultLighting();

          // ビューマトリックスをあらかじめ設定
          effect.View = Matrix.CreateLookAt(
              new Vector3(3.0f, 3.0f, 6.0f),
              Vector3.Zero,
              Vector3.Up
          );
        }
      }

      // アスペクト比の初期値を設定
      _aspect = (float)GraphicsDevice.Viewport.Width / GraphicsDevice.Viewport.Height;
    }

    /// <summary>
    /// ゲームが終了するときに一回だけ呼ばれ
    /// すべてのゲームコンテンツをアンロードします
    /// </summary>
    protected override void UnloadContent()
    {
      // TODO: ContentManager で管理されていないコンテンツを
      //       ここでアンロードしてください
    }

    /// <summary>
    /// 描画以外のデータ更新等の処理を行うメソッド
    /// 主に入力処理、衝突判定などの物理計算、オーディオの再生など
    /// </summary>
    /// <param name="gameTime">このメソッドが呼ばれたときのゲーム時間</param>
    protected override void Update(GameTime gameTime)
    {
      KeyboardState keyState = Keyboard.GetState();
      MouseState mouseState = Mouse.GetState();
      GamePadState gamePadState = GamePad.GetState(PlayerIndex.One);

      // ゲームパッドの Back ボタン、またはキーボードの Esc キーを押したときにゲームを終了させます
      if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
      {
        Exit();
      }

      // マウスによる自動動作切り替え
      if (_isMousePressed == false &&
          mouseState.LeftButton == ButtonState.Pressed)
      {
        _isMousePressed = true;

        _autoMode = (_autoMode + 1) % 4;
      }
      _isMousePressed = mouseState.LeftButton == ButtonState.Pressed;

      ///// 視野角を増減させる /////
      float angleSpeed = 0.05f;
      _angle += gamePadState.ThumbSticks.Left.Y * _angle * angleSpeed;
      if (keyState.IsKeyDown(Keys.Down))
      {
        _angle -= _angle * angleSpeed;
      }
      if (keyState.IsKeyDown(Keys.Up))
      {
        _angle += _angle * angleSpeed;
      }
      if (_autoMode == 1)
      {
        _angle = (float)(Math.Sin(gameTime.TotalGameTime.TotalSeconds) + 1.0f) * 90.0f;
      }

      // 視野角は０～１８０度までの範囲とする
      if (_angle <= 0.0f)
      {
        _angle = 0.0001f;
      }
      else if (_angle >= 180.0f)
      {
        _angle = 179.9999f;
      }

      /////// アスペクト比 /////
      float aspectSpeed = 0.01f;
      _aspect -= gamePadState.Triggers.Left * _aspect * aspectSpeed;
      _aspect += gamePadState.Triggers.Right * _aspect * aspectSpeed;
      if (keyState.IsKeyDown(Keys.Left))
      {
        _aspect -= _aspect * aspectSpeed;
      }
      if (keyState.IsKeyDown(Keys.Right))
      {
        _aspect += _aspect * aspectSpeed;
      }
      if (_autoMode == 2)
      {
        _aspect = (float)(Math.Sin(gameTime.TotalGameTime.TotalSeconds) + 1.0f) * 2.0f;
      }

      /////// 奥のクリッピング位置 /////
      float clipSpeed = 0.05f;
      _farLimit += gamePadState.ThumbSticks.Right.Y * _farLimit * clipSpeed;
      if (keyState.IsKeyDown(Keys.Z))
      {
        _farLimit -= _farLimit * clipSpeed;
      }
      if (keyState.IsKeyDown(Keys.A))
      {
        _farLimit += _farLimit * clipSpeed;
      }
      if (_autoMode == 3)
      {
        _farLimit = (float)(Math.Sin(gameTime.TotalGameTime.TotalSeconds) + 1.0f) * 8.0f;
      }

      if (_farLimit <= _nearLimit)
      {
        // 手前のクリッピング位置よりも手前に来てはいけない
        _farLimit = _nearLimit + 0.0001f;
      }

      // 登録された GameComponent を更新する
      base.Update(gameTime);
    }

    /// <summary>
    /// 描画処理を行うメソッド
    /// </summary>
    /// <param name="gameTime">このメソッドが呼ばれたときのゲーム時間</param>
    protected override void Draw(GameTime gameTime)
    {
      // 画面を指定した色でクリアします
      GraphicsDevice.Clear(Color.CornflowerBlue);

      // プロジェクションマトリックスを作成
      Matrix projection = Matrix.CreatePerspectiveFieldOfView(
              MathHelper.ToRadians(_angle),
              _aspect,
              _nearLimit,
              _farLimit
          );

      foreach (ModelMesh mesh in _model.Meshes)
      {
        foreach (BasicEffect effect in mesh.Effects)
        {
          // プロジェクションマトリックスを設定
          effect.Projection = projection;
        }

        // モデルを描画
        mesh.Draw();
      }

      // スプライトの描画準備
      _spriteBatch.Begin();

      // テキストをスプライトとして描画する
      _spriteBatch.DrawString(_font,
          "Angle : " + _angle + Environment.NewLine +
          "Aspect : " + _aspect + Environment.NewLine +
          "NearLimit : " + _nearLimit + Environment.NewLine +
          "FarLimit : " + _farLimit + Environment.NewLine +
          "MousePressAutoMode : " + _autoMode,
          new Vector2(50, 50), Color.White);

      // スプライトの一括描画
      _spriteBatch.End();

      // 登録された DrawableGameComponent を描画する
      base.Draw(gameTime);
    }
  }
}
