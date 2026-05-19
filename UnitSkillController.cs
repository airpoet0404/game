using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 키보드 스킬 입력 처리
/// A: 딜러 스킬 - 플레이어 위치 집결 + 집중 공격
/// S: 힐러 스킬 - 범위 내 아군+플레이어 즉시 회복
/// D: 탱커 스킬 - 가장 가까운 적 스킬 봉쇄
/// F: 전체 스킬 - 3초간 아군 전체 무적
/// </summary>
public class UnitSkillController : MonoBehaviour
{
    [Header("Skill Cooldowns (seconds)")]
    [SerializeField] private float dealerSkillCooldown = 8f;
    [SerializeField] private float healerSkillCooldown = 6f;
    [SerializeField] private float tankerSkillCooldown = 10f;
    [SerializeField] private float invincibleSkillCooldown = 15f;
    [SerializeField] private float invincibleDuration = 3f;

    private float dealerCooldown;
    private float healerCooldown;
    private float tankerCooldown;
    private float invincibleCooldown;

    private List<RangedDealerUnit> rangedDealers = new();
    private List<MeleeDealerUnit> meleeDealers = new();
    private List<HealerUnit> healers = new();
    private List<TankerUnit> tankers = new();
    private List<UnitBase> allFriendlyUnits = new();

    private void Start()
    {
        RefreshUnitLists();
    }

    private void Update()
    {
        dealerCooldown -= Time.deltaTime;
        healerCooldown -= Time.deltaTime;
        tankerCooldown -= Time.deltaTime;
        invincibleCooldown -= Time.deltaTime;

        HandleSkillInput();
    }

    private void HandleSkillInput()
    {
        if (Input.GetKeyDown(KeyCode.A) && dealerCooldown <= 0f)
        {
            ActivateDealerSkill();
            dealerCooldown = dealerSkillCooldown;
        }

        if (Input.GetKeyDown(KeyCode.S) && healerCooldown <= 0f)
        {
            ActivateHealerSkill();
            healerCooldown = healerSkillCooldown;
        }

        if (Input.GetKeyDown(KeyCode.D) && tankerCooldown <= 0f)
        {
            ActivateTankerSkill();
            tankerCooldown = tankerSkillCooldown;
        }

        if (Input.GetKeyDown(KeyCode.F) && invincibleCooldown <= 0f)
        {
            ActivateInvincibleSkill();
            invincibleCooldown = invincibleSkillCooldown;
        }
    }

    // A키: 모든 딜러 유닛 스킬 발동
    private void ActivateDealerSkill()
    {
        int count = 0;
        foreach (var d in rangedDealers) if (!d.IsDead) { d.TryUseSkill(); count++; }
        foreach (var d in meleeDealers) if (!d.IsDead) { d.TryUseSkill(); count++; }
        Debug.Log($"[SkillController] A: 딜러 스킬 ({count}명 발동)");
    }

    // S키: 모든 힐러 유닛 스킬 발동
    private void ActivateHealerSkill()
    {
        int count = 0;
        foreach (var h in healers) if (!h.IsDead) { h.TryUseSkill(); count++; }
        Debug.Log($"[SkillController] S: 힐러 스킬 ({count}명 발동)");
    }

    // D키: 모든 탱커 유닛 스킬 발동
    private void ActivateTankerSkill()
    {
        int count = 0;
        foreach (var t in tankers) if (!t.IsDead) { t.TryUseSkill(); count++; }
        Debug.Log($"[SkillController] D: 탱커 스킬 ({count}명 발동)");
    }

    // F키: 생존한 모든 아군 유닛 무적 적용
    private void ActivateInvincibleSkill()
    {
        RefreshUnitLists();
        int count = 0;
        foreach (var unit in allFriendlyUnits)
        {
            if (!unit.IsDead) { unit.ApplyInvincible(invincibleDuration); count++; }
        }
        Debug.Log($"[SkillController] F: 전체 무적 {invincibleDuration}초 ({count}명)");
    }

    // 씬 내 유닛 목록 갱신 - 유닛 생성/사망 후 호출
    public void RefreshUnitLists()
    {
        rangedDealers = new List<RangedDealerUnit>(FindObjectsOfType<RangedDealerUnit>());
        meleeDealers = new List<MeleeDealerUnit>(FindObjectsOfType<MeleeDealerUnit>());
        healers = new List<HealerUnit>(FindObjectsOfType<HealerUnit>());
        tankers = new List<TankerUnit>(FindObjectsOfType<TankerUnit>());

        allFriendlyUnits.Clear();
        allFriendlyUnits.AddRange(rangedDealers);
        allFriendlyUnits.AddRange(meleeDealers);
        allFriendlyUnits.AddRange(healers);
        allFriendlyUnits.AddRange(tankers);
    }

    // UI에서 쿨다운 비율 참조용 (0=사용 가능, 1=쿨다운 중)
    public float DealerCooldownRatio => Mathf.Clamp01(dealerCooldown / dealerSkillCooldown);
    public float HealerCooldownRatio => Mathf.Clamp01(healerCooldown / healerSkillCooldown);
    public float TankerCooldownRatio => Mathf.Clamp01(tankerCooldown / tankerSkillCooldown);
    public float InvincibleCooldownRatio => Mathf.Clamp01(invincibleCooldown / invincibleSkillCooldown);
}
