using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerRPG : MonoBehaviour
{
    public enum PlalyerState {Ible = 0, ATTack,UNDER_ATTACK,DEAD}
    public PlalyerState playerstate = PlalyerState.Ible;
    [Tooltip("�ȱ� �ӵ�")] public float walkspeed = 5f;
    [Tooltip("�޸��� �ӵ�")] public float runspeed = 5f;
    [Header("Camer ���� ����")]
    [SerializeField] private Transform camerTr; // ī�޶� ��ġ
    [SerializeField] private Transform cameraPivotTr; //  ī�޶� �ǹ� ��ġ
    [SerializeField] private float camerDistance = 0f; // ī�޶���� �Ÿ�
    [SerializeField] private Vector3 mouseMove = Vector3.zero; // ���콺 �̵� ��ǥ
    [SerializeField] private int playerLayer; // �÷��̾� ���̾�
    [Header("�÷��̾� move ���ú���")]
    [SerializeField] private Transform modleTr;
    [SerializeField] private Animator animator;
    [SerializeField] private CharacterController characterController;
    [SerializeField] private Vector3 moveVelocity = Vector3.zero; // ������ ����
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
        camerDistance = 5f; // ī�޶��� �Ÿ� 
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
        // ī�޶��� ������ ����
        cameraPivotTr.position = transform.position+(Vector3.up * cameraHeight);
                    // ���콺�� ���Ϸ� ī�޶�� x�� ȸ��
        mouseMove += new Vector3(-Input.GetAxis("Mouse Y") * 100f*0.1f, Input.GetAxisRaw("Mouse X")*100f*0.1f);
        // ���콺�� �¿�� ī�޶�� y��ȸ��
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
        
               //     ����ī�޶� ��ġ���� ī�޶� �ǹ� ��ġ�� ���� ������ ���Ѵ�. 
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
            CalcInputMove(); // ������ ���
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
                isGrounded = false; // ���� �������� ���� �� isGrounded�� false�� ����
                animator.SetBool("IsGrounded", false); // �̰����� �ִϸ��̼� ���� ������Ʈ
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
        moveVelocity = transform.TransformDirection(moveVelocity); // moveVelocity ������ǥ
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
    void CameraDistanceCtrl() // ���콺 �ٷ�  ī�޶� �Ÿ� ����
    {
        camerDistance -= Input.GetAxis("Mouse ScrollWheel");
    }
    void FreezeXz()// ĳ���� ��Ʈ�ѷ��� ȸ�� ���� x�� ȸ�� �� z�� ȸ��
    {
        transform.eulerAngles = new Vector3 (0f,transform.eulerAngles.y,0f);
    }
    bool GroundCheack(out RaycastHit hit)
    {
        return Physics.Raycast(transform.position, Vector3.down, out hit,0.25f);
              // �����ɽ�Ʈ�� ���� �������� ��Ƽ� �浹���� �ؼ� 
              // ��Ȯ�ϰ� �� �׶��忡 ���� �Ǿ��ִ��� �浹 �˻�
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
        if (Input.GetButtonDown("Fire1"))// ���콺 ���ʹ�ư�� ���� ��Ʈ�� Ű�� ��� ����
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
