using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class Player : MonoBehaviour
{
    public float speed; // 인스펙터 창에서 수정가능 
    public float jumpPower; // 점프높이 조정
    public GameObject[] weapons; // 무기를 입수하면 해당무기의 모습을 나타내도록 하는 코드를 구현하기 위한 배열 변수
    public bool[] hasWeapons; // 가지고 있는 무기를 구분하기 위한 배열 변수 
    public GameObject[] grenades; // 공전하는 물체를 만들기 위한 배열 변수
    public int hasGrenades;

    public int ammo;
    public int coin;
    public int health;

    public int maxAmmo;
    public int maxCoin;
    public int maxHealth;
    public int maxHasGrenades;

    float hAxis;
    float vAxis;
    float originalSpeed; // 원래 속도를 저장할 변수
    int wallCollisionCount = 0; // 벽과의 충돌 수를 추적

    bool wDown; // 걸을 때를 표현하기 위한 변수
    bool jDown; // 점프하는 순간을 나타내기 위한 변수
    bool fDown; // 공격하는 순간을 나타내기 위한 변수
    bool iDown; // Weapon 장착을 위한 변수
    bool sDown1; // 장비 1
    bool sDown2; // 장비 2
    bool sDown3; // 장비 3

    bool isJump; // 점프하는 중인지를 나타내기 위한 변수
    bool isDodge; // 대쉬 중인지를 나타내기 위한 변수
    bool isSwap; // 교체시간동안은 아무것도 못하도록 하는 코드를 위한 변수
    bool isFireReady = true; // 공격할 준비가 되었다는 걸 나타내는 변수

    Vector3 moveVec;
    Vector3 dodgeVec;

    Rigidbody rigid; // rigid 효과를 부르기 위한 변수 생성
    Animator anim;

    GameObject nearObject; // 트리거된 아이템을 저장하기 위한 변수
    Weapon equipWeapon; // 장착중인 무기를 구분하기 위한 변수, 타입을 GameObject에서 Weapon으로 변경함(무엇이 변경되는지 확인할 것)
    int equipWeaponIndex = -1; // 장착중인 무기로 다시 스왑을 하려고 하는 경우를 구분하기 위한 변수, 초기값을 -1로 주어주기 않고 그냥 0으로 둘 경우 Swap에서 오류발생
    float fireDelay; // 공격 딜레이를 위한 변수

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
        Attack();
        Dodge();
        Swap();
        Interation();
    }

    void GetInput()
    {
        hAxis = Input.GetAxisRaw("Horizontal"); // GetAxisRaw() = Axis값을 정수로 반환하는 함수, Horizontal = left/ right, Unity의 Input Manager에서 수정가능
        vAxis = Input.GetAxisRaw("Vertical"); // Vertical = up/ down
        wDown = Input.GetButton("Walk"); // 방향값이 아니라 shift버튼만 누르는 것이므로 GetButton으로 받아야함 
        jDown = Input.GetButtonDown("Jump"); // 버튼을 누른 그 순간만 읽는 것이므로 GetButtonDown을 쓴다
        fDown = Input.GetButtonDown("Fire1"); // 마우스 왼쪽을 누르면 공격
        iDown = Input.GetButtonDown("Interation"); // 장비 장착을 구현하는 함수
        sDown1 = Input.GetButtonDown("Swap1"); // 1 무기로 변경 
        sDown2 = Input.GetButtonDown("Swap2"); // 2 무기로 변경 
        sDown3 = Input.GetButtonDown("Swap3"); // 3 무기로 변경 
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

        if(isSwap || !isFireReady) // isFireReady사 false일 경우 움직일 수 없기에 초기값을 true로 주어준다
            moveVec = Vector3.zero;

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
        if (jDown && moveVec == Vector3.zero && !isJump && !isDodge && !isSwap)
        { // jDown이 눌러졌고 움직이는 방향값이 0이며 isJump가 flase일때만 실행
            rigid.AddForce(Vector3.up * jumpPower, ForceMode.Impulse);
            anim.SetBool("isJump", true); // 애니메이션의 Bool함수 설정
            anim.SetTrigger("doJump");
            isJump = true;
        }
    }

    void Attack() 
    {
        if (equipWeapon == null) 
            return;

        fireDelay += Time.deltaTime; // 공격딜레이에 시간을 더해주고 공격가능 여부를 확인, update마다 fireDelay에 시간이 추가됨    
        isFireReady = equipWeapon.rate < fireDelay; // 공격딜레이가 공격속도보다 큰지를 확인하고 isFireReady에 집어넣음

        if(fDown && isFireReady && !isDodge && !isSwap) {
            equipWeapon.Use(); // Weapon 스크립트의 Use 함수 실행
            anim.SetTrigger("doSwing");
            fireDelay = 0; // 공격딜레이를 0으로 돌려서 다음 공격까지 기다리도록 함
        } 
    }

    void Dodge()
    {
        if (jDown && moveVec != Vector3.zero && !isJump && !isDodge && !isSwap)
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

    void Swap() 
    {
        if(sDown1 && (!hasWeapons[0] || equipWeaponIndex == 0)) // 괄호 주의, or 조건이 어디에 적용되는지를 잘 살펴봐야함
            return; // 아직 무기를 획득하지 못했거나 기존에 장착하고 있던 무기에 Swap 호출시 Swap이 실행되지 않도록 한다
        if(sDown2 && (!hasWeapons[1] || equipWeaponIndex == 1)) // 괄호 주의, or 조건이 어디에 적용되는지를 잘 살펴봐야함
            return;
        if(sDown3 && (!hasWeapons[2] || equipWeaponIndex == 2)) // 괄호 주의, or 조건이 어디에 적용되는지를 잘 살펴봐야함
            return;        

        int weaponIndex = -1;
        if (sDown1) weaponIndex = 0;
        if (sDown2) weaponIndex = 1;
        if (sDown3) weaponIndex = 2;

        if((sDown1 || sDown2 || sDown3) && !isJump && !isDodge) {
            if(equipWeapon != null) // equipWeapon이 -1일때 SetActive(false)를 실행하면 오류발생
                equipWeapon.gameObject.SetActive(false); // 장착중인 무기를 교환하기 위하여 새로운 변수 equipWeapon이라는 새로운 변수를 사용함
                // Swap호출시 기존에 장착하고 있던 무기를 안보이도록 한다

            equipWeaponIndex = weaponIndex; // 장착중인 무기에 스왑을 요청하는 경우를 나타내기 위한 코드
            equipWeapon = weapons[weaponIndex].GetComponent<Weapon>();
            equipWeapon.gameObject.SetActive(true); // Swap시 버튼마다 비활성화되어있는 무기들을 보이도록함

            anim.SetTrigger("doSwap"); // doSwap 애니메이션 실행

            isSwap = true; // isSwap = true인 동안에는 어떤 행동도 할 수 없음

            Invoke("SwapOut", 0.4f); // 행동하기 위해서는 0.4초 후 SwapOut이 호출되어야 함
        }
    }

    void SwapOut()
    {
        isSwap = false;
    }

    void Interation()
    {
        if(iDown && nearObject != null && !isJump && !isDodge) { 
            if(nearObject.tag == "Weapon") {
                Item item = nearObject.GetComponent<Item>(); // neatObject의 컴포넌트를 Item 변수에 넣음
                int weaponIndex = item.value; // 무기의 value값을 저장하기 위한 변수
                hasWeapons[weaponIndex] = true; // 무기마다 다른 value 값으로 어떤 무기를 입수헀는지를 구분함

                Destroy(nearObject);
            }
        }
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

   void OnTriggerEnter(Collider other)
    { // 
        if(other.tag == "Item") {
            Item item = other.GetComponent<Item>(); // other가 가지고 있는 Item 스크립트를 가지고 옴
            switch (item.type) {
                case Item.Type.Ammo:
                    ammo += item.value;
                    if (ammo > maxAmmo)
                        ammo = maxAmmo; 
                    break;
                case Item.Type.Coin:
                    coin += item.value;
                    if (coin > maxCoin)
                        coin = maxCoin;
                    break;
                case Item.Type.Heart:
                    health += item.value;
                    if (health > maxHealth)
                        health = maxHealth;
                    break;
                case Item.Type.Grenade:
                    grenades[hasGrenades].SetActive(true); // 먹은 수류탄의 갯수에 해당하는 객체를 보이도록 설정
                    hasGrenades += item.value;
                    if (hasGrenades > maxHasGrenades)
                        hasGrenades = maxHasGrenades;
                    break;
            }
            Destroy(other.gameObject);
        }     
    }
    
    void OnTriggerStay(Collider other) // Collision = 물리적 충돌 발생, Trigger = 단순 감지, On Stay = 맞닿아있을때
    {
        if(other.tag == "Weapon") // Collider.tag = Collider의 tag, Collider.GameObject.tag = Collider가 가진 GameObject의 tag
            nearObject = other.gameObject; 
        
    }

    void OnTriggerExit(Collider other) // On Exit = 떨어졌을때
    {
        if(other.tag == "Weapon") 
            nearObject = null; 
    }
    
}
