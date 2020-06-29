using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Bit {
	public class BitAsset : MonoBehaviour {
	    public SpriteRenderer renderer;
	    public Animator animator;
	    public float localRotation = 0f;
	    public bool isMirroredX = false;
	    public bool isMirroredY = false;
	    public bool isTweening = false;
	    public void PlayAnimation(string animationName){
	    	animator.Play(animationName);
	    }
	    void Update(){
	    	transform.Rotate (0,0, (isMirroredX ? localRotation : -localRotation)  * 60f * Time.deltaTime);
	    }
	}
}
