using System.Collections;
using TMPro;
using UnityEngine;

public class AuthManager : MonoBehaviour
{
    private string Url = "https://sid-restapi.onrender.com";
    private string Token;
    string Username;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    public void Login()
    {
        Credentials credentials = new Credentials();
        credentials.username = GameObject.Find("InputFieldUsername").GetComponent<TMP_InputField>().text;
        
        credentials.password = GameObject.Find("InputFieldPassword").GetComponent<TMP_InputField>().text;

        string postData = JsonUtility.ToJson(credentials);
        
        StartCoroutine(RegisterPostPost(postData));
    }

    IEnumerator LoginPost(string postData)
    {
        throw new System.NotImplementedException();
    }
    
    IEnumerator RegisterPostPost(string postData)
    {
        throw new System.NotImplementedException();
    }

    public void Register()
    {
        
    }
    
}

public class Credentials
{
    public string username;
    public string password;
}
