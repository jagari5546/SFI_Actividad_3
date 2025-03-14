using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;
using System.Net.Mime;
using System.Linq;
using UnityEngine.UI;
using JetBrains.Annotations;
using System.IO;
public class GameManager : MonoBehaviour
{
    private string Url = "https://sid-restapi.onrender.com";
    private string Token;
    
    [SerializeField] private int waitTime = 3;
    [SerializeField] private int gameTime = 8;
    [SerializeField] private int scorePerClick = 100;
    [SerializeField] private Button clickButton;
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text clicksText;
    [SerializeField] private TMP_Text timeText;
    [SerializeField] private AuthManager auth;
    [SerializeField] private TMP_Text usernameText;   
    
    private int clickCount = 0; 
    private float timeLeft;
    private bool isGameOver = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Token = PlayerPrefs.GetString("token");
        string username = PlayerPrefs.GetString("username");
        usernameText.text = username;
        clickCount = 0;
        timeLeft = gameTime;
        clickButton.interactable = false;
        StartCoroutine(EnableClickButtonAfterWait());
    }
    IEnumerator EnableClickButtonAfterWait()
    {
        yield return new WaitForSeconds(waitTime);
        clickButton.interactable = true;
    }

    public void addClick()
    {
        clickCount++;
        clicksText.text = scorePerClick.ToString();
        int actualScore = clickCount * scorePerClick;
        scoreText.text = "Puntaje: "+actualScore.ToString();
        
        UserModel credentials = new UserModel()
        {
            username = PlayerPrefs.GetString("username"),
            data = new UserData { score = actualScore }
        };
        
        string postData = JsonUtility.ToJson(credentials);
        StartCoroutine("PatchUser", postData);
    }
    
    IEnumerator PatchUser(string postData)  
    {
        Token = PlayerPrefs.GetString("token");
        
        string path = "/api/usuarios";
        
        UnityWebRequest www = UnityWebRequest.Put(Url + path, postData);
        www.method = "PATCH";
        www.SetRequestHeader("Content-Type", "application/json");
        www.SetRequestHeader("x-token", Token);
        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.ConnectionError)
        {
            Debug.Log(www.error);
        }
        else
        {
            if (www.responseCode == 200)
            {
                string json = www.downloadHandler.text;
                UserModel response = JsonUtility.FromJson<UserModel>(json);
            }
            else
            {
                string mensaje = "status:"+www.responseCode;
                mensaje += "\nError: " + www.downloadHandler.text;
                Debug.Log(mensaje);
            }
        }
    }
    

    // Update is called once per frame
    void Update()
    {
        if (isGameOver) return;
        
        timeLeft -= Time.deltaTime;
        timeText.text = timeLeft.ToString("F1");
        
        if (timeLeft <= 0)
        {
            GameOver();
        }
    }
    
    private void GameOver()
    {
        isGameOver = true;
        timeLeft = 0;
        timeText.text = "0.0";
        clickButton.interactable = false;
        Debug.Log("Se acabó el tiempo!");
    }
}
