using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using TMPro;

public class HttpHandler : MonoBehaviour
{
    [SerializeField] private string url = "https://my-json-server.typicode.com/jagari5546/SFI_Actividad_2";
    [SerializeField] private TMP_InputField userIdInput;
    [SerializeField] private Image[] cartasImages;
    [SerializeField] private TMP_Text[] cartasText;
    [SerializeField] private TMP_Text nameText;
    
    private void Start()
    {
        if (gameObject.scene.rootCount != 0) // 
        {
            DontDestroyOnLoad(gameObject);
        }
    }

    public void SendRequest()
    {
        if (int.TryParse(userIdInput.text, out int userId))
        {
            StartCoroutine(GetUser(userId));
        }
        else
        {
            Debug.LogError("Por favor, ingresa un número válido.");
        }
    }

    IEnumerator GetUser(int id)
    {
        UnityWebRequest www = UnityWebRequest.Get(url);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Error al conectar: " + www.error);
        }
        else
        {
            string jsonResult = "{\"users\":" + www.downloadHandler.text + "}";
            UserData usersData = JsonUtility.FromJson<UserData>(jsonResult);

            User personaje = usersData.users.Find(u => u.id == id);

            if (personaje != null)
            {
                Debug.Log("Usuario encontrado: " + personaje.username);
                nameText.text = personaje.username;
                StartCoroutine(MostrarCartas(personaje.deck));
            }
            else
            {
                Debug.LogError("Usuario no encontrado.");
            }
        }
    }

    private IEnumerator MostrarCartas(List<Card> deck)
    {
        for (int i = 0; i < cartasImages.Length; i++)
        {
            if (i < deck.Count)
            {
                cartasText[i].text = deck[i].value; // Asigna el valor de la carta al texto
                yield return StartCoroutine(LoadImage(deck[i].image, cartasImages[i])); // Carga la imagen y la asigna
            }
            else
            {
                cartasImages[i].gameObject.SetActive(false); // Desactiva el Image si no hay carta
                cartasText[i].text = ""; // Limpia el texto si no hay carta
            }
        }
    }

    private IEnumerator LoadImage(string imageUrl, Image imageComponent)
    {
        UnityWebRequest request = UnityWebRequestTexture.GetTexture(imageUrl);
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Error al cargar la imagen: " + request.error);
        }
        else
        {
            Texture2D texture = ((DownloadHandlerTexture)request.downloadHandler).texture;

            Texture2D newTexture = new Texture2D(texture.width, texture.height, TextureFormat.RGBA32, false);
            newTexture.SetPixels(texture.GetPixels());
            newTexture.Apply();

            Sprite newSprite = Sprite.Create(newTexture, new Rect(0, 0, newTexture.width, newTexture.height), Vector2.one * 0.5f);

            if (imageComponent != null)
            {
                imageComponent.sprite = newSprite;
                imageComponent.gameObject.SetActive(true);
            }
            else
            {
                Debug.LogError("El componente de imagen es nulo.");
            }

            request.Dispose();
        }
    }


    [System.Serializable]
    public class Card
    {
        public string value;
        public string image;
    }

    [System.Serializable]
    public class User
    {
        public int id;
        public string username;
        public bool state;
        public List<Card> deck;
    }

    [System.Serializable]
    public class UserData
    {
        public List<User> users;
    }
}
