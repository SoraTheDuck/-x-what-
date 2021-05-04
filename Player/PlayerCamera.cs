using System;
using UnityEngine;
using UnityEngine.InputSystem;

  public class PlayerCamera : MonoBehaviour
  {
	 private controls_2 PControls;

	 public float mousesens = 100f;
	 private float xRotation = 0;
	 private Vector2 look;

	 private float desiredX;

	[SerializeField] private boomerang Bmr;
	[SerializeField] private Transform DefaultPos;
	[SerializeField] private Transform maincamera;

	//Player playerBD;
	[SerializeField] private Transform PlayerBody;

	void Awake()
    {
		 PControls = new controls_2();
		 PlayerBody = transform.parent;
    }

    void OnEnable()
    {
		PControls.Enable();
    }

    void OnDisable()
    {
		PControls.Disable();
    }


    void Start()
	{
	 	Cursor.lockState = CursorLockMode.Locked;
	}
	 
	 void FixedUpdate()
	 {
		look = PControls.Player.Look.ReadValue<Vector2>();

		float mouseX = look.x * mousesens * Time.fixedDeltaTime;
		float mouseY = look.y * mousesens * Time.fixedDeltaTime;

		xRotation -= mouseY;
		xRotation = Mathf.Clamp(xRotation, -90f, 85f);
		
		transform.localRotation = Quaternion.Euler(xRotation, 0, 0);
		PlayerBody.transform.Rotate(Vector3.up * mouseX); // rotate camera
	}

    private void LateUpdate()
    {
		BmrPosition();
	}

	void BmrPosition()
	{
		if (Bmr.isReadyToThrow)
		{
			Bmr.bmr.transform.position = DefaultPos.transform.position;
		}
	}
}
