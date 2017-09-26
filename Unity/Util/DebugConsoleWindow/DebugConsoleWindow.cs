using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

/// <summary>
/// デバッグコンソールウィンドウ
/// シングルトンで管理されているので DebugConsoleWindow.Instance でアクセス可能
/// </summary>
public class DebugConsoleWindow : MonoBehaviour
{
	//****************************************************************
	#region Singleton

	/// trueなら、シーン読み込みや切り替え時にDestroyしない
	[SerializeField] private bool _dontDestroyOnLoad = false;

	private static DebugConsoleWindow _instance;
	public static DebugConsoleWindow Instance
	{
		get {
			if( _instance == null )
			{
				Type t = typeof(DebugConsoleWindow);
				_instance = (DebugConsoleWindow)FindObjectOfType( t );
				if( _instance == null ) {
					Debug.LogError( t + " をアタッチしているGameObjectはありません" );
				}
			}
			return _instance;
		}
	}

	/// <summary>
	/// シングルトンインスタンスが生成されていればtrue
	/// </summary>
	/// <returns></returns>
	public static bool CheckInstance() {
		return ( _instance != null );
	}

	/// <summary>
	/// シングルトンの初期化処理
	/// </summary>
	private void InitSingleton()
	{
		// 他のGameObjectにアタッチされているか調べる
		// アタッチされている場合は破棄する
		if( this != Instance )
		{
			Destroy( this );
			Debug.LogError(
				typeof(DebugConsoleWindow) +
				" は既に他のGameObjectにアタッチされているため、コンポーネントを破棄しました。" +
				" 生成済みのGameObjectは " + Instance.gameObject.name + " です。"
			);
			return;
		}

		if( _dontDestroyOnLoad == true ) {
			DontDestroyOnLoad( this.gameObject );
		}
	}

	#endregion


	//****************************************************************
	#region Variables

	/// <summary>
	/// デバッグログの情報
	/// </summary>
	private class LogInfo {
		public string text = "";
		public LogType type = LogType.Log;
		public bool isConsoleCommand = false;
	}

	[Header("-- UI --")]
	public Button _debugLogButton = null;
	public InputField _consoleInputField = null;
	public Transform _debugLogRootObject = null;
	public RecycleGridLayoutGroup _debugLogGrid = null;

	[Header("-- Animation --")]
	public AnimationCurve _transitAnimation = null;


	private List<LogInfo> _log = new List<LogInfo>();
	private Coroutine _animationCoroutine = null;
	private bool _isTogglePush = false;

	// デバッグコマンド
	private Dictionary<string,ConsoleCommandBase> _dicCommand = new Dictionary<string, ConsoleCommandBase>();

	#endregion

	//****************************************************************
	#region Properties

	/// <summary>
	/// 登録されたデバッグコマンドのDictionary
	/// </summary>
	public Dictionary<string,ConsoleCommandBase> debugCommands {
		get { return _dicCommand; }
	}

	#endregion

	//****************************************************************
	#region MonoBehaviour LifeCycle

	public virtual void Awake() {
		InitSingleton();
	}

	// Use this for initialization
	public virtual void Start() {
		InitDebugLogUI();
	}

	#endregion



	//****************************************************************
	#region デバッグログ初期化

	/// <summary>
	/// デバッグログUIの初期化処理
	/// </summary>
	private void InitDebugLogUI()
	{
		Application.logMessageReceived += LogCallBackHandler;

		RectTransform rtf = this.transform as RectTransform;
		Vector2 size =  _debugLogGrid.cellSize;
		size.x = rtf.rect.width;
		_debugLogGrid.cellSize = size;

		_debugLogGrid.onVisibleListItem = OnUpdateDebugLogListItem;
		_debugLogGrid.listItemCount = 0;

		// デバッグコマンドの初期化
		InitDebugCommand();

		// デバッグコンソールウィンドウ開閉ボタン
		_debugLogButton.onClick.AddListener( OnPushDebugButton );
	}

	/// <summary>
	/// デバッグコマンドの初期化処理
	/// ここに ConsoleCommandBase を継承したクラスを追加していきます。
	/// </summary>
	private void InitDebugCommand()
	{
		// ヘルプコマンドを追加
		AddDebugCommand( new HelpCommand() );
		
		// サンプル用のデバッグコマンドを追加
		// console clear と入力するとコンソールのログがクリアされるコマンドです
		AddDebugCommand( new ConsoleLog() );
	}

	#endregion


