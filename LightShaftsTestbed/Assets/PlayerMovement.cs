using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour {

	public CharacterController2D controller;

	float horizontalMove = 0f;

	bool shouldJump = false;


	//public Animator animator;
	
	// Update is called once per frame
	void Update () {
		horizontalMove = Input.GetAxisRaw("Horizontal");
		

		//animator.SetFloat("speed", Mathf.Abs(horizontalMove));

		if(Input.GetButtonDown("Jump")){
			shouldJump = true;
			//animator.SetBool("Jump", true);
		}
	}

	void FixedUpdate()
	{
		controller.Move(horizontalMove, false, shouldJump);
		shouldJump = false;
	}

	public void OnLanding(){

			//animator.SetBool("Jump", false);
		
	}

	public void Jump(){
			shouldJump = true;
			//animator.SetBool("Jump", true);

	}
}
