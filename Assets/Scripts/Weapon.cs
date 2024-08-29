using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    public enum Type { Melee, Range }; // 무기의 타입 종류
    public Type type; // 무기의 실제 타입
    public int damage; // 무기의 데미지
    public float rate; // 무기의 공격 속도
    public BoxCollider meleeArea; // 무기의 공격 범위
    public TrailRenderer trailEffect; // 무기의 공격 효과;
    
}