	//****************************************************************
	#region デバッグログの更新イベント

	/// <summary>
	/// デバッグボタンを推した時の処理
	/// </summary>
	public void OnPushDebugButton()
	{
		if( _isTogglePush == false ) {
			EnterConsoleWindowAnimation();
			_isTogglePush = true;
		}
		else {
			ExitConsoleWindowAnimation();
			_isTogglePush = false;
		}
	}

	/// <summary>
	/// デバッグログが出力されるたびに呼ばれるコールバック
	/// </summary>
	void LogCallBackHandler( string condition, string stackTrace, LogType type )
	{
		//Environment.NewLine
		string[] lines = condition.Split( '\n' );
		foreach( string text in lines ) {
			LogInfo log = new LogInfo();
			log.text = text;
			log.type = type;
			_log.Add( log );
		}

		_debugLogGrid.listItemCount = _log.Count;
	}

	/// <summary>
	/// ログリストの表示を更新
	/// (RecycleGridLayoutGroupからのコールバック)
	/// </summary>
	private void OnUpdateDebugLogListItem( ListItemContent item, Action cbComplete )
	{
		Vector2 offsetMin = item.rectTransform.offsetMin;
		Vector2 offsetMax = item.rectTransform.offsetMax;
		offsetMin.x = 0;
		offsetMax.x = 0;
		item.rectTransform.offsetMin = offsetMin;
		item.rectTransform.offsetMax = offsetMax;
		
		Text label = FindChildAndGetComponent<Text>( item.transform, "Text" );
		label.text = _log[item.index].text;

		Color col;
		// コンソールコマンドの時
		if( _log[item.index].isConsoleCommand ) {
			col = new Color( 0.5f, 0.5f, 0.5f );
		}
		// ログコマンドの時
		else {
			switch( _log[item.index].type ) {
				case LogType.Log:		ColorUtility.TryParseHtmlString( "#c8c8c8", out col );	break;
				case LogType.Warning:	ColorUtility.TryParseHtmlString( "#FFCC66", out col );	break;
				case LogType.Assert:
				case LogType.Exception:
				case LogType.Error:		ColorUtility.TryParseHtmlString( "#FF6666", out col );	break;
				default:				ColorUtility.TryParseHtmlString( "#c8c8c8", out col );	break;
			}
		}
		label.color = col;

		cbComplete();
	}

	#endregion


	//****************************************************************
	#region コンソール入力関連

	/// <summary>
	/// デバッグコマンドを追加する
	/// </summary>
	private void AddDebugCommand( ConsoleCommandBase command )
	{
		// 既に同じコマンドが登録されていたらエラーを通知
		if( _dicCommand.ContainsKey( command.commandName ) == true ) {
			Debug.LogError(
				"既にこのコマンドは登録済みです。\n" +
				"commandName : " + command.commandName
			);
			return;
		}
		
		_dicCommand.Add( command.commandName, command );
	}

	/// <summary>
	/// コンソール入力時のコールバック
	/// InputFieldで文字を入力し、Enterをおした時に呼ばれるコールバック
	/// </summary>
	public void OnEndEditConsole( string text )
	{
		if( string.IsNullOrEmpty( _consoleInputField.text ) ) return;

		AddConsoleLog( _consoleInputField.text );

		ExecInputCommand( _consoleInputField.text );

		_consoleInputField.text = "";
		_consoleInputField.ActivateInputField();

		_debugLogGrid.listItemCount = _log.Count;
	}


	/// <summary>
	/// コンソールから入力された文字列を追加する時に使う
	/// </summary>
	private void AddConsoleLog( string text )
	{
		if( string.IsNullOrEmpty( text ) ) return;

		LogInfo log = new LogInfo();
		log.text = text;
		log.isConsoleCommand = true;	// "コンソールから入力された"フラグをtrueに。
		_log.Add( log );
	}

	/// <summary>
	/// 入力したコマンドを識別し対応した処理を実行する
	/// </summary>
	private void ExecInputCommand( string command )
	{
		if( string.IsNullOrEmpty( command ) ) return;

		string[] split = command.Split( ' ' );

		if( split == null || split.Length == 0 ) return;

		if( _dicCommand.ContainsKey( split[0] ) == true ) {
			int ret = _dicCommand[split[0]].ExecCommand( split );
			if( ret < 0 ) {
				Debug.LogError( "Command Error : result " + ret );
			}
		}
		else {
			AddConsoleLog( "存在しないコマンドです。" );
		}
	}

