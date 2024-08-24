using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour
{
    public enum Type { Ammo, Coin, Grenade, Heart, Weapon }; // enum = 열거형 타입 (타입 이름 지정 필요)
    public Type type; // 아이템 종류를 저장할 변수, Type에 열거된 것 중 타입을 지정할 수 있다
    public int value; // 아이템 값

    void Update()
    {
        transform.Rotate(Vector3.up * 20 * Time.deltaTime); // Rotate(회전축 * 회전속도 * fps 조절)
    }
}
