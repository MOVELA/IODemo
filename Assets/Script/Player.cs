﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Player : Photon.PunBehaviour {

  

    public float initialSize = 1.0f;
    public float initialSpeed = 20.0f;
    public float health;


    private float playerEnergy;
    private Vector3 playerSize;

    private float sizeEffect;//用于本地视口玩家角色大小显示效果
    private float speedOffset;//用于暂时性的增加速度
    private Vector3 sizeOffset;//用于暂时性的增加体型

    private float speed;
    private string playerName;//玩家自定义的名字
    private int count;//碰撞后偏移的执行次数
    public int Lock=0;

    //Debug
    public GameObject other;


    // Use this for initialization
    void Start () {
        playerEnergy = initialSize * initialSize;
        playerSize = new Vector3(initialSize, initialSize, initialSize);
        transform.localScale = playerSize;
        speed = initialSpeed;
        sizeEffect = 1.0f;
        speedOffset = 0.0f;
        sizeOffset = Vector3.zero;
        StartCoroutine(Recover());

    }
	
	// Update is called once per frame
	void FixedUpdate () {

 
        if (photonView.isMine && other != null)
        {
            Debug.Log("实时更新当前血量： " + health);
            Debug.Log("实时更新对方血量： " + other.gameObject.gameObject.GetComponent<Player>().health);
        }
        if (health <= 0)
        {
            this.photonView.RPC("DestroyThis", PhotonTargets.AllViaServer);
        }
        Debug.Log("当前Lock值: "+Lock);

        //networkManager.GetPlayerList();
        //networkManager.localPlayer.GetComponent<Player>().photonView.RPC("SetPlayerName", PhotonTargets.All, LobbyUIManager.playerName);//设置玩家名字

    }

     void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("food"))
        {
			Debug.Log ("玩家：碰到了食物");
            if(transform.localScale.x<25){
            playerEnergy+=0.2f; 
            float sq=Mathf.Sqrt(playerEnergy);     
            speed = 10 / sq+2;
            playerSize = new Vector3(playerEnergy, playerEnergy, playerEnergy);
            transform.localScale = playerSize + sizeOffset;
            }

            //播放音效
            if (this.photonView.isMine)
            {
                GameObject Audio = GameObject.Find("Audio");
                Audio.GetComponent<AudioManager>().PlayEatFood();
            }
            
        }
        else if(other.gameObject.tag=="poison"){
            Debug.Log("玩家：碰到了毒物");
            playerEnergy-=0.5f; 
            float sq=Mathf.Sqrt(playerEnergy);     
            speed = 10 / sq+2;
            playerSize = new Vector3(playerEnergy, playerEnergy, playerEnergy);
            transform.localScale = playerSize + sizeOffset;
 
        }

    }


    void OnControllerColliderHit(ControllerColliderHit other)
    {
        if (other.gameObject != this.gameObject && other.gameObject.tag == "player" && this.photonView.isMine)
        {
            Debug.Log("碰撞到了玩家");
            Debug.Log("对方Lock值："+other.gameObject.GetComponent<Player>().Lock);

            //生命值大于0才能伤害敌人
            Player enemy = other.gameObject.GetComponent<Player>();
            if(enemy.health > 0)
            {
                this.photonView.RPC("GetDamage", PhotonTargets.AllViaServer, other.gameObject.transform.localScale.x *0.5f);
            }
            
            if(health > 0)
            {
                enemy.photonView.RPC("GetDamage",
               PhotonTargets.AllViaServer, this.gameObject.transform.localScale.x *0.5f);
            }

            //Debug
            this.other = other.gameObject;
            GameObject Audio = GameObject.Find("Audio");
            if(other.gameObject.GetComponent<Player>().Lock==0&&this.gameObject.transform.localScale.x>=other.gameObject.transform.localScale.x){
                //other.gameObject.GetComponent<Player>().StartCoroutine(other.gameObject.GetComponent<Player>().Bomb(-other.normal));
                other.gameObject.GetComponent<Player>().Lock=1;
                other.gameObject.GetComponent<Player>().photonView.RPC("DoBomb",PhotonTargets.All,-other.normal);
                Debug.Log("对方弹开");
                Audio.GetComponent<AudioManager>().PlayTouchSmallEnemy();
            }
            else if (this.gameObject.transform.localScale.x<other.gameObject.transform.localScale.x){
                Debug.Log("自己弹开");
                this.StartCoroutine(Bomb(other.normal));
                Audio.GetComponent<AudioManager>().PlayTouchBigEnemy();
            }


        }
        else if(/*other.gameObject.tag == "Wall" &&*/ this.photonView.isMine)
        {     
            //播放音效
            GameObject Audio = GameObject.Find("Audio");
            Audio.GetComponent<AudioManager>().PlayTouchWall();
        }

    }

        IEnumerator Bomb(Vector3 direction){
        while(count<5){
            yield return null;
            this.gameObject.GetComponent<CharacterController>().Move(direction*Time.deltaTime*40);
            Debug.Log("弹开方向： "+direction);
            Debug.Log("弹开执行次数： "+count);
            count++;
        }
        count = 0;
        this.photonView.RPC("ReleaseLock",PhotonTargets.All);
        Debug.Log("Lock= " +Lock);
    }

    IEnumerator Recover(){
        while(true){
            if(this.health<=99)
            this.health+=1;
            yield return new WaitForSeconds(1);
        }
    }
    public void AddSpeedOffset(float speedOffset)
    {
        this.speedOffset += speedOffset;    
        Debug.Log("速度改变: "+ speedOffset);
    }

    public void AddSizeEffect(float sizeEffect)
    {
        this.sizeEffect *= sizeEffect;
    }

    public void AddSizeOffset(Vector3 sizeOffset)
    {
        this.sizeOffset += sizeOffset;
        transform.localScale = playerSize + this.sizeOffset;
    }

    public float GetSpeed()
    {
        return speed + speedOffset;
    }

    public float GetPlayerSize()
    {
        return transform.localScale.x;
    }


    public Vector3 GetRenderPlayerSize()
    {
        if (sizeEffect != 0)
        {
            return (transform.localScale) / sizeEffect;
        }
        else
        {
            return Vector3.positiveInfinity;
        }

    }


    public string GetPlayerName()
    {
        return this.playerName;
    }


    [PunRPC]
    void DestroyThis(){
        PhotonNetwork.Destroy(this.gameObject);
    }

    [PunRPC]
    void GetDamage(float damage){
        health -= damage;  
    }
    [PunRPC]
    void DoBomb(Vector3 direction){
        if(this.gameObject==networkManager.localPlayer){
        this.StartCoroutine(Bomb(direction));
        Debug.Log("弹开RPC调用");
        }
    }

    [PunRPC]
    void ReleaseLock(){
            this.Lock =0;
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info){
        if(stream.isWriting){
            stream.SendNext(this.health);
        }
        else{
            this.health = (float)stream.ReceiveNext();
        }
    }

    [PunRPC]
    public void SetPlayerName(string name)
    {
        this.playerName = name;
        networkManager.GetPlayerList();

    }
    //更新玩家列表
    void OnDestroy()
    {
        networkManager.GetPlayerList();

    }

    void Awake()
    {

        Debug.LogWarning("调用OnAwake");   
        StartCoroutine(WaitForInitName());



    }



    IEnumerator WaitForInitName()
    {
        yield return new WaitForSeconds(0.5f);
        networkManager.localPlayer.GetComponent<Player>().photonView.RPC("SetPlayerName", PhotonTargets.All, LobbyUIManager.playerName);//设置玩家名字

    }


}
