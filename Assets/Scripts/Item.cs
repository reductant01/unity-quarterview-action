using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour
{
    public enum Type { Ammo, Coin, Grenade, Heart, Weapon }; // enum = 열거형 타입 (타입 이름 지정 필요)
    public Type type; // 아이템 종류를 저장할 변수, Type에 열거된 것 중 타입을 지정할 수 있다
    public int value; // 아이템 값
    
    Rigidbody rigid;
    SphereCollider sphereCollider; // 작은 sphereCollider와 Player가 충돌하는 문제를 해결

    void Awake()
    {
        rigid = GetComponent<Rigidbody>();
        sphereCollider = GetComponent<SphereCollider>(); // 가장 위에 있는 SphereCollider를 가져온다
                                                        // 물리효과를 담당하는 SphereCollider가 가장 위에 있어야한다
    }

    void Update()
    {
        transform.Rotate(Vector3.up * 20 * Time.deltaTime); // Rotate(회전축 * 회전속도 * fps 조절)
    }

    void OnCollisionEnter(Collision collision) 
    {
        if (collision.gameObject.tag == "Floor") {
            rigid.isKinematic = true; // 물리엔진에 의한 움직임을 중지시킨다
            sphereCollider.enabled = false; // 오브젝트가 다른 오브젝트와 충돌하지 않도록 한다
        }
    }
}
