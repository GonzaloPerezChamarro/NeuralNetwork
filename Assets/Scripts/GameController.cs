using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

//GameController de la escena
public class GameController : MonoBehaviour
{
    //True si se está ejecutando el juego. False si ha acabado.
    public bool running = true;

    //Canvas de final de Juego
    [SerializeField]
    private Canvas endGameCanvas;

    //Panel de fondo del Canvas de Final de juego
    [SerializeField]
    private Image panel;

    //Texto de final de juego
    [SerializeField]
    private Text text;
    

    void Awake() { endGameCanvas.gameObject.SetActive(false); }

    //Establece el final del juego
    public void End(Player player)
    {
        running = false;
        Time.timeScale = 0f;

        if (player.GetTeam() == 0)
            panel.color = Color.blue;
        else
            panel.color = Color.red;

        
        text.text = "HA PERDIDO EL JUGADOR: " + player.gameObject.name;
        endGameCanvas.gameObject.SetActive(true);
    }

    //Vuelve a cargar la escena. (No se utiliza)
    public void Restart() { SceneManager.LoadScene("IA"); }
}
