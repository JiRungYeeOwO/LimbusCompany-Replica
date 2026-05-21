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

    [Header("오버헤드 UI 연결")]
    [SerializeField] private GameObject _overheadSkillUIPrefab;
    [SerializeField] private RectTransform _overheadContainer;

    [Header("스프라이트 캐싱 파일")]
    [Tooltip("Image/Skills/Frames/ 경로의 아틀라스")]
    [SerializeField] private Texture2D[] _frameAtlasFiles;
    [Tooltip("Image/Skills/SkillType/ 경로의 아틀라스")]
    [SerializeField] private Texture2D[] _skillTypeAtlasFiles;

    private const string FRAME_PATH_PREFIX = "Image/Skills/Frames/";
    private const string SKILL_TYPE_PATH_PREFIX = "Image/Skills/SkillType/";

    private Dictionary<string, Sprite> _uiSpriteCache = new Dictionary<string, Sprite>();
    private List<GameObject> _spawnedSlots = new List<GameObject>();

    private Dictionary<int, Sprite> _skillIconCache = new Dictionary<int, Sprite>();

    private Dictionary<BattleCharacter, OverheadSkillController> _overheadUIDictionary = new Dictionary<BattleCharacter, OverheadSkillController>();

    protected override void Awake()
    {
        base.Awake();
        PreloadAllUISprites();
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

    public void GenerateAllOverheadUIs(List<BattleCharacter> characters)
    {
        foreach (var kvp in _overheadUIDictionary)
        {
            if (kvp.Value != null)
            {
                Destroy(kvp.Value.gameObject);
            }
        }

        _overheadUIDictionary.Clear();

        foreach (BattleCharacter character in characters)
        {
            GameObject overheadGO = Instantiate(_overheadSkillUIPrefab, _overheadContainer);
            OverheadSkillController overheadSkillController = overheadGO.GetComponent<OverheadSkillController>();

            if (overheadSkillController != null)
            {
                int slotCount = (character is PlayerCharacter) ? 1 : 3;
                overheadSkillController.Initialize(character, slotCount);
                _overheadUIDictionary.Add(character, overheadSkillController);
            }
        }
    }

    public void RefreshSkillUI()
    {
        List<PlayerCharacter> players = BattleManager.Instance.GetPlayerCharacters();
        ShowSkillSelectionUI(players);
    }

    public void OnClickStartClashButton()
    {
        _slotContainer.SetActive(false);
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

    private void PreloadAllUISprites()
    {
        _uiSpriteCache.Clear();

        LoadAtlasGroup(_frameAtlasFiles, FRAME_PATH_PREFIX);

        LoadAtlasGroup(_skillTypeAtlasFiles, SKILL_TYPE_PATH_PREFIX);

        CustomLogger.LogSystem($"[UI] 총 {_uiSpriteCache.Count}개의 UI 스프라이트 캐싱 완료.");
    }

    private void LoadAtlasGroup(Texture2D[] files, string prefixPath)
    {
        if (files == null || files.Length == 0) return;

        foreach (var file in files)
        {
            if (file == null) continue;

            string fullPath = prefixPath + file.name;
            Sprite[] loadedSprites = Resources.LoadAll<Sprite>(fullPath);

            if (loadedSprites == null || loadedSprites.Length == 0)
            {
                CustomLogger.Warn($"[UI] 경로에서 스프라이트를 찾을 수 없습니다: {fullPath}");
                continue;
            }

            foreach (var sprite in loadedSprites)
            {
                if (!_uiSpriteCache.ContainsKey(sprite.name))
                {
                    _uiSpriteCache.Add(sprite.name, sprite);
                }
            }
        }
    }

    public Sprite GetFrameSprite(string spriteName)
    {
        if (_uiSpriteCache.TryGetValue(spriteName, out Sprite sprite))
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

    public void UpdateCharacterOverheadSlot(BattleCharacter character, int slotIndex, SkillData skill)
    {
        CustomLogger.LogSystem("[UpdateCharacterOverheadSlot] 호출");

        if (_overheadUIDictionary.TryGetValue(character, out OverheadSkillController controller))
        {
            controller.SetSkill(slotIndex, skill);
        }
        else
        {
            CustomLogger.Warn($"[UI] {character.name}의 오버헤드 UI를 찾을 수 없습니다.");
        }
    }
}
