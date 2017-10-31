using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class fireScript : MonoBehaviour {

	float elapsedTime = 0;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		elapsedTime += Time.deltaTime;
		if (elapsedTime > 1) {
			for (int i = 0; i < this.transform.childCount; i++) {
				Destroy (this.transform.GetChild (i));
			}

			Destroy (this.gameObject);
		}
	}
}
