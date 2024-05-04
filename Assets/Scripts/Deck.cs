using System;
using UnityEngine;
using UnityEngine.UI;

public class Deck : MonoBehaviour
{

    public Sprite[] faces;
    public GameObject dealer;
    public GameObject player;
    public Button hitButton;
    public Button stickButton;
    public Button playAgainButton;
    public Text finalMessage;
    public Text probMessage;
    public Text prob1;
    public Text prob2;
    public Text prob3;
    public Text pointsPlayer;
    public Text pointsDealer;

    public int[] values = new int[52];
    int cardIndex = 0;

    int[] order = new int[52];

    private void Awake()
    {
        InitCardValues();

    }

    private void Start()
    {
        ShuffleCards();
        StartGame();
    }

    private void InitCardValues()
    {
        for (int i = 0; i < values.Length; i++)
        {
            values[i] = (i % 13) + 1;
            if (values[i] > 10)
            {
                values[i] = 10;
            }
            if (values[i] == 1)
            {
                // Si el valor actual es un As (1), asignamos 11 a menos que 
                // al hacerlo se exceda el límite de 21, en cuyo caso, asignamos 1.
                if (dealer.GetComponent<CardHand>().points + 11 <= 21 || player.GetComponent<CardHand>().points + 11 <= 21)
                {
                    values[i] = 11;
                }
                else
                {
                    values[i] = 1;
                }
            }
        }
    }
    private void ShuffleCards()
    {
        System.Random rng = new System.Random();
        int n = values.Length;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            int tempValue = values[k];
            values[k] = values[n];
            values[n] = tempValue;

            // Asumiendo que también tienes un array de caras que necesitas barajar
            Sprite tempFace = faces[k];
            faces[k] = faces[n];
            faces[n] = tempFace;

            // Guardar el orden de las cartas
            order[n] = k;
        }
    }

    void StartGame()
    {
        for (int i = 0; i < 2; i++)
        {
            PushPlayer();
            PushDealer();
        }

        // Verificar Blackjacks iniciales 
        if (GetPlayerScore() == 21)
        {
            finalMessage.text = "El jugador tiene Blackjack!";
        }
        else if (GetDealerScore() == 21)
        {
            finalMessage.text = "El crupier tiene Blackjack!";
        }
        else // Si no hay Blackjack, se habilita la opción de pedir carta
        {
            hitButton.interactable = true;
            stickButton.interactable = true;
        }
    }

    private void CalculateProbabilities()
    {
        int playerScore = GetPlayerScore();
        int dealerScore = GetDealerScore();
        int remainingCards = GetRemainingCards();

        // Probabilidad de que el dealer tenga más puntuación que el jugador
        int dealerHigherScoreCount = 0;
        for (int i = 0; i < remainingCards; i++)
        {
            int newDealerScore = dealerScore + values[i];
            if (newDealerScore > playerScore)
            {
                dealerHigherScoreCount++;
            }
        }
        float dealerHigherScoreProbability = (float)dealerHigherScoreCount / remainingCards;

        // Probabilidad de que el jugador obtenga entre 17 y 21 o blackjack
        int player17to21Count = 0;
        int playerBlackjackCount = 0;
        for (int i = 0; i < remainingCards; i++)
        {
            int newScore = playerScore + values[i];
            if (newScore <= 21)
            {
                player17to21Count++;
            }
            else if (newScore == 22 && playerScore == 11) // Blackjack
            {
                playerBlackjackCount++;
            }
        }
        float player17to21Probability = (float)(player17to21Count + playerBlackjackCount) / remainingCards;

        // Probabilidad de que el jugador se pase de 21
        int playerBustCount = 0;
        for (int i = 0; i < remainingCards; i++)
        {
            if (playerScore + values[i] > 21)
            {
                playerBustCount++;
            }
        }
        float playerBustProbability = (float)playerBustCount / remainingCards;

        prob1.text = Math.Round(dealerHigherScoreProbability, 4).ToString();
        prob2.text = Math.Round(player17to21Probability, 4).ToString();
        prob3.text = Math.Round(playerBustProbability, 4).ToString();
    }

    void PushDealer()
    {
        // Asegurarse de que no se repartan más cartas de las que hay en la baraja
        if (cardIndex >= faces.Length)
        {
            Debug.Log("No hay más cartas en la baraja.");
            return;
        }

        // Repartir la carta al crupier
        dealer.GetComponent<CardHand>().Push(faces[order[cardIndex]], values[order[cardIndex]]);
        cardIndex++;
    }

    void PushPlayer()
    {
        // Asegurarse de que no se repartan más cartas de las que hay en la baraja
        if (cardIndex >= faces.Length)
        {
            Debug.Log("No hay más cartas en la baraja.");
            return;
        }

        // Repartir la carta al jugador
        player.GetComponent<CardHand>().Push(faces[order[cardIndex]], values[order[cardIndex]]);
        cardIndex++;

        // Actualizar el texto de los puntos del jugador
        pointsPlayer.text = player.GetComponent<CardHand>().points.ToString();

        // Verificar si el jugador se pasa de 21 automáticamente
        if (GetPlayerScore() > 21)
        {
            Stand(); // forzar Stand para cumplir la lógica
        }
    }


    public void Hit()
    {
        

        PushPlayer();

    }
    public void Stand()
    {
      

        // Lógica del dealer (pedir hasta llegar a 17 o más)
        while (GetDealerScore() < 17)
        {
            PushDealer();
        }

        int winner = DetermineWinner();
        ShowResult(winner);
    }

    private int DetermineWinner()
    {
        int playerScore = GetPlayerScore();
        int dealerScore = GetDealerScore();

        // Lógica considerando pasarse de 21
        if (playerScore > 21)
        {
            return 2; // Dealer gana
        }
        else if (dealerScore > 21)
        {
            return 1; // Jugador gana
        }
        // Resto de la lógica de comparación...
        else if (playerScore == dealerScore)
        {
            return 0; // Empate
        }
        else if (playerScore > dealerScore)
        {
            return 1; // Jugador gana
        }
        else
        {
            return 2; // Dealer gana
        }
    }
    private void ShowResult(int winner)
    {
        switch (winner)
        {
            case 0: // Empate
                finalMessage.text= "Empate";
                break;
            case 1: // Jugador gana
                finalMessage.text = "¡Has ganado!";
                break;
            case 2: // Dealer gana
                finalMessage.text = "El dealer ha ganado";
                break;
        }
    }



    public void PlayAgain()
    {
        hitButton.interactable = true;
        stickButton.interactable = true;
        finalMessage.text = "";
        prob1.text="";
        prob2.text="";
        prob3.text="";
        player.GetComponent<CardHand>().Clear();
        dealer.GetComponent<CardHand>().Clear();
        cardIndex = 0;
        ShuffleCards();
        StartGame();
        //isInitialHand = true; // Restaurar la bandera de mano inicial
    }



    //--------------------------------
    public int GetPlayerScore()
    {
        pointsPlayer.text = player.GetComponent<CardHand>().points.ToString();
        return player.GetComponent<CardHand>().points;
    }

    public int GetDealerScore()
    {
        pointsDealer.text = dealer.GetComponent<CardHand>().points.ToString();
        return dealer.GetComponent<CardHand>().points;
    }

    public int GetRemainingCards()
    {
        return values.Length - cardIndex;
    }




    // Assuming you have a boolean variable isInitialHand that is set to true when the game starts and set to false when the first action (Hit or Stand) is taken
    /*public bool IsInitialHand()
    {
        return isInitialHand;
    }*/
}
