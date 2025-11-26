using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;


public class CardVisual : MonoBehaviour
{
    private bool initalize = false;

    [Header("Card")]
    public Card parentCard;
    private Transform cardTransform;
    private Vector3 rotationDelta;
    private int savedIndex;
    Vector3 movementDelta;
    private Canvas canvas;

    [Header("References")]
    public Transform visualShadow;
    private readonly float shadowOffset = 20;
    private Vector2 shadowDistance;
    private Canvas shadowCanvas;
    public Image cardImage;
    [SerializeField] private Transform shakeParent;
    [SerializeField] private Transform tiltParent;


    [Header("Follow Parameters")]
    [SerializeField] private float followTime = 1.0f;

    [Header("Rotation Parameters")]
    [SerializeField] private float rotationAmount = 20;
    [SerializeField] private float rotationSpeed = 20;
    [SerializeField] private float autoTiltAmount = 30;
    [SerializeField] private float manualTiltAmount = 20;
    [SerializeField] private float tiltSpeed = 20;

    [Header("Scale Parameters")]
    [SerializeField] private bool scaleAnimations = true;
    [SerializeField] private float scaleOnHover = 1.15f;
    [SerializeField] private float scaleOnSelect = 1.25f;
    [SerializeField] private float scaleTransition = .15f;
    [SerializeField] private Ease scaleEase = Ease.OutBack;

    [Header("Select Parameters")]
    [SerializeField] private float selectPunchAmount = 20;

    [Header("Hober Parameters")]
    [SerializeField] private float hoverPunchAngle = 5;
    [SerializeField] private float hoverTransition = .15f;

    [Header("Swap Parameters")]
    [SerializeField] private bool swapAnimations = true;
    [SerializeField] private float swapRotationAngle = 30;
    [SerializeField] private float swapTransition = .15f;
    [SerializeField] private int swapVibrato = 5;

    [Header("Curve")]
    [SerializeField] private CurveParameters curve;

    private float curveYOffset;
    private float curveRotationOffset;
    private Tweener positionTweener;
    //private Coroutine pressCoroutine;
    private InputActionAsset inputActions;

    private  void Start()
    {
        shadowDistance = visualShadow.localPosition;
        inputActions = Resources.Load<InputActionAsset>("Input/PlayerInput");
        inputActions.FindActionMap("Gameplay").Enable();
    }

    public void Initialize(Card target/*, int index = 0*/)
    {
        //Declarations
        parentCard = target;
        cardTransform = target.transform;
        canvas = GetComponent<Canvas>();
        shadowCanvas = visualShadow.GetComponent<Canvas>();

        //Event Listening
        parentCard.PointerEnterEvent.AddListener(PointerEnter);
        parentCard.PointerExitEvent.AddListener(PointerExit);
        parentCard.BeginDragEvent.AddListener(BeginDrag);
        parentCard.EndDragEvent.AddListener(EndDrag);
        parentCard.PointerDownEvent.AddListener(PointerDown);
        parentCard.PointerUpEvent.AddListener(PointerUp);
        parentCard.SelectEvent.AddListener(Select);

        //Initialization
        initalize = true;
    }

    private Vector2 GetInputPosition()
    {
        // 检测是否有活跃的触摸（单指）
        if (Touchscreen.current != null && Touchscreen.current.touches.Count > 0)
        {
            TouchControl touch = Touchscreen.current.touches[0];
            // 显式指定 InputSystem 命名空间下的 TouchPhase
            if (touch.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Began ||
                touch.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Moved ||
                touch.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Stationary)
            {
                // 返回第一根活跃手指的屏幕坐标
                return touch.position.ReadValue();
            }
        }
        // 没有触摸时，返回鼠标坐标
        if (Mouse.current != null)
        {
            return Mouse.current.position.ReadValue();
        }
        else
        {
            Debug.Log("No input device detected.");
            return Vector2.zero;
        }
    }

    public void UpdateIndex(/*int length*/)
    {
        transform.SetSiblingIndex(parentCard.transform.parent.GetSiblingIndex());
    }

    public void Update()
    {
        if (!initalize || parentCard == null) return;
        HandPositioning(); // 1. 计算卡片在手牌中的曲线偏移（如扇形排列）        
        SmoothFollow(); // 2. 平滑跟随卡片的基础位置（避免瞬移）
        FollowRotation(); // 3. 跟随拖拽/移动产生的旋转（如拖拽时卡片随鼠标偏移旋转）
        CardTilt();// 4. 卡片的倾斜效果（自动波动 + 鼠标位置感应）
    }

    #region 效果实现
    private void HandPositioning()
    {
        curveYOffset = (curve.positioning.Evaluate(parentCard.NormalizedPosition()) * curve.positioningInfluence) * parentCard.SiblingAmount();
        curveYOffset = parentCard.SiblingAmount() < 5 ? 0 : curveYOffset; //优化：卡片数量少于5张时，不启用曲线偏移（避免过度弯曲）
        curveRotationOffset = curve.rotation.Evaluate(parentCard.NormalizedPosition());//计算曲线对应的旋转偏移（让卡片随曲线角度同步旋转）
    }

