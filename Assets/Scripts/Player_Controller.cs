using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using System;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using System.Threading;

public class Player_Controller : MonoBehaviour
{
    InputSystem_Actions playerActions;
    TextMeshProUGUI text;
    InputSystem_Actions.PlayerActions player_Actions;
    Vector2  player_Joystick_Left, player_Joystick_Right, player_Mouse_Position;
    bool player_Back,player_Move_Left, player_Move_Right, player_Move_Up, player_Move_Down, player_Interact;      
    float speed = 0f, joystick_speed = 0f, move_speed = 0f;
    bool move_up, move_down, move_left, move_right;
    bool collided, player_control = true;
    string collidedObj;
    void Awake() {
        playerActions = new InputSystem_Actions();
        text = GameObject.Find("Text").GetComponent<TextMeshProUGUI>();
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
        
        print("colliding: " + collided);

    }
    void SetVariables() {
        player_Mouse_Position = player_Actions.MousePosition.ReadValue<Vector2>();

        player_Move_Left = player_Actions.Left.IsPressed();
        player_Move_Right = player_Actions.Right.IsPressed();
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
            //lerp player to target position 
            // transform.position = Vector3.Lerp(transform.position, targetPOS, Time.deltaTime * 2f);
            // transform.position = targetPOS;
           
            //play player grab animation
            Animator animator = GameObject.Find("Player").GetComponent<Animator>();
            GameObject paper = GameObject.Find("Moveable Paper");

            var paperAnim = paper.GetComponent<Animator>();


            if (player_Interact)
            {
                animator.Play("Grab");
                //set player rotation to target rotation
                //lock player rotation 
                player_control = false;
                // get moveable paper 
                //get & play paper anim
                paperAnim.Play("Wiggle");


                var timer = 0f;
                if (Mouse.current.leftButton.isPressed) timer += Time.deltaTime;

                if (timer > 2f) timer = 0f;
                
                if (player_Mouse_Position.y < 10f && timer < 2f)
                {
                    //play drag animation
                    animator.Play("Drag");
                    //play ripped paper anim
                    paperAnim.Play("Ripped1");

                }
                //if mouse position is at 1 within 1 sec, then paper breaks 
                //exit player animation when not grabbing
            }
            //get the mouse position within 1 sec 
            text.text = ("Mouse Input: " + player_Mouse_Position.y);



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
         move_up = player_Move_Up || player_Joystick_Left.y > 0;
         move_down = player_Move_Down || player_Joystick_Left.y < 0;
        if (!move_left && !move_right)
        {
            if (move_up)
            {
                move_speed = 2f;
            }
            else if (move_down)
            {
                move_speed = -2f;
            }
            else if (!move_up && !move_down)
            {
                move_speed = 0f;
            }
            //lock player rotation on y axis when moving up

            else if (move_up || move_down)
            {
                transform.rotation = Quaternion.Euler(transform.rotation.x, 0f, transform.rotation.z);

            }

        }
            transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.z + move_speed);

        
    }

    void OnColliding()
    {
        var Target_Col = new Vector3(215, 142.3f, 310f);
        if (transform.position.z >= Target_Col.z) {
            transform.position = Target_Col;
        } if (transform.position.z <= Target_Col.z - 50f) {
            collided = false;
        }

        if (collided) { 
            text.text = "Interact with " + collidedObj;
        }else
        { 
            text.text = " ";
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
        // use theta = tan ^-1 (y/x) to get the angle to rotate the camera to
        var angle = Mathf.Atan2(player_Mouse_Position.y, player_Mouse_Position.x) * Mathf.Rad2Deg;
        var joystickAngle = Mathf.Atan2(player_Joystick_Right.y, player_Joystick_Right.x) * Mathf.Rad2Deg;
        
        //set bool for left and right inputs 
        move_left = player_Move_Left || player_Joystick_Left.x < 0;
        move_right = player_Move_Right || player_Joystick_Left.x > 0;

        //set camera to rotate based on y position 
        angle *= -0.5f;

        if (player_Joystick_Right.y < 0f)
        {
            joystick_speed += 2f;
        }
        else if(player_Joystick_Right.y > 0f){
            joystick_speed -= 2f;
        }

        // track if player is moving left or right based on player_Move
        if (!move_up || !move_down)
        {
            if (move_left)
            {
                speed -= 2f;

            }
            else if (move_right)
            {
                speed += 2;

            }
        }
            //set rotation
         transform.rotation = Quaternion.Euler(transform.rotation.x + angle + joystick_speed, transform.rotation.y + speed, 0f);
        
       }

}
