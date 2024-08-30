using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public int damage;

    void OnCollisionEnter(Collision collision) // 총알이 맞았을 때의 효과도 있어야 하므로 Collision으로 구현
    {
        if (collision.gameObject.tag == "Floor") {
            Destroy(gameObject, 3); // 탄피가 땅과 충돌할 경우 3초뒤에 사라짐
        }
        else if (collision.gameObject.tag == "Wall") {
            Destroy(gameObject); // 총알이 벽과 충돌할 경우 바로 사라짐
        }
    }
}
