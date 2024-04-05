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

    private void InitCardValues()
    {
        for (int i = 0; i < values.Length; i++)
        {
            if (i % 13 == 0 || i % 13 > 9)
            {
                // Asignar 10 a las figuras J, Q, K y al As
                values[i] = 10;
            }
            else
            {
                // Asignar el valor nominal a las cartas del 2 al 10
                values[i] = (i % 13) + 1;
            }
        }
    }

    private void ShuffleCards()
    {
        System.Random rand = new System.Random();
        for (int i = values.Length - 1; i > 0; i--)
        {
            // Elige una posición aleatoria anterior
            int j = rand.Next(i + 1);
            // Intercambia values[i] con values[j]
            int tempValue = values[i];
            values[i] = values[j];
            values[j] = tempValue;
            // Intercambia faces[i] con faces[j]
            Sprite tempFace = faces[i];
            faces[i] = faces[j];
            faces[j] = tempFace;
        }
    }


    void StartGame()
{
    for (int i = 0; i < 2; i++)
    {
        PushPlayer();
        PushDealer();
    }

    // Comprobar si el jugador o el dealer tienen Blackjack
    if ((player.GetComponent<CardHand>().points == 21 && player.GetComponent<CardHand>().cards.Count == 2) ||
        (dealer.GetComponent<CardHand>().points == 21 && dealer.GetComponent<CardHand>().cards.Count == 2))
    {
        // Desactivar los botones de Hit y Stand
        hitButton.interactable = false;
        stickButton.interactable = false;
        // Mostrar mensaje de que alguien tiene Blackjack
        finalMessage.text = "Blackjack!";
    }
}

    private void CalculateProbabilities()
{
    int playerScore = player.GetComponent<CardHand>().points;
    int dealerScore = dealer.GetComponent<CardHand>().points - dealer.GetComponent<CardHand>().cards[0].GetComponent<CardModel>().value; // Excluyendo la carta oculta

    // Probabilidad de que el dealer tenga más puntuación que el jugador
    int dealerHigherScoreCount = 0;
    for (int i = 0; i < values.Length; i++)
    {
        if (dealerScore + values[i] > playerScore && dealerScore + values[i] <= 21)
        {
            dealerHigherScoreCount++;
        }
    }
    float dealerHigherScoreProbability = (float)dealerHigherScoreCount / (values.Length - player.GetComponent<CardHand>().cards.Count - dealer.GetComponent<CardHand>().cards.Count);

    // Probabilidad de que el jugador obtenga entre un 17 y un 21 si pide una carta
    int playerGoodScoreCount = 0;
    for (int i = 0; i < values.Length; i++)
    {
        if (playerScore + values[i] >= 17 && playerScore + values[i] <= 21)
        {
            playerGoodScoreCount++;
        }
    }
    float playerGoodScoreProbability = (float)playerGoodScoreCount / (values.Length - player.GetComponent<CardHand>().cards.Count - dealer.GetComponent<CardHand>().cards.Count);

    // Probabilidad de que el jugador obtenga más de 21 si pide una carta
    int playerBustCount = 0;
    for (int i = 0; i < values.Length; i++)
    {
        if (playerScore + values[i] > 21)
        {
            playerBustCount++;
        }
    }
    float playerBustProbability = (float)playerBustCount / (values.Length - player.GetComponent<CardHand>().cards.Count - dealer.GetComponent<CardHand>().cards.Count);

    // Imprimir las probabilidades
    Debug.Log("Probabilidad de que el dealer tenga más puntuación que el jugador: " + dealerHigherScoreProbability);
    Debug.Log("Probabilidad de que el jugador obtenga entre un 17 y un 21 si pide una carta: " + playerGoodScoreProbability);
    Debug.Log("Probabilidad de que el jugador obtenga más de 21 si pide una carta: " + playerBustProbability);
}

    void PushDealer()
    {
        // Obtiene el componente CardHand del objeto dealer
        CardHand dealerHand = dealer.GetComponent<CardHand>();

        // Obtiene la cara y el valor de la carta en la posición cardIndex del mazo
        Sprite cardFace = faces[cardIndex];
        int cardValue = values[cardIndex];

        // Añade la carta a la mano del dealer
        dealerHand.Push(cardFace, cardValue);

        // Incrementa cardIndex para la próxima vez que se llame a la función
        cardIndex++;
    }

    void PushPlayer()
    {
        /*TODO:
         * Dependiendo de cómo se implemente ShuffleCards, es posible que haya que cambiar el índice.
         */
        player.GetComponent<CardHand>().Push(faces[cardIndex], values[cardIndex]/*,cardCopy*/);
        cardIndex++;
        CalculateProbabilities();
    }

    public void Hit()
    {
        // Si estamos en la mano inicial, debemos voltear la primera carta del dealer.
        if (cardIndex == 4) // 4 cartas ya han sido repartidas (2 al jugador, 2 al dealer)
        {
            dealer.GetComponent<CardHand>().InitialToggle();
        }

        // Repartimos carta al jugador
        PushPlayer();

        // Comprobamos si el jugador ya ha perdido y mostramos mensaje
        if (player.GetComponent<CardHand>().points > 21)
        {
            // Aquí necesitarás reemplazar "finalMessage" y "hitButton" y "stickButton" con tus propios objetos UI
            finalMessage.text = "Has perdido!";
            hitButton.interactable = false;
            stickButton.interactable = false;
        }
    }

    public void Stand()
    {
        // Si estamos en la mano inicial, debemos voltear la primera carta del dealer.
        if (cardIndex == 4) // 4 cartas ya han sido repartidas (2 al jugador, 2 al dealer)
        {
            dealer.GetComponent<CardHand>().InitialToggle();
        }

        // Repartimos cartas al dealer si tiene 16 puntos o menos
        while (dealer.GetComponent<CardHand>().points <= 16)
        {
            PushDealer();
        }

        // Mostramos el mensaje del que ha ganado
        if (dealer.GetComponent<CardHand>().points > 21 || player.GetComponent<CardHand>().points > dealer.GetComponent<CardHand>().points)
        {
            // Aquí necesitarás reemplazar "finalMessage" con tu propio objeto UI
            finalMessage.text = "¡Has ganado!";
        }
        else if (dealer.GetComponent<CardHand>().points > player.GetComponent<CardHand>().points)
        {
            finalMessage.text = "Has perdido!";
        }
        else
        {
            finalMessage.text = "¡Empate!";
        }

        // Desactivamos los botones
        // Aquí necesitarás reemplazar "hitButton" y "stickButton" con tus propios objetos UI
        hitButton.interactable = false;
        stickButton.interactable = false;
    }

    public void PlayAgain()
    {
        hitButton.interactable = true;
        stickButton.interactable = true;
        finalMessage.text = "";
        player.GetComponent<CardHand>().Clear();
        dealer.GetComponent<CardHand>().Clear();
        cardIndex = 0;
        ShuffleCards();
        StartGame();
    }

}
