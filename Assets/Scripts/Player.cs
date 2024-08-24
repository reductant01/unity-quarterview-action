using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class Player : MonoBehaviour
{
    public float speed; // 인스펙터 창에서 수정가능 
    public float jumpPower; // 점프높이 조정
    float hAxis;
    float vAxis;
    float originalSpeed; // 원래 속도를 저장할 변수
    int wallCollisionCount = 0; // 벽과의 충돌 수를 추적

    bool wDown; // 걸을 때를 표현하기 위한 함수
    bool jDown; // 점프하는 순간을 나타내기 위한 함수
    bool iDown; // Weapon 장착을 위한 함수
    bool isJump; // 점프하는 중인지를 나타내기 위한 함수
    bool isDodge;

    Vector3 moveVec;
    Vector3 dodgeVec;

    Rigidbody rigid; // rigid 효과를 부르기 위한 변수 생성
    Animator anim;

    GameObject nearObject; // 트리거된 아이템을 저장하기 위한 변수

    // Start is called before the first frame update
    void Awake()
    {
        rigid = GetComponent<Rigidbody>(); // Rigidbody를 불러와줌
        anim = GetComponentInChildren<Animator>(); // Animator는 player의 자식 Component가 가지고 있기 때문에 GetComponentInChildren를 써야한다
    }

    void Start()
    {
        originalSpeed = speed; // 초기 속도 저장
    }

    // Update is called once per frame
    void Update()
    {
        GetInput();
        Move();
        Turn();
        Jump();
        Dodge();
    }

    void GetInput()
    {
        hAxis = Input.GetAxisRaw("Horizontal"); // GetAxisRaw() = Axis값을 정수로 반환하는 함수, Horizontal = left/ right, Unity의 Input Manager에서 수정가능
        vAxis = Input.GetAxisRaw("Vertical"); // Vertical = up/ down
        wDown = Input.GetButton("Walk"); // 방향값이 아니라 shift버튼만 누르는 것이므로 GetButton으로 받아야함 
        jDown = Input.GetButtonDown("Jump"); // 버튼을 누른 그 순간만 읽는 것이므로 GetButtonDown을 쓴다
        iDown = Input.GetButtonDown("Interation");
    }

    void Move()
    {
        moveVec = new Vector3(hAxis, 0, vAxis).normalized;
        // normalized로 속력을 1로 보정할 필요있음 (예를 들어 Horizontal과 Vertical이 둘다 1으로 입력되었을 경우에 대각선으로 루트2만큼의 속력으로 좀 더 빠르게 이동함)

        // if(wDown)    
        //     transform.position += moveVec * speed * 0.3f * Time.deltaTime; // 걸을때에는 속도가 줄도록 설정
        // else    
        //     transform.position += moveVec * speed * Time.deltaTime; // fps의 영향을 받지 않도록 반드시 Time.deltaTime 추가!!
        if (isDodge)
            moveVec = dodgeVec; // 회피중에는 moveVec를 변경할 수 없도록 함

        transform.position += moveVec * speed * (wDown ? 0.5f : 1f) * Time.deltaTime; // 삼항연산자 = (bool ? true일때의 값 : false일때의값)

        anim.SetBool("isRun", moveVec != Vector3.zero); // Animation을 만들때 bool 함수로 만들었음, moveVec값이 0인지 아닌지를 isRun이름의 bool함수로 저장
        anim.SetBool("isWalk", wDown); // wDown인지를 판단하여 isWalk 함수를 만들어라
    }

    void Turn()
    {
        transform.LookAt(transform.position + moveVec); // 회전을 구현하기 위한 코드이다
        // 예를 들어 moveVec가 (1, 0, 0)일때 transform.LookAt(moveVec)만 입력한다면 아무리 움직여도 거의 중앙지점만 보게 된다. 
        // transform.LookAt(transform.position)를 입력하면 플레이어의 위치를 바라보게 되어 회전하지 않는다 
        // 따라서 transform.LookAt(transform.position + moveVec)를 입력하여 플레이어의 위치에서 (1, 0, 0)만큼 떨어진 지점을 보게 되어 가려는 방향을 바라보게 하는 것처럼 구현한다
    }

    void Jump()
    {
        if (jDown && moveVec == Vector3.zero && !isJump && !isDodge)
        { // jDown이 눌러졌고 움직이는 방향값이 0이며 isJump가 flase일때만 실행
            rigid.AddForce(Vector3.up * jumpPower, ForceMode.Impulse);
            anim.SetBool("isJump", true); // 애니메이션의 Bool함수 설정
            anim.SetTrigger("doJump");
            isJump = true;
        }
    }

    void Dodge()
    {
        if (jDown && moveVec != Vector3.zero && !isJump && !isDodge)
        {
            dodgeVec = moveVec;
            speed *= 2;
            anim.SetTrigger("doDodge");
            isDodge = true;

            Invoke("DodgeOut", 0.5f);
        }
    }

    void DodgeOut()
    {
        speed *= 0.5f;
        isDodge = false;
    }

    void OnCollisionEnter(Collision collision) // 매개변수를 통하여 충돌정보를 얻음
    {
        if (collision.gameObject.tag == "Floor")
        { // 충돌한 게임 오브젝트의 태그가 바닥일때 실행
            anim.SetBool("isJump", false);
            isJump = false;
        }

        if (collision.gameObject.tag == "Wall")
        {
            wallCollisionCount++; // 벽과 충돌할 때마다 카운터 증가
            if (wallCollisionCount == 1) // 첫 충돌시에만 속도를 0으로 설정
            {
                speed = 0;
            }
        }
    }

    void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.tag == "Wall")
        {
            wallCollisionCount--; // 벽과의 충돌이 끝날 때마다 카운터 감소
            if (wallCollisionCount == 0) // 마지막 벽과의 충돌이 끝났을 때만 속도 복구
            {
                speed = originalSpeed;
            }
        }
    }

    void OnTriggerStay(Collider other) // Trigger했을때 = 맞닿다
    {
        if(other.tag == "Weapon") // Collider.tag = Collider의 tag, Collider.GameObject.tag = Collider가 가진 GameObject의 tag
            nearObject = other.gameObject; 
        
    }

    void OnTriggerExit(Collider other)
    {
        if(other.tag == "Weapon") 
            nearObject = null; 
    }
    
}
