using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

[Serializable]
public class CardBehaviour : CardBase, ICloneable
{
    public string description = "Description";
    public Texture2D image;
    public int health;
    [FormerlySerializedAs("_Attack")] public int attack;
    public int mana;

    public TextMesh nameText;
    public TextMesh healthText;
    [FormerlySerializedAs("AttackText")] public TextMesh attackText;
    public TextMesh manaText;
    [FormerlySerializedAs("DescriptionText")] public TextMesh descriptionText;
    [FormerlySerializedAs("DebugText")] public TextMesh debugText;

    [FormerlySerializedAs("GenerateRandomeData")] public bool generateRandomData;
    public bool canPlay;

    public enum CardStatus
    {
        InDeck,
        InHand,
        OnTable,
        Destroyed
    }

    public CardStatus cardStatus = CardStatus.InDeck;

    public enum CardType
    {
        Monster,
        Magic
    }

    [FormerlySerializedAs("cardtype")] public CardType cardType;

    public enum CardEffect
    {
        ToAll,
        ToEnemies,
        ToSpecific
    }

    [FormerlySerializedAs("cardeffect")] public CardEffect cardEffect;
    public int AddedHealth;
    public int AddedAttack;

    public enum Team
    {
        My,
        AI
    }

    public Team team = Team.My;

    public Vector3 newPos;

    private float _distanceToScreen;
    private bool _selected;

    public delegate void CustomAction();

    private void Start()
    {
        if (Camera.main != null)
            _distanceToScreen = Camera.main.WorldToScreenPoint(gameObject.transform.position).z - 1;
        descriptionText.text = description;

        if (generateRandomData)
        {
            cardType = (CardType)Random.Range(0, 2);


            mana = Random.Range(1, 10);

            //Generate Random Name
            string[] characters = { "a", "b", "c", "d", "e", "f" };
            for (var i = 0; i < 5; i++)
                _name += characters[Random.Range(0, characters.Length)];
            if (cardType == CardType.Magic)
            {
                health = 0;
                attack = 0;
                AddedAttack = Random.Range(1, 8);
                AddedHealth = Random.Range(1, 8);
                cardEffect = (CardEffect)Random.Range(0, 3);
                if (cardEffect == CardEffect.ToSpecific)
                    descriptionText.text =
                        "Add " + AddedAttack + "/" + AddedHealth + "\n" + "   To Any Selected Monster";
                else if (cardEffect == CardEffect.ToAll)
                    descriptionText.text = "Add " + AddedAttack + "/" + AddedHealth + "\n" + "   To ALL";
                else if (cardEffect == CardEffect.ToEnemies)
                    descriptionText.text = "Add " + AddedAttack + "/" + AddedHealth + "\n" + "   To ALL Enemies";
            }
            else
            {
                //Generate Random Data
                health = Random.Range(1, 8);
                attack = Random.Range(1, 8);
            }
        }
    }

    private void FixedUpdate()
    {
        if (!_selected)
        {
            transform.position = Vector3.Lerp(transform.position, newPos, Time.deltaTime * 3);
            if (cardStatus != CardStatus.InDeck)
                transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(0.0f, 180.0f, 0.0f),
                    Time.deltaTime * 3);
        }

