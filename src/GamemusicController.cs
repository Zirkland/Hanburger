// GameController.cs
using UnityEngine;

public class GameController : MonoBehaviour
{
    // 上菜方法
    public void ServeDish()
    {
        // 你的上菜逻辑（如 UI 更新、状态变更等）
        Debug.Log("上菜啦！");

        // 触发音效
        AudioManager.Instance?.PlayServeDish();
    }

    // 重 Roll 方法
    public void Reroll()
    {
        // 你的重 Roll 逻辑（如重新生成菜品、消耗资源等）
        Debug.Log("重新 Roll！");

        // 触发音效
        AudioManager.Instance?.PlayReroll();
    }
}