using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Orbit : MonoBehaviour
{
    public Transform target;
    public float orbitSpeed;
    Vector3 offSet;
    
    // Start is called before the first frame update
    void Start()
    {
        offSet = transform.position - target.position;
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = target.position + offSet; // transform의 위치를 offSet을 이용해서 계속 변화시킴
        transform.RotateAround(target.position, Vector3.up, orbitSpeed * Time.deltaTime); // 대상의 주위를 회전, transform.Rotate 와 구분
        // (회전 중심, 회전축, 회전 속도), RotateAround는 목표가 움직이면 일그러지는 단점이 있음음 
        offSet = transform.position - target.position; // RotateAround 후의 위치가 변할 가능성이 있으므로 목표와의 거리를 유지시킬 필요가 있다. 
    }
}
