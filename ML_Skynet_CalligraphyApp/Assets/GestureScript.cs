using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// Get access to all our ML stuff.
using UnityEngine.XR.MagicLeap;

public class GestureScript: MonoBehaviour {

	// Tutorial specific variables:
    //private bool OKHandPose = false;
    // private float speed = 30.0f;  // Speed of our cube
    private float distance = .70f; // Distance between Main Camera and Cube



    public GameObject inkSpot; // Reference to our Cube
    private MLHandKeyPose[] gestures; // Holds the different gestures we will look for


    void Awake () {
		// Start up the hands.
        MLHands.Start();

		// Intialize the gestures that we could recognize.
        gestures = new MLHandKeyPose[2];
		// We will recognize the following gestures:
        gestures[0] = MLHandKeyPose.Pinch;
        gestures[1] = MLHandKeyPose.Fist;
		// Turn on the key pose manager to our gesture array.
        MLHands.KeyPoseManager.EnableKeyPoses(gestures, true, false);
		
    }

    void OnDestroy() {
		// If our application turns off, turn off our ML hands object.
        MLHands.Stop();
    }

	// 
    void Update() {

			// If we recognize a particular handpose, instatiate a cube where our thumb is.
        if (GetGesture(MLHands.Right, MLHandKeyPose.Pinch)) {
            Instantiate(inkSpot, MLHands.Right.Index.KeyPoints[0].Position, Quaternion.identity);
        }
			
			// FIST functionality
            // if (GetGesture(MLHands.Left, MLHandKeyPose.Fist)
            // || GetGesture(MLHands.Right, MLHandKeyPose.Fist))
                //cube.transform.Rotate(Vector3.up, -speed * Time.deltaTime);


			// WE ARE NO LONGER TRACKING THESE HANDPOSES:
            // if (GetGesture(MLHands.Left, MLHandKeyPose.Finger))
            //     cube.transform.Rotate(Vector3.right, +speed * Time.deltaTime);

            // if (GetGesture(MLHands.Right, MLHandKeyPose.Finger))
            //     cube.transform.Rotate(Vector3.right, -speed * Time.deltaTime);
        
	}

	// Method for getting gestures.
	bool GetGesture(MLHand hand, MLHandKeyPose type)  {
		// If we have the correct gestures, 
		// The correct type of hands,
		// and the appropriate CI
		// WE HAVE A GESTURE!
        if (hand != null) {
            if (hand.KeyPose == type) {
                if (hand.KeyPoseConfidence > 0.9f) {
                    return true;
                }
            }
        }
        return false;
    }

}

// Code for turning on cube when we flash an OK sign.
//if (GetGesture(MLHands.Left, MLHandKeyPose.Ok)
            // || GetGesture(MLHands.Right, MLHandKeyPose.Ok)) {
            //     OKHandPose = true;
            //     cube.SetActive(true);
            //     cube.transform.position = transform.position + transform.forward * distance;
            //     cube.transform.rotation = transform.rotation;
            // }