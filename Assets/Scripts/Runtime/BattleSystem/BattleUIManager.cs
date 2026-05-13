using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleUIManager : MonoSingleton<BattleUIManager>
{
    [Header("UI 연결")]
    [SerializeField] private GameObject _skillSelectionPanel;
    [SerializeField] private GameObject _actionTimelinePanel;
    [SerializeField] private GameObject _skillIconPrefab;
    [SerializeField] private Transform _slotContainer;

    private PlayerCharacter _currentlySelectingPlayer;
    private SkillData _selectedSkillInfo;

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
        _skillSelectionPanel.SetActive(true);

        ClearExistingSlots();

        foreach (PlayerCharacter player in players)
        {
            List<SkillData> skills = player.GetSkillList();

            foreach (SkillData skill in skills)
            {
                GameObject slotGO = Instantiate(_skillIconPrefab, _slotContainer);
                _spawnedSlots.Add(slotGO);

                SkillSlotUI slotUI = slotGO.GetComponent<SkillSlotUI>();
                if (slotUI != null)
                {
                    slotUI.SetupSlot(player, skill);
                }
            }
        }

        CustomLogger.LogSystem($"[UI] 스킬 선택 UI 활성화. 총 {_spawnedSlots.Count}개의 스킬 표시 중.");
    }

    public void ShowActionTimelineUI(List<BattleCharacter> sortedParticipants)
    {
        _skillSelectionPanel.SetActive(false);
        _actionTimelinePanel.SetActive(true);

        CustomLogger.LogSystem("[UI] 액션 타임라인 패널 활성화.");
    }

    public void OnSkillSelected(PlayerCharacter owner, SkillData skill)
    {
        _currentlySelectingPlayer = owner;
        _selectedSkillInfo = skill;
        CustomLogger.LogSystem($"[UI] {_currentlySelectingPlayer.gameObject.name}의 '{skill.SkillName}' 스킬 선택됨. 타겟을 지정해주세요.");
    }

    public void OnTargetSelected(EnemyCharacter target)
    {
        if (_currentlySelectingPlayer == null || _selectedSkillInfo == null) return;

        _currentlySelectingPlayer.SetSelectedSkill(_selectedSkillInfo);
        _currentlySelectingPlayer.SetTarget(target);

        CustomLogger.LogSystem($"[UI] {_currentlySelectingPlayer.gameObject.name} -> {target.gameObject.name} 타겟팅 완료!");

        _currentlySelectingPlayer = null;
        _selectedSkillInfo = null;
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
