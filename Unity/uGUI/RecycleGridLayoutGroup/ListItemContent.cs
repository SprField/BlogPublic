using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ListItemContent : MonoBehaviour
{
	public enum eState {
		Ready,
		Loading,
		LoadCompleted,
	}
	
	[SerializeField] private int _index = 0;
	[SerializeField] private eState _state = eState.Ready;
	[SerializeField] private bool _isHide = false;
	private RectTransform _rectTransform = null;

	public int index {
		get { return _index; }
		set { _index = value; }
	}
	public eState state {
		get { return _state; }
		set { 
			if( _isHide == true && value == eState.LoadCompleted ) {
				_state = eState.Ready;
			} else {
				_state = value;
			}
		}
	}
	public bool isHide {
		get { return _isHide; }
		set {
			_isHide = value;
			if( _isHide == true && _state == eState.LoadCompleted ) {
				_state = eState.Ready;
			}
		}
	}
	public RectTransform rectTransform {
		get {
			if( _rectTransform == null ) _rectTransform = this.transform as RectTransform;
			return _rectTransform;
		}
		set { _rectTransform = value; }
	}
}
