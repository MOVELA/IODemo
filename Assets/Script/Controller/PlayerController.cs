﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;




public class PlayerController : MonoBehaviour
{
    public float mouseSensitivity = 1.0f;

    private Player player;
    private ObjectBehaviour objectBehaviour;
    private PlayerBehaviour playerBehaviour;

    // Use this for initialization
    void Start()
    {
        player = gameObject.GetComponent<Player>();

        objectBehaviour = gameObject.GetComponent<ObjectBehaviour>();
        playerBehaviour = gameObject.GetComponent<PlayerBehaviour>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void FixedUpdate()
    {
        HandleMouseMove();
        HandleWSAD();
    }

    public Vector3 GetTowards()
    {
        return transform.forward;
    }
    

    private void HandleMouseMove()
    {
        //当鼠标右键未被按下时，才需要对角色的朝向方向作修改
        if (!Input.GetMouseButton(1))
        {
            float mouseX = -Input.GetAxis("Mouse X") * mouseSensitivity;
            float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

            objectBehaviour.Turn(mouseX, mouseY);
        }

    }

    private void HandleWSAD()
    {
        //处理WSAD输入（改变角色的方向）
        float moveHorizontal = Input.GetAxis("Horizontal");
        float moveVertical = Input.GetAxis("Vertical");
        if (moveHorizontal > 0.0f)
            if (moveVertical > 0.0f)
                objectBehaviour.Move(ObjectBehaviour.MoveDirection.FrontRight);
            else
                objectBehaviour.Move(ObjectBehaviour.MoveDirection.Right);
        else if (moveHorizontal < 0.0f)
            if (moveVertical > 0.0f)
                objectBehaviour.Move(ObjectBehaviour.MoveDirection.FrontLeft);
            else
                objectBehaviour.Move(ObjectBehaviour.MoveDirection.Left);
        else if (moveVertical > 0.0f)
        {
            if (playerBehaviour.flyState == PlayerBehaviour.FlyState.Flying)
            {
                Vector3 towards = objectBehaviour.GetForwardDirection();
                playerBehaviour.MoveInSky(towards);
            }
            else if (playerBehaviour.enterSky&&playerBehaviour.flyState == PlayerBehaviour.FlyState.WaitForFly)
            {
                playerBehaviour.MoveInSeaSurface();
            }
            else
            {
                objectBehaviour.Move(ObjectBehaviour.MoveDirection.Front);
            }
        }
        else
            objectBehaviour.Move(ObjectBehaviour.MoveDirection.Stay);
    }

    public bool GetFlyState()
    {
        return playerBehaviour.flyState == PlayerBehaviour.FlyState.Flying;
    }

}
