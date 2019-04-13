using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonoSingle<T> : MonoBehaviour where T : MonoSingle<T>{

    protected T _instance;

    protected T Instance {
        get {
            return this._instance;
        }
    }

    protected virtual void Awake() {
        if (_instance == null) {
            _instance = this as T;
        }
    }
}
