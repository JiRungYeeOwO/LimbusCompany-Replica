using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleUIManager : MonoSingleton<BattleUIManager>
{
    [Header("UI 연결")]
    [SerializeField] private GameObject _actionTimelinePanel;
    [SerializeField] private GameObject _skillIconPrefab;
    [SerializeField] private GameObject _characterColumnPrefab;
    [SerializeField] private GameObject _slotContainer;

    private Dictionary<string, Sprite> _frameSpriteCache = new Dictionary<string, Sprite>();
    private List<GameObject> _spawnedSlots = new List<GameObject>();

    private Dictionary<int, Sprite> _skillIconCache = new Dictionary<int, Sprite>();

    protected override void Awake()
    {
        base.Awake();
        PreloadFrameSprites();
    }

    public void ShowSkillSelectionUI(List<PlayerCharacter> players)
    {
        _actionTimelinePanel.SetActive(false);
        _slotContainer.SetActive(true);

        ClearExistingSlots();

        players.Sort((a, b) => b.Speed.CompareTo(a.Speed));

        foreach (PlayerCharacter player in players)
        {
            GameObject columnGO = Instantiate(_characterColumnPrefab, _slotContainer.transform);
            _spawnedSlots.Add(columnGO);

            List<SkillData> skills = player.GetSkillQueue(3);

            for (int i = skills.Count - 1; i >= 0; i--)
            {
                GameObject skillGO = Instantiate(_skillIconPrefab, columnGO.transform);

                SkillSlotUI slotUI = skillGO.GetComponent<SkillSlotUI>();

                if (slotUI != null)
                {
                    bool isNext = (i == 2);
                    slotUI.SetupSlot(player, skills[i], isNext, i);

                    AdjustSlotVisual(skillGO.GetComponent<RectTransform>(), i);
                }
            }
        }

        CustomLogger.LogSystem($"[UI] 스킬 선택 UI 활성화. 총 {_spawnedSlots.Count}개의 슬롯 표시 중.");
    }

    public void ShowActionTimelineUI(List<BattleCharacter> sortedParticipants)
    {
        _slotContainer.SetActive(false);
        _actionTimelinePanel.SetActive(true);

        CustomLogger.LogSystem("[UI] 액션 타임라인 패널 활성화.");
    }

    public void RefreshSkillUI()
    {
        List<PlayerCharacter> players = BattleManager.Instance.GetPlayerCharacters();
        ShowSkillSelectionUI(players);
    }

    public void OnClickStartClashButton()
    {
        gameObject.SetActive(false);
        BattleManager.Instance.ConfirmSkillSelection();
    }

    private void ClearExistingSlots()
    {
        foreach (var slot in _spawnedSlots)
        {
            if (slot != null)
            {
                Destroy(slot);
            }
        }
        _spawnedSlots.Clear();
    }

    private void AdjustSlotVisual(RectTransform rect, int index)
    {
        float yOffset = index * 120f;
        float scale = 1f - (index * 0.1f);

        rect.anchoredPosition += new Vector2(0, yOffset);
        rect.localScale = new Vector3(scale, scale, 1f);

        rect.SetAsLastSibling();
    }

    private void PreloadFrameSprites()
    {
        string[] atlasPaths =
        {
            "Image/Skills/Frames/Icon_skillFrame",
            "Image/Skills/Frames/Icon_skillFrame_5",
            "Image/Skills/Frames/Icon_skillFrame_6",
            "Image/Skills/Frames/Icon_skillFrame_7"
        };

        foreach (var item in atlasPaths)
        {
            Sprite[] loadedSprites = Resources.LoadAll<Sprite>(item);
            foreach (var sprite in loadedSprites)
            {
                if (!_frameSpriteCache.ContainsKey(sprite.name))
                {
                    _frameSpriteCache.Add(sprite.name, sprite);
                }
            }
        }

        CustomLogger.LogSystem($"[UI] 스킬 프레임 스프라이트 {_frameSpriteCache.Count}개 캐싱 완료.");
    }

    public Sprite GetFrameSprite(string spriteName)
    {
        if (_frameSpriteCache.TryGetValue(spriteName, out Sprite sprite))
        {
            return sprite;
        }

        CustomLogger.LogSystem($"[Warning] 캐시에서 {spriteName} 프레임을 찾을 수 없습니다.");
        return null;
    }

    public Sprite GetSkillIconSprite(int skillID)
    {
        if (_skillIconCache.TryGetValue(skillID, out Sprite cachedSprite))
        {
            return cachedSprite;
        }

        string iconPath = $"Image/Skills/Icon_{skillID}";
        Sprite newSprite = Resources.Load<Sprite>(iconPath);

        if (newSprite != null)
        {
            _skillIconCache.Add(skillID, newSprite);
            return newSprite;
        }

        CustomLogger.Warn($"[Warning] {skillID}번 아이콘 리소스를 찾을 수 없습니다.");
        return null;
    }
}
