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

public class AuthManager : MonoBehaviour
{
    private string Url = "https://sid-restapi.onrender.com";
    private string Token;
    string Username;
    
    [SerializeField] private GameObject gamePanel;
    [SerializeField] private GameObject loginPanel;
    [SerializeField] private GameObject scorePanel;
    [SerializeField] TMP_Text[] score = new TMP_Text[3];
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Awake()
    {
        gamePanel.SetActive(false);
        loginPanel.SetActive(true);        
        scorePanel.SetActive(false);    
        
    }
    void Start()
    {
        Token = PlayerPrefs.GetString("token");
        Username = PlayerPrefs.GetString("username");
        if (string.IsNullOrEmpty(Token) || string.IsNullOrEmpty(Username))
        {
            Debug.Log("No se ha encontrado el token");
            gamePanel.SetActive(false);
            loginPanel.SetActive(true);        
            scorePanel.SetActive(false);        
        }
        else
        {
            StartCoroutine(GetProfile());
        }

    }
    
    public void Login()
    {
        Credentials credentials = new Credentials
        {
            username = GameObject.Find("InputFieldUsername").GetComponent<TMP_InputField>().text,
            password = GameObject.Find("InputFieldPassword").GetComponent<TMP_InputField>().text
        };

        string postData = JsonUtility.ToJson(credentials);
        StartCoroutine(LoginPost(postData));
    }
    public void Register()
    {
        Credentials credentials = new Credentials
        {
            username = GameObject.Find("InputFieldUsername").GetComponent<TMP_InputField>().text,
            password = GameObject.Find("InputFieldPassword").GetComponent<TMP_InputField>().text
        };

        string postData = JsonUtility.ToJson(credentials);
        StartCoroutine(RegisterPost(postData));
    }
    public void Logout()
    {
        Username = "";
        Token = "";
        loginPanel.SetActive(true);
        gamePanel.SetActive(false);
        scorePanel.SetActive(false);        

        PlayerPrefs.DeleteKey("username");
        PlayerPrefs.DeleteKey("token");
        PlayerPrefs.Save(); 
    }
    IEnumerator RegisterPost(string postData)
 {
     string path = "/api/usuarios";
     UnityWebRequest www = UnityWebRequest.Put(Url+path, postData);
     www.method = "POST";
     www.SetRequestHeader("Content-Type", "application/json");
     yield return www.SendWebRequest();

     if (www.result == UnityWebRequest.Result.ConnectionError)
     {
         Debug.Log(www.error);
     }
     else
     {
         if (www.responseCode == 200)
         {
             Debug.Log(www.downloadHandler.text);
             StartCoroutine(LoginPost(postData));
         }
            
         else 
         {
             string mensaje = "status:" + www.responseCode;
             mensaje += "\nError: " + www.downloadHandler.text;
             Debug.Log(mensaje);
         }
     }
 }

 IEnumerator LoginPost(string postData)
 {
     string path = "/api/auth/login";
     UnityWebRequest www = UnityWebRequest.Put(Url+path, postData);
     www.method = "POST";
     www.SetRequestHeader("Content-Type", "application/json");
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

             AuthResponse response = JsonUtility.FromJson<AuthResponse>(json);



             PlayerPrefs.SetString("token", response.token);
             PlayerPrefs.SetString("username", response.usuario.username);
             
             loginPanel.SetActive(false);
             gamePanel.SetActive(true);
             scorePanel.SetActive(false); 

             StartCoroutine(GetUsers());
         }
         else
         {
             string mensaje = "status:" + www.responseCode;
             mensaje += "\nError: " + www.downloadHandler.text;
             Debug.Log(mensaje);
         }
     }
 }
 
 IEnumerator GetProfile()
 {
     string path = "/api/usuarios/"+Username;
     UnityWebRequest www = UnityWebRequest.Get(Url + path);
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
             AuthResponse response = JsonUtility.FromJson<AuthResponse>(json);
             
             loginPanel.SetActive(false);
             gamePanel.SetActive(true);
             scorePanel.SetActive(false);

         }
         else
         {
             Debug.Log("Token Vencido... redirecionar a Login");
             
             loginPanel.SetActive(true);
             gamePanel.SetActive(false);
             scorePanel.SetActive(false);

         }
     }
 }

 public void GetScoreboard()
 {
     loginPanel.SetActive(false);
     gamePanel.SetActive(false);
     scorePanel.SetActive(true);
     StartCoroutine(GetUsers());
 }

 IEnumerator GetUsers()
 {
     Token = PlayerPrefs.GetString("token");
     Username = PlayerPrefs.GetString("username");
     
     string path = "/api/usuarios";
     UnityWebRequest www = UnityWebRequest.Get(Url + path);
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
             UsersList response = JsonUtility.FromJson<UsersList>(json);

             UserModel[] leaderboard = response.usuarios
                 .OrderByDescending(u => u.data.score)
                 .Take(3).ToArray();
             int index = 0;
             foreach (var user in leaderboard)
             {
               
                 Debug.Log(user.username +" | "+user.data.score);
                 score[index].text = index+1 + ". " + user.username + "     " + user.data.score;
                 index++;    
             }
         }
         else
         {
             Debug.Log("Token Vencido... redirecionar a Login");
         }
     }
 }


    
}

[System.Serializable]
public class Credentials
{
    public string username;
    public string password;
}
[System.Serializable]
public class AuthResponse
{
    public UserModel usuario;
    public string token;
}

[System.Serializable]
public class UserModel
{
    public string _id;
    public string username;
    public bool estado;
    public UserData data = new UserData();
}
[System.Serializable]
public class UsersList
{
    public UserModel[] usuarios;
}

[System.Serializable]
public class UserData
{
    public int score;
}
