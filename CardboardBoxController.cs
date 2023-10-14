using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardboardBoxController : MonoBehaviour
{
    [SerializeField] private GameObject grabedEffect;
    [SerializeField] private bool horizontalMovementOnly;
    [SerializeField] private bool verticalMovementOnly;
    private PlayerController playerController;
    private GameInput gameInput;
    private BoxCollider boxCollider;
    private bool isGrabed;
    private float speed;
    public float dropSpeed;
    [SerializeField] private float grabSpeedModifier;

    private SoundManager soundManager;
    private AudioSource _cachedAudioSource;

    public bool isResearchMachine;

    void Start()
    {
        isGrabed = false;
        boxCollider = transform.GetComponent<BoxCollider>();

        GameObject player = GameObject.Find("Player_TVguy");
        GameObject inputObject = GameObject.Find("GameInput");
        playerController = player.GetComponent<PlayerController>();
        gameInput = inputObject.GetComponent<GameInput>();

        var soundManagerArray = FindObjectsOfType<SoundManager>();
        soundManager = soundManagerArray[0];
    }

    void Update()
    {
        //handle box drop
        Vector3 dropDir = new Vector3(0, -1f, 0);
        float dropDistance = dropSpeed * Time.deltaTime;
        Vector3 boxCenter = transform.position + new Vector3(0, 1.05f, 0);
        Vector3 boxHalfExt = new Vector3(boxCollider.size.x / 2, boxCollider.size.y / 2, boxCollider.size.z / 2); 
        bool canDrop = !Physics.BoxCast(boxCenter, boxHalfExt, dropDir, transform.rotation, dropDistance, 1 << 0);
        if (canDrop)
        {
            transform.position += dropDir * dropSpeed * Time.deltaTime;
            if (transform.position.y < 0)
            {
                transform.position = new Vector3(transform.position.x, 0, transform.position.z);
            }
        }

        //handle player grab box movement
        if (isGrabed)
        {
            UpdatePlayerMoveStat();
            //get Player movement input
            Vector2 inputVector = gameInput.getMovementVectorNormalized();

            //disable switch
            if (playerController.GetDisableInputStatus())
            {
                inputVector *= Vector2.zero;
            }

            //play audio
            if (inputVector != Vector2.zero)
            {
                if (!isResearchMachine)
                {
                    _cachedAudioSource = soundManager.PlaySoundAndReturnSource(soundManager.boxSlide, 0.1f);
                }
                else
                {
                    _cachedAudioSource = soundManager.PlaySoundAndReturnSource(soundManager.metalSlide, 0.1f);
                }
            }
            else
            {
                _cachedAudioSource.Stop();
            }

            //horizontal movement only
            if (horizontalMovementOnly == true)
            {
                inputVector.x = -inputVector.y;
                boxHalfExt = new Vector3(boxCollider.size.y / 2, boxCollider.size.y / 2, boxCollider.size.y / 2);
            }

            //vertical movement only
            if (verticalMovementOnly == true)
            {
                inputVector.y = inputVector.x;
                boxHalfExt = new Vector3(boxCollider.size.y / 2, boxCollider.size.y / 2, boxCollider.size.y / 2);
            }



            Vector3 moveDir = new Vector3(inputVector.x, 0.0f, inputVector.y);
            float moveDistance = speed  * Time.deltaTime;

            bool canMove = !Physics.BoxCast(boxCenter, boxHalfExt, moveDir, transform.rotation, moveDistance, 1 << 0);

            if (!canDrop && canMove && playerController.getCanUseItemStatus())
            {
                transform.position += moveDir * speed * Time.deltaTime;
            }

            //when too far break grab
            float _HorDistance;
            _HorDistance = Vector3.Distance(transform.position, playerController.transform.position);
            //Debug.Log("_HorDistance:" + _HorDistance);
            if (_HorDistance > 2)
            {
                isGrabed = false;
                grabedEffect.SetActive(false);
                playerController.isGrabbing = false;
            }
        }



		//when too high or low , break grab
		float _VerDistance;
		_VerDistance = transform.position.y - playerController.transform.position.y;
		_VerDistance = System.Math.Abs(_VerDistance);
        //Debug.Log("_VerDistance" + _VerDistance);

    }

    private void UpdatePlayerMoveStat()
    {
        speed = playerController.GetPlayerGrabMoveSpeed();
        dropSpeed = playerController.GetPlayerDropSpeed();
    }

    public void toggleGrabStatus()
    {
        isGrabed = !isGrabed;
        if (isGrabed == false)
        {
            grabedEffect.SetActive(false);
        }
        else
        {
            grabedEffect.SetActive(true);
        }
    }

    public bool getGrabStatus()
    {
        return isGrabed;
    }
}
