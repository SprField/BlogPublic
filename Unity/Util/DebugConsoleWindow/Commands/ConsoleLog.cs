using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;

public class ConsoleLog : ConsoleCommandBase
{
	public ConsoleLog() {
		commandName = "console";
		commandOverview = "console clear と入力するとコンソールログをクリアするサンプルです";
		commandHelp = 
			"--------------------------------------------------------------\n" + 
			"console clear\n" +
			"コンソールログを全て消去します\n" +
			"--------------------------------------------------------------";

		// isEnableCommandをfalseにすると、入力されても処理を実行しないようにできる。
		isEnableCommand = true;
	}

	/// <summary>
	/// コマンド実行関数をオーバライド
	/// </summary>
	/// <returns>原則としてマイナス値ならエラー、０以上なら正常終了</returns>
	/// <param name="args">引数の文字列</param>
	public override int ExecCommand( string[] args)
	{
		if( args.Length < 2 ) {
			return -1;	// 引数が足りない場合はエラー
		}

		// 書き分け方はご自由に
		switch( args[1] )
		{
			// コンソールログ消去コマンド
			case "clear":
				return ExecClearLog( args );
		}

		return 0;
	}

	/// <summary>
	/// "console clear"した時の処理
	/// </summary>
	/// <returns>The console log.</returns>
	/// <param name="args">Arguments.</param>
	private int ExecClearLog( string[] args )
	{
		// デバッグウィンドウはシングルトンなのでどこからでもアクセス可能
		DebugConsoleWindow.Instance.ClearConsoleLog();
		DebugConsoleWindow.Instance.ConsoleLog( GetCommandAll(args) );	// 一応コマンド使った形跡だけ残す
		return 0;
	}

	/// <summary>
	/// スプリット済みのコマンドを再構築して元の文字列を作る
	/// </summary>
	/// <returns>The command all.</returns>
	/// <param name="splitCommands">Split commands.</param>
	protected string GetCommandAll( string[] splitCommands )
	{
		StringBuilder sb = new StringBuilder();
		int i = 0;
		foreach( string str in splitCommands ) {
			if( i++ > 0 ) sb.Append( " " );
			sb.Append( str );
		}
		return sb.ToString();
	}
}