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
    
    public void Use() // 이 함수는 Player에서 사용하도록 구현
    {
        if(type == Type.Melee) {
            StopCoroutine("Swing"); // 코루틴 정지 함수, 같은 coroutine 함수를 실행시키기 전 로직이 꼬이지 않도록 지금 동작하고 있는 coroutine을 정지시킬 필요가 있다
            StartCoroutine("Swing"); // 코루틴 실행 함수      
        }
    }

    // 일반 함수 : Use() 메인루틴 -> Swing() 서브루틴 -> Use() 메인루틴 : 교차실행
    // 코루틴 함수 : 메인루틴 + Swing() 코루틴 : 동시실행
    IEnumerator Swing() // 열거형 함수 클래스, invoke와 비교했을 때 호출자를 멈추게 하지않고 2함수가 같이 실행되도록 할 수 있다
    {
        // 1
        // yield return null; // 결과를 전달하는 키워드, 코루틴은 한개의 yield를 필요로 함 -> 1 프레임 대기
        // 2
        // yield return null; -> 1 프레임 대기, yield 키워드를 여러 개 사용하여 시간차 로직 작성 가능
        //yield break; // 코루틴 탈출 

        yield return new WaitForSeconds(0.1f); // 1 프레임이 아닌 주어진 수치만큼 기다리는 함수
        meleeArea.enabled = true; // SetActive랑 구분하기
        trailEffect.enabled = true;

        yield return new WaitForSeconds(0.3f); 
        meleeArea.enabled = false;

        yield return new WaitForSeconds(0.3f); 
        trailEffect.enabled = false;
        
    }
}
