using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.ImageEffects;

public class MyDynamicCamera : MonoBehaviour {

	public Transform character;

	public Transform upperLimit, lowerLimit, leftLimit, rightLimit;

	private Vector3 velocity = Vector3.zero;

	public Transform targetTrans;

	public Camera particleCamera;
	public Camera parallaxCamera;
	public Camera parallaxBackgroundCamera;
	public Camera exitIconCamera;
	public Blur blurScript;

	private bool shouldWalk = true;

	private float direction = 0;

	private bool initializedX = false;
	private bool initializedY = false;

	public bool deactivateCameraZoom = false;


	// Use this for initialization
	void Start () {
		Vector3 cameraPosition = GameManager.instance.cameraPosition;
		if (!(cameraPosition.x == 0 && cameraPosition.y == 0 && cameraPosition.z == 0)) {
			Vector3 tempPosition = gameObject.transform.position;
			tempPosition.y = cameraPosition.y;
			tempPosition.x = cameraPosition.x;
			gameObject.transform.position = tempPosition;
		}		


		#if UNITY_STANDALONE 
			
		#elif UNITY_IOS || UNITY_ANDROID || UNITY_WP8 || UNITY_IPHONE
		    float screenWidthInch = Screen.width / Screen.dpi;
     		float screenHeightInch = Screen.height / Screen.dpi;
			float screenDiagonal = Mathf.Sqrt(screenWidthInch*screenWidthInch + screenHeightInch*screenHeightInch);
			Debug.Log("ScreenDiagonal "+screenDiagonal);
			if(screenDiagonal < 6.0f && ! deactivateCameraZoom)
			{
				Camera.main.orthographicSize = 4.2f;
				particleCamera.orthographicSize = 4.2f;
				exitIconCamera.orthographicSize = 4.2f;
				parallaxCamera.fieldOfView = 30.95f;

			}else
			{
				Camera.main.orthographicSize = 5.4f;
				particleCamera.orthographicSize = 5.4f;
				exitIconCamera.orthographicSize = 5.4f;
				parallaxCamera.fieldOfView = 39;
			}
		#endif
	}
		

	float velocityX;
	float velocityY;

