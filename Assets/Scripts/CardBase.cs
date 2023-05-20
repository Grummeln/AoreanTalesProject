using UnityEngine;
using System.Collections;

[System.Serializable]
public class CardBase : MonoBehaviour {
    public string _name = "";
    public override string ToString() {
        return _name;
    }
}

