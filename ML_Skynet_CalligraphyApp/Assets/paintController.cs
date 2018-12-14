using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.MagicLeap;


public class paintController : MonoBehaviour {

	private GameObject canvas;
	//public GameObject canvas;
	private MLInputController _controller;
	private const float _rotationSpeed = 30.0f;
	private const float _distance = .8f;
	private const float _moveSpeed = .5f;
	private bool _enabled = false;
	private bool _bumper = false;
	public GameObject inkSpot;
	public GameObject mainPoint;

  void Awake() {
    canvas = GameObject.Find("origin");
    //canvas.SetActive (false);
    MLInput.Start();
    MLInput.OnControllerButtonDown += OnButtonDown;
    MLInput.OnControllerButtonUp += OnButtonUp;
    _controller = MLInput.GetController(MLInput.Hand.Left);
  }

  void OnDestroy () {
    MLInput.OnControllerButtonDown -= OnButtonDown;
    MLInput.OnControllerButtonUp -= OnButtonUp;
    MLInput.Stop();
  }

  void Update() {
    if (_bumper && _enabled) {
      canvas.transform.Rotate(Vector3.up, + _rotationSpeed * Time.deltaTime);
    }
    CheckControl();
  }

  void CheckControl() {
    if (_controller.TriggerValue > 0.2f && _enabled) {
		 //paintbrushPrefab.transform.position = MLHands.Right.Center;
		//Vector3 newControllerPosition = new Vector3(_controller.Position.x, (_controller.Position.y -.05f), _controller.Position.z);

        Instantiate(inkSpot, mainPoint.transform.position, Quaternion.identity);
		_bumper = false;
		//canvas.transform.Rotate(Vector3.up, - _rotationSpeed * Time.deltaTime);
    }
    else if (_controller.Touch1PosAndForce.z > 0.0f && _enabled){
      float X = _controller.Touch1PosAndForce.x;
      float Y = _controller.Touch1PosAndForce.y;
      Vector3 forward = Vector3.Normalize(Vector3.ProjectOnPlane(transform.forward, Vector3.up));
      Vector3 right = Vector3.Normalize(Vector3.ProjectOnPlane(transform.right, Vector3.up));
      Vector3 force = Vector3.Normalize((X * right) + (Y * forward));
      canvas.transform.position += force * Time.deltaTime * _moveSpeed;
    }
  }

  void OnButtonDown(byte controller_id, MLInputControllerButton button) {
    if ((button == MLInputControllerButton.Bumper && _enabled)) {
      _bumper = true;
    }
  }

  void OnButtonUp(byte controller_id, MLInputControllerButton button) {
    if (button == MLInputControllerButton.HomeTap) {
      canvas.SetActive (true);
      canvas.transform.position = transform.position + transform.forward * _distance;
	  Quaternion newCanvasRotation = new Quaternion(0, 0, 0, 0);
      canvas.transform.rotation = newCanvasRotation;
      _enabled = true;
    }
  }
}
