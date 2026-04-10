using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine.UI;
using System.IO.MemoryMappedFiles;

public class Player_Controller : MonoBehaviour
{
    InputSystem_Actions playerActions;
    TextMeshProUGUI text;
    InputSystem_Actions.PlayerActions player_Actions;
    Rigidbody rb;
    [SerializeField] Slider slider;
    Vector2  player_Joystick_Left, player_Joystick_Right, player_Mouse_Position;
    bool player_Back,player_Move_Left, player_Move_Right, player_Move_Up, player_Move_Down, player_Interact;

    bool r_joystick_right, r_joystick_left, r_joystick_move_up, r_joystick_move_down;
    float joystick_move_speed = 0f, joystick_rot_h_speed = 0f;
  
    float speed = 0f, vSpeed = 0f, joystick_rot_v_speed = 0f, move_speed = 0f;

    bool joystick_move_up, joystick_move_down, move_left, move_right;

    bool collided, player_control = true;
    string collidedObj;
    [Header("Target Printer Interaction")]
    public GameObject target;



    void Awake() {
        playerActions = new InputSystem_Actions();
        text = GameObject.Find("Text").GetComponent<TextMeshProUGUI>();
        rb = GetComponent<Rigidbody>();

    }
    void OnEnable(){
        playerActions.Player.Enable();
    }

    void OnDisable(){
        playerActions.Player.Disable();
    }

    void Update()
    {
        player_Actions = playerActions.Player;

        SetVariables();
        if (player_control)
        {
            //add player rotation based on mouse position and joystick position
            PlayerLook();
            //add player movement based on keyboard input and joystick input
            PlayerMove();
        }
        //add player collision with objects in the scene
        OnColliding();
        //add player grab animation
        PlayerInteract();
        
    }
    void SetVariables() {
        player_Mouse_Position = player_Actions.MousePosition.ReadValue<Vector2>();

        player_Move_Up = player_Actions.Up.IsPressed();
        player_Move_Down = player_Actions.Down.IsPressed();

        player_Joystick_Left = player_Actions.Left_Joystick.ReadValue<Vector2>();
        player_Joystick_Right = player_Actions.Right_Joystick.ReadValue<Vector2>();

        player_Interact = player_Actions.Interact.WasPressedThisFrame();
        player_Back = player_Actions.Back.WasPressedThisFrame();
    }
    void PlayerInteract()
    {
        if (collided){
           
           
            //play player grab animation
            Animator animator = GameObject.Find("Player").GetComponent<Animator>();
            GameObject paper = GameObject.Find("Moveable Paper");

            var paperAnim = paper.GetComponent<Animator>();


            if (player_Interact)
            {
                animator.Play("Grab");
                //lerp player to target position 
                transform.position = Vector3.Lerp(transform.position, target.transform.position, Time.deltaTime * 2f);
                transform.position = target.transform.position;
                //lock rotation 
                transform.rotation = target.transform.rotation;

                //lock player rotation 
                player_control = false;
                //get & play paper anim
                paperAnim.Play("Wiggle");


                var timer = 0f;
                if (Mouse.current.leftButton.isPressed) timer += Time.deltaTime;

                if (timer > 2f) timer = 0f;
                //set slider as measurment of movement 
                //if player is using controller 
                var mappedValue = player_Joystick_Left.y * 10f;
                //if player is using mouse input 

                slider.value = mappedValue;
                if (mappedValue == 10f && timer < 2f)
                {
                    //play drag animation
                    animator.Play("Drag");
                    //play ripped paper anim
                    paperAnim.Play("Ripped1");

                }
                //if mouse position is at 1 within 1 sec, then paper breaks 
                //exit player animation when not grabbing
            }
           
            if (player_Back)
            {
                animator.Play("Idle");
                //stop wiggling paper 
                paperAnim.Play("Idle");
                player_control = true;
            }

        }
    }
    void PlayerMove() {
        var current_angle = transform.rotation.eulerAngles.y;
        print(current_angle);
        joystick_move_up = player_Joystick_Left.y > 0;
         joystick_move_down = player_Joystick_Left.y < 0;
        //if player isn't rotating 
        if ((!move_left && !move_right) || (!r_joystick_right && !r_joystick_left))
        {

            //joystick movement controls
            if (joystick_move_up)
            {
                joystick_move_speed =2f;

            }
            else if (joystick_move_down)
            {
                joystick_move_speed =-2f;
            }
            else {
                joystick_move_speed = 0f;
            }


            //mouse movement controls 

            //move the player forward 
            if (player_Move_Up)
            {
                move_speed = 2f;
            }
            else if (player_Move_Down)
            {
                move_speed = -2f;
            }
            else if (!player_Move_Down && !player_Move_Up)
            {
                move_speed = 0f;
            }

        }
 
        //if (current_angle > 160f) { 
        //    joystick_move_speed *= -1;
        //}
        //set position with speed applied
        //grab rb
            transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.z + move_speed + joystick_move_speed);

        
    }

    void OnColliding()
    {
        if (transform.position.z >= 350f)
        {
            transform.position = new Vector3(transform.position.x,
                                   transform.position.y, 350f);
        }
        else if (transform.position.z <= 18f) {
            transform.position = new Vector3(transform.position.x,
                                  transform.position.y, 18f);
        }
        if (collidedObj != null)
        {
            //set text
            text.text = "Interact with " + collidedObj;
        }
        else
        {
            text.text = " ";
        }

    }
    public void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("wall"))
        {
            collided = true;
        }
        else {
            collided = false;
        }
    }
    public void OnTriggerEnter(Collider col)
    {
        if (col.gameObject.CompareTag("Interactable"))
        {
            collided = true;
            collidedObj = col.gameObject.name;
        }
        else
        {
            collided = false;
        }
      }
    void PlayerLook() {

        //set bool for left and right inputs 
        move_left = player_Mouse_Position.x <-0.5f;
        move_right = player_Mouse_Position.x > 0.5f;
  
        r_joystick_left = player_Joystick_Right.x > 0.5f;
        r_joystick_right = player_Joystick_Right.x < -0.5f;

        // Right joystick rotation
        //if (r_joystick_move_up)
        //{
        //    joystick_rot_v_speed += 0.5f;
        //}
        //else if (r_joystick_move_down)
        //{
        //    joystick_rot_v_speed -= 0.5f;
        //}


        if (r_joystick_left)
        {
            joystick_rot_h_speed += 0.5f;
        }
        else if (r_joystick_right)
        {
            joystick_rot_h_speed -= 0.5f;
        }
        
        //mouse rotation
         if (move_left)
        {
            speed -= 0.5f;

        }
        else if (move_right)
        {
            speed += 0.5f;

        }
        //if (!move_left && !move_right)
        //{
        //    if (player_Mouse_Position.y > 0.5f)
        //    {
        //        vSpeed += 2f;
        //    }
        //    else if (player_Mouse_Position.x < -0.5f)
        //    {
        //        vSpeed -= 2f;
        //    }
        //}
            //set rotation
            transform.rotation = Quaternion.Euler(transform.rotation.x + vSpeed + joystick_rot_v_speed, transform.rotation.y + speed +joystick_rot_h_speed, 0f);
        
       }

}
