using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Bit {
	public class BitEntity : MonoBehaviour {
		public string animationName = "idle";
		public List<BitAsset> assets;
		public BitEntityDataOptions data;
		public int currentAnimation = 0;
		public BitAssetData assetData;
		public int baseScale = 1;

		float lastRotation = 0;
		float lastRotateSpeed = 0;
		float lastAlpha = 0;
		float lastMirrorDistanceX = 0;
		float lastMirrorDistanceY = 0;
		float lastScale = 0;

		void Start() {
			lastRotateSpeed = data.rotationSpeed;
			lastRotation = data.rotation;
			lastAlpha = data.alpha;
			lastScale = (baseScale-1) + data.scale;

			SetRotateSpeed(data.rotationSpeed);
			foreach ( BitAsset a in assets ) {
				a.PlayAnimation(animationName);
			}
			if( !data.animationsEnabled || data.animations.Count <= 0 ) {
				return;
			}
			lastMirrorDistanceX = ( data.mirror.x / 2) * 0.01f;
			lastMirrorDistanceY = ( data.mirror.y / 2) * 0.01f;
			AnimateStep(currentAnimation);
		}

		public void SetRotateSpeed(float rotateSpeed) {
			lastRotateSpeed = rotateSpeed;
			foreach ( BitAsset a in assets ) {
				a.localRotation = rotateSpeed ;
			}
		}

		public void SetTweening(bool toggle) {
			foreach ( BitAsset a in assets ) {
				a.isTweening = toggle ;
			}
		}

		public void SetScale(float scale) {
			lastScale = Mathf.Round(scale);
			foreach ( BitAsset a in assets ) {
				a.transform.localScale = new Vector3(scale +(baseScale-1), scale +(baseScale-1), scale  +(baseScale-1));
			}
		}

		public void SetRotation(float rotation) {
			lastRotation = rotation;
			foreach ( BitAsset a in assets ) {
				a.transform.eulerAngles = new Vector3( 0, 0, a.isMirroredX ? -rotation : rotation );
			}
		}

		public void SetAlpha(float alpha) {
			lastAlpha = alpha;
			Color alphaColor = assets[0].assetRenderer.color;
			alphaColor.a = (float)alpha / 255f;
			foreach ( BitAsset a in assets ) {
				a.assetRenderer.color = alphaColor;
			}
		}

		public void SetMirrorDistance(Vector2 mirrorPosition){
			if(!data.mirrorX && !data.mirrorY){
				return;
			}
			lastMirrorDistanceX = mirrorPosition.x;
			lastMirrorDistanceY = mirrorPosition.y;
			if(data.mirrorX && data.mirrorY){
				Vector2 pos1 = new Vector2( -mirrorPosition.x, mirrorPosition.y ); // <-- top left corner
                Vector2 pos2 = new Vector2( mirrorPosition.x, mirrorPosition.y ); // <-- top right corner
                Vector2 pos3 = new Vector2( -mirrorPosition.x, -mirrorPosition.y ); // <-- bottom left corner
                Vector2 pos4 = new Vector2( mirrorPosition.x, -mirrorPosition.y ); // <-- bottom right corner
				assets[0].transform.localPosition = pos1;
				assets[1].transform.localPosition = pos2;
				assets[2].transform.localPosition = pos3;
				assets[3].transform.localPosition = pos4;
			} else if(data.mirrorX){
				Vector2 pos1 = new Vector2( -mirrorPosition.x, mirrorPosition.y ); // <-- left side
               	Vector2 pos2 = new Vector2(  mirrorPosition.x, mirrorPosition.y ); // <-- right side
               	assets[0].transform.localPosition = pos1;
				assets[1].transform.localPosition = pos2;
			} else if(data.mirrorY){
				Vector2 pos1 = new Vector2( mirrorPosition.x, mirrorPosition.y ); // <-- top side
                Vector2 pos2 = new Vector2( mirrorPosition.x, -mirrorPosition.y ); // <-- bottom side
                assets[0].transform.localPosition = pos1;
				assets[1].transform.localPosition = pos2;
			}
		}

		public void setFlipX(int flipX){
			foreach ( BitAsset a in assets ) {
				a.assetRenderer.flipX = flipX == 0 ? true : false;
			}

			if(data.mirrorX && data.mirrorY){
				assets[0].assetRenderer.flipX = flipX == 0 ? true : false;
				assets[1].assetRenderer.flipX = flipX != 0 ? true : false;
				assets[2].assetRenderer.flipX = flipX == 0 ? true : false;
				assets[3].assetRenderer.flipX = flipX != 0 ? true : false;
			} else if(data.mirrorX){
               	assets[0].assetRenderer.flipX = flipX == 0 ? true : false;
				assets[1].assetRenderer.flipX = flipX != 0 ? true : false;
			} else if(data.mirrorY){
                assets[0].assetRenderer.flipX = flipX == 0 ? true : false;
				assets[1].assetRenderer.flipX = flipX == 0 ? true : false;
			} else {
				assets[0].assetRenderer.flipX = flipX == 0 ? true : false;
			}
		}

		public void setFlipY(int flipY){
			foreach ( BitAsset a in assets ) {
				a.assetRenderer.flipY = flipY == 0 ? true : false;
			}
			if(data.mirrorX && data.mirrorY){
				assets[0].assetRenderer.flipY = flipY == 0 ? true : false;
				assets[1].assetRenderer.flipY = flipY == 0 ? true : false;
				assets[2].assetRenderer.flipY = flipY != 0 ? true : false;
				assets[3].assetRenderer.flipY = flipY != 0 ? true : false;
			} else if(data.mirrorX){
               	assets[0].assetRenderer.flipY = flipY == 0 ? true : false;
				assets[1].assetRenderer.flipY = flipY == 0 ? true : false;
			} else if(data.mirrorY){
                assets[0].assetRenderer.flipY = flipY == 0 ? true : false;
				assets[1].assetRenderer.flipY = flipY != 0 ? true : false;
			} else {
				assets[0].assetRenderer.flipY = flipY == 0 ? true : false;
			}
		}

		public void AnimateStep(int index){
			BitEntityAnimationData animation = data.animations[index];
			StartCoroutine(Animate(animation));
		}

		IEnumerator Animate(BitEntityAnimationData animation){
			SetTweening(true);
			yield return new WaitForSeconds(animation.delay*0.016f);
			float currentTime = 0;

			float duration = animation.duration;

			Vector2 newPosition = new Vector2(transform.localPosition.x, transform.localPosition.y);
			float fromXValue = transform.localPosition.x;
			float toXValue = transform.localPosition.x;
			if(animation.data.hasTransformX){
				toXValue = ( animation.data.transformX * 0.01f) - fromXValue;
			}
			float fromYValue = transform.localPosition.y;
			float toYValue = transform.localPosition.y;
			if(animation.data.hasTransformY){
				toYValue =  ( -animation.data.transformY * 0.01f) - fromYValue;
			}

			float fromRotationValue = lastRotation;
			float toRotationValue = lastRotation;
			if( animation.data.hasRotation ){
				toRotationValue = animation.data.rotation - lastRotation;
			}

			float fromScaleValue = lastScale;
			float toScaleValue = lastScale;
			if( animation.data.hasScale ){
				toScaleValue = animation.data.scale - lastScale;
			}

			float fromAlphaValue = lastAlpha;
			float toAlphaValue = lastAlpha;
			if( animation.data.hasAlpha ){
				toAlphaValue = animation.data.alpha - lastAlpha;
			}

			float fromRotateSpeedValue = lastRotateSpeed;
			float toRotateSpeedValue = lastRotateSpeed;
			if( animation.data.hasRotateSpeed ){
				toRotateSpeedValue = animation.data.rotateSpeed - lastRotateSpeed;
			}

			Vector2 newMirrorPosition = new Vector2(lastMirrorDistanceX, lastMirrorDistanceY);
			float fromMirrorXValue = lastMirrorDistanceX;
			float toMirrorXValue = lastMirrorDistanceX;
			if(animation.data.hasMirrorDistanceX){
				toMirrorXValue = ( (animation.data.mirrorDistanceX/2 ) * 0.01f) - fromMirrorXValue;
			}

			float fromMirrorYValue = lastMirrorDistanceY;
			float toMirrorYValue = lastMirrorDistanceY;
			if(animation.data.hasMirrorDistanceY){
				toMirrorYValue =  ( animation.data.mirrorDistanceY/2 * 0.01f) - fromMirrorYValue;
			}

			float fixedDuration = Mathf.Round( ( duration * 0.016f ) * 100f ) / 100f;

			if(fixedDuration == 0){
				if(animation.data.hasTransformX){
					float currentXValue = Tween.animate( 1, fromXValue, toXValue, 1, animation.easing);
					newPosition.x = Mathf.Round( currentXValue * 100f ) / 100f;
				}

				if(animation.data.hasTransformY){
					float currentYValue = Tween.animate( 1, fromYValue, toYValue, 1, animation.easing);
					newPosition.y = Mathf.Round( currentYValue * 100f ) / 100f;
				}
				transform.localPosition = newPosition;


				if(animation.data.hasRotation){
					float currentRotationValue = Tween.animate(1, fromRotationValue, toRotationValue, 1, animation.easing);
					SetRotation(currentRotationValue);
				}

				if(animation.data.hasScale){
					float currentScaleValue = Tween.animate(1, fromScaleValue, toScaleValue, 1, animation.easing);
					SetScale(currentScaleValue);
				}

				if(animation.data.hasAlpha){
					float currentAlphaValue = Tween.animate(1, fromAlphaValue, toAlphaValue, 1, animation.easing);
					SetAlpha(currentAlphaValue);
				}

				if(animation.data.hasRotateSpeed){
					float currentRotateSpeedValue = Tween.animate(1, fromRotateSpeedValue, toRotateSpeedValue, 1, animation.easing);
					SetRotateSpeed(currentRotateSpeedValue);
				}

				if(animation.data.hasMirrorDistanceX){
					float currentMirrorXValue = Tween.animate( 1, fromMirrorXValue, toMirrorXValue, 1, animation.easing);
					newMirrorPosition.x = Mathf.Round( currentMirrorXValue * 100f ) / 100f;
				}
				if(animation.data.hasMirrorDistanceY){
					float currentMirrorYValue = Tween.animate( 1, fromMirrorYValue, toMirrorYValue, 1, animation.easing);
					newMirrorPosition.y = Mathf.Round( currentMirrorYValue * 100f ) / 100f;
				}
				if(animation.data.hasMirrorDistanceX || animation.data.hasMirrorDistanceY){
					SetMirrorDistance(newMirrorPosition);
				}

				yield return new WaitForEndOfFrame();
			}

			if(animation.data.hasSpriteFlipX){
				setFlipX(animation.data.spriteFlipX);
			}
			if(animation.data.hasSpriteFlipY){
				setFlipY(animation.data.spriteFlipY);
			}

			while(currentTime < fixedDuration && fixedDuration> 0f) {
				currentTime += Time.deltaTime;
				currentTime = Mathf.Min(currentTime, fixedDuration);
				if(animation.data.hasTransformX){
					float currentXValue = Tween.animate(currentTime, fromXValue, toXValue, fixedDuration, animation.easing);
					newPosition.x = Mathf.Round(currentXValue * 100f) / 100f;
				}

				if(animation.data.hasTransformY){
					float currentYValue = Tween.animate(currentTime, fromYValue, toYValue, fixedDuration, animation.easing);
					newPosition.y = Mathf.Round(currentYValue * 100f) / 100f;
				}

				transform.localPosition = newPosition;

				if(animation.data.hasRotation){
					float currentRotationValue = Tween.animate(currentTime, fromRotationValue, toRotationValue, fixedDuration, animation.easing);
					SetRotation(currentRotationValue);
				}

				if(animation.data.hasScale){
					float currentScaleValue = Tween.animate(currentTime, fromScaleValue, toScaleValue, fixedDuration, animation.easing);
					SetScale( Mathf.Floor(currentScaleValue));
				}

				if(animation.data.hasAlpha){
					float currentAlphaValue = Tween.animate(currentTime, fromAlphaValue, toAlphaValue, fixedDuration, animation.easing);
					SetAlpha(currentAlphaValue);
				}

				if(animation.data.hasRotateSpeed){
					float currentRotateSpeedValue = Tween.animate(currentTime, fromRotateSpeedValue, toRotateSpeedValue, fixedDuration, animation.easing);
					SetRotateSpeed(currentRotateSpeedValue);
				}

				if(animation.data.hasMirrorDistanceX){
					float currentMirrorXValue = Tween.animate( currentTime, fromMirrorXValue, toMirrorXValue, fixedDuration, animation.easing);
					newMirrorPosition.x = Mathf.Round( currentMirrorXValue * 100f ) / 100f;
				}
				if(animation.data.hasMirrorDistanceY){
					float currentMirrorYValue = Tween.animate( currentTime, fromMirrorYValue, toMirrorYValue, fixedDuration, animation.easing);
					newMirrorPosition.y = Mathf.Round( currentMirrorYValue * 100f ) / 100f;
				}
				if(animation.data.hasMirrorDistanceX || animation.data.hasMirrorDistanceY){
					SetMirrorDistance(newMirrorPosition);
				}

				yield return new WaitForEndOfFrame();
			}

			SetTweening(false);

			yield return new WaitForSeconds(0.16f);

			currentAnimation ++;
			if(currentAnimation >=  data.animations.Count){
				currentAnimation = 0;
			}
			AnimateStep(currentAnimation);
		}
	}
}
