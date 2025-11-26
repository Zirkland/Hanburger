using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class BossList : MonoBehaviour
{
    private static BossList _instance;
    public static BossList Instance { get { if (_instance == null) _instance = FindObjectOfType<BossList>(); return _instance; } }
    

    [Header("Card列表")]
    public CardSave cardSave;
    public CardMoney cardMoney;
    [Header("Boss列表")]
    [SerializeField] private CardBoss[] bosss;
    [SerializeField] private List<CardBoss.CardType> bossType;

    public void Start()
    {
        SetBossType(cardSave.GetSeed());
    }
    public void Copy(out CardSave cardSave_, out CardMoney cardMoney_)
    {
        cardSave_ = cardSave;
        cardMoney_ = cardMoney;
    }
    public void Init(ref CardSave cardSave_, ref CardMoney cardMoney_)
    {
        cardSave_.Copy(out int seed, out int DAY,out int[] Date,out List<BookCard> bookSavecards_, out List<CookCard> cookSavecards_, out List<FoodCard> foodSavecards_, out List<GoodCard> goodSavecards_, out List<BookCard> cardtypes_);
        cardSave.Init(ref seed,ref DAY, ref Date, ref bookSavecards_,  ref cookSavecards_, ref foodSavecards_, ref goodSavecards_, ref cardtypes_);
        cardMoney_.Copy(out int goldcoin, out int interest);
        cardMoney.Init(ref goldcoin, ref interest);
    }
    public void BossListReset_()
    {
        cardSave.Reset_();
        cardMoney.Reset_();
        foreach (CardBoss boss in bosss){ boss.Reset_(); }
    }
    public void SetBossType(int seed_)//更换种子调用
    {
        List<CardBoss.CardType> bossType_ = new(bossType);
        for (int i = 0; i < bosss.Length; i++)
        {
            if (i % 3 == 0) bosss[i].cardType = CardBoss.CardType.Shop_Goods;
            else if (i % 6 == 1) bosss[i].cardType = CardBoss.CardType.Shop_Books;
            else if (i % 6 == 4) bosss[i].cardType = CardBoss.CardType.Shop_Cooks;
            else { bosss[i].cardType = Random(bossType_, seed_); bossType_.Remove(bosss[i].cardType); }
        }
        string str = "初始化Boss列表种子:" + seed_.ToString() + "  Boss:";
        for (int i = 1; i < bosss.Length + 1; i++)
        {
            if (GetCardBoss(i).cardType == CardBoss.CardType.None) str += "None ";
            else if (GetCardBoss(i).cardType == CardBoss.CardType.Shop_Books) str += "B__";
            else if (GetCardBoss(i).cardType == CardBoss.CardType.Shop_Cooks) str += "C__";
            else if (GetCardBoss(i).cardType == CardBoss.CardType.Shop_Goods) str += "G__";
            else str += GetCardBoss(i).GetCardType() + "   ";
        }
        Debug.Log(str);
    }
    public void Update()
    {
        //if (Keyboard.current.zKey.wasPressedThisFrame)
        //{
           
        //}
       
    }

    //用法：BossList.Instance.GetBoss(index);
    public CardBoss GetCardBoss(int NextDay)//后续改
    {
        if(NextDay == 0) NextDay = 1;
        return bosss[NextDay-1];
    }

    public CardSave GetCardSave()
    {
        return cardSave;
    }

    public CardMoney GetCardMoney()
    {
        return cardMoney;
    }
   


    private CardBoss.CardType Random(List<CardBoss.CardType> types, int seed)
    {
        if (types.Count == 0) return CardBoss.CardType.None;
        System.Random random = new(seed);
        int randomIndex = random.Next(types.Count);
        (types[^1], types[randomIndex]) = (types[randomIndex], types[^1]);
        return types[^1];
    }
}
