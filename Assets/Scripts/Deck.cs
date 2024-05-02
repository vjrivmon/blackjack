using System;
using UnityEngine;
using UnityEngine.UI;

public class Deck : MonoBehaviour
{

    /* values: para asignar un valor a cada una de las 52 cartas. cardIndex: para mantener el contador de las cartas repartidas. Adem´ as, se definen los siguientes m´etodos que representan la din´amica del juego: InitCardValues. Este m´etodo se encarga de inicializar los valores de las 52 cartas. En principio, la posici´on de los valores se deber´an corresponder con la posici´on de faces. En este sentido, si la carta de la posici´on 1 de faces es el 2 de corazones, el valor de la posici´on 1 deber´ ıa ser un 2. ShuffleCards. Este m´etodo se encarga de barajar las cartas de manera aleatoria. StartGame. Este m´etodo se encarga de repartir las dos primeras manos. CalculateProbabilities. Este m´ etodo se encarga de calcular varias probabilidades en funci´on de las cartas que ya hay sobre la mesa. PushDealer. Este m´etodo se encarga de repartir una carta al Dealer. PushPlayer. Este m´etodo se encarga de repartir una carta al Dealer. Hit. Este m´etodo se asocia al bot´on Hit e implementa la l´ogica de pedir carta. Stand. Este m´etodo se asocia al bot´on Stand e implementa la l´ogica de plantarse. PlayAgain. Este m´ etodo se asocia al bot´on PlayAgain y se encarga de inicializar los valores para volver a jugar.*/
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
    private bool isInitialHand = true;

    public int[] values = new int[52];
    int cardIndex = 0;

    private void Awake()
    {
        InitCardValues();

    }

    private void Start()
    {
        ShuffleCards();
        StartGame();
    }
    // Assuming you have a CardHand script with a method GetScore() that calculates the score of a hand
    public int GetPlayerScore()
    {
        return player.GetComponent<CardHand>().GetScore();
    }

    public int GetDealerScore()
    {
        return dealer.GetComponent<CardHand>().GetScore();
    }

    public int GetRemainingCards()
    {
        return values.Length - cardIndex;
    }

    // Assuming you have a boolean variable isInitialHand that is set to true when the game starts and set to false when the first action (Hit or Stand) is taken
    public bool IsInitialHand()
    {
        return isInitialHand;
    }
    private void InitCardValues()
    {
        values = new int[52];
        for (int i = 0; i < 52; i++)
        {
            if (i % 13 < 9) // Los números 2 a 10
            {
                values[i] = (i % 13) + 2;
            }
            else if (i % 13 < 12) // Las figuras J, Q, K
            {
                values[i] = 10;
            }
            else // El As
            {
                values[i] = 11; // En el juego de Blackjack, el As puede valer 1 u 11. Aquí lo inicializamos a 11.
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
        }
    }

    void StartGame()
    {
        for (int i = 0; i < 2; i++)
        {
            PushPlayer();
            PushDealer();

            // Asumiendo que tienes métodos GetPlayerScore() y GetDealerScore() que devuelven la puntuación actual
            if (GetPlayerScore() == 21 && i == 1) // Verificar si el jugador tiene Blackjack después de repartir las dos cartas
            {
                finalMessage.text = "El jugador tiene Blackjack!";
                return; // Terminar el juego
            }
            else if (GetDealerScore() == 21 && i == 1) // Verificar si el crupier tiene Blackjack después de repartir las dos cartas
            {
                finalMessage.text = "El crupier tiene Blackjack!";
                return; // Terminar el juego
            }
        }
    }

    private void CalculateProbabilities()
    {
        // Asumiendo que tienes métodos GetPlayerScore() y GetDealerScore() que devuelven la puntuación actual
        // y un método GetRemainingCards() que devuelve el número de cartas restantes en la baraja

        int playerScore = GetPlayerScore();
        int dealerScore = GetDealerScore();
        int remainingCards = GetRemainingCards();

        // Probabilidad de que el dealer tenga más puntuación que el jugador
        int dealerHigherScoreCount = 0;
        for (int i = 0; i < remainingCards; i++)
        {
            if (dealerScore + values[i] > playerScore)
            {
                dealerHigherScoreCount++;
            }
        }
        float dealerHigherScoreProbability = (float)dealerHigherScoreCount / remainingCards;

        // Probabilidad de que el jugador obtenga entre un 17 y un 21 si pide una carta
        int player17to21Count = 0;
        for (int i = 0; i < remainingCards; i++)
        {
            int newScore = playerScore + values[i];
            if (newScore >= 17 && newScore <= 21)
            {
                player17to21Count++;
            }
        }
        float player17to21Probability = (float)player17to21Count / remainingCards;

        // Probabilidad de que el jugador obtenga más de 21 si pide una carta
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
        dealer.GetComponent<CardHand>().Push(faces[cardIndex], values[cardIndex]);
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
        player.GetComponent<CardHand>().Push(faces[cardIndex], values[cardIndex]);
        cardIndex++;

        // Calcular las probabilidades después de repartir la carta
        CalculateProbabilities();
    }

    public void Hit()
    {
        // Asumiendo que tienes un método IsInitialHand() que devuelve true si estamos en la mano inicial
        if (IsInitialHand())
        {
            // Voltear la primera carta del dealer
            dealer.GetComponent<CardHand>().FlipFirstCard();
        }

        // Repartir carta al jugador
        PushPlayer();

        // Asumiendo que tienes un método GetPlayerScore() que devuelve la puntuación actual del jugador
        if (GetPlayerScore() > 21)
        {
            // El jugador ha perdido, mostrar mensaje
            finalMessage.text = "El jugador ha perdido.";
        }
    }
    public void Stand()
    {
        // Asumiendo que tienes un método IsInitialHand() que devuelve true si estamos en la mano inicial
        if (IsInitialHand())
        {
            // Voltear la primera carta del dealer
            dealer.GetComponent<CardHand>().FlipFirstCard();
        }

        // Asumiendo que tienes un método GetDealerScore() que devuelve la puntuación actual del dealer
        while (GetDealerScore() <= 16)
        {
            // Repartir cartas al dealer si tiene 16 puntos o menos
            PushDealer();
        }

        // Mostrar el mensaje del que ha ganado
        if (GetPlayerScore() > 21)
        {
            finalMessage.text = "El dealer ha ganado.";
        }
        else if (GetDealerScore() > 21)
        {
            finalMessage.text = "El jugador ha ganado.";
        }
        else if (GetPlayerScore() > GetDealerScore())
        {
            finalMessage.text = "El jugador ha ganado.";
        }
        else if (GetDealerScore() > GetPlayerScore())
        {
            finalMessage.text = "El dealer ha ganado.";
        }
        else
        {
            finalMessage.text = "Es un empate.";
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
    }

}
