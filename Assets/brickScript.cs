using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class brickScript : MonoBehaviour {
	
	public Sprite onFireSprite;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void SetOnFire() {
		SpriteRenderer sr = GetComponent<SpriteRenderer>();
		sr.sprite = onFireSprite;
		Destroy (this.gameObject, 1);
	}
}
