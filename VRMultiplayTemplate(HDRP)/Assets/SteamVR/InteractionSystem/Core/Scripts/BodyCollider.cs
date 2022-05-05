//======= Copyright (c) Valve Corporation, All rights reserved. ===============
//
// Purpose: Collider dangling from the player's head
//
//=============================================================================

using UnityEngine;
using System.Collections;

namespace Valve.VR.InteractionSystem
{
	//-------------------------------------------------------------------------
	//[RequireComponent( typeof(CharacterController) )]
	public class BodyCollider : MonoBehaviour
	{
		public Transform head;

		public CharacterController characterController;

		//-------------------------------------------------
		void Awake()
		{
			//characterController = GetComponent<CharacterController>();
		}


		//-------------------------------------------------
		void FixedUpdate()
		{
			float distanceFromFloor = Vector3.Dot( head.localPosition, Vector3.up );
			characterController.height = Mathf.Max(characterController.radius, distanceFromFloor );
			transform.localPosition = head.localPosition - 0.5f * distanceFromFloor * Vector3.up;
		}
	}
}
