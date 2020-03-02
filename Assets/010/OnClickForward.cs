using UnityEngine;
using System.Collections;

class OnClickForward : MonoBehaviour {
public ShooterDriver parent;

void OnMouseDown ()
{
	parent.OnMouseDown();
}
}