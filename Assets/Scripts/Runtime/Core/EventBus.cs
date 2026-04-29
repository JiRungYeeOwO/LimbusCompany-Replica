using System;

public static class EventBus<T> where T : IEvent
{
    private static event Action<T> OnEvent;

    /// <summary>
    /// 이벤트 구독
    /// </summary>
    /// <param name="handler"></param>
    public static void Subscribe(Action<T> handler)
    {
        OnEvent -= handler;
        OnEvent += handler;
    }

    /// <summary>
    /// 구독 해제
    /// </summary>
    /// <param name="handler"></param>
    public static void Unsubscribe(Action<T> handler)
    {
        OnEvent -= handler;
    }

    /// <summary>
    /// 이벤트 발행
    /// </summary>
    /// <param name="eventData"></param>
    public static void Publish(T eventData)
    {
        OnEvent?.Invoke(eventData);
    }

    /// <summary>
    /// 해당 타입의 이벤트 구독을 모두 초기화
    /// </summary>
    public static void Clear()
    {
        OnEvent = null;
    }
}
