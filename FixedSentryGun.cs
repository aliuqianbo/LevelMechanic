using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FixedSentryGun : MonoBehaviour
{
    [SerializeField] private BoxAreaScan boxAreaScan;
    [SerializeField] private Transform bulletSpawnPoint;
    [SerializeField] private Animator BarrelAnimator;
    [SerializeField] private ParticleSystem muzzleFlare;
    [SerializeField] private ParticleSystem bulletImpact;
    [SerializeField] private GameObject bullet;
    [SerializeField] private GameObject electricEffect;
    [SerializeField] private AudioSource audioSource_shoot;
    [SerializeField] private AudioSource audioSource_hit;
    [SerializeField] private GameObject audioSource_broken;

    private bool addBulleSpread = true;
    private Vector3 bulletSpreadVariance = new Vector3(0.05f, 0f, 0.05f);
    private PlayerStats playerStats;
    private StateManager_Entity stateManager_Entity;
    private float SHOOT_INTERVAL = 0.25f;
    private float lastShootTime = 0;
    private int BULLET_DAMAGE = 20;


    private float brokenTime = 999999f;
    private float timer = 0;
    [HideInInspector] public bool isBroken = false;

    private SoundManager soundManager;

    private void Start()
    {
        var soundManagerArray = FindObjectsOfType<SoundManager>();
        soundManager = soundManagerArray[0];
    }

    void Update()
    {

        if (boxAreaScan.counter > 0)
        {
            if (!boxAreaScan.coveredBySmoke && !boxAreaScan.blockedByWall && !isBroken)
            {
                Shoot();
            }         
        }

        if (isBroken)
        {
            electricEffect.SetActive(true);
            timer += Time.deltaTime;

            //play audio
            audioSource_broken.SetActive(true);

            if (timer >= brokenTime)
            {
                electricEffect.SetActive(false);
                timer = 0;
                isBroken = false;

                //stop audio
                audioSource_broken.SetActive(true);
            }
            
        }
    }

    private void Shoot()
    {
        if ((lastShootTime + SHOOT_INTERVAL) < Time.time)
        {
            Debug.Log("shooting!!!!!");
            lastShootTime = Time.time;
            //play animation
            BarrelAnimator.Play("Fire");
            //play effect
            muzzleFlare.Play();
            //play audio
            soundManager.PlaySoundWithTargetAudioSource(audioSource_shoot, soundManager.shootBullet, 1f);

            //doing Raycast, dealing damage

            Vector3 direction = GetDirection();

            if (Physics.Raycast(bulletSpawnPoint.position, direction, out RaycastHit hit, 21.6f))
            {

                GameObject actualBullet = Instantiate(bullet, bulletSpawnPoint.position, transform.rotation);
                StartCoroutine(MoveBullet(actualBullet, hit));

                if (hit.transform.tag == ("Player"))
                {
                    Debug.Log("Hit Player");
                    playerStats = hit.transform.GetComponent<PlayerStats>();
                    playerStats.PlayerTakingHPDamage(BULLET_DAMAGE);

                    //play audio
                    soundManager.PlaySoundWithTargetAudioSource(audioSource_hit, soundManager.bulletHit, 1f);
                }
                else if (hit.transform.tag == ("Entity"))
                {
                    Debug.Log("Hit Entity!");
                    stateManager_Entity = hit.transform.GetComponent<StateManager_Entity>();
                    stateManager_Entity.currentState = stateManager_Entity.STATE_HURT;

                    //play audio
                    soundManager.PlaySoundWithTargetAudioSource(audioSource_hit, soundManager.bulletHit, 1f);
                }
                
            }
            
        }
    }

    private Vector3 GetDirection()
    {
        Vector3 direction = transform.forward;
        if (addBulleSpread)
        {
            direction += new Vector3(
                Random.Range(-bulletSpreadVariance.x, bulletSpreadVariance.x),
                Random.Range(-bulletSpreadVariance.y, bulletSpreadVariance.y),
                Random.Range(-bulletSpreadVariance.z, bulletSpreadVariance.z)
            );

            direction.Normalize();
        }

        return direction;
    }

    private IEnumerator MoveBullet(GameObject bullet, RaycastHit hit)
    {
        float time = 0;
        Vector3 startPosition = bullet.transform.position;

        while (time < 1)
        {
            bullet.transform.position = Vector3.Lerp(startPosition, hit.point, time);
            time += Time.deltaTime / 0.05f;

            yield return null;
        }

        bullet.transform.position = hit.point;
        Instantiate(bulletImpact, hit.point, Quaternion.LookRotation(hit.normal));

        Destroy(bullet, 0.05f);

    }
}
