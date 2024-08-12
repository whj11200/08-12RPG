using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerRPG : MonoBehaviour
{
    public enum PlalyerState {Ible = 0, ATTack,UNDER_ATTACK,DEAD}
    public PlalyerState playerstate = PlalyerState.Ible;
    [Tooltip("걷기 속도")] public float walkspeed = 5f;
    [Tooltip("달리기 속도")] public float runspeed = 5f;
    [Header("Camer 관련 변수")]
    [SerializeField] private Transform camerTr; // 카메라 위치
    [SerializeField] private Transform cameraPivotTr; //  카메라 피벗 위치
    [SerializeField] private float camerDistance = 0f; // 카메라와의 거리
    [SerializeField] private Vector3 mouseMove = Vector3.zero; // 마우스 이동 좌표
    [SerializeField] private int playerLayer; // 플레이어 레이어
    [Header("플레이어 move 관련변수")]
    [SerializeField] private Transform modleTr;
    [SerializeField] private Animator animator;
    [SerializeField] private CharacterController characterController;
    [SerializeField] private Vector3 moveVelocity = Vector3.zero; // 움직임 방향
    private bool isGrounded = false;
    private bool isRun = false;

    private readonly int hashAttack = Animator.StringToHash("Sword-AttackTrigger");
    private readonly int hashSpeedX = Animator.StringToHash("SpeedX");
    private readonly int hashSPeedY = Animator.StringToHash("SpeedY");
    private readonly int hashSheld = Animator.StringToHash("Shield-Attack");
    void Start()
    {
        camerTr = Camera.main.transform;
        cameraPivotTr = Camera.main.transform.parent;
        playerLayer = LayerMask.NameToLayer("Player");
        modleTr = GetComponentsInChildren<Transform>()[1];
        animator = transform.GetChild(0).GetComponent<Animator>();
        characterController = GetComponent<CharacterController>();
        camerDistance = 5f; // 카메라의 거리 
    }
    public bool IsRun
    {
        get { return isRun; }
        set { 
            isRun = value;
            animator.SetBool("isRun", value); 
            } 
    }

  
    void Update()
    {
        PlayerAttack();
        FreezeXz();
      
        switch (playerstate)
        {
            case PlalyerState.Ible:
                PlayerIdleAAndMove();
                break;
            case PlalyerState.ATTack:
                AttackTime();
                break;
            case PlalyerState.DEAD:
                break;
            case PlalyerState.UNDER_ATTACK:
                break;
        }
        CameraDistanceCtrl();
    }
    private void LateUpdate()
    {
        float cameraHeight = 1.3f;
        // 카메라의 높낮이 조절
        cameraPivotTr.position = transform.position+(Vector3.up * cameraHeight);
                    // 마우스는 상하로 카메라는 x축 회전
        mouseMove += new Vector3(-Input.GetAxis("Mouse Y") * 100f*0.1f, Input.GetAxisRaw("Mouse X")*100f*0.1f);
        // 마우스는 좌우로 카메라는 y축회전
        cameraPivotTr.eulerAngles = mouseMove; 

        if(mouseMove.x < -40f)
        {
            mouseMove.x = 40f;
        }
        else if (mouseMove.x > 40f)
        {
            mouseMove.x = 40f;
        }

        cameraPivotTr.eulerAngles = mouseMove;

        RaycastHit hit;
        
               //     실제카메라 위치에서 카메라 피벗 위치를 빼서 방향을 구한다. 
        Vector3 dir = (camerTr.position - cameraPivotTr.position).normalized;
        Ray ray = new Ray(camerTr.position, dir);
        Debug.DrawRay(ray.origin, ray.direction * 30f, Color.red);
        if (Physics.Raycast(cameraPivotTr.position,dir,out hit,camerDistance,~(1<<playerLayer)))
        {
            
            camerTr.localPosition = Vector3.back * hit.distance;
        }
        else
        {
            camerTr.localPosition = Vector3.back * camerDistance;
        }

       
    }
    void PlayerIdleAAndMove()
    {
        RunCheak();
        if (characterController.isGrounded)
        {
            if (!isGrounded)
            {
                isGrounded = true;
                animator.SetBool("IsGrounded", true);
            }
            CalcInputMove(); // 움직임 계산
            RaycastHit groundHit;
            if (GroundCheack(out groundHit))
                moveVelocity.y = IsRun ? -runspeed : -walkspeed;
            else
                moveVelocity.y = -1f;
            PlayerAttack();
            SheildAttack();

        }
        else
        {
            if (isGrounded)
            {
                isGrounded = false; // 땅에 착지하지 않을 때 isGrounded를 false로 설정
                animator.SetBool("IsGrounded", false); // 이곳에서 애니메이션 상태 업데이트
            }
            moveVelocity += Physics.gravity * Time.deltaTime;
        }
        characterController.Move(moveVelocity * Time.deltaTime);
    }


    void RunCheak()
    {
        if (IsRun == false && Input.GetKey(KeyCode.LeftShift))
        {
            IsRun = true;
        }
        else if (IsRun == true && Input.GetAxis("Horizontal") == 0 && Input.GetAxis("Vertical") == 0)
        {
            IsRun = false;
        }
        

    }


    void CalcInputMove()
    {
        moveVelocity = new Vector3(Input.GetAxisRaw("Horizontal"), 0f, 
            Input.GetAxisRaw("Vertical")).normalized*(IsRun ? runspeed : walkspeed);
        animator.SetFloat("SpeedX", Input.GetAxis("Horizontal"));
        animator.SetFloat("SpeedY", Input.GetAxis("Vertical"));
        moveVelocity = transform.TransformDirection(moveVelocity); // moveVelocity 절대좌표
        if(0.01f < moveVelocity.sqrMagnitude)
        {
            Quaternion cameraRot = cameraPivotTr.rotation;
            cameraRot.x = cameraRot.z = 0f;
            transform.rotation = cameraRot;
            if(isRun)
            {
                Quaternion chearacterRot = Quaternion.LookRotation(moveVelocity);
                chearacterRot.x = cameraRot.z = 0f;
                transform.rotation = Quaternion.Slerp(modleTr.rotation, chearacterRot, Time.deltaTime * 10f);
            }
            else
            {
                modleTr.rotation = Quaternion.Slerp(modleTr.rotation, cameraRot, Time.deltaTime * 10f);
            }
        }
    }
    void CameraDistanceCtrl() // 마우스 휠로  카메라 거리 조절
    {
        camerDistance -= Input.GetAxis("Mouse ScrollWheel");
    }
    void FreezeXz()// 캐릭터 컨트롤러의 회전 제한 x축 회전 과 z축 회전
    {
        transform.eulerAngles = new Vector3 (0f,transform.eulerAngles.y,0f);
    }
    bool GroundCheack(out RaycastHit hit)
    {
        return Physics.Raycast(transform.position, Vector3.down, out hit,0.25f);
              // 레이케스트로 땅쪽 방향으로 쏘아서 충돌감지 해서 
              // 정확하게 땅 그라운드에 착지 되어있는지 충돌 검사
    }
    private float nexttime = 0f;
    void AttackTime()
    {
        nexttime += Time.deltaTime;
        if(1f <= nexttime)
        {
            nexttime += Time.deltaTime;
            playerstate = PlalyerState.Ible;
        }
    }
    void PlayerAttack()
    {
        if (Input.GetButtonDown("Fire1"))// 마우스 왼쪽버튼과 왼쪽 컨트롤 키를 사용 가능
        {
            playerstate = PlalyerState.ATTack;
            animator.SetTrigger(hashAttack);
            animator.SetFloat(hashSpeedX, 0f);
            animator.SetFloat(hashSPeedY, 0f);
            nexttime = 0f;
        }
       
    }
    void SheildAttack()
    {
        if (Input.GetButtonDown("Fire2"))
        {
            playerstate = PlalyerState.ATTack;
            animator.SetTrigger(hashSheld);
            animator.SetFloat(hashSpeedX, 0f);
            animator.SetFloat(hashSPeedY, 0f);
            nexttime = 0f;
        }
    }
    
}