	/// <summary>
	/// コンソールログを全てクリアする
	/// </summary>
	public void ClearConsoleLog()
	{
		_log = new List<LogInfo>();
		_debugLogGrid.listItemCount = _log.Count;
		_debugLogGrid.UpdateListAll();
	}

	/// <summary>
	/// コンソールログを直接書き込む
	/// (Debug.Log()はしないのでUnityEditorのデバッグログには残らない)
	/// </summary>
	public void ConsoleLog( string message )
	{
		string[] lines = message.Split( '\n' );
		foreach( string text in lines ) {
			AddConsoleLog( text );
		}

		_debugLogGrid.listItemCount = _log.Count;
	}

	#endregion


	//****************************************************************
	#region コンソールウィンドウ入場/退場アニメーション

	/// <summary>
	/// コンソールウィンドウ入場アニメーション
	/// </summary>
	private void EnterConsoleWindowAnimation()
	{
		if( _animationCoroutine != null ) StopCoroutine( _animationCoroutine );

		_consoleInputField.gameObject.SetActive( true );
		_consoleInputField.text = "";

		// 色の初期化
		{
			// コンソール入力UIをフェードイン
			float fadeAlpha = 0;
			Color tempColor;

			foreach( Graphic img in _consoleInputField.GetComponentsInChildren<Graphic>() ) {
				tempColor = img.color;
				tempColor.a = fadeAlpha;
				img.color = tempColor;
			}

			Graphic image = _consoleInputField.GetComponent<Graphic>();
			tempColor = image.color;
			tempColor.a = fadeAlpha;
			image.color = tempColor;
		}

		_animationCoroutine = StartCoroutine( AnimationCurveCoroutine( 
			0, 0.3f,
			_transitAnimation,
			( float time ) => {
				// コンソールウィンドウを画面内に移動
				Vector3 pos = _debugLogRootObject.localPosition;
				pos.y = Mathf.Lerp( -300.0f, 0.0f, time );
				_debugLogRootObject.localPosition = pos;

				// コンソール入力UIをフェードイン
				float fadeAlpha = Mathf.Lerp( 0.0f, 1.0f, time );
				Color tempColor;

				// あまり良くないけどこれでまとめてフェードさせる・・・
				foreach( Graphic img in _consoleInputField.GetComponentsInChildren<Graphic>() ) {
					tempColor = img.color;
					tempColor.a = fadeAlpha;
					img.color = tempColor;
				}

				Graphic image = _consoleInputField.GetComponent<Graphic>();
				tempColor = image.color;
				tempColor.a = fadeAlpha;
				image.color = tempColor;

				return false;
			},
			// finalize
			() => {
			}
		));
	}

	/// <summary>
	/// コンソールウィンドウの退場アニメーション
	/// </summary>
	private void ExitConsoleWindowAnimation()
	{
		if( _animationCoroutine != null ) StopCoroutine( _animationCoroutine );

		_animationCoroutine = StartCoroutine( AnimationCurveCoroutine(
			0, 0.3f,
			_transitAnimation,
			( float time ) => {
				// コンソールウィンドウを画面外へ
				Vector3 pos = _debugLogRootObject.localPosition;
				pos.y = Mathf.Lerp( 0.0f, -300.0f, time );
				_debugLogRootObject.localPosition = pos;

				// コンソール入力UIをフェードアウト
				float fadeAlpha = Mathf.Lerp( 1.0f, 0.0f, time );
				Color tempColor;

				// あまり良くないけどこれでまとめてフェードさせる・・・
				foreach( Graphic img in _consoleInputField.GetComponentsInChildren<Graphic>() ) {
					tempColor = img.color;
					tempColor.a = fadeAlpha;
					img.color = tempColor;
				}

				Graphic image = _consoleInputField.GetComponent<Graphic>();
				tempColor = image.color;
				tempColor.a = fadeAlpha;
				image.color = tempColor;

				return false;
			},
			// finalize
			() => {
				_consoleInputField.gameObject.SetActive( false );
				_consoleInputField.text = "";
			}
		));
	}

	#endregion


	//****************************************************************
	#region ユーティリティ関連

	/// <summary>
	/// Transformから直接対象のコンポーネントを取得する関数
	/// </summary>
	/// <returns>見つかったコンポーネントが返る。なければnull。</returns>
	/// <param name="self">Self.</param>
	/// <param name="findName">検索するオブジェクト名</param>
	/// <typeparam name="T">The 1st type parameter.</typeparam>
	public T FindChildAndGetComponent<T>( Transform self, string findName ) where T : Component
	{
		Transform findChild = self.Find( findName );
		if( findChild == null ) return null;

		return findChild.GetComponent<T>();
	}

