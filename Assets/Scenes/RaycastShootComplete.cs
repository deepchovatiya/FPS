using UnityEngine;
using System.Collections;

public class RaycastShootComplete : MonoBehaviour
{

    public int gunDamage = 1;                                            // Set the number of hitpoints that this gun will take away from shot objects with a health script
    public float fireRate = 0.25f;                                        // Number in seconds which controls how often the player can fire
    public float weaponRange = 50f;                                        // Distance in Unity units over which the player can fire
    public float hitForce = 100f;                                        // Amount of force which will be added to objects with a rigidbody shot by the player
    public Transform gunEnd;                                            // Holds a reference to the gun end object, marking the muzzle location of the gun

    private Camera fpsCam;                                                // Holds a reference to the first person camera
    private WaitForSeconds shotDuration = new WaitForSeconds(0.07f);    // WaitForSeconds object used by our ShotEffect coroutine, determines time laser line will remain visible
    private AudioSource gunAudio;                                        // Reference to the audio source which will play our shooting sound effect
    private LineRenderer laserLine;                                        // Reference to the LineRenderer component which will display our laserline
    private float nextFire;                                                // Float to store the time the player will be allowed to fire again, after firing
    public float zoomFOV = 15f;         // Field of view when scoped in
    private float defaultFOV;           // To store original FOV
    private bool isScoped = false;      // Track if currently scoped

    public GameObject scopeOverlay;     // UI overlay for scope (assign in inspector)
    public GameObject weaponModel;      // Assign your weapon model to hide it when scoped (optional)
    public ParticleSystem muzzleFlash;
    public GameObject hitEffectPrefab;


    void Start()
    {
        // Get and store a reference to our Camera by searching this GameObject and its parents
        fpsCam = GetComponentInParent<Camera>();

        defaultFOV = fpsCam.fieldOfView;

        scopeOverlay.SetActive(false); // Make sure it's off at start

        // Get and store a reference to our LineRenderer component
        laserLine = GetComponent<LineRenderer>();

        // Get and store a reference to our AudioSource component
        gunAudio = GetComponent<AudioSource>();
    }

    void Update()
    {
        if (Input.GetButtonDown("Fire2")) // Right mouse button by default
        {
            isScoped = !isScoped;

            if (isScoped)
                StartCoroutine(OnScoped());
            else
                OnUnscoped();
        }

        // Check if the player has pressed the fire button and if enough time has elapsed since they last fired
        if (Input.GetButtonDown("Fire1") && Time.time > nextFire)
        {
            // Update the time when our player can fire next
            nextFire = Time.time + fireRate;

            // Start our ShotEffect coroutine to turn our laser line on and off
            StartCoroutine(ShotEffect());

            // Create a vector at the center of our camera's viewport
            Vector3 rayOrigin = fpsCam.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 0.0f));

            // Declare a raycast hit to store information about what our raycast has hit
            RaycastHit hit;

            // Set the start position for our visual effect for our laser to the position of gunEnd
            laserLine.SetPosition(0, gunEnd.position);

            // Check if our raycast has hit anything
            if (Physics.Raycast(rayOrigin, fpsCam.transform.forward, out hit, weaponRange))
            {
                laserLine.SetPosition(1, hit.point);

                // Instantiate hit effect
                if (hitEffectPrefab != null)
                {
                    GameObject hitFX = Instantiate(hitEffectPrefab, hit.point, Quaternion.LookRotation(hit.normal));
                    Destroy(hitFX, 1f); // Clean up after 1 sec
                }

                if (hit.rigidbody != null)
                {
                    hit.rigidbody.AddForce(-hit.normal * hitForce);
                }
            }

            else
            {
                // If we did not hit anything, set the end of the line to a position directly in front of the camera at the distance of weaponRange
                laserLine.SetPosition(1, rayOrigin + (fpsCam.transform.forward * weaponRange));
            }
        }
    }


    private IEnumerator ShotEffect()
    {
        // Play the shooting sound effect
        gunAudio.Play();

        // Play muzzle flash
        if (muzzleFlash != null)
            muzzleFlash.Play();

        laserLine.enabled = true;
        yield return shotDuration;
        laserLine.enabled = false;

        // Play the shooting sound effect
        gunAudio.Play();

        // Turn on our line renderer
        laserLine.enabled = true;

        //Wait for .07 seconds
        yield return shotDuration;

        // Deactivate our line renderer after waiting
        laserLine.enabled = false;
    }
    private IEnumerator OnScoped()
    {
        yield return new WaitForSeconds(0.15f); // Optional delay to simulate scope animation

        scopeOverlay.SetActive(true);
        fpsCam.fieldOfView = zoomFOV;

        if (weaponModel != null)
            weaponModel.SetActive(false); // Hide weapon while scoped
    }

    private void OnUnscoped()
    {
        scopeOverlay.SetActive(false);
        fpsCam.fieldOfView = defaultFOV;

        if (weaponModel != null)
            weaponModel.SetActive(true);
    }

}