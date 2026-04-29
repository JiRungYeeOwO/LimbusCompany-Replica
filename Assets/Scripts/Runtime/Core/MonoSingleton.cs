using UnityEngine;

/// <summary>
/// 싱글톤 패턴 적용이 필요한 매니저 클래스에서 상속할 클래스
/// <para>DDOL 적용이 필요하면 IsDontDestroyOnLoad를 true로 전달</para>
/// </summary>
/// <typeparam name="T"></typeparam>
public class MonoSingleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T _instance;
    private static readonly object _lock = new object();
    private static bool _applicationIsQuitting = false;

    protected virtual bool IsDontDestroyOnLoad => true;

    public static T Instance
    {
        get
        {
            if (_applicationIsQuitting)
            {
                CustomLogger.Warn($"[Singleton] 게임 종료 중, '{typeof(T)}' 인스턴스를 반환하지 않습니다.");
                return null;
            }

            lock (_lock)
            {
                if (_instance == null)
                {
                    _instance = (T)FindObjectOfType(typeof(T));

                    if (FindObjectsOfType(typeof(T)).Length > 1)
                    {
                        CustomLogger.Error($"[Singleton] {typeof(T)} 싱글톤이 씬에 2개 이상 존재 중");
                        return _instance;
                    }

                    if (_instance == null)
                    {
                        GameObject singletonObject = new GameObject();
                        _instance = singletonObject.AddComponent<T>();
                        singletonObject.name = typeof(T).ToString() + " (Singleton)";

                        if ((_instance as MonoSingleton<T>).IsDontDestroyOnLoad)
                        {
                            DontDestroyOnLoad(singletonObject);
                            CustomLogger.LogSystem($"[Singleton] '{singletonObject.name}' 자동 생성 및 DontDestroyOnLoad 적용 완료.");
                        }
                        else
                        {
                            CustomLogger.LogSystem($"[Singleton] '{singletonObject.name}' 생성 완료 (씬 전용).");
                        }
                    }
                }

                return _instance;
            }
        }
    }

    protected virtual void Awake()
    {
        if (_instance != null && _instance != this)
        {
            CustomLogger.Warn($"[Singleton] 중복된 {typeof(T)} 객체를 파괴합니다: {gameObject.name}");
            Destroy(gameObject);
            return;
        }

        _instance = this as T;

        if (IsDontDestroyOnLoad)
        {
            DontDestroyOnLoad(gameObject);
        }
    }

    protected virtual void OnApplicationQuit()
    {
        _applicationIsQuitting = true;
    }

    protected virtual void OnDestroy()
    {
        if (_instance == this)
        {
            _instance = null;
            _applicationIsQuitting = true;
        }
    }
}
