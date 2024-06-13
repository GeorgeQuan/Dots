using System.Collections;
using UnityEngine;
using UnityEngine.UI;


public class UIManager : MonoBehaviour
{

    private int Score=0;
    public Text scoreText;
   

    // Update is called once per frame
    void Update()
    {
        if(Score!=SharedData.GameSharedData.Data.DeadCounter)
        {
            Score = SharedData.GameSharedData.Data.DeadCounter;
            scoreText.text = Score.ToString();
        }
    }
}
