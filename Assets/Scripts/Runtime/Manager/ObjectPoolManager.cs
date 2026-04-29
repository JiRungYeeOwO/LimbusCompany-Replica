using System.Collections.Generic;
using UnityEngine;

public class ObjectPoolManager : MonoSingleton<ObjectPoolManager>
{
    private Dictionary<int, Queue<GameObject>> _poolDictionary = new Dictionary<int, Queue<GameObject>>();
    private Dictionary<int, GameObject> _prefabDictionary = new Dictionary<int, GameObject>();

    private Transform _poolRoot;

    protected override void Awake()
    {
        base.Awake();

        if (Instance != this) return;

        _poolRoot = new GameObject("Pool_Root").transform;
        _poolRoot.SetParent(transform);
    }

    /// <summary>
    /// 게임 시작 시 미리 오브젝트를 생성해두는 함수
    /// </summary>
    /// <param name="prefab"></param>
    /// <param name="poolSize"></param>
    public void CreatePool(GameObject prefab, int poolSize)
    {
        if (prefab == null) return;

        int poolKey = Animator.StringToHash(prefab.name);

        if (!_poolDictionary.ContainsKey(poolKey))
        {
            _poolDictionary.Add(poolKey, new Queue<GameObject>());
            _prefabDictionary.Add(poolKey, prefab);

            GameObject groupObj = new GameObject($"{prefab.name}_Pool");
            groupObj.transform.SetParent(_poolRoot);

            for (int i = 0; i < poolSize; i++)
            {
                GameObject obj = Instantiate(prefab, groupObj.transform);
                obj.SetActive(false);
                _poolDictionary[poolKey].Enqueue(obj);
            }
            CustomLogger.LogSystem($"[ObjectPool] '{prefab.name}' {poolSize}개 풀링 완료 (Key: {poolKey})");
        }
    }

    /// <summary>
    /// 풀에서 오브젝트를 소환하는 함수
    /// </summary>
    /// <param name="prefabName"></param>
    /// <param name="position"></param>
    /// <param name="rotation"></param>
    /// <returns></returns>
    public GameObject Spawn(string prefabName, Vector3 position, Quaternion rotation)
    {
        int poolKey = Animator.StringToHash(prefabName);

        if (!_poolDictionary.ContainsKey(poolKey))
        {
            CustomLogger.Error($"[ObjectPool] '{prefabName}' 풀이 존재하지 않습니다! CreatePool을 먼저 호출하세요.");
            return null;
        }

        GameObject obj = null;

        if (_poolDictionary[poolKey].Count == 0)
        {
            obj = Instantiate(_prefabDictionary[poolKey], _poolRoot);
            CustomLogger.Warn($"[ObjectPool] '{prefabName}' 풀 사이즈 초과, 추가로 생성합니다.");
        }
        else
        {
            obj = _poolDictionary[poolKey].Dequeue();
        }

        obj.transform.position = position;
        obj.transform.rotation = rotation;
        obj.SetActive(true);

        return obj;
    }

    /// <summary>
    /// 풀로 오브젝트를 반환하는 함수
    /// </summary>
    /// <param name="prefabName"></param>
    /// <param name="obj"></param>
    public void Despawn(string prefabName, GameObject obj)
    {
        int poolKey = Animator.StringToHash(prefabName);

        if (!_poolDictionary.ContainsKey(poolKey))
        {
            CustomLogger.Warn($"[ObjectPool] 등록되지 않은 객체 반납 시도. 파괴합니다: {prefabName}");
            Destroy(obj);
            return;
        }

        obj.SetActive(false);
        _poolDictionary[poolKey].Enqueue(obj);
    }
}
