using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;
using System.Net.Mime;
using System.Linq;

public class AuthManager : MonoBehaviour
{
    private string Url = "https://sid-restapi.onrender.com";
    private string Token;
    string Username;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Token = PlayerPrefs.GetString("token");
        Username = PlayerPrefs.GetString("username");
        GameObject.Find("PanelAuth").SetActive(true);
        if (string.IsNullOrEmpty(Token) || string.IsNullOrEmpty(Username))
        {
            Debug.Log("No se ha encontrado el token");
        }
        else
        {
            StartCoroutine(GetProfile());
        }

    }

    // Update is called once per frame
    public void Login()
    {
        Credentials credentials = new Credentials();
        credentials.username = GameObject.Find("InputFieldUsername").GetComponent<TMP_InputField>().text;
        
        credentials.password = GameObject.Find("InputFieldPassword").GetComponent<TMP_InputField>().text;

        string postData = JsonUtility.ToJson(credentials);
        
        StartCoroutine(RegisterPost(postData));
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
             
             GameObject.Find("PanelAuth").SetActive(false);
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
             GameObject.Find("PanelAuth").SetActive(false);
             StartCoroutine(GetUsers());
         }
         else
         {
             Debug.Log("Token Vencido... redirecionar a Login");
         }
     }
 }

 IEnumerator GetUsers()
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
             UsersList response = JsonUtility.FromJson<UsersList>(json);

             UserModel[] leaderboard = response.usuarios.
                 OrderByDescending(u => u.data.score).Take(3).ToArray();
             
             foreach (var user in leaderboard)
             {
                 Debug.Log(user.username+"|"+user.data.score);
             }
         }
         else
         {
             Debug.Log("Token Vencido... redirecionar a Login");
         }
     }
 }
    
}

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
    public UserData data;
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
