﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoginUI : MonoBehaviour
{
    public int maxInputBytes = 10;

    public Text nameTip;
    public InputField nameInput;
    public Button loginButton;
    public Text connectTip;
    public GameObject loadingCircle;

    private void Start()
    {
        // 输入框获取输入焦点
        nameInput.ActivateInputField();

        // 播放大厅背景音乐
        GameObject audio = GameObject.Find("Audio");
        audio.GetComponent<AudioManager>().PlayLobbyBackground();
        DontDestroyOnLoad(audio);

    }

    private void Update()
    {
        // 按enter键开始游戏
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            OnLoginGame();
        }
    }

    // 限制输入的账号长度
    /// <字符编码>
    ///     utf-8：中文3个字节，英文1个字节
    ///     unicode：中英文都是2个字节
    ///     gbk：中文2个字节，英文1个字节
    /// </字符编码>
    public void OnInputName()
    {
        // 使用这段代码转换GBK,会有bug：exe运行时nameInput的文本不会实时更新，但调试时不会出现
        //byte[] bytestr = System.Text.Encoding.GetEncoding("GBK").GetBytes("nameInput.text");
        //if (bytestr.Length > maxInputBytes)
        //{
        //nameInput.text = nameInput.text.Substring(0, nameInput.text.Length - 1);
        //}

        // 自己统计中文占用的bytes
        string inputStr = nameInput.text;
        string outputStr = "";
        int count = 0;
        for(int i = 0; i < inputStr.Length; i++)
        {
            string tempChar = inputStr.Substring(i, 1);
            byte[] encodedBytes = System.Text.ASCIIEncoding.Default.GetBytes(tempChar);
            if(encodedBytes.Length == 1)
            {
                count += 1;
            }
            else
            {
                count += 2;
            }
            if(count <= maxInputBytes)
            {
                outputStr += tempChar;
            }
            else
            {
                break;
            }
        }
        nameInput.text = outputStr;

    }


    // 登陆游戏
    public void OnLoginGame()
    {
        // 判断输入是否为空
        if (nameInput.text.Length == 0)
        {
            Debug.LogWarning("请输入账号！");
            nameInput.ActivateInputField();
            return;
        }
        // 改变UI：隐藏输入框和进入游戏按钮，显示连接提示文本和加载进度条
        SetEnterUI(false);

        // 开始连接
        // 房间唯一ID，相同ID的用户不会加入同一个房间
        PhotonNetwork.AuthValues.UserId = nameInput.text;
        NetworkMatch.playerName = nameInput.text;
        if (!PhotonNetwork.connected)
        {
            connectTip.text = "正在连接服务器";
            PhotonNetwork.ConnectUsingSettings("0.0.1");
        }

    }

    // 退出游戏
    public void OnQuitGame()
    {
        Application.Quit();
    }


    // 输入框UI、加载显示UI切换
    public void SetEnterUI(bool flag)
    {
        nameTip.gameObject.SetActive(flag);
        nameInput.gameObject.SetActive(flag);
        loginButton.gameObject.SetActive(flag);

        connectTip.gameObject.SetActive(!flag);
        loadingCircle.SetActive(!flag);
    }

    // 连接服务器信息显示
    public void SetConnectTip(string tip)
    {
        connectTip.text = tip;
    }
}
