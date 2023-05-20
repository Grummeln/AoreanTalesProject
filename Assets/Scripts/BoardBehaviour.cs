using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Random = UnityEngine.Random;

[Serializable]
public class BoardBehaviour : MonoBehaviour
{
    public static BoardBehaviour BoardInstance;
    public Text winnertext;
    [FormerlySerializedAs("MYDeckPos")] public Transform myDeckPos;
    [FormerlySerializedAs("MyHandPos")] public Transform myHandPos;
    [FormerlySerializedAs("MyTablePos")] public Transform myTablePos;

    [FormerlySerializedAs("AIDeckPos")] public Transform aiDeckPos;
    [FormerlySerializedAs("AIHandPos")] public Transform aiHandPos;
    [FormerlySerializedAs("AITablePos")] public Transform aiTablePos;

    [FormerlySerializedAs("MyDeckCards")] public List<GameObject> myDeckCards = new List<GameObject>();
    [FormerlySerializedAs("MyHandCards")] public List<GameObject> myHandCards = new List<GameObject>();
    [FormerlySerializedAs("MyTableCards")] public List<GameObject> myTableCards = new List<GameObject>();

    [FormerlySerializedAs("AIDeckCards")] public List<GameObject> aiDeckCards = new List<GameObject>();
    [FormerlySerializedAs("AIHandCards")] public List<GameObject> aiHandCards = new List<GameObject>();
    [FormerlySerializedAs("AITableCards")] public List<GameObject> aiTableCards = new List<GameObject>();

    [FormerlySerializedAs("MyManaText")] public TextMesh myManaText;
    [FormerlySerializedAs("AIManaText")] public TextMesh aiManaText;

    [FormerlySerializedAs("MyHero")] public HeroBehaviour myHero;
    [FormerlySerializedAs("AIHero")] public HeroBehaviour aiHero;

    public enum Turn
    {
        MyTurn,
        AITurn
    }

    #region SetStartData

    public Turn turn = Turn.MyTurn;

    private int _maxMana = 1;
    private int _myMana = 1;
    private int _aiMana = 1;

    public bool gameStarted;
    private int _turnNumber = 1;

    #endregion

    public CardBehaviour currentCard;
    public CardBehaviour targetCard;
    public HeroBehaviour currentHero;
    public HeroBehaviour targetHero;

    public List<Hashtable> BoardHistory = new List<Hashtable>();
    [FormerlySerializedAs("AILEVEL")] public int ailevel;
    public LayerMask layer;

    public void AddHistory(CardBase a, CardBase b)
    {
        var hash = new Hashtable { { a, b } };

        BoardHistory.Add(hash);
        currentCard = null;
        targetCard = null;
        currentHero = null;
        targetHero = null;
    }

    private void Awake()
    {
        BoardInstance = this;
    }

    private void Start()
    {
        foreach (var cardObject in GameObject.FindGameObjectsWithTag("Card"))
        {
            cardObject.GetComponent<Rigidbody>().collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
            cardObject.GetComponent<Rigidbody>().isKinematic = true;
            var c = cardObject.GetComponent<CardBehaviour>();

            if (c.team == CardBehaviour.Team.My)
                myDeckCards.Add(cardObject);
            else
                aiDeckCards.Add(cardObject);
        }

        //Update Deck Cards Position
        DecksPositionUpdate();
        //Update Hand Cards Position
        HandPositionUpdate();

        //Start Game
        StartGame();
    }

    public void StartGame()
    {
        gameStarted = true;
        UpdateGame();

        for (var i = 0; i < 3; i++)
        {
            DrawCardFromDeck(CardBehaviour.Team.My);
            DrawCardFromDeck(CardBehaviour.Team.AI);
        }
    }

    public void DrawCardFromDeck(CardBehaviour.Team team)
    {
        if (team == CardBehaviour.Team.My && myDeckCards.Count != 0 && myHandCards.Count < 6)
        {
            var random = Random.Range(0, myDeckCards.Count);
            var tempCard = myDeckCards[random];

            //tempCard.transform.position = MyHandPos.position;
            tempCard.GetComponent<CardBehaviour>().newPos = myHandPos.position;
            tempCard.GetComponent<CardBehaviour>().SetCardStatus(CardBehaviour.CardStatus.InHand);

            myDeckCards.Remove(tempCard);
            myHandCards.Add(tempCard);
        }

        if (team == CardBehaviour.Team.AI && aiDeckCards.Count != 0 && aiHandCards.Count < 6)
        {
            var random = Random.Range(0, aiDeckCards.Count);
            var tempCard = aiDeckCards[random];

            tempCard.transform.position = aiHandPos.position;
            tempCard.GetComponent<CardBehaviour>().SetCardStatus(CardBehaviour.CardStatus.InHand);

            aiDeckCards.Remove(tempCard);
            aiHandCards.Add(tempCard);
        }

        UpdateGame();
        //Update Hand Cards Position
        HandPositionUpdate();
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            currentCard = null;
            targetCard = null;
            currentHero = null;
            targetHero = null;
            Debug.Log("Action Revert");
        }

