using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class CardList : MonoBehaviour
{
    private static CardList _instance;
    public static CardList Instance { get { if (_instance == null) _instance = FindObjectOfType<CardList>(); return _instance; } }

    public int Seed = 114514;

    public List<Card> bookcards = new();
    public List<Card> cookcards = new();
    public List<Card> foodcards = new();
    public List<Card> goodcards = new();

    public List<Card> bookGroupcards = new();
    public List<Card> cookGroupcards = new();
    public List<Card> foodGroupcards = new();
    public List<Card> goodGroupcards = new();
    public int BookCardCount = 50;
    public int FoodCardCount = 50;
    [Header("Save Card List")]
    public CardSave cardSave;
    public CardBoss cardBoss;
    public CardMoney cardMoney;
    [Header("Card Show")]
    public GameObject cardbossshow;
   
    public async UniTask StartGame(CardSave cardSave_, CardBoss cardBoss_, CardMoney cardMoney_, int seed_)
    {        
        cardSave = cardSave_;
        cardBoss = cardBoss_;
        cardMoney = cardMoney_;
        Seed = seed_;
        await UniTask.Delay(1000);
        ReadSaveCard();
        await SetBossStart_(cardBoss);
    }

    public int GetSeed() { return Seed; }
    public async UniTask SetBossStart_(CardBoss cardBoss, float time = 0.5f)
    {
        if (cardBoss)
        {
            Transform show1 = cardbossshow.transform.Find("Show1");
            Transform show2 = cardbossshow.transform.Find("Show2");
            string text1 = cardBoss.GetCardType();
            string text2 = cardBoss.GetCardTypeDesc();
            await ShowTextWithFadeInOnly(show1.GetComponentsInChildren<TextMeshProUGUI>()[0], text1, time);
            await ShowTextWithFadeInOnly(show2.GetComponentsInChildren<TextMeshProUGUI>()[0], text2, time);
            cardBoss.SetBossStart();
        }
    }
    public List<BookCard> GetBookCardTypes()
    {
        return cardSave.GetBookCardTypes();
    }
    
    private void Update()
    {
        if (Keyboard.current.qKey.wasPressedThisFrame)
        {
            Debug.Log("按下q键__手动加载");
            ReadSaveCard();
            _ = SetBossStart_(cardBoss);
        }
        
    }


    public void SaveCard()
    {
        cardSave.bookSavecards = bookGroupcards.ConvertAll(card => card.addCardType.bookCard);
        cardSave.cookSavecards = cookGroupcards.ConvertAll(card => card.addCardType.cookCard);
        cardSave.foodSavecards = foodGroupcards.ConvertAll(card => card.addCardType.foodCard);
        cardSave.goodSavecards = goodGroupcards.ConvertAll(card => card.addCardType.goodCard);
    }

    public async void ReadSaveCard()
    {
        foreach (BookCard card in cardSave.bookSavecards) await BookCardGroup.Instance.Refresh_Card(BookCardDeck.Instance.GetCard(card));
        foreach (CookCard card in cardSave.cookSavecards) await CookCardGroup.Instance.Refresh_Card(CookCardDeck.Instance.GetCard(card));
        foreach (FoodCard card in cardSave.foodSavecards) await FoodCardGroup.Instance.Refresh_Card(FoodCardDeck.Instance.GetCard(card));
        foreach (GoodCard card in cardSave.goodSavecards) await GoodCardGroup.Instance.Refresh_Card(GoodCardDeck.Instance.GetCard(card));
    }

    public void CleanFoodSaveCard()
    {
        cardSave.foodSavecards.Clear();
    }

    #region 动画工具方法

    private async UniTask ShowTextWithFadeInOnly(TMP_Text textComponent, string message, float duration = 0.4f, CancellationToken ct = default)
    {
        if (textComponent == null) return;

        if (!textComponent.TryGetComponent<CanvasGroup>(out var canvasGroup))
        {
            canvasGroup = textComponent.gameObject.AddComponent<CanvasGroup>();
        }

        // 保存原始颜色用于动画起点
        _ = textComponent.color;
        textComponent.text = message;
        canvasGroup.alpha = 0f;

        float fadeTime = Mathf.Clamp(duration, 0.05f, 1f);

        // 调用DoFade方法
        await DoFade(canvasGroup, 0f, 1f, fadeTime, ct);
    }

    private async UniTask DoFade(CanvasGroup cg, float from, float to, float duration, CancellationToken ct)
    {
        await UniTask.Create(async (token) =>
        {
            // 创建透明度动画
            var alphaTween = cg.DOFade(to, duration).From(from).SetEase(Ease.OutQuad);
            // 等待动画完成
            while (alphaTween.IsActive() && !alphaTween.IsComplete())
            {
                token.ThrowIfCancellationRequested();
                await UniTask.Yield(token);
            }
            // 取消处理：终止动画
            if (token.IsCancellationRequested)
            {
                alphaTween.Kill();
                throw new OperationCanceledException(token);
            }
        }, ct);
    }
    #endregion
}
