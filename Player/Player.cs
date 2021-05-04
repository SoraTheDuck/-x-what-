using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Player : MonoBehaviour
{
	[SerializeField] private bool isMovingForward;
	[SerializeField] private bool isGrounded;
	[SerializeField] private bool isSprinting = false;
	[SerializeField] private bool isSliding;
	[SerializeField] private bool isGrappling;
	[SerializeField] private bool sliding;
	[SerializeField] private bool canLanding;
	private bool PlayerisMoving;


	public controls_2 PControls;
	public GameObject PlayerPos;
	public ParticleSystem LandingDust;
	public ParticleSystem SpeedLine;

	[SerializeField] private float jumpforce = 4f;
	[SerializeField] private float walkspeed = 5f;
	[SerializeField] private float sprintspeed = 13f;
	private float airstrafe;
	[SerializeField] private float extragravity;
	[SerializeField] private float GroundDistance = 0.4f;
	[SerializeField] private float slideheight; //
	[SerializeField] private float normalheight;//
	[SerializeField] private float speed;
	[SerializeField] private float slideforce;//
	[SerializeField] private float Multiplier;
	[SerializeField] private float countermovement;
	[SerializeField] private float MaxSpeed;
	[SerializeField] private float slideModifier;
	[SerializeField] private float slide_time;
	[SerializeField] private float SlideLength;

	public float maxStepHeight = 0.25f;
	public int stepDetail = 1;
	public LayerMask stepMask;

	private Vector2 move;
	[SerializeField] private Vector3 pmovement;
	[SerializeField] private Vector3 old_direction = Vector3.zero;
	[SerializeField] private Vector3 SlideVector;
	private Vector3 PlayerVector;
	private Vector3 GravityVector;

	public LayerMask GroundMask;
	public CapsuleCollider playercollider; //will change
	public Rigidbody PlayerBody;
	public Transform DefaultDestination;


	void Awake()
	{
		PControls = new controls_2();
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
		playercollider = GetComponent<CapsuleCollider>();
		PlayerBody = GetComponent<Rigidbody>();
		normalheight = playercollider.height;
		gameObject.tag = "Player";

		PControls.Player.Grappling.performed += x => isGrappling = true;
		PControls.Player.GrapplingReturn.performed += x => isGrappling = false;

		PControls.Player.Sprint.performed += ctx => Sprint();
		PControls.Player.Walk.performed += ctx => Walking();

		PControls.Player.Jump.performed += ctx => Jump();
		PControls.Player.Slide.performed += ctx => Slide();


		speed = walkspeed;
	}


	void FixedUpdate()
	{
		Stair();
		GroundCheck();
		ExtraGravity();
		Landing();
		LimitSpeed();
		Move();
		Balance();
		CounterMovement();
		Speed_Line();

		//Sliding
		if (!sliding)// can change to test
		{
			old_direction = new Vector3(move.x, 0, move.y); //slide vector off
			old_direction.Normalize();
			old_direction = transform.TransformDirection(old_direction);
		}
		else
		{
			SlideVector = old_direction; //slide vector on
		}

		//Slide cooldown
		if (isSliding)
		{
			slide_time -= Time.deltaTime;
			if (slide_time > 0)
			{
				sliding = true;
			}
			else if (slide_time <= 0)
			{
				sliding = false;
				GoUp();
			}
		}

		//slide apply
		if (sliding)
		{
			playercollider.height = slideheight;
			SlideVector = old_direction;
			PlayerBody.AddForce(PlayerVector * slideforce * Multiplier, ForceMode.Acceleration);
			//speed *= slideModifier;
		}

	}


	void ExtraGravity()
	{
		//Extra Gravity
		if (!isGrounded && !isGrappling)
		{
			GravityVector.y += extragravity * Time.fixedDeltaTime;
			PlayerBody.AddForce(-GravityVector * Multiplier, ForceMode.Acceleration);
		}
		else
		{
			GravityVector.y = 0.2f;
			PlayerBody.AddForce(-GravityVector * Multiplier/5);
		}
	}

	void Move()
	{
		if (!isGrounded)
			airstrafe = 0.95f;
		else
			airstrafe = 1.0f;

		move = PControls.Player.Move.ReadValue<Vector2>(); // read

		if (move != Vector2.zero)
		{
			PlayerisMoving = true;
		}
		else
		{
			PlayerisMoving = false;
		}

		Vector3 pmovement = (move.y * transform.forward) + (move.x * transform.right);

		if (!sliding)
		{
			PlayerVector = pmovement * speed * Time.fixedDeltaTime * airstrafe * Multiplier;
		}
		else if (sliding)
		{
			var rot = Quaternion.FromToRotation(transform.up, Vector3.up);
			PlayerVector = old_direction * speed * Time.fixedDeltaTime * airstrafe * Multiplier;
			PlayerBody.AddForce(Vector3.down * extragravity / 5, ForceMode.Acceleration);
			PlayerBody.AddTorque(new Vector3(rot.x, rot.y, rot.z) * 1000);
		}

		PlayerBody.AddForceAtPosition(PlayerVector, transform.position, ForceMode.VelocityChange);

		if (isSprinting)
		{
			PlayerBody.AddForceAtPosition(PlayerVector, transform.position, ForceMode.Force);
		}

		Balance();
	}

	void GroundCheck()
	{
		isGrounded = Physics.Raycast(PlayerBody.transform.position, Vector3.down, GroundDistance);
	}

	void Jump()
	{
		if (isGrounded)
		{
			PlayerBody.AddForce(Vector2.up * jumpforce, ForceMode.Acceleration);
		}
	}

	void Sprint()
	{
		isSprinting = true;
		speed = sprintspeed;
	}

	void Walking()
	{
		isSprinting = false;
		speed = walkspeed;
	}

	void Slide()
	{
		if (PlayerBody.velocity.magnitude >= 15.7 && isGrounded)
		{
			isSliding = true;
		}
	}

	void Balance()
	{
		var rot = Quaternion.FromToRotation(transform.up, Vector3.up);
		PlayerBody.AddTorque(new Vector3(rot.x, rot.y * 5, rot.z) * 7000);
	}

	void GoUp()
	{
		playercollider.height = normalheight;
		slide_time = SlideLength;
		isSliding = false;
	}

	void Landing()
	{
		if (PlayerBody.velocity.y <= -18)
		{
			canLanding = true;
		}
		else
		{
			canLanding = false;
		}
	}

	void CounterMovement()
	{
		PlayerBody.AddForce(PlayerVector * speed / 10 * countermovement * Time.fixedDeltaTime * Multiplier);
	}

	void LimitSpeed()
	{
		Mathf.Clamp(speed, 0, MaxSpeed);
		if (PlayerBody.velocity.magnitude > MaxSpeed)
		{
			PlayerBody.velocity = PlayerBody.velocity.normalized * MaxSpeed;
		}
	}

	void Speed_Line()
	{
		if (PlayerBody.velocity.magnitude > 15f)
		{
			SpeedLine.Play();
		}
		else if (PlayerBody.velocity.magnitude <= 15f)
		{
			SpeedLine.Stop();
		}
	}

	void Stair()
	{
		bool isFirstStepCheck = false;

		for (int i = stepDetail; i >= 1; i--)
		{
			Collider[] c = Physics.OverlapBox(transform.position + new Vector3(0, i * maxStepHeight / stepDetail - transform.localScale.y, 0), new Vector3(1.05f, maxStepHeight / stepDetail / 2, 1.05f), Quaternion.identity, stepMask);

			if (PlayerisMoving)
			{
				if (c.Length > 0 && i == stepDetail)
				{
					isFirstStepCheck = true;
				}
				
				if (c.Length > 0 && !isFirstStepCheck)
				{
					PlayerBody.transform.position += new Vector3(0, i * maxStepHeight / stepDetail, 0);
				}
			}
		}
	}

	private void OnCollisionEnter(Collision collision)
	{
		if (collision.gameObject.tag == "GroundMap" && canLanding)
		{
			LandingDust.Play();
		}

	}
}
