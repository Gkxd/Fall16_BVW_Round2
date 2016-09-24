using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour {
    
    public enum GameStates { MainMenu, Intro, InGame, AfterGame };
    public GameStates gameState;

    // Use this for initialization
    void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        if (gameState == GameStates.MainMenu)
        {

        }
        else if (gameState == GameStates.Intro)
        {
            
        }
        else if (gameState == GameStates.InGame)
        {

        }
        else if (gameState == GameStates.AfterGame)
        {

        }
    }
}
