using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class bmrSway : MonoBehaviour
{
	//sway
	public float intensity;
	public float SwaySmoothRotate;
	public float SwaySmoothPos;

	public Transform maincamera;

	private float origin_rotation_y;
	private float origin_rotation_z;

	private Vector3 initialPos;
	private Quaternion origin_rotation;
	private Vector2 look;

	private controls_2 PControls;

	void Awake()
	{
		PControls = new controls_2();
	}

	private void Start()
	{
		origin_rotation_y = transform.localRotation.y;
		origin_rotation_z = transform.localRotation.z;

        origin_rotation = Quaternion.Euler(maincamera.transform.rotation.x, origin_rotation_y, origin_rotation_z);

		initialPos = transform.localPosition;
	}

	void OnEnable()
	{
		PControls.Enable();
	}

	void OnDisable()
	{
		PControls.Disable();
	}
	// Update is called once per frame
	void FixedUpdate()
    {
		UpdateSwayRotate();
		UpdateSwayPosition();
    }

	private void UpdateSwayRotate()
	{
		look = PControls.Player.Look.ReadValue<Vector2>();

		//controls
		float t_x_mouse = look.x;
		float t_y_mouse = look.y;

		//calculate target rotation
		Quaternion t_x_adj = Quaternion.AngleAxis(-intensity * t_x_mouse, Vector3.up);
		Quaternion t_y_adj = Quaternion.AngleAxis(intensity * t_y_mouse, Vector3.right);
		Quaternion target_rotation = origin_rotation * t_x_adj * t_y_adj;

		//rotate towards target rotation
		transform.localRotation = Quaternion.Lerp(transform.localRotation, target_rotation, Time.fixedDeltaTime * SwaySmoothRotate);
	}

    private void UpdateSwayPosition()
    {
		look = PControls.Player.Look.ReadValue<Vector2>();

		//controls
		float movementX = look.x;
		float movementY = look.y;

		Vector3 finalPos = new Vector3(movementX, movementY, 0);
		transform.localPosition = Vector3.Lerp(transform.localPosition, finalPos + initialPos, Time.fixedDeltaTime * SwaySmoothPos);
	}
}
