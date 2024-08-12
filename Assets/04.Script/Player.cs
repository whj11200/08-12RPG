using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static PlayerRPG;

public class Player : MonoBehaviour
{
    // 애니메이션 선택지
    public enum PlayerList {IDLE = 0, ATTACK,UNDER_ATTACK,DEAD}
    // 현재 애니메이션 IDLE로 설정
    public PlayerList playerlist = PlayerList.IDLE;
    // 현재 스피드
    public float walkspeed = 5f;
    // 달리는 속도
    public float runspeed = 5f;
    private Transform cameTr;
    private Transform camerPivotTr;
    private float camerDistance = 0f;
    private Vector3 mouseMove = Vector3.zero;
    private int playerLayer;
    private Transform modleTr;
    private Animator ani;
    private CharacterController characterController;
    private Vector3 moveVelocity = Vector3.zero;
    private bool isGround = false;
    private bool isRun = false;
    private readonly int hashattack = Animator.StringToHash("Attack1");
    private readonly int hashattacktwo = Animator.StringToHash("Attack2");
    private readonly int hashspeeedx = Animator.StringToHash("speedX");
    private readonly int hashspeeedy = Animator.StringToHash("speedY");
    void Start()
    {
        modleTr = transform;
        cameTr = Camera.main.transform;
        camerPivotTr = Camera.main.transform.parent;
        playerLayer = LayerMask.NameToLayer("Player");
        ani = GetComponent<Animator>();
        characterController = GetComponent<CharacterController>();
        camerDistance = 5f;

    }
    public bool IsRun
    {
        get { return isRun; }
        set { isRun = value; ani.SetBool("isRun", value); }
    }

  
    void Update()
    {
        FrezeXz();
       switch (playerlist)
        {
            case PlayerList.IDLE:
                PlayerIdleAndMove();
                break;
            case PlayerList.ATTACK:
                AttackTime();
                break;
            case PlayerList.UNDER_ATTACK:
                break;
            case PlayerList.DEAD:
                break;
            
        }
        CamerDistanceCtrl();
    }
    private void LateUpdate()
    {
        // 카메라 높이값을
        float camerHeight = 1.3f;
        // 카메라의 부모의 위치값에 현재 위치에서 높에이값을 할당
        camerPivotTr.position = transform.position +(Vector3.up * camerHeight);

        mouseMove += new Vector3(Input.GetAxis("Mouse Y") * 100f + 0.1f, Input.GetAxis("Mouse X") * 100f * 0.1f);

        camerPivotTr.eulerAngles = mouseMove;
        if(mouseMove.x < -40f)
        {
            mouseMove.x = 40f;
        }
        else
        {
            mouseMove.x = 40f;
        }
        camerPivotTr.eulerAngles = mouseMove;

        RaycastHit hit;

        Vector3 dir = (cameTr.position - camerPivotTr.position).normalized;
        if( Physics.Raycast(camerPivotTr.position,dir,out hit, camerDistance, ~(1 << playerLayer)))
        {
            cameTr.localPosition = Vector3.back * hit.distance;
        }
        else
        {
            cameTr.localPosition = Vector3.back * camerDistance;
        }

     

    }
    void PlayerIdleAndMove()
    {
        Runcheak();
        if (characterController.isGrounded)
        {
            if (!isGround)
            {
                isGround = true;
                ani.SetBool("IsGrounded", true);
            }
            Calcinputmove();
            RaycastHit groundhit;
            if (GroundCheaking(out groundhit))
                moveVelocity.y = isRun ? -runspeed : -walkspeed;
            else
                moveVelocity.y = -1f;
            Attacktwo();
            Attackone();
        }
        else
        {
            if (isGround)
            {
                isGround = false;
                ani.SetBool("IsGrounded", false);
            }
            moveVelocity += Physics.gravity * Time.deltaTime;

        }
        characterController.Move(moveVelocity * Time.deltaTime);
    }

    void Runcheak()
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
    void Calcinputmove()
    {

        moveVelocity = new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical")).normalized * (IsRun ? runspeed : walkspeed);
        ani.SetFloat("speedX", Input.GetAxis("Horizontal"));
        ani.SetFloat("speedY", Input.GetAxis("Vertical"));
        moveVelocity = transform.TransformDirection(moveVelocity);
        if (0.01f < moveVelocity.magnitude)
        {
            Quaternion camerRot = camerPivotTr.rotation;
            camerRot.x = camerRot.z = 0f;
            transform.rotation = camerRot;
            if (isRun)
            {
                Quaternion chearacterRot = Quaternion.LookRotation(moveVelocity);
                chearacterRot.x = camerRot.z = 0f;
                transform.rotation = Quaternion.Slerp(modleTr.rotation, chearacterRot, Time.deltaTime * 10f);
            }
            else
            {
                modleTr.rotation = Quaternion.Slerp(modleTr.rotation, camerRot, Time.deltaTime * 10f);
            }
        }
    }
    bool GroundCheaking(out RaycastHit hit)
    {
        return Physics.Raycast(transform.position,Vector3.down, out hit,0.25f);
    }
    void FrezeXz()
    {
        transform.eulerAngles = new Vector3(0f,transform.eulerAngles.y,0f);
    }
    void CamerDistanceCtrl()
    {
        camerDistance -= Input.GetAxis("Mouse ScrollWheel");
    }
    void Attackone()
    {
        if (Input.GetButtonDown("Fire1"))
        {
            playerlist = PlayerList.ATTACK;
            ani.SetTrigger(hashattack);
            ani.SetFloat(hashspeeedx, 0f);
            ani.SetFloat(hashspeeedy, 0f);
            nexttime = 0f;
        }
       

    }
    void Attacktwo()
    {
        if (Input.GetButtonDown("Fire2"))
        {
            playerlist = PlayerList.ATTACK;
            ani.SetTrigger(hashattacktwo);
            ani.SetFloat(hashspeeedx, 0f);
            ani.SetFloat(hashspeeedy, 0f);
            nexttime = 0f;
        }
    }
    private float nexttime = 0f;
    void AttackTime()
    {
        nexttime += Time.deltaTime;
        if (1f <= nexttime)
        {
            nexttime += Time.deltaTime;
            playerlist = PlayerList.IDLE;
        }
    }
}
