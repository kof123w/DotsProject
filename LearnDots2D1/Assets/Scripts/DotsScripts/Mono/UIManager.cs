using System;
using UnityEngine;
using UnityEngine.UI;
public class UIManager : MonoBehaviour
{
      private int score;
      public Text scoreText;


      private void Update()
      {
            if (score != ShareData.gameSharedData.Data.DeadCounter)
            {
                  score = ShareData.gameSharedData.Data.DeadCounter;
                  scoreText.text = $"{score}";
            }
      }
}