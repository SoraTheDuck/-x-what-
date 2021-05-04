using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class boomerang : MonoBehaviour
{
	public float damage;


	[SerializeField] private Enemy enemy;
	[SerializeField] private Transform player;
	[SerializeField] private Transform bmrpack;

	[SerializeField] private float TimePercentage = 0f;
	[SerializeField] private bool canTeleport;
	[SerializeField] private bool isThrowing;
	[SerializeField] private bool isReturning;
	[SerializeField] private bool isGrappling;
	[SerializeField] private bool WallHitted;
	[SerializeField] private bool isFloating;
	[SerializeField] private bool BeforeReturn;
	public bool isReadyToThrow;


	//animation bool
	private bool Animation_Throwing;
	private bool Animation_Floating;

	[SerializeField] private float smooth = 1.5f;
	[SerializeField] private float distance;
	[SerializeField] private float time = 0f;
	[SerializeField] private float BeforeReturnTime = 0f;
	[SerializeField] private float teleporttime;
	[SerializeField] private float CooldownTime = 15f;
	[SerializeField] private float FloatingTime;
	[SerializeField] private float TimeArrive;
	[SerializeField] private float TimeReturn;
	[SerializeField] private float TPCooldown;
	[SerializeField] private float rotatespeed;
	[SerializeField] private float Multiplier;
	[SerializeField] private float ThrowDistance;
	private float ThrowFloat;
	private float CDTime;

	[SerializeField] private Transform GrappleTip;
	[SerializeField] private Transform DefaultPos;
	[SerializeField] private Transform destination, cpoint;
	public Transform maincamera;
	//public Transform BarrierPlace; //barrier
	public Transform DefaultDestination;
	public Rigidbody bmr;

	private Vector3 old_default_des = Vector3.zero;
	private Vector3 old_des = Vector3.zero;

	private Vector3 old_pos; //destination
	private Vector3 StartPos; //Default Player bmr pos
	private Vector3 CPointPos;
	[SerializeField] private Quaternion old_rot;

	private Vector3 RotateVector;
	private Vector3 GrapplePoint;
	[SerializeField] private Vector3 PlayerToBmr;


	private LineRenderer lr;
	private SpringJoint joint;
	private Animator bmr_animation;
	public controls_2 bmrcontrols;

	//sway
	public float intensity;
	public float SwaySmooth;

	private Quaternion origin_rotation;
	private Vector2 look;

	void Awake()
	 {
	 	bmrcontrols = new controls_2();
		bmr_animation = GetComponent<Animator>();

		TimePercentage = 0;
		RotateVector = new Vector3(0f,0f,1f);
	}

	private void OnEnable()
	  {
		 bmrcontrols.Enable();
	  }
	  
	  private void Disable()
	  { 
		 bmrcontrols.Disable();
	  }
	  
	  
	  void Start()
	  {
		  //add cooldown textbox
		  lr = GetComponent<LineRenderer>();
		  
		  /*GameObject TheEnemy = GameObject.Find("Enemy");  		  
		  Enemy enemy = TheEnemy.GetComponent<Enemy>();
		  
		  GameObject ThePlayer = GameObject.Find("Player");
		  PlayerStatus player = ThePlayer.GetComponent<PlayerStatus>();
		  
		  GameObject ThePlayerBarrier = GameObject.Find("PlayerBarrier");
		  PlayerBarrier pbarrier = ThePlayerBarrier.GetComponent<PlayerBarrier>();*/ //barrier

		 bmrcontrols.Player.Grappling.performed += x => StartGrapple();
		 bmrcontrols.Player.GrapplingReturn.performed += x => StopGrapple();
		 bmrcontrols.Player.Throw.performed += ctx => BmrThrow();
		 bmrcontrols.Player.Teleport.performed += ctx => Teleport();
		 bmrcontrols.Player.Adjust_Distance.performed += x => ThrowFloat = x.ReadValue<float>();
		/*bmrcontrols.Player.Aim.performed += ctx => Aim();
		
		bmrcontrols.Player.Grappling.canceled += ctx => StopGrapple();*/

		isReadyToThrow = true;
	}

	Vector3 getBQCPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2)
	{
		float u = 1 - t;
		float uu = u * u;
		float tt = t * t;
		Vector3 p = (uu * p0) + (2 * u * t * p1) + (tt * p2);
		return p;
	}

    void FixedUpdate()
	  {
		old_pos = destination.transform.position;
		StartPos = DefaultPos.transform.position;

		float distance = Vector3.Distance(StartPos, old_pos); //adjust cpoint with distance value

		//Change Distance
			AdjustDistance();
			UpdateSway();

		//Wall Check
			WallCheck();

		//throw
		if (isThrowing)
		{
			bmr_animation.SetBool("Animation_Throwing", true);
			while (TimePercentage < 1f)
			{
				TimePercentage += Time.fixedDeltaTime / (TimeArrive * 0.6f);
				bmr.transform.position = Vector3.Lerp(StartPos, old_pos, TimePercentage * smooth);
				bmr.transform.rotation = Quaternion.Lerp(bmr.transform.rotation, destination.transform.rotation, TimePercentage * smooth);
				return;
			}
			isThrowing = false;
			bmr_animation.SetBool("Animation_Throwing", false);
			EndPoint();
			bmr.transform.parent = null;
			BmrStop();
			TimePercentage = 0;
		}

		//before return
		if(BeforeReturn)
        {
			old_default_des = DefaultDestination.transform.position;

			if(BeforeReturnTime < 1.0f)
            {
				BeforeReturnTime += Time.fixedDeltaTime / TimeArrive;
				bmr.transform.position = Vector3.Lerp(bmr.transform.position, old_default_des, BeforeReturnTime);
				return;
			}
			PlayerToBmr = Vector3.MoveTowards(player.transform.position, bmr.transform.position, 100);
			BeforeReturnTime = 0;
            time = 0;
			isReturning = true;
			bmr_animation.SetBool("Animation_Floating", false);
			BeforeReturn = false;

		}


		//return
		if(isReturning)
		  {
			if (time < 1.0f)
			{
				bmr.transform.position = getBQCPoint(time, destination.position, cpoint.transform.position, StartPos);
				bmr.transform.rotation = Quaternion.Slerp(bmr.transform.rotation, DefaultPos.transform.rotation, 50 * Time.deltaTime);
				time += Time.deltaTime * TimeReturn;
			}
			else
				ResetBmr();
		  }
		 
		 //cooldown
		 if(CooldownTime > 0)
		 {
		    CooldownTime = CooldownTime - Time.fixedDeltaTime;
			CDTime = Mathf.Round(CooldownTime);
			return;
		 }
		 else if (CooldownTime > 0)
		 {
			canTeleport = false;
		 }
		 else if (CooldownTime <= 0)
		 {
			canTeleport = true;
		 }
	  }

	void LateUpdate()
	   {
		 if(isGrappling)
		 {
			 DrawRope();
		 }
	   }

	//sway
	private void UpdateSway()
	{
		look = bmrcontrols.Player.Look.ReadValue<Vector2>();

		//controls
		float t_x_mouse = look.x;
		float t_y_mouse = look.y;

		//calculate target rotation
		Quaternion t_x_adj = Quaternion.AngleAxis(-intensity * t_x_mouse, Vector3.up);
		Quaternion t_y_adj = Quaternion.AngleAxis(intensity * t_y_mouse, Vector3.right);
		Quaternion target_rotation = origin_rotation * t_x_adj * t_y_adj;

		//rotate towards target rotation
		bmr.transform.localRotation = Quaternion.Lerp(bmr.transform.localRotation, target_rotation, Time.fixedDeltaTime * SwaySmooth);
	}

	void AdjustDistance()
	{
		ThrowDistance = Vector3.Distance(DefaultDestination.transform.position, player.transform.position);

		Vector3 ChangeCPointDis = new Vector3(1, 0 , 1);
		Vector3 ChangeDistance = new Vector3(0, 0, 1);

		if (ThrowFloat > 0)
		{
			DefaultDestination.transform.localPosition += ChangeDistance;
			cpoint.transform.localPosition += ChangeCPointDis;
		}
		else if (ThrowFloat < 0)
		{
			DefaultDestination.transform.localPosition -= ChangeDistance;
			cpoint.transform.localPosition -= ChangeCPointDis;
		}
			destination.position = DefaultDestination.position;
	}

	void BmrThrow()
      {
		if (isReadyToThrow)
		{
			isReadyToThrow = false;
			isThrowing = true;
		}
		else
		{
			BmrReturn();
		}
	}

	void EndPoint() //destination
    {
		destination.transform.parent = null;
    }
	  
	  void BmrStop()
	  {
	  	 bmr.constraints = RigidbodyConstraints.FreezePosition;
		 isFloating = true;
		 bmr_animation.SetBool("Animation_Floating", true);
		 isThrowing = false;
		 canTeleport = true;
	  }
	  
	  void BmrReturn()
	  {
		if (isFloating && !isReadyToThrow)
		{
			isFloating = false;
			BeforeReturn = true;
			StopGrapple();
			BeforeReturnTime = 0;
			bmr.isKinematic = true;
			old_pos = bmr.position;
		}
	  }
	  
	  void ResetBmr()
	  {
	  	 isReturning = false;
		 isFloating = false;
		 isThrowing = false;

		 bmr_animation.SetBool("Animation_Floating", false);
		 bmr_animation.SetBool("Animation_Throwing",false);

		 destination.transform.position = DefaultDestination.position;
		 destination.transform.rotation = DefaultDestination.rotation; //can remove if u want XD

	 	 bmr.transform.SetParent(bmrpack);
	 	
		 destination.transform.SetParent(maincamera);
		 bmr.transform.position = DefaultPos.position;
		 bmr.transform.rotation = DefaultPos.rotation;
		 isReadyToThrow = true;
	  }

	  void WallCheck()
	  {
		//isGrounded = Physics.Raycast(PlayerBody.transform.position, Vector3.down, GroundDistance);
		distance = Vector3.Distance(player.transform.position, DefaultDestination.position) + 1 ;
		WallHitted = Physics.Raycast(player.transform.position, DefaultDestination.position, distance);
	  }

	  void WallHitBmr()
	  {
	  	 bmr.isKinematic = false;
		 isFloating = true;
	  }
	  
	 /* void Aim()
	  {
		if (isReadyToThrow)
        {
			lr.positionCount = 2;
			CurrentTip = GrappleTip.position;
			DrawLine();
		}	
	  }*/
	  
	  
	  void StartGrapple()
	  {
			if(isFloating)
			{
			isGrappling = true;

			GrapplePoint = bmr.transform.position;

			joint = player.gameObject.AddComponent<SpringJoint>();
			joint.autoConfigureConnectedAnchor = false;
			joint.connectedAnchor = GrapplePoint;

			float distanceFromPoint = Vector3.Distance(player.transform.position, bmr.transform.position);

			joint.maxDistance = distanceFromPoint * 0.8f;
			joint.minDistance = distanceFromPoint * 0.25f;

			joint.spring = 5;
			joint.damper = 7.5f;
			joint.massScale = 90;

			lr.positionCount = 2;
			CurrentTip = GrappleTip.position;
			}
	  }
	  
	  private Vector3 CurrentTip;
	  
	  void StopGrapple()
	  {
	  	 lr.positionCount = 0;
		 Destroy(joint);
		 isGrappling = false;
	  }

	void DrawRope()
	{
	   CurrentTip = Vector3.Lerp(CurrentTip, destination.transform.position, Time.deltaTime * 8f);

	   lr.SetPosition(0, GrappleTip.transform.position);
	   lr.SetPosition(1, player.transform.position);
	}

	void DrawLine()
	{
	   CurrentTip = Vector3.Lerp(CurrentTip, destination.transform.position, Time.deltaTime * 8f);

	   lr.SetPosition(0, GrappleTip.transform.position);
	   lr.SetPosition(1, player.transform.position);
	}


	void Teleport()
	{
		if (isFloating && canTeleport && !isGrappling)
		{
			player.transform.DOMove(bmr.transform.position, teleporttime);
			Invoke("ResetBmr", teleporttime);
		}
		CDTime = TPCooldown;
		CooldownTime = TPCooldown;
		ResetTeleport();
	}

	void ResetTeleport()
	{
		if (!isGrappling)
		{
			canTeleport = false;
		}
	}
	  
	  void OnCollisionEnter(Collision bmrhitbox)
	  {
         if(bmrhitbox.gameObject.tag == "Enemy" && isThrowing)
         {
             doDamage();
         }	      
	  }

    void doDamage() => Enemy.EnemyHP = Enemy.EnemyHP - damage;
}
	  
