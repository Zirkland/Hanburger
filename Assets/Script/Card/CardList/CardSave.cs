using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Card/CardSave")]

[System.Serializable]
public class CardSave : ScriptableObject
{
    [Header("随机种子")]
    public int seed;
    [Header("当前天数")]
    public int DAYS;
    [Header("0-出牌  1-弃牌  2-刷新  3-购买")]
    public int[] Date = new int[4]{0,0,0,0};//0-Publish  1-Abandon  2-RefNum  3-BuyNum
    public List<string> bookSavecardsName;
    public List<string> cookSavecardsName;
    public List<string> foodSavecardsName;
    public List<string> goodSavecardsName;
    public List<string> cardtypesName;

    [Header("购买食谱")]
    public List<BookCard> bookSavecards;
    [Header("场上员工")]
    public List<CookCard> cookSavecards;
    [Header("场上食物")]
    public List<FoodCard> foodSavecards;
    [Header("购买商品")]
    public List<GoodCard> goodSavecards;
    [Header("用来存储卡牌的计数的")]
    //[NonSerialized]
    public List<BookCard> cardtypes;
    public void Reset_()
    {
        seed = UnityEngine.Random.Range(0, 100000);
        DAYS = 0;
        Date = new int[4];
        foreach (var card in cardtypes) card.UseCountReset(true);
        bookSavecards.Clear();
        cookSavecards.Clear();
        foodSavecards.Clear();
        goodSavecards.Clear();
        
    }
    public int GetSeed() => seed;
    public int GetDays() => DAYS;
    public int GetDate(int i) => Date[i];
    public void AddDate(int i) => Date[i]++;

    public List<BookCard> GetBookCardTypes() => cardtypes;

    public void Copy(out int seed_,out int DAYS_, out int [] Date_, out List<BookCard> bookSavecards_, out List<CookCard> cookSavecards_, out List<FoodCard> foodSavecards_, out List<GoodCard> goodSavecards_, out List<BookCard> cardtypes_)
    {
        seed_ = seed;
        DAYS_ = DAYS;
        Date_ = Date;
        bookSavecards_ = bookSavecards;
        cookSavecards_ = cookSavecards;
        foodSavecards_ = foodSavecards;
        goodSavecards_ = goodSavecards;
        cardtypes_ = cardtypes;
    }
    public void Init(ref int seed_, ref int DAYS_,ref int[] Date_, ref List<BookCard> bookSavecards_, ref List<CookCard> cookSavecards_, ref List<FoodCard> foodSavecards_, ref List<GoodCard> goodSavecards_, ref List<BookCard> cardtypes_)
    {
        seed = seed_;
        DAYS = DAYS_;
        Date = Date_;
        bookSavecards = bookSavecards_;
        cookSavecards = cookSavecards_;
        foodSavecards = foodSavecards_;
        goodSavecards = goodSavecards_;
        for(int i = 0; i < cardtypes_.Count; i++){ cardtypes[i].InitUseNum(cardtypes_[i].GetUseNum()); }
    }

    public void Load(int index = 1)
    {
        string bookSavecardsPath = "Card/Book/";
        string cookSavecardsPath = "Card/Cook/";
        string foodSavecardsPath = "Card/Food/";
        string goodSavecardsPath = "Card/Good/";       
        bookSavecards.Clear();
        cookSavecards.Clear();
        foodSavecards.Clear();
        goodSavecards.Clear();
        foreach (var name in bookSavecardsName) bookSavecards.Add(Resources.Load<BookCard>(bookSavecardsPath + name));
        foreach (var name in cookSavecardsName) cookSavecards.Add(Resources.Load<CookCard>(cookSavecardsPath + name));
        foreach (var name in foodSavecardsName) foodSavecards.Add(Resources.Load<FoodCard>(foodSavecardsPath + name));
        foreach (var name in goodSavecardsName) goodSavecards.Add(Resources.Load<GoodCard>(goodSavecardsPath + name));
        cardtypes.Clear();
        cardtypesName.Clear();
        for (int i = 1; i <= 7;i++) cardtypesName.Add($"Book{i} {index}");  
        string cardtypesPath = "Card/Save/CardTypes/";
        foreach (var name in cardtypesName) cardtypes.Add(Resources.Load<BookCard>(cardtypesPath + name));
    }

    public void Save()
    {
        bookSavecardsName = new();
        cookSavecardsName = new();
        foodSavecardsName = new();
        goodSavecardsName = new();
        foreach (var card in bookSavecards) bookSavecardsName.Add(card.name);
        foreach (var card in cookSavecards) cookSavecardsName.Add(card.name);
        foreach (var card in foodSavecards) foodSavecardsName.Add(card.name);
        foreach (var card in goodSavecards) goodSavecardsName.Add(card.name);
        cardtypes.Clear();
    }


    


    public string GetMostBookCardType(out int mostNum)
    {
        BookCard mostCard = cardtypes[0];
        for(int i = 0; i < cardtypes.Count; i++)
        {
            if(cardtypes[i].GetUseNum() > mostCard.GetUseNum())
            {
                mostCard = cardtypes[i];
            }
        }
        mostNum = mostCard.GetUseNum();
        return mostCard.GetCardType(mostCard.cardType,false);
    }
}