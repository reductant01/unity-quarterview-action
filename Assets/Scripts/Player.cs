using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Player : MonoBehaviour
{
    public float speed; // 인스펙터 창에서 수정가능 
    float hAxis;
    float vAxis;
    bool wDown; // 걸을 때를 표현하기 위한 함수

    Vector3 moveVec;

    Animator anim;

    // Start is called before the first frame update
    void Awake()
    {
        anim = GetComponentInChildren<Animator>(); // Animator는 player의 자식 Component가 가지고 있기 때문에 GetComponentInChildren를 써야한다
    }

    // Update is called once per frame
    void Update()
    {
        hAxis = Input.GetAxisRaw("Horizontal"); // GetAxisRaw() = Axis값을 정수로 반환하는 함수, Horizontal = left/ right, Unity의 Input Manager에서 수정가능
        vAxis = Input.GetAxisRaw("Vertical"); // Vertical = up/ down
        wDown = Input.GetButton("Walk"); // 방향값이 아니라 shift버튼만 누르는 것이므로 GetButton으로 받아야함 
        
        moveVec = new Vector3(hAxis, 0, vAxis).normalized; 
        // normalized로 속력을 1로 보정할 필요있음 (예를 들어 Horizontal과 Vertical이 둘다 1으로 입력되었을 경우에 대각선으로 루트2만큼의 속력으로 좀 더 빠르게 이동함)
        
        // if(wDown)    
        //     transform.position += moveVec * speed * 0.3f * Time.deltaTime; // 걸을때에는 속도가 줄도록 설정
        // else    
        //     transform.position += moveVec * speed * Time.deltaTime; // fps의 영향을 받지 않도록 반드시 Time.deltaTime 추가!!
        transform.position += moveVec * speed * (wDown ? 0.5f : 1f) * Time.deltaTime; // 삼항연산자 = (bool ? true일때의 값 : false일때의값)

        anim.SetBool("isRun", moveVec != Vector3.zero); // Animation을 만들때 bool 함수로 만들었음, moveVec값이 0인지 아닌지를 isRun이름의 bool함수로 저장
        anim.SetBool("isWalk", wDown); // wDown인지를 판단하여 isWalk 함수를 만들어라

        transform.LookAt(transform.position + moveVec); // 회전을 구현하기 위한 코드이다
        // 예를 들어 moveVec가 (1, 0, 0)일때 transform.LookAt(moveVec)만 입력한다면 아무리 움직여도 거의 중앙지점만 보게 된다. 
        // transform.LookAt(transform.position)를 입력하면 플레이어의 위치를 바라보게 되어 회전하지 않는다 
        // 따라서 transform.LookAt(transform.position + moveVec)를 입력하여 플레이어의 위치에서 (1, 0, 0)만큼 떨어진 지점을 보게 되어 가려는 방향을 바라보게 하는 것처럼 구현한다
    }
}