	/// <summary>
	/// アニメーションコルーチンクラス
	/// cbEachFrameコールバックはアニメーション中に毎フレームコールされる。
	/// 中断したい場合はcbEachFrameコールバックの戻り値にtrueを指定する。
	/// </summary>
	/// <param name="delay">アニメーションが開始されるまでの遅延時間</param>
	/// <param name="duration">アニメーション時間</param>
	/// <param name="cbEachFrame">アニメーション中に毎フレーム呼ばれるコールバック。ただし、delay中は呼ばれません。</param>
	/// <param name="cbFinalize">コルーチンが終了したときに必ず呼ばれるコールバック</param>
	/// <returns></returns>
	static public IEnumerator AnimationCurveCoroutine( float delay, float duration, AnimationCurve curve, Func<float,bool> cbEachFrame = null, Action cbFinalize = null )
	{
		if( curve == null || delay == 0 && duration == 0 ) {
			if( cbFinalize != null ) cbFinalize();
			yield break;
		}

		// ディレイ
		yield return new WaitForSeconds( delay );

		float startTime = Time.time;
		float endTime = startTime + duration;
		bool isBreak = false;

		// 初回
		if( cbEachFrame != null ) isBreak = cbEachFrame( 0.0f );
		yield return null;

		// アニメーションループ中
		while( !isBreak && Time.time <= endTime )
		{
			// 進捗の計算
			float progress = (Time.time - startTime) / duration;
			float curveValue = curve.Evaluate( progress );

			// 進捗を0.0f～1.0fで返す
			if( cbEachFrame != null ) isBreak = cbEachFrame( curveValue );
			yield return null;
		}

		// 終了時は必ず1.0fを返す
		if( cbEachFrame != null ) cbEachFrame( 1.0f );
		if( cbFinalize != null ) cbFinalize();

		yield return null;
	}


	/// <summary>
	/// アニメーションコルーチンクラス
	/// cbEachFrameコールバックはアニメーション中に毎フレームコールされる。
	/// 中断したい場合はcbEachFrameコールバックの戻り値にtrueを指定する。
	/// </summary>
	/// <param name="delay">アニメーションが開始されるまでの遅延時間</param>
	/// <param name="duration">アニメーション時間</param>
	/// <param name="cbEachFrame">アニメーション中に毎フレーム呼ばれるコールバック。ただし、delay中は呼ばれません。</param>
	/// <param name="cbFinalize">コルーチンが終了したときに必ず呼ばれるコールバック</param>
	/// <returns></returns>
	static public IEnumerator AnimationCoroutine( float delay, float duration, Func<float,bool> cbEachFrame = null, Action cbFinalize = null )
	{
		if( delay == 0 && duration == 0 ) {
			if( cbFinalize != null ) cbFinalize();
			yield break;
		}

		// ディレイ
		yield return new WaitForSeconds( delay );

		float startTime = Time.time;
		float endTime = startTime + duration;
		bool isBreak = false;

		// 初回
		if( cbEachFrame != null ) isBreak = cbEachFrame( 0.0f );
		yield return null;

		// アニメーションループ中
		while( !isBreak && Time.time <= endTime )
		{
			// 進捗の計算
			float progress = (Time.time - startTime) / duration;

			// 進捗を0.0f～1.0fで返す
			if( cbEachFrame != null ) isBreak = cbEachFrame( progress );
			yield return null;
		}

		// 終了時は必ず1.0fを返す
		if( cbEachFrame != null ) cbEachFrame( 1.0f );
		if( cbFinalize != null ) cbFinalize();

		yield return null;
	}
	/* 【 上記関数の使用例 】
	// 開始直後に200ミリ秒のウェイト
	// 1秒かけて posX に 0.0f～640.0f が代入される
	// コルーチンなのでStartCoroutine()を忘れずに。
	StartCoroutine( AnimationCoroutine( 
		0.2f,	// 開始直後のディレイ
		1.0f,	// アニメーション時間
		
		// アニメーションループ中のコールバック
		( float time ) => {
			posX = Mathf.Lerp( 0.0f, 640.0f, time );
			return false;　// 条件を持たせて true を返すと処理を中断します
		}
	));
	*/

	#endregion
}