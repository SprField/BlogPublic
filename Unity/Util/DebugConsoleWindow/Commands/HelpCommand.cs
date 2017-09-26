using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// デバッグコンソールにhelpと入力した時のコマンド
/// </summary>
public class HelpCommand : ConsoleCommandBase
{
	public HelpCommand()
	{
		commandName = "help";
		commandOverview = "ヘルプコマンドです";
		commandHelp =
			"--------------------------------------------------------------\n" + 
			"help\n" +
			"全てのコマンド一覧を表示します。\n" +
			"　\n" +
			"help <コマンド名>\n" +
			"第一引数にコマンド名を入力すると対象のコマンドの詳細なヘルプが見られます。\n" +
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
		// "help"単発の場合は全コマンドの概要説明一覧を吐き出す
		if( args.Length == 1 ) {
			return AllCommandOverview();
		}

		// "help [コマンド名]"と入力されたら詳細な説明文を表示
		return CommandHelpLog( args[1] );
	}

	/// <summary>
	/// 全てのコマンドと概要説明をログに出力
	/// </summary>
	/// <returns>The command overview.</returns>
	private int AllCommandOverview()
	{
		foreach( var command in DebugConsoleWindow.Instance.debugCommands.Values )
		{
			DebugConsoleWindow.Instance.ConsoleLog( command.commandName + " : " + command.commandOverview );
		}
		return 0;
	}

	private int CommandHelpLog( string command_name )
	{
		if( DebugConsoleWindow.Instance.debugCommands.ContainsKey( command_name ) == false ) {
			Debug.LogError( "指定したコマンド名は見つかりませんでした。" );
			return -10;
		}

		ConsoleCommandBase command = DebugConsoleWindow.Instance.debugCommands[ command_name ];

		// コマンドがあれば commandHelp の内容を表示
		DebugConsoleWindow.Instance.ConsoleLog( command.commandHelp );

		return 0;
	}
}