    private void SmoothFollow()
    {
        Vector3 targetPosition = cardTransform.position + (Vector3.up * (parentCard.isDragging ? 0 : curveYOffset));

        // 如果已有Tween，直接更新目标位置            // 没有活跃的Tween时，创建新的
        if (positionTweener != null && positionTweener.IsActive())
        {
            positionTweener.ChangeEndValue(targetPosition, followTime, true);
        }
        else
        {
            positionTweener = transform.DOMove(targetPosition, followTime)
                .SetEase(Ease.Linear)
                .SetUpdate(true)
                .OnComplete(() => positionTweener = null);                // 完成后清空引用
        }
    }

    private void FollowRotation()//采用了线性插值，是否采用DoTween的缓动效果待定
    {
        Vector3 movement = (transform.position - cardTransform.position);
        movementDelta = Vector3.Lerp(movementDelta, movement, 25 * Time.deltaTime);
        Vector3 movementRotation = (parentCard.isDragging ? movementDelta : movement) * rotationAmount;
        rotationDelta = Vector3.Lerp(rotationDelta, movementRotation, rotationSpeed * Time.deltaTime);
        transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, Mathf.Clamp(rotationDelta.x, -60, 60));
    }

    private void CardTilt()
    {
        savedIndex = parentCard.isDragging ? savedIndex : parentCard.ParentIndex();
        float sin = Mathf.Sin(Time.time + savedIndex) * (parentCard.isHovering ? .2f : 1);
        float cos = Mathf.Cos(Time.time + savedIndex) * (parentCard.isHovering ? .2f : 1);

        Vector3 offset = transform.position - Camera.main.ScreenToWorldPoint(GetInputPosition());
        float tiltX = parentCard.isHovering ? ((offset.y * -1) * manualTiltAmount) : 0;
        float tiltY = parentCard.isHovering ? ((offset.x) * manualTiltAmount) : 0;
        float tiltZ = parentCard.isDragging ? tiltParent.eulerAngles.z : (curveRotationOffset * (curve.rotationInfluence * parentCard.SiblingAmount()));

        float lerpX = Mathf.LerpAngle(tiltParent.eulerAngles.x, tiltX + (sin * autoTiltAmount), tiltSpeed * Time.deltaTime);
        float lerpY = Mathf.LerpAngle(tiltParent.eulerAngles.y, tiltY + (cos * autoTiltAmount), tiltSpeed * Time.deltaTime);
        float lerpZ = Mathf.LerpAngle(tiltParent.eulerAngles.z, tiltZ, tiltSpeed / 2 * Time.deltaTime);

        tiltParent.eulerAngles = new Vector3(lerpX, lerpY, lerpZ);
    }

    #endregion

    #region 事件监听
    private void Select(Card card, bool state)
    {
        DOTween.Kill(2, true);
        float dir = state ? 1 : 0;
        shakeParent.DOPunchPosition(dir * selectPunchAmount * shakeParent.up, scaleTransition, 10, 1);
        shakeParent.DOPunchRotation(Vector3.forward * (hoverPunchAngle / 2), hoverTransition, 20, 1).SetId(2);

        if (scaleAnimations)
            transform.DOScale(scaleOnHover, scaleTransition).SetEase(scaleEase);

    }

    public void Swap(float dir = 1)
    {
        if (!swapAnimations)
            return;

        DOTween.Kill(2, true);
        shakeParent.DOPunchRotation((Vector3.forward * swapRotationAngle) * dir, swapTransition, swapVibrato, 1).SetId(3);
    }

    private void BeginDrag(Card card)
    {
        if (scaleAnimations)
            transform.DOScale(scaleOnSelect, scaleTransition).SetEase(scaleEase);

        canvas.overrideSorting = true;
    }

    private void EndDrag(Card card)
    {
        canvas.overrideSorting = false;
        transform.DOScale(1, scaleTransition).SetEase(scaleEase);
    }

    private void PointerEnter(Card card)
    {
        if (scaleAnimations)
            transform.DOScale(scaleOnHover, scaleTransition).SetEase(scaleEase);

        DOTween.Kill(2, true);
        shakeParent.DOPunchRotation(Vector3.forward * hoverPunchAngle, hoverTransition, 20, 1).SetId(2);
    }

    private void PointerExit(Card card)
    {
        if (!parentCard.wasDragged)
            transform.DOScale(1, scaleTransition).SetEase(scaleEase);
    }

    private void PointerUp(Card card, bool longPress)
    {
        if (scaleAnimations)
            transform.DOScale(longPress ? scaleOnHover : scaleOnSelect, scaleTransition).SetEase(scaleEase);
        canvas.overrideSorting = false;

        visualShadow.localPosition = shadowDistance;
        shadowCanvas.overrideSorting = true;
    }

    private void PointerDown(Card card)
    {
        if (scaleAnimations)
            transform.DOScale(scaleOnSelect, scaleTransition).SetEase(scaleEase);

        visualShadow.localPosition += (-Vector3.up * shadowOffset);
        shadowCanvas.overrideSorting = false;
    }
    #endregion

}
