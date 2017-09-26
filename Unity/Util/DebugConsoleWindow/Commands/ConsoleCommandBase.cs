using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;

//_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/
/// <summary>
/// コンソールコマンドを定義するための基底クラス
/// コンソールコマンドを実装する場合、このクラスを必ず継承する。
/// </summary>
//_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/
[Serializable]
public abstract class ConsoleCommandBase
{
	//****************************************************************
	//  Defines
	//****************************************************************


	//****************************************************************
	//  Values
	//****************************************************************

	/// コンソールコマンド名
	[SerializeField] private string _commandName;
	[SerializeField] private string _commandOverview = "----";
	[SerializeField] private string _commandHelp = "----";
	[SerializeField] private bool _isEnableCommand = true;


	//****************************************************************
	//  Properties
	//****************************************************************

	/// コマンド名
	public string commandName {
		get { return _commandName; }
		protected set { _commandName = value; }
	}
	public string commandOverview {
		get { return _commandOverview; }
		protected set { _commandOverview = value; }
	}
	public string commandHelp {
		get { return _commandHelp; }
		protected set { _commandHelp = value; }
	}

	/// trueなら使用可能なコマンド
	public bool isEnableCommand {
		get { return _isEnableCommand; }
		protected set { _isEnableCommand = value; }
	}


	//****************************************************************
	//
	//  コマンドの実装
	//
	//****************************************************************

	//======================================================
	/// <summary>
	/// コマンドの実行
	/// </summary>
	/// <returns>原則としてマイナス値ならエラー、０以上なら正常終了</returns>
	/// <param name="args">引数の文字列</param>
	//======================================================
	public virtual int ExecCommand( string[] args )
	{
		return 0;
	}
}