	// Update is called once per frame
	void LateUpdate () {
		Vector3 destination = transform.position;
		Bounds cameraBounds =  GetBounds(Camera.main);

		if (targetTrans == null) {
			//for dynamic camera (character standing at 40% of screendireciton)
			Vector2 dir = (Vector2)(Quaternion.Euler(0,0,direction) * Vector2.down);

			if (upperLimit != null && lowerLimit != null) {
				if (!(cameraBounds.max.y + 0.1f > upperLimit.position.y && cameraBounds.min.y - 0.1f < lowerLimit.position.y)) {
					if (character.position.y + cameraBounds.extents.y + dir.y < upperLimit.position.y && character.position.y - cameraBounds.extents.y + dir.y > lowerLimit.position.y) {
						destination.y = character.position.y;
						destination.y += dir.y;
					} else {
						if (character.position.y + cameraBounds.extents.y + dir.y > upperLimit.position.y) {
							destination.y = upperLimit.position.y - cameraBounds.extents.y;
						} else {
							destination.y = lowerLimit.position.y + cameraBounds.extents.y;
						}
					}
					if(!initializedY){
						initializedY = true;
						Vector3  vec = transform.position;
						vec.y = destination.y;
						transform.position = vec;
					}
				} else {
					//Debug.Log ("holy damn");

				}
			}

			if (rightLimit != null && leftLimit != null && character != null) {

				if (!(cameraBounds.max.x+0.1f > rightLimit.position.x && cameraBounds.min.x -0.1f  < leftLimit.position.x)) {

					if (character.position.x + cameraBounds.extents.x + dir.x < rightLimit.position.x && character.position.x - cameraBounds.extents.x +dir.x > leftLimit.position.x) {
						destination.x = character.position.x;
						destination.x += dir.x;
					} else {
						if (character.position.x + cameraBounds.extents.x +dir.x > rightLimit.position.x) {
							//transform.position = temp;
							destination.x = rightLimit.position.x - cameraBounds.extents.x;
						} else {
							destination.x = leftLimit.position.x + cameraBounds.extents.x;
						}
					} 

					if(!initializedX){
						initializedX = true;
						Vector3  vec = transform.position;
						vec.x = destination.x;
						transform.position = vec;
					}
				}
			}

			

			//float x = Mathf.SmoothDamp(transform.position.x,destination.x,ref velocityX,0.5f);
			//float y = Mathf.SmoothDamp(transform.position.y,destination.y,ref velocityY,0.5f);

			Vector3 vec3 = Vector3.SmoothDamp (transform.position, destination, ref velocity, 0.5f);
			//if(Vector3.Distance(vec3,transform.position) > 0.01)
			transform.position = vec3;
			//Vector3 newVec3 = new Vector3(MyRoundToNearestPixel(vec3.x,Camera.main),MyRoundToNearestPixel(vec3.y,Camera.main),vec3.z);
			
			
		} else {
			if (targetTrans.position.y + cameraBounds.extents.y < upperLimit.position.y && targetTrans.position.y - cameraBounds.extents.y  > lowerLimit.position.y) {
				destination.y = targetTrans.position.y;
			} else {
				if (targetTrans.position.y + cameraBounds.extents.y > upperLimit.position.y) {
					destination.y = upperLimit.position.y - cameraBounds.extents.y;
				} else {
					destination.y = lowerLimit.position.y + cameraBounds.extents.y;

				}
			}

			if (targetTrans.position.x + cameraBounds.extents.x < rightLimit.position.x && targetTrans.position.x - cameraBounds.extents.x > leftLimit.position.x) {
				destination.x = targetTrans.position.x;
			} else {
				if (targetTrans.position.x + cameraBounds.extents.x> rightLimit.position.x) {
					//transform.position = temp;

					destination.x = rightLimit.position.x - cameraBounds.extents.x;
				} else {
					destination.x = leftLimit.position.x + cameraBounds.extents.x;
				}
			}
			Vector3 vec3 = Vector3.SmoothDamp (transform.position, destination, ref velocity, 1f);
			//Vector3 newVec3 = new Vector3(MyRoundToNearestPixel(vec3.x,Camera.main),MyRoundToNearestPixel(vec3.y,Camera.main),vec3.z);
			transform.position = vec3;
			
		}
	}

	public void SetDirection(float direction)
	{
		this.direction = direction;
	}

	public Bounds GetBounds(Camera camera)
	{
		float screenAspect = (float)Screen.width / (float)Screen.height;
		float cameraHeight = camera.orthographicSize * 2;
		Bounds bounds = new Bounds(
			camera.transform.position,
			new Vector3(cameraHeight * screenAspect, cameraHeight, 0));
		return bounds;
	}


	public bool ShouldWalk()
	{
		return shouldWalk;
	}

	public void MoveCameraToTarget(Transform target,bool shouldWalk)
	{
		StartCoroutine (MoveTo(target,shouldWalk));
	}


	IEnumerator MoveTo(Transform target,bool shouldWalk)
	{
		targetTrans = target;
		this.shouldWalk = shouldWalk;
		yield return new WaitForSeconds (5.0f);
		targetTrans = null;
		this.shouldWalk = true;
	}

	/* 
	public float MyRoundToNearestPixel(float unityUnits, Camera viewingCamera)
	{
		float valueInPixels = (Screen.height / (viewingCamera.orthographicSize * 2)) * unityUnits;
		Debug.Log(unityUnits);

        valueInPixels = Mathf.Round(valueInPixels);

        float adjustedUnityUnits = valueInPixels / (Screen.height / (viewingCamera.orthographicSize * 2));
        return adjustedUnityUnits;
    }
	*/
}
