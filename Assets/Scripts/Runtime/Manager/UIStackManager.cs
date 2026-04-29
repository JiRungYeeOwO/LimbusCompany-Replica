using System.Collections.Generic;
using UnityEngine;

public class UIStackManager : MonoSingleton<UIStackManager>
{
    private Stack<GameObject> _uiStack = new Stack<GameObject>();

    public int StackCount => _uiStack.Count;

    protected override void Awake()
    {
        base.Awake();

        if (Instance != this) return;
    }

    private void OnEnable()
    {
        if (InputDispatcher.Instance != null)
        {
            InputDispatcher.Instance.OnUICancel += HandleCancelInput;
        }
    }

    private void OnDisable()
    {
        if (InputDispatcher.Instance != null)
        {
            InputDispatcher.Instance.OnUICancel -= HandleCancelInput;
        }
    }

    private void HandleCancelInput()
    {
        if (_uiStack.Count > 0)
        {
            Pop();
        }
        else
        {
            CustomLogger.LogSystem("[UIStack] 스택이 비어있음. 시스템 메뉴 이벤트를 발행합니다.");
            EventBus<RequestSystemMenuEvent>.Publish(new RequestSystemMenuEvent());
        }
    }

    public void Push(GameObject uiPanel)
    {
        if (uiPanel == null) return;

        uiPanel.SetActive(true);
        uiPanel.transform.SetAsLastSibling();

        _uiStack.Push(uiPanel);
        CustomLogger.LogSystem($"[UIStack] '{uiPanel.name}' 패널 열림 (현재 스택: {_uiStack.Count}개)");
    }

    public void Pop()
    {
        if (_uiStack.Count > 0)
        {
            GameObject topUI = _uiStack.Pop();
            topUI.SetActive(false);
            CustomLogger.LogSystem($"[UIStack] '{topUI.name}' 패널 닫힘 (남은 스택: {_uiStack.Count}개)");
        }
        else
        {
            CustomLogger.LogSystem("[UIStack] 닫을 UI가 없습니다. (게임 일시정지 창 호출 등 연동)");
        }
    }

    public void Clear()
    {
        while (_uiStack.Count > 0)
        {
            GameObject ui = _uiStack.Pop();
            if (ui != null) ui.SetActive(false);
        }
        CustomLogger.LogSystem("[UIStack] 모든 UI 스택 초기화 완료.");
    }
}
