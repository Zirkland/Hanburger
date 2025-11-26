
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

public class Card : MonoBehaviour,
    IDragHandler, IBeginDragHandler, IEndDragHandler,
    IPointerEnterHandler, IPointerExitHandler,
    IPointerUpHandler, IPointerDownHandler
{
    private Canvas canvas;
    private Image imageComponent;
    [SerializeField] private bool instantiateVisual = true;
    private VisualCardsHandler visualHandler;
    private Vector3 offset;

    [Header("Movement")]
    [SerializeField] private float moveSpeedLimit = 50;
   
    public event System.Action<Card> OnSelectedChanged;
    private bool _selected;
    public bool Selected
    {
        get => _selected;
        set
        {
            if (_selected != value)
            {
                _selected = value;
                OnSelectedChanged?.Invoke(this);
            }
        }
    }

    [Header("Selection")]
    public bool itcanbeSelect;
    public float selectionOffset = 50;
    private float pointerDownTime;
    private float pointerUpTime;

    [Header("Visual")]
    [SerializeField] private GameObject cardVisualPrefab;
    public CardVisual cardVisual;
    [Header("Card Type")]
    public AddCardType addCardType;

    [Header("States")]
    public bool isHovering;
    public bool isDragging;
    [HideInInspector] public bool wasDragged;

    [Header("Events")]
    [HideInInspector] public UnityEvent<Card> PointerEnterEvent;
    [HideInInspector] public UnityEvent<Card> PointerExitEvent;
    [HideInInspector] public UnityEvent<Card, bool> PointerUpEvent;
    [HideInInspector] public UnityEvent<Card> PointerDownEvent;
    [HideInInspector] public UnityEvent<Card> BeginDragEvent;
    [HideInInspector] public UnityEvent<Card> EndDragEvent;
    [HideInInspector] public UnityEvent<Card, bool> SelectEvent;

    
    void Start()
    {
        canvas = GetComponentInParent<Canvas>();
        imageComponent = GetComponent<Image>();
        if (!instantiateVisual) return;
        visualHandler = FindObjectOfType<VisualCardsHandler>();//���ܿ�����
        cardVisual = Instantiate(cardVisualPrefab, visualHandler ? visualHandler.transform : canvas.transform).GetComponent<CardVisual>();
        cardVisual.Initialize(this);
    }


    void Update()
    {
        ClampPosition();
        
        if (isDragging)
        {
            Vector2 targetPosition = Camera.main.ScreenToWorldPoint(GetInputPosition()) - offset;
            Vector2 direction = (targetPosition - (Vector2)transform.position).normalized;
            Vector2 velocity = direction * Mathf.Min(moveSpeedLimit, Vector2.Distance(transform.position, targetPosition) / Time.deltaTime);
            transform.Translate(velocity * Time.deltaTime);
        }
    }


    private Vector2 GetInputPosition()
    {
        // ����Ƿ��л�Ծ�Ĵ�������ָ��
        if (Touchscreen.current != null && Touchscreen.current.touches.Count > 0)
        {
            TouchControl touch = Touchscreen.current.touches[0];
            // ��ʽָ�� InputSystem �����ռ��µ� TouchPhase
            if (touch.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Began ||
                touch.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Moved ||
                touch.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Stationary)
            {
                // ���ص�һ����Ծ��ָ����Ļ����
                return touch.position.ReadValue();
            }
        }
        // û�д���ʱ�������������
        return Mouse.current.position.ReadValue();
    }



    void ClampPosition()
    {
        Vector2 screenBounds = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, Camera.main.transform.position.z));
        Vector3 clampedPosition = transform.position;
        clampedPosition.x = Mathf.Clamp(clampedPosition.x, -screenBounds.x, screenBounds.x);
        clampedPosition.y = Mathf.Clamp(clampedPosition.y, -screenBounds.y, screenBounds.y);
        transform.position = new Vector3(clampedPosition.x, clampedPosition.y, 0);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        BeginDragEvent.Invoke(this);
        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(GetInputPosition());//
        offset = mousePosition - (Vector2)transform.position;
        isDragging = true;
        canvas.GetComponent<GraphicRaycaster>().enabled = false;
        imageComponent.raycastTarget = false;
        wasDragged = true;
    }

    public void OnDrag(PointerEventData eventData)
    {

    }

    public void OnEndDrag(PointerEventData eventData)
    {
        EndDragEvent.Invoke(this);
        isDragging = false;
        canvas.GetComponent<GraphicRaycaster>().enabled = true;
        imageComponent.raycastTarget = true;

        StartCoroutine(FrameWait());

        IEnumerator FrameWait()
        {
            yield return new WaitForEndOfFrame();
            wasDragged = false;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        PointerEnterEvent.Invoke(this);
        isHovering = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        PointerExitEvent.Invoke(this);
        isHovering = false;
    }


    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
            return;

        PointerDownEvent.Invoke(this);
        pointerDownTime = Time.time;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left) return;
        pointerUpTime = Time.time;
        PointerUpEvent.Invoke(this, pointerUpTime - pointerDownTime > .2f);
        if (pointerUpTime - pointerDownTime > .2f) return;
        if (wasDragged) return;

        if (itcanbeSelect)
        {
            Selected = !Selected;
            SelectEvent.Invoke(this, Selected);
            if (Selected)
                transform.localPosition += (cardVisual.transform.up * selectionOffset);
            else
                transform.localPosition = Vector3.zero;
        }
        else
        {
            Deselect();
        }
    }

    public void Deselect()
    {
        if (Selected)
        {
            Selected = false;
            if (Selected)
                transform.localPosition += (cardVisual.transform.up * 50);
            else
                transform.localPosition = Vector3.zero;
        }
    }

    public int SiblingAmount()
    {
        if (!transform.parent.CompareTag("Slot")) return 0;
        Transform slotContainer = transform.parent.parent; // �������
        int activeSlotCount = 0;
        for (int i = 0; i < slotContainer.childCount; i++)
        {
            if (slotContainer.GetChild(i).gameObject.activeSelf)
                activeSlotCount++;
        }
        return activeSlotCount;
    }

    public int ParentIndex()
    {
        if (!transform.parent.CompareTag("Slot")) return 0;
        Transform container = transform.parent.parent;// �������
        int activeIndex = 0;
        for (int i = 0; i < container.childCount; i++)
        {
            if (container.GetChild(i).gameObject.activeSelf)
            {
                if (container.GetChild(i) == transform.parent)
                    return activeIndex; // �����ڼ������е�����
                activeIndex++;
            }
        }
        return 0; // ����ǰ��۱��������أ�����0
    }

    public float NormalizedPosition()
    {
        return transform.parent.CompareTag("Slot") ? ExtensionMethods.Remap((float)ParentIndex(), 0, (float)(SiblingAmount() - 1), 0, 1) : 0;
    }
}