        if (cardType == CardType.Monster)
            if (health <= 0)
                Destroy(this);
        //Update Visuals
        nameText.text = _name;
        healthText.text = health.ToString();
        attackText.text = attack.ToString();
        manaText.text = mana.ToString();
        debugText.text = canPlay ? "Ready to attack" : "Can't attack on this turn";
    }

    public void PlaceCard()
    {
        if (BoardBehaviour.BoardInstance.turn == BoardBehaviour.Turn.MyTurn && cardStatus == CardStatus.InHand &&
            team == Team.My)
            //Selected = false;
            BoardBehaviour.BoardInstance.PlaceCard(this);
    }

    private void OnMouseDown()
    {
        if (cardStatus == CardStatus.InHand) _selected = true;


        if (!BoardBehaviour.BoardInstance.currentCard && cardStatus == CardStatus.OnTable)
        {
            //clicked on friendly card on table to attack another table card
            BoardBehaviour.BoardInstance.currentCard = this;
            print("Selected card: " + attack + ":" + health);
        }
        else if (BoardBehaviour.BoardInstance.currentCard &&
                 BoardBehaviour.BoardInstance.currentCard.cardType == CardType.Magic &&
                 BoardBehaviour.BoardInstance.turn == BoardBehaviour.Turn.MyTurn &&
                 cardStatus == CardStatus.OnTable)
        {
            if (BoardBehaviour.BoardInstance.currentCard.cardEffect != CardEffect.ToSpecific) return; //Magic VS Card
            //What Magic Card Will Do To MonsterCard
            BoardBehaviour.BoardInstance.targetCard = this;
            print("Target card: " + attack + ":" + health);
            if (BoardBehaviour.BoardInstance.currentCard.canPlay)
                AddToMonster(BoardBehaviour.BoardInstance.currentCard, BoardBehaviour.BoardInstance.targetCard,
                    true,
                    delegate
                    {
                        BoardBehaviour.BoardInstance.currentCard.Destroy(BoardBehaviour.BoardInstance
                            .currentCard);
                    });
        }
        else if (BoardBehaviour.BoardInstance.currentCard &&
                 BoardBehaviour.BoardInstance.currentCard.cardType == CardType.Monster &&
                 BoardBehaviour.BoardInstance.turn == BoardBehaviour.Turn.MyTurn &&
                 cardStatus == CardStatus.OnTable && BoardBehaviour.BoardInstance.currentCard != this) //Card VS Card
        {
            //clicked opponent card on table on your turn
            if (BoardBehaviour.BoardInstance.currentCard != null && BoardBehaviour.BoardInstance.currentCard.canPlay)
            {
                BoardBehaviour.BoardInstance.targetCard = this;
                print("Target card: " + attack + ":" + health);
                if (BoardBehaviour.BoardInstance.currentCard.canPlay)
                    AttackCard(BoardBehaviour.BoardInstance.currentCard, BoardBehaviour.BoardInstance.targetCard,
                        true, delegate { BoardBehaviour.BoardInstance.currentCard.canPlay = false; });
                else print("Card cannot attack");
            }

            print("Cannot Attack this Target card: ");
        }
        else if (BoardBehaviour.BoardInstance.turn == BoardBehaviour.Turn.MyTurn &&
                 BoardBehaviour.BoardInstance.currentHero && cardStatus == CardStatus.OnTable) //Hero VS Card
        {
            if (BoardBehaviour.BoardInstance.currentHero.canAttack)
            {
                BoardBehaviour.BoardInstance.targetCard = this;
                print("Target card: " + attack + ":" + health);
                BoardBehaviour.BoardInstance.currentHero.AttackCard(BoardBehaviour.BoardInstance.currentHero,
                    BoardBehaviour.BoardInstance.targetCard,
                    delegate { BoardBehaviour.BoardInstance.currentHero.canAttack = false; });
            }
        }
        else
        {
            BoardBehaviour.BoardInstance.currentCard = null;
            BoardBehaviour.BoardInstance.currentHero = null;
            BoardBehaviour.BoardInstance.targetCard = null;
            BoardBehaviour.BoardInstance.targetHero = null;
            Debug.Log("Action Reset");
        }
    }

    private void OnMouseUp()
    {
        //Debug.Log("On Mouse Up Event");
        _selected = false;
    }

    private void OnMouseOver()
    {
        //Debug.Log("On Mouse Over Event");
    }

    private void OnMouseEnter()
    {
        //Debug.Log("On Mouse Enter Event");
        newPos += new Vector3(0,0.5f,0);
    }

    private void OnMouseExit()
    {
        //Debug.Log("On Mouse Exit Event");
        newPos -= new Vector3(0,0.5f, 0);
    }

    private void OnMouseDrag()
    {
        //Debug.Log("On Mouse Drag Event");
        if (Camera.main != null)
            GetComponent<Rigidbody>()
                .MovePosition(Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y,
                    _distanceToScreen)));
    }

    public void SetCardStatus(CardStatus status)
    {
        cardStatus = status;
    }

    public void AttackCard(CardBehaviour attacker, CardBehaviour target, bool addhistory,
        CustomAction action)
    {
        if (attacker.canPlay)
        {
            target.health -= attacker.attack;
            attacker.health -= target.attack;

            if (target.health <= 0) Destroy(target);

            if (attacker.health <= 0) attacker.Destroy(attacker);

            action();
            if (addhistory)
                BoardBehaviour.BoardInstance.AddHistory(attacker, target);
        }
    } //Attack

    public void AttackHero(CardBehaviour attacker, HeroBehaviour target, bool addhistory,
        CustomAction action)
    {
        if (attacker.canPlay)
        {
            target.health -= attacker.attack;
            attacker.health -= target.attack;

            action();
            if (addhistory)
                BoardBehaviour.BoardInstance.AddHistory(attacker, target);
        }
    } //Attack

    public void Destroy(CardBehaviour card)
    {
        if (!card) return;
        if (card.gameObject == null) return;
        if (card.team == Team.My)
            BoardBehaviour.BoardInstance.myTableCards.Remove(card.gameObject);
        else if (card.team == Team.AI)
            BoardBehaviour.BoardInstance.aiTableCards.Remove(card.gameObject);


        //BoardBehaviour.instance.PlaySound(BoardBehaviour.instance.cardDestroy);
        Destroy(card.gameObject);

        BoardBehaviour.BoardInstance.TablePositionUpdate();
        //card = null;
    }

    public void AddToHero(CardBehaviour magic, HeroBehaviour target, CustomAction action)
    {
        if (!magic.canPlay) return;
        target.attack += magic.AddedAttack;
        if (target.health + magic.AddedHealth <= 30)
            target.health += magic.AddedHealth;
        else
            target.health = 30;
        action();
        BoardBehaviour.BoardInstance.AddHistory(magic, target);
    } //Magic

    public void AddToMonster(CardBehaviour magic, CardBehaviour target, bool addhistory,
        CustomAction action)
    {
        if (!magic.canPlay) return;
        target.attack += magic.AddedAttack;
        target.health += magic.AddedHealth;
        action();
        if (addhistory)
            BoardBehaviour.BoardInstance.AddHistory(magic, target);
    } //Magic

    public void AddToAll(CardBehaviour magic, bool addhistory, CustomAction action)
    {
        if (!magic.canPlay) return;
        foreach (var target in BoardBehaviour.BoardInstance.aiTableCards)
            AddToMonster(magic, target.GetComponent<CardBehaviour>(), addhistory, delegate { });
        foreach (var target in BoardBehaviour.BoardInstance.myTableCards)
            AddToMonster(magic, target.GetComponent<CardBehaviour>(), addhistory, delegate { });
        action();
    } //Magic

    public void AddToEnemies(CardBehaviour magic, List<GameObject> targets, bool addhistory, CustomAction action)
    {
        if (!magic.canPlay) return;
        foreach (var target in targets)
            AddToMonster(magic, target.GetComponent<CardBehaviour>(), addhistory, delegate { });
        action();
    } //Magic

    public void AddToEnemies(CardBehaviour magic, List<CardBehaviour> targets, bool addhistory,
        CustomAction action)
    {
        if (magic.canPlay)
        {
            foreach (var target in targets) AddToMonster(magic, target, addhistory, delegate { });
            action();
        }
    } //Magic

    public object Clone()
    {
        var temp = new CardBehaviour();
        temp._name = _name;
        temp.description = description;
        temp.health = health;
        temp.attack = attack;
        temp.mana = mana;
        temp.canPlay = canPlay;
        temp.cardStatus = cardStatus;
        temp.cardType = cardType;
        temp.cardEffect = cardEffect;
        temp.AddedHealth = AddedHealth;
        temp.AddedAttack = AddedAttack;
        temp.team = team;
        temp.newPos = newPos;
        temp._distanceToScreen = _distanceToScreen;
        temp._selected = _selected;
        return temp;
    }
}