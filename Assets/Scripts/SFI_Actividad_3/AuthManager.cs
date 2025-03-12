using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;
using System.Net.Mime;
using System.Linq;
using UnityEngine.UI;
using JetBrains.Annotations;

public class AuthManager : MonoBehaviour
{
    private string Url = "https://sid-restapi.onrender.com";
    private string Token;
    string Username;
    
    [SerializeField] private TMPro.TextMeshProUGUI puntaje1;
    [SerializeField] private TMPro.TextMeshProUGUI puntaje2;
    [SerializeField] private TMPro.TextMeshProUGUI puntaje3;
    [SerializeField] private TMP_InputField inputScore;  
    [SerializeField] private TMP_Text puntajeActual;

    [SerializeField] private GameObject gamePanel;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Token = PlayerPrefs.GetString("token");
        Username = PlayerPrefs.GetString("username");
        if (string.IsNullOrEmpty(Token) || string.IsNullOrEmpty(Username))
        {
            gamePanel.SetActive(false);
            Debug.Log("No se ha encontrado el token");
            GameObject.Find("PanelAuth").SetActive(true);
        }
        else
        {
            StartCoroutine(GetProfile());
        }

    }
    
    IEnumerator ValidateToken()
    {
        string path = "/api/usuarios/" + Username;
        UnityWebRequest www = UnityWebRequest.Get(Url + path);
        www.SetRequestHeader("x-token", Token);
        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.ConnectionError || www.responseCode != 200)
        {
            Debug.Log("Token inválido o expirado. Redirigiendo a Login...");
            PlayerPrefs.DeleteKey("token");
            PlayerPrefs.DeleteKey("username");
            GameObject.Find("PanelAuth").SetActive(true);
        }
        else
        {
            GameObject.Find("PanelAuth").SetActive(false);
            StartCoroutine(GetUsers());
        }
    }

    // Update is called once per frame
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
 
 public void UpdateScore()
 {
     if (int.TryParse(inputScore.text, out int newScore))
     {
         StartCoroutine(UpdateUserScore(newScore));
     }
     else
     {
         Debug.LogError("El valor ingresado no es un número válido.");
     }
 }
 
 IEnumerator UpdateUserScore(int newScore)
 {
     string path = "/api/usuarios/" + Username;
     UserData userData = new UserData { score = newScore };
     string jsonData = JsonUtility.ToJson(userData);

     UnityWebRequest www = UnityWebRequest.Put(Url + path, jsonData);
     www.method = "PUT";
     www.SetRequestHeader("Content-Type", "application/json");
     www.SetRequestHeader("x-token", Token);
     yield return www.SendWebRequest();

     if (www.result == UnityWebRequest.Result.ConnectionError || www.responseCode != 200)
     {
         Debug.LogError("Error al actualizar puntaje: " + www.downloadHandler.text);
     }
     else
     {
         Debug.Log("Puntaje actualizado exitosamente.");
         puntajeActual.text = newScore.ToString();
         StartCoroutine(GetUsers());
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
             
             if (leaderboard.Length > 0) puntaje1.text = $"{leaderboard[0].username}: {leaderboard[0].data.score}";
             if (leaderboard.Length > 1) puntaje2.text = $"{leaderboard[1].username}: {leaderboard[1].data.score}";
             if (leaderboard.Length > 2) puntaje3.text = $"{leaderboard[2].username}: {leaderboard[2].data.score}";

             UserModel currentUser = response.usuarios.FirstOrDefault(u => u.username == Username);
             if (currentUser != null) puntajeActual.text = currentUser.data.score.ToString();
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
