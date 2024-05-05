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
    public Dropdown betDropdown;
    public Text bankText;
    public Text finalApuestaMessage;
    public Button confirmButton;
    public int[] values = new int[52];
    int cardIndex = 0;
    int[] order = new int[52];
    private int bank = 1000;
    private int bet = 0;
    private void Awake()
    {
        InitCardValues();
    }
    private void Start()
    {
        ShuffleCards();
        StartGame();
        confirmButton.onClick.AddListener(OnConfirm);
        bank = 1000;
        betDropdown.onValueChanged.AddListener(OnBetDropdownChanged);
        UpdateBankText();
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
        float probability1 = 0;
        float probability2 = 0;
        float probability3 = 0;

        int remainingCards = values.Length - cardIndex;

        //calcular la probabilidad de que sabiendo que la baraja del blackjack tiene 52 cartas, probabilidad de que teniendo la carta oculta, el dealer tenga más puntuación que el jugador
        for (int i = cardIndex; i < values.Length; i++)
        {
            if (dealer.GetComponent<CardHand>().points + values[i] > player.GetComponent<CardHand>().points)
            {
                probability1++;
            }
        }

        //calcular la probabilidad de que sabiendo que la baraja del blackjack tiene 52 cartas, probabilidad de que el jugador obtenga entre un 17 y un 21 si pide una carta
        for (int i = cardIndex; i < values.Length; i++)
        {
            if (player.GetComponent<CardHand>().points + values[i] >= 17 && player.GetComponent<CardHand>().points + values[i] <= 21)
            {
                probability2++;
            }
        }

        //calcular la probabilidad de que sabiendo que la baraja del blackjack tiene 52 cartas, probabilidad de que el jugador obtenga más de 21 si pide una carta
        for (int i = cardIndex; i < values.Length; i++)
        {
            if (player.GetComponent<CardHand>().points + values[i] > 21)
            {
                probability3++;
            }
        }

        if (remainingCards != 0)
        {
            prob1.text = Math.Round((probability1 / remainingCards), 4).ToString();
            prob2.text = Math.Round((probability2 / remainingCards), 4).ToString();
            prob3.text = Math.Round((probability3 / remainingCards), 4).ToString();
        }
        else
        {
            prob1.text = "0";
            prob2.text = "0";
            prob3.text = "0";
        }
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

        CalculateProbabilities();
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
            Stand(); 
        }

        CalculateProbabilities();
    }


    public void Hit()
    {
        PushPlayer();

        // Desactiva el Dropdown y el botón de confirmación para que el jugador no pueda cambiar su apuesta
        betDropdown.interactable = false;
        confirmButton.interactable = false;
    }


    public void Stand()
    {
        hitButton.interactable = false;
        // Obtener la mano del dealer
        CardHand dealerHand = dealer.GetComponent<CardHand>();

        // Mostrar la cara de la carta del dealer que está boca abajo

        dealerHand.cards[0].GetComponent<CardModel>().ToggleFace(true);

        // Lógica del dealer (pedir hasta llegar a 17 o más)
        while (GetDealerScore() < 17)
        {
            PushDealer();
        }

        int winner = DetermineWinner();
        ShowResult(winner);

        CalculateProbabilities();
    }


    private int DetermineWinner()
    {
        int playerScore = GetPlayerScore();
        int dealerScore = GetDealerScore();

        // Lógica considerando pasarse de 21
        if (playerScore > 21)
        {
            OnLose();
            return 2; // Dealer gana
        }
        else if (dealerScore > 21)
        {
            OnWin();
            return 1; // Jugador gana
        }
        // Resto de la lógica de comparación...
        else if (playerScore == dealerScore)
        {
            return 0; // Empate
        }
        else if (playerScore > dealerScore)
        {
            OnWin();
            return 1; // Jugador gana
        }
        else
        {
            OnLose();
            return 2; // Dealer gana
        }
    }


    private void ShowResult(int winner)
    {
        switch (winner)
        {
            case 0: // Empate
                finalMessage.text = "Empate";
                break;
            case 1: // Jugador gana
                finalMessage.text = "¡Has ganado!";
                OnWin();
                break;
            case 2: // Dealer gana
                finalMessage.text = "El dealer ha ganado";
                break;
        }
        stickButton.interactable = false;
    }


    public void PlayAgain()
    {
        hitButton.interactable = true;
        stickButton.interactable = true;
        betDropdown.interactable = true;
        confirmButton.interactable = true;
        finalMessage.text = "";
        prob1.text = "";
        prob2.text = "";
        prob3.text = "";
        player.GetComponent<CardHand>().Clear();
        dealer.GetComponent<CardHand>().Clear();
        cardIndex = 0;
        ShuffleCards();
        StartGame();
        UpdateBankText();
    }


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


    public void OnConfirm()
    {
        string betText = betDropdown.options[betDropdown.value].text.Replace(" Credits", "");
        int betValue;
        if (int.TryParse(betText, out betValue))
        {
            if (betValue > bank)
            {
                finalApuestaMessage.text = "No tienes suficiente dinero para hacer esa apuesta";
            }
            else
            {
                bet = betValue;
                bank -= bet;
                UpdateBankText();
            }
        }
        else
        {
            Debug.LogError("Valor de apuesta no válido: " + betDropdown.options[betDropdown.value].text);
        }
    }


    public void OnWin()
    {
        bank += bet * 2;
        UpdateBankText();
    }


    public void OnLose()
    {
        UpdateBankText();
        if (bank <= 0)
        {
            EndGame();
        }
    }


    private void UpdateBankText()
    {
        bankText.text = bank.ToString() + "€";
    }


    private void EndGame()
    {
        hitButton.interactable = false;
        stickButton.interactable = false;
        playAgainButton.interactable = false;
        betDropdown.interactable = false;
        confirmButton.interactable = false;
        finalApuestaMessage.text = "¡Has perdido! No te queda más dinero";
    }


    public void OnBetDropdownChanged(int index)
    {
    finalApuestaMessage.text = "";
    }
}