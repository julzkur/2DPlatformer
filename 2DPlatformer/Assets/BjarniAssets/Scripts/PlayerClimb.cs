// using UnityEditor.Callbacks;
// using UnityEngine;

// public class PlayerClimb : MonoBehaviour
// {
    
//     private bool isFacingRight = true;
//     private bool jump = false; 
//     private bool jumpHeld = false;
//     private bool crouchHeld = false;
//     private bool isUnderPlatform = false;
//     private bool isCloseToLadder = false;
//     private bool climbHeld = false;
//     private bool hasStartedClimb = false;
 
//     private Transform ladder;
//     private float vertical = 0f;
//     private float climbSpeed = 0.2f;
  
//     void Update()
//     {
//         vertical = Input.GetAxisRaw("Vertical") * climbSpeed;
         
//         if (isOnGround() && horizontal.Equals(0) && !isCloseToLadder && (crouchHeld || isUnderPlatform))
//             GetComponent<Animator>().Play("CharacterCrouchIdle");
//         else if (isOnGround() && !isCloseToLadder && (horizontal > 0 || horizontal < 0) && (crouchHeld || isUnderPlatform))
//             GetComponent<Animator>().Play("CharacterCrouch");
//         else if(isOnGround() && !hasStartedClimb && horizontal.Equals(0))
//             GetComponent<Animator>().Play("CharacterIdle");
//         else if(isOnGround() && !hasStartedClimb && (horizontal > 0 || horizontal < 0))
//             GetComponent<Animator>().Play("CharacterWalk");
          
//         crouchHeld = (isOnGround() && !isCloseToLadder && Input.GetButton("Crouch")) ? true : false;
//         climbHeld = (isCloseToLadder && Input.GetButton("Climb")) ? true : false;
 
//         if (climbHeld)
//         {
//             if (!hasStartedClimb) hasStartedClimb = true;
//         }
//         else
//         {
//             if (hasStartedClimb)
//             {
//                 GetComponent<Animator>().Play("CharacterClimbIdle");
//             }
//         }
//     }
 
//     void FixedUpdate()
//     {
//         // Climbing
//         if(hasStartedClimb && !climbHeld)
//         {
//             if(horizontal > 0 || horizontal < 0) ResetClimbing();
//         }
//         else if(hasStartedClimb && climbHeld)
//         {
//             float height         = GetComponent<SpriteRenderer>().size.y;
//             float topHandlerY    = Half(ladder.transform.GetChild(0).transform.position.y + height);
//             float bottomHandlerY = Half(ladder.transform.GetChild(1).transform.position.y + height);
//             float transformY     = Half(transform.position.y);
//             float transformVY    = transformY + vertical;
    
//             if (transformVY > topHandlerY || transformVY < bottomHandlerY)
//             {
//                 ResetClimbing();
//             }
//             else if (transformY <= topHandlerY && transformY >= bottomHandlerY)
//             {
//                 rigidBody2D.bodyType = RigidbodyType2D.Kinematic;
//                 if (!transform.position.x.Equals(ladder.transform.position.x))
//                     transform.position = new Vector3(ladder.transform.position.x,transform.position.y,transform.position.z);
    
//                 GetComponent<Animator>().Play("CharacterClimb");
//                 Vector3 forwardDirection = new Vector3(0, transformVY, 0); 
//                 Vector3 newPos = Vector3.zero;
//                 if (vertical > 0)
//                     newPos = transform.position + forwardDirection * Time.deltaTime * climbSpeed;
//                 else if(vertical < 0)
//                     newPos = transform.position - forwardDirection * Time.deltaTime * climbSpeed;
//                 if (newPos != Vector3.zero) rigidBody2D.MovePosition(newPos);
//             }
//         }
//     }

//     public static float Half(float value)
//     {
//         return Mathf.Floor(value) + 0.5f;
//     }
 
//     private void OnTriggerStay2D(Collider2D collision)
//     { 
//         if (collision.gameObject.tag.Equals("Ladder"))
//         {
//             isCloseToLadder = true;
//             this.ladder = collision.transform;
//         }
//     }
//     private void ResetClimbing()
//     {
//         if(hasStartedClimb)
//         {
//             hasStartedClimb = false;
//             rigidBody2D.bodyType = RigidbodyType2D.Dynamic;
//             transform.position = new Vector3(transform.position.x, Half(transform.position.y),transform.position.z);
//         }
//     }
// }