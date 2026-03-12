using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class Slingshot : MonoBehaviour
{
    [Header("Inscribed")]
    public GameObject projectilePrefab;
    public float velocityMult = 10f;
    public GameObject projLinePrefab;
    public LineRenderer rubberBand;
    public Transform leftFork;
    public Transform rightFork;
    public AudioClip snapSound;

    [Header("Dynamic")]
    public GameObject launchPoint;
    public Vector3 launchPos;
    public GameObject projectile;
    public bool aimingMode;
    private AudioSource audioSource;

    void Awake()
    {
        Transform launchPointTrans = transform.Find("LaunchPoint");
        launchPoint = launchPointTrans.gameObject;
        launchPoint.SetActive(false);
        launchPos = launchPointTrans.position;
        
        if (rubberBand != null)
            rubberBand.enabled = false;
        
        audioSource = GetComponent<AudioSource>();
    }

    void OnMouseEnter()
    {
        launchPoint.SetActive(true);
    }

    void OnMouseExit()
    {
        launchPoint.SetActive(false);
    }

    void OnMouseDown()
    {
        aimingMode = true;
        projectile = Instantiate(projectilePrefab) as GameObject;
        projectile.transform.position = launchPos;
        projectile.GetComponent<Rigidbody>().isKinematic = true;
    }

    void Update()
    {
        if (!aimingMode)
        {
            if (rubberBand != null)
                rubberBand.enabled = false;
            
            return;
        }
        
        Vector3 mousePos2D = Input.mousePosition;
        mousePos2D.z = -Camera.main.transform.position.z;
        Vector3 mousePos3D = Camera.main.ScreenToWorldPoint(mousePos2D);

        Vector3 mouseDelta = mousePos3D - launchPos;
        float maxMagnitude = this.GetComponent<SphereCollider>().radius;
        if (mouseDelta.magnitude > maxMagnitude)
        {
            mouseDelta.Normalize();
            mouseDelta *= maxMagnitude;
        }

        Vector3 projPos = launchPos + mouseDelta;
        projectile.transform.position = projPos;

        if (rubberBand != null && leftFork != null && rightFork != null)
        {
            rubberBand.enabled = true;
            rubberBand.positionCount = 3;
            rubberBand.SetPosition(0, leftFork.position);
            rubberBand.SetPosition(1, projPos);
            rubberBand.SetPosition(2, rightFork.position);
        }

        if (Input.GetMouseButtonUp(0))
        {
            aimingMode = false;
            if (rubberBand != null)
                rubberBand.enabled = false;
            
            if (snapSound != null)
                audioSource.PlayOneShot(snapSound);
            
            Rigidbody projRB = projectile.GetComponent<Rigidbody>();
            projRB.isKinematic = false;
            projRB.collisionDetectionMode = CollisionDetectionMode.Continuous;
            projRB.velocity = -mouseDelta * velocityMult;
            
            FollowCam.SWITCH_VIEW(FollowCam.eView.slingshot);
            FollowCam.POI = projectile;
            
            Instantiate<GameObject>(projLinePrefab, projectile.transform);
            
            projectile = null;
            
            MissionDemolition.SHOT_FIRED();
        }
    }
}