        //if(BoardBehaviour.instance.currentCard&&BoardBehaviour.instance.targetCard)
        {
            if (Camera.main != null)
            {
                var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit, 250, layer))
                    if (hit.transform.CompareTag("Board"))
                    {
                    }
            }
        }

        if (myHero.health <= 0)
            EndGame(aiHero);
        if (aiHero.health <= 0)
            EndGame(myHero);
    }

    private void UpdateGame()
    {
        myManaText.text = _myMana + "/" + _maxMana;
        aiManaText.text = _aiMana + "/" + _maxMana;

        if (myHero.health <= 0)
            EndGame(aiHero);
        if (aiHero.health <= 0)
            EndGame(myHero);

        //UpdateBoard();
    }

    private void DecksPositionUpdate()
    {
        foreach (var cardObject in myDeckCards)
        {
            var c = cardObject.GetComponent<CardBehaviour>();

            if (c.cardStatus == CardBehaviour.CardStatus.InDeck)
                c.newPos = myDeckPos.position;
        }

        foreach (var cardObject in aiDeckCards)
        {
            var c = cardObject.GetComponent<CardBehaviour>();

            if (c.cardStatus == CardBehaviour.CardStatus.InDeck)
                c.newPos = aiDeckPos.position;
        }
    }

    public void HandPositionUpdate()
    {
        var space = 0f;
        var space2 = 0f;
        var gap = 1.3f;

        foreach (var card in myHandCards)
        {
            var numberOfCards = myHandCards.Count;
            card.GetComponent<CardBehaviour>().newPos =
                myHandPos.position + new Vector3(-numberOfCards / 2 + space, 0, 0);
            space += gap;
        }

        foreach (var card in aiHandCards)
        {
            var numberOfCards = aiHandCards.Count;
            card.GetComponent<CardBehaviour>().newPos =
                aiHandPos.position + new Vector3(-numberOfCards / 2 + space2, 0, 0);
            space2 += gap;
        }
    }

    public void TablePositionUpdate()
    {
        var space = 0f;
        var space2 = 0f;
        float gap = 2;

        foreach (var card in myTableCards)
        {
            var numberOfCards = myTableCards.Count;
            //card.transform.position = myTablePos.position + new Vector3(-numberOfCards + space - 2,0,0);
            card.GetComponent<CardBehaviour>().newPos =
                myTablePos.position + new Vector3(-numberOfCards + space - 2, 0, 0);
            space += gap;
        }

        foreach (var card in aiTableCards)
        {
            var numberOfCards = aiTableCards.Count;
            //card.transform.position = AITablePos.position + new Vector3(-numberOfCards + space2,0,0);
            card.GetComponent<CardBehaviour>().newPos =
                aiTablePos.position + new Vector3(-numberOfCards + space2, 0, 0);
            space2 += gap;
        }
    }

    public void PlaceCard(CardBehaviour card)
    {
        if (card.team == CardBehaviour.Team.My && _myMana - card.mana >= 0 && myTableCards.Count < 10)
        {
            //card.gameObject.transform.position = MyTablePos.position;
            card.GetComponent<CardBehaviour>().newPos = myTablePos.position;

            myHandCards.Remove(card.gameObject);
            myTableCards.Add(card.gameObject);

            card.SetCardStatus(CardBehaviour.CardStatus.OnTable);
            //PlaySound(cardDrop);

            if (card.cardType == CardBehaviour.CardType.Magic) ///Apply Magic Effect 
            {
                card.canPlay = true;
                if (card.cardEffect == CardBehaviour.CardEffect.ToAll)
                    card.AddToAll(card, true, delegate { card.Destroy(card); });
                else if (card.cardEffect == CardBehaviour.CardEffect.ToEnemies)
                    card.AddToEnemies(card, aiTableCards, true, delegate { card.Destroy(card); });
            }

            _myMana -= card.mana;
        }

        if (card.team == CardBehaviour.Team.AI && _aiMana - card.mana >= 0 && aiTableCards.Count < 10)
        {
            //card.gameObject.transform.position = AITablePos.position;
            card.GetComponent<CardBehaviour>().newPos = aiTablePos.position;

            aiHandCards.Remove(card.gameObject);
            aiTableCards.Add(card.gameObject);

            card.SetCardStatus(CardBehaviour.CardStatus.OnTable);
            //PlaySound(cardDrop);

            if (card.cardType == CardBehaviour.CardType.Magic) //Apply Magic Effect 
            {
                card.canPlay = true;
                if (card.cardEffect == CardBehaviour.CardEffect.ToAll)
                    card.AddToAll(card, true, delegate { card.Destroy(card); });
                else if (card.cardEffect == CardBehaviour.CardEffect.ToEnemies)
                    card.AddToEnemies(card, myTableCards, true, delegate { card.Destroy(card); });
            }

            _aiMana -= card.mana;
        }

        TablePositionUpdate();
        HandPositionUpdate();
        UpdateGame();
    }

    public void PlaceRandomCard(CardBehaviour.Team team)
    {
        if (team == CardBehaviour.Team.My && myHandCards.Count != 0)
        {
            var random = Random.Range(0, myHandCards.Count);
            var tempCard = myHandCards[random];

            PlaceCard(tempCard.GetComponent<CardBehaviour>());
        }

        if (team == CardBehaviour.Team.AI && aiHandCards.Count != 0)
        {
            var random = Random.Range(0, aiHandCards.Count);
            var tempCard = aiHandCards[random];

            PlaceCard(tempCard.GetComponent<CardBehaviour>());
        }

        UpdateGame();
        EndTurn();

        TablePositionUpdate();
        HandPositionUpdate();
    }

    public void EndGame(HeroBehaviour winner)
    {
        if (winner == myHero)
        {
            Debug.Log("MyHero");
            Time.timeScale = 0;
            winnertext.text = "You Won";
            Destroy(this);
        }

        if (winner == aiHero)
        {
            Time.timeScale = 0;
            Debug.Log("AIHero");
            winnertext.text = "You Lost";
            Destroy(this);
        }
    }

    private void OnGUI()
    {
        if (gameStarted)
        {
            if (turn == Turn.MyTurn)
                if (GUI.Button(new Rect(Screen.width - 200, Screen.height / 2 - 50, 100, 50), "End Turn"))
                    EndTurn();

            GUI.Label(new Rect(Screen.width - 200, Screen.height / 2 - 100, 100, 50),
                "Turn: " + turn + " Turn Number: " + _turnNumber);

            foreach (var history in BoardHistory)
            foreach (DictionaryEntry entry in history)
            {
                var card1 = entry.Key as CardBase;
                var card2 = entry.Value as CardBase;

                GUILayout.Label(card1._name + " > " + card2._name);
            }

            if (BoardHistory.Count > 6)
            {
                Hashtable temp;
                temp = BoardHistory[BoardHistory.Count - 1];
                BoardHistory.Clear();
                BoardHistory.Add(temp);
            }
        }
    }

    private void EndTurn()
    {
        _maxMana += (_turnNumber - 1) % 2;
        if (_maxMana >= 10) _maxMana = 10;
        _myMana = _maxMana;
        _aiMana = _maxMana;
        _turnNumber += 1;
        currentCard = new CardBehaviour();
        targetCard = new CardBehaviour();
        currentHero = new HeroBehaviour();
        targetHero = new HeroBehaviour();
        foreach (var card in myTableCards)
            card.GetComponent<CardBehaviour>().canPlay = true;

        foreach (var card in aiTableCards)
            card.GetComponent<CardBehaviour>().canPlay = true;
        myHero.canAttack = true;
        aiHero.canAttack = true;

        if (turn == Turn.AITurn)
        {
            DrawCardFromDeck(CardBehaviour.Team.My);
            turn = Turn.MyTurn;
        }
        else if (turn == Turn.MyTurn)
        {
            DrawCardFromDeck(CardBehaviour.Team.AI);
            turn = Turn.AITurn;
        }

        HandPositionUpdate();
        TablePositionUpdate();

        OnNewTurn();
    }

    private void OnNewTurn()
    {
        UpdateGame();

        if (turn == Turn.AITurn)
            AI_Think();
        //Invoke("AI_Think", 5.0f);
    }

    private void AI_Think()
    {
        if (ailevel == 0)
        {
            if (turn == Turn.AITurn) Invoke("RendomActions", 2.0f);
        }
        else if (ailevel > 0)
        {
            if (turn == Turn.AITurn) Invoke("AIthink", 2.0f);
        }
    }

    private void AIthink()
    {
        AIGameState.AllStates.Clear(); // = new List<AIGameState>();
        AIGetPlacing();
        AIGetAttacks();
        EndTurn();
    }

    private void RendomActions()
    {
        #region placing cards

        //float chanceToPlace = Random.value;

        if (aiHandCards.Count == 0)
            EndTurn();
        else
            PlaceRandomCard(CardBehaviour.Team.AI);

        #endregion

        #region attacking

        var attacks = new Hashtable();
        foreach (var Card in aiTableCards)
        {
            var card = Card.GetComponent<CardBehaviour>();

            if (card.canPlay)
            {
                var changeToAttackhero = Random.value;

                if (changeToAttackhero < 0.50f)
                {
                    card.AttackHero(card, myHero, true, delegate { card.canPlay = false; });
                }
                else if (myTableCards.Count > 0)
                {
                    var random = Random.Range(0, myTableCards.Count);
                    var cardToAttack = myTableCards[random];

                    attacks.Add(card, cardToAttack.GetComponent<CardBehaviour>());
                }
            }
        }

        foreach (DictionaryEntry row in attacks)
        {
            var tempCard = row.Key as CardBehaviour;
            var temp2 = row.Value as CardBehaviour;

            if (tempCard.cardType == CardBehaviour.CardType.Monster)
                tempCard.AttackCard(tempCard, temp2, true, delegate { tempCard.canPlay = false; });
            else if (tempCard.cardType == CardBehaviour.CardType.Magic)
                tempCard.AddToMonster(tempCard, temp2, true, delegate
                {
                    tempCard.canPlay = false;
                    tempCard.Destroy(tempCard);
                });
        }

        #endregion
    }

    private void AIGetPlacing()
    {
        var initialState = new AIGameState( /*MyHandCards,*/myTableCards, aiHandCards, aiTableCards, myHero, aiHero,
            _maxMana, _myMana, _aiMana, turn, null);
        initialState.GetAllPlacingAction();
        //Find Best Score
        var maxScore = float.MinValue;
        var bestState = new AIGameState();
        foreach (var item in AIGameState.AllStates)
            if (item.StateScore > maxScore)
            {
                maxScore = item.StateScore;
                bestState = item;
            }

        var count = bestState.Actions.Count;
        //GetActions
        for (var i = 0; i < count; i++)
        {
            AIGameState.Action a;
            a = bestState.Actions.Dequeue();
            if (a.OpCode == 0)
                foreach (var item in aiHandCards)
                    if (item.GetComponent<CardBehaviour>()._name == a.Card1)
                    {
                        PlaceCard(item.GetComponent<CardBehaviour>());
                        break;
                    }
        }

        AIGameState.AllStates.Clear();
    }

    private void AIGetAttacks()
    {
        var initialState = new AIGameState( /*MyHandCards,*/myTableCards, aiHandCards, aiTableCards, myHero, aiHero,
            _maxMana, _myMana, _aiMana, turn, null);
        initialState.GetAllAttackingActions(ailevel);
        //Find Best Score
        var maxScore = float.MinValue;
        var bestState = new AIGameState();
        foreach (var item in AIGameState.AllStates)
            if (item.StateScore > maxScore)
            {
                maxScore = item.StateScore;
                bestState = item;
            }

        //Debug.Log("Best choice Index" + BestState.Index);
        var count = bestState.Actions.Count;
        //GetActions
        for (var i = 0; i < count; i++)
        {
            AIGameState.Action a;
            a = bestState.Actions.Dequeue();
            if (a.OpCode == 1)
            {
                foreach (var item in aiTableCards) //Find Card1
                    if (item.GetComponent<CardBehaviour>()._name == a.Card1)
                    {
                        currentCard = item.GetComponent<CardBehaviour>();
                        break;
                    }

                foreach (var item in myTableCards) //Find Card2
                    if (item.GetComponent<CardBehaviour>()._name == a.Card2)
                    {
                        targetCard = item.GetComponent<CardBehaviour>();
                        break;
                    }

                if (currentCard != null && targetCard != null) //MakeAction
                    currentCard.AttackCard(currentCard, targetCard, true, delegate { currentCard.canPlay = false; });
            }
            else if (a.OpCode == 2)
            {
                foreach (var item in aiTableCards) //Find Card1
                    if (item.GetComponent<CardBehaviour>()._name == a.Card1)
                    {
                        currentCard = item.GetComponent<CardBehaviour>();
                        break;
                    }

                if (a.Hero == "MyHero") targetHero = myHero;
                if (currentCard != null && targetHero != null)
                    currentCard.AttackHero(currentCard, myHero, true, delegate { currentCard.canPlay = false; });
            }
            else if (a.OpCode == 3)
            {
                foreach (var item in aiTableCards) //Find Card1
                    if (item.GetComponent<CardBehaviour>()._name == a.Card1)
                    {
                        currentCard = item.GetComponent<CardBehaviour>();
                        break;
                    }

                foreach (var item in myTableCards) //Find Card2
                    if (item.GetComponent<CardBehaviour>()._name == a.Card2)
                    {
                        targetCard = item.GetComponent<CardBehaviour>();
                        break;
                    }

                if (currentCard != null && targetCard != null) //MakeAction
                    currentCard.AddToMonster(currentCard, targetCard, true,
                        delegate { currentCard.Destroy(currentCard); });
            }
        }
        //AIGameState.AllStates=new List< AIGameState > ();
    }

    private void OnTriggerEnter(Collider obj)
    {
        var card = obj.GetComponent<CardBehaviour>();
        if (card) card.PlaceCard();
    }
}