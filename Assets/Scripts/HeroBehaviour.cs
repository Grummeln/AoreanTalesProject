using System;
using UnityEngine;
using UnityEngine.Serialization;

[Serializable]
public class HeroBehaviour : CardBase, ICloneable
{
    public int health = 30;
    [FormerlySerializedAs("CanAttack")] public bool canAttack = true;
    [FormerlySerializedAs("_Attack")] public int attack;

    public TextMesh healthText;
    [FormerlySerializedAs("AttackText")] public TextMesh attackText;
    [FormerlySerializedAs("DebugText")] public TextMesh debugText;

    public delegate void CustomAction();

    public void OnMouseDown()
    {
        if (BoardBehaviour.BoardInstance.currentCard) //Card [Magic,Monster] VS Hero
        {
            if (!BoardBehaviour.BoardInstance.currentCard.canPlay) return;
            if (BoardBehaviour.BoardInstance.currentCard.cardType == CardBehaviour.CardType.Monster &&
                BoardBehaviour.BoardInstance.turn == BoardBehaviour.Turn.MyTurn)
                BoardBehaviour.BoardInstance.currentCard.AttackHero(BoardBehaviour.BoardInstance.currentCard,
                    this, true, delegate { BoardBehaviour.BoardInstance.currentCard.canPlay = false; });
            else if (BoardBehaviour.BoardInstance.currentCard.cardType == CardBehaviour.CardType.Magic)
                BoardBehaviour.BoardInstance.currentCard.AddToHero(BoardBehaviour.BoardInstance.currentCard, this,
                    delegate
                    {
                        BoardBehaviour.BoardInstance.currentCard.Destroy(BoardBehaviour.BoardInstance
                            .currentCard);
                    });
        }
        else if (BoardBehaviour.BoardInstance.turn == BoardBehaviour.Turn.MyTurn &&
                 !BoardBehaviour.BoardInstance.currentHero)
        {
            //if (BoardBehaviour.instance.currentHero._name == "MyHero")
            {
                BoardBehaviour.BoardInstance.currentHero = this;
                Debug.Log(name + "   Hero Selected");
            }
        }
        else if (BoardBehaviour.BoardInstance.turn == BoardBehaviour.Turn.MyTurn &&
                 BoardBehaviour.BoardInstance.currentHero && canAttack) //Hero Vs Hero
        {
            BoardBehaviour.BoardInstance.targetHero = this;

            if (BoardBehaviour.BoardInstance.currentHero.canAttack && BoardBehaviour.BoardInstance.targetHero !=
                BoardBehaviour.BoardInstance.currentHero)
                AttackHero(BoardBehaviour.BoardInstance.currentHero, BoardBehaviour.BoardInstance.targetHero,
                    delegate { BoardBehaviour.BoardInstance.currentHero.canAttack = false; });
            else print("Hero cannot attack");
        }
    }

    private void FixedUpdate()
    {
        healthText.text = health.ToString();
        attackText.text = attack.ToString();
        debugText.text = canAttack ? "Ready to attack" : "Can't Attack";
    }

    public void AttackCard(HeroBehaviour attacker, CardBehaviour target, CustomAction action)
    {
        if (attacker.canAttack)
        {
            target.health -= attacker.attack;
            attacker.health -= target.attack;

            if (target.health <= 0) target.Destroy(target);

            //if (attacker.health <= 0)
            //{
            //    BoardBehaviour.instance.
            //}

            action();
            BoardBehaviour.BoardInstance.AddHistory(attacker, target);
        }
    }

    public void AttackHero(HeroBehaviour attacker, HeroBehaviour target, CustomAction action)
    {
        if (attacker.canAttack)
        {
            target.health -= attacker.attack;
            attacker.health -= target.attack;

            if (target.health <= 0)
            {
                Destroy(target.gameObject);
                BoardBehaviour.BoardInstance.EndGame(attacker);
            }

            action();
            BoardBehaviour.BoardInstance.AddHistory(attacker, target);
        }
    }

    public object Clone()
    {
        var temp = new HeroBehaviour();
        temp._name = _name;
        temp.health = health;
        temp.canAttack = canAttack;
        temp.attack = attack;
        return temp;
    }
}