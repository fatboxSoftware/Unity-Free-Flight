﻿using UnityEngine;
using System;
using System.Collections;
using UnityFreeFlight;

namespace UnityFreeFlight {

	//TODO (Apr 14th 2014/v0.5.0): This class is still used, however it's very old. It resembles an 
	//old prototype for testing ground movement with rigidbodies. In the future,
	//it will be refactored into a true "mode", with mechanics to handle movement,
	//animation, and sound. 

	/// <summary>
	/// Apply ground movements when enabled by the mode manager. 
	/// </summary>
	[Serializable]
	public class GroundMode : BaseMode {

		public GroundInputs inputs;
		public FreeFlightAnimationHashIDs hashIDs; 
		private Rigidbody rigidbody;
		private Animator animator;

		public override void init (GameObject go)
		{
			base.init (go);
			inputs = new GroundInputs ();
			hashIDs = new FreeFlightAnimationHashIDs ();
			rigidbody = go.GetComponent<Rigidbody> ();
			animator = go.GetComponentInChildren<Animator> ();
		}


		public bool enabledTakeoff = true;
		public AudioClip takeoffSoundClip;
		private AudioSource takeoffSoundSource;

		public float launchTime = 0.2f;
		private float launchTimer;

		public bool enabledLaunchIfAirborn = true;
		public float minHeightToLaunchIfAirborn = 2f;

		public bool enabledGround = true;
		public AudioClip walkingNoiseClip;
		private AudioSource walkingNoiseSource;
		//meters/second
		public float maxGroundForwardSpeed = 40;
		//degrees/second
		public float groundDrag = 5;
		public float maxGroundTurningDegreesSecond = 40;
		//meters
		public bool enabledJumping = false;
		public float jumpHeight = .5f;
		public AudioClip jumpingNoiseClip;
		private AudioSource jumpingNoiseSource;

		public override void getInputs ()
		{
			inputs.getInputs ();
		}

		public override void applyInputs () {
			
			
			if (enabledJumping && inputs.inputJump) {
				jump ();
			} else if(enabledGround) {
				groundMove();
			}
			
			
			if (enabledTakeoff)
				timedLaunch (inputs.inputTakeoff);
			
			
			if (enabledLaunchIfAirborn)
				launchIfAirborn (minHeightToLaunchIfAirborn);
		}

		protected override void applyPhysics ()
		{
			throw new System.NotImplementedException ("Physics: No special physics available for ground.");
		}

		public override void startMode () {}
		public override void finishMode () {
			animator.SetBool (hashIDs.walkingBool, false);
		}
		
		private void jump() {
			animator.SetTrigger ("Jumping");
			//playSound(jumpingNoiseSource);
			rigidbody.AddForce (0, jumpHeight, 0, ForceMode.Force);
			
		}
		
		private void groundMove() {
			rigidbody.drag = groundDrag;
			if (inputs.inputGroundForward > 0f) {
				animator.SetBool (hashIDs.walkingBool, true);
				rigidbody.AddRelativeForce (Vector3.forward * maxGroundForwardSpeed * inputs.inputGroundForward * Time.deltaTime, ForceMode.VelocityChange);
			} else {
				animator.SetBool (hashIDs.walkingBool, false);
			}
			
			float turningSpeed = maxGroundTurningDegreesSecond * inputs.inputGroundTurning * Time.deltaTime;
			rigidbody.rotation *= Quaternion.AngleAxis (turningSpeed, Vector3.up);
			
			animator.SetFloat (hashIDs.speedFloat, rigidbody.velocity.magnitude);
			animator.SetFloat (hashIDs.angularSpeedFloat, turningSpeed);
		}

		/// <summary>
		/// Launchs if airborn.
		/// </summary>
		/// <param name="minHeight">Minimum height.</param>
		private void launchIfAirborn(float minHeight) {
			if (!Physics.Raycast (rigidbody.position, Vector3.down, minHeight)) {
				takeoff (false);
			}
		}
	
		/// <summary>
		/// Calls takeoff() after "triggerSet" has been true for "launchTime". 
		/// This method needs to be called in Update or FixedUpdate to work properly. 
		/// </summary>
		/// <param name="triggerSet">If set to <c>true</c> for duration of launchTimer, triggers takeoff.</param>
		private void timedLaunch(bool triggerSet) {
			if (triggerSet == true) {
				if (launchTimer > launchTime) {
					takeoff(true);
					launchTimer = 0.0f;
				} else {
					launchTimer += Time.deltaTime;
				}
			} else {
				launchTimer = 0.0f;
			}
		}
	
		/// <summary>
		/// Set the state to flying and enable flight physics. Optionally, flapLaunch
		/// can be set to true to apply a "flap" to help get the object off the ground. 
		/// </summary>
		/// <param name="flapLaunch">If set to <c>true</c> flap launch.</param>
		protected void takeoff(bool flapLaunch = false) {
			gameObject.SendMessage ("SwitchModes", MovementModes.Flight, SendMessageOptions.RequireReceiver);
			//playSound (takeoffSoundSource);
//			if(flapLaunch) 
//				flap ();

		}
	


	}

}
