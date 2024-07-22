using UnityEngine;
using System.Collections.Generic;

public class PlayerController : MonoBehaviour {
    public float moveSpeed = 5f;
    public float smoothSpeed = 0.125f;
    public float turnSpeed = 5f;
    public float climbSpeed = 3f;
    public Camera mainCamera;
    public List<Light> moonlightSources = new List<Light>(); // List of moonlight sources assigned in the Inspector
    public List<Light> artificialLightSources = new List<Light>(); // List of artificial light sources assigned in the Inspector
    public GameObject playerFire; // Reference to the Player Fire GameObject
    private Rigidbody rb;
    private Renderer playerRenderer;
    private bool isInMoonlight = false;
    private bool isInArtificialLight = false;
    private bool isClimbing = false;
    private Vector3 climbNormal;
    private Color moonlightColor = new Color(158 / 255f, 159 / 255f, 248 / 255f);
    private Color artificialLightColor = new Color(255 / 255f, 223 / 255f, 128 / 255f);
    private Color originalColor;
    public float jumpForce = 10f;
    private bool isGrounded;
    private bool isJumping;

    private void Start() {
        rb = GetComponent<Rigidbody>();
        playerRenderer = GetComponent<Renderer>();
        originalColor = playerRenderer.material.color;

        // Freeze rotation on the X and Z axes
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

        // Populate the light sources based on tags
        PopulateLightSources();
    }

    private void PopulateLightSources() {
        foreach (GameObject lightObject in GameObject.FindGameObjectsWithTag("Moonlight")) {
            Light light = lightObject.GetComponent<Light>();
            if (light != null) {
                moonlightSources.Add(light);
            }
        }

        foreach (GameObject lightObject in GameObject.FindGameObjectsWithTag("ArtificialLight")) {
            Light light = lightObject.GetComponent<Light>();
            if (light != null) {
                artificialLightSources.Add(light);
            }
        }
    }

    private void FixedUpdate() {
        MovePlayer();
        CheckIfInShadow();
        UpdatePlayerColor();
        UpdatePlayerFire();
        CheckGrounded();
        if (Input.GetButtonDown("Jump") && isGrounded) {
            Debug.Log("Walahi I am Here");
            Jump();
        }
    }

private void CheckGrounded() {
    if(isInMoonlight){
        isGrounded = Physics.Raycast(transform.position, Vector3.down, 1.3f);
    } else {
        isGrounded = Physics.Raycast(transform.position, Vector3.down, 1.1f);
    }
}

    private void Jump() {
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        isJumping = true;
    }

    private void MovePlayer() {
        if (isClimbing) {
            Climb();
        }
        else {
            // Get input
            float moveHorizontal = Input.GetAxis("Horizontal");
            float moveVertical = Input.GetAxis("Vertical");

            // Calculate movement direction relative to the camera
            Vector3 forward = mainCamera.transform.forward;
            Vector3 right = mainCamera.transform.right;
            forward.y = 0f; // Ignore vertical component
            right.y = 0f; // Ignore vertical component
            forward.Normalize();
            right.Normalize();

            Vector3 moveDirection = (forward * moveVertical + right * moveHorizontal).normalized;

            // Calculate the target position
            Vector3 targetPosition = rb.position + moveDirection * moveSpeed * Time.fixedDeltaTime;

            // Smoothly interpolate towards the target position
            Vector3 smoothedPosition = Vector3.Lerp(rb.position, targetPosition, smoothSpeed);

            // Apply movement to the player
            rb.MovePosition(smoothedPosition);

            // Rotate player to face the movement direction
            if (moveDirection != Vector3.zero) {
                // Rotate player to face the movement direction with an offset if needed
                Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
                rb.rotation = Quaternion.Slerp(rb.rotation, targetRotation, turnSpeed * Time.fixedDeltaTime);
            }

            CheckForClimbableSurface();
        }
    }

    private void CheckForClimbableSurface() {
        RaycastHit hit;
        Vector3 forward = transform.TransformDirection(Vector3.forward);

        if (Physics.Raycast(transform.position, forward, out hit, 1.0f)) {
            if (hit.collider.CompareTag("Climbable")) {
                isClimbing = true;
                climbNormal = hit.normal;
                rb.useGravity = false;
                rb.velocity = Vector3.zero;
            }
        }
    }

    private void Climb() {
        float moveHorizontal = Input.GetAxis("Horizontal");
        float moveVertical = Input.GetAxis("Vertical");

        // Get the climb direction vectors relative to the camera
        Vector3 cameraForward = mainCamera.transform.forward;
        Vector3 cameraRight = mainCamera.transform.right;
        cameraForward.y = 0; // Ignore vertical component for forward direction
        cameraRight.y = 0; // Ignore vertical component for right direction
        cameraForward.Normalize();
        cameraRight.Normalize();

        // Get the direction vectors for climbing based on the surface normal
        Vector3 climbRight = Vector3.Cross(Vector3.up, climbNormal).normalized;
        Vector3 climbUp = Vector3.Cross(climbNormal, climbRight).normalized;

        // Combine the camera and climb vectors to determine movement
        Vector3 climbMovement = (cameraRight * moveHorizontal + cameraForward * moveVertical) * climbSpeed * Time.fixedDeltaTime;

        // Apply the climb movement
        rb.MovePosition(transform.position + climbMovement);

        // If no input, stop climbing
        if (moveHorizontal == 0 && moveVertical == 0) {
            isClimbing = false;
            rb.useGravity = true;
        }
    }

    private void CheckIfInShadow() {
        rb.mass = 1f;

        isInMoonlight = false; // Assume the player is not in shadow until proven otherwise
        isInArtificialLight = false; // Assume the player is not in shadow until proven otherwise

        foreach (Light light in moonlightSources) {
            Vector3 directionToLight = light.transform.position - transform.position;
            float distanceToLight = directionToLight.magnitude;

            // Draw line between player and light source for visualization (moonlight color: 158, 159, 248)
            Debug.DrawLine(transform.position, light.transform.position, moonlightColor);

            // Check if there's an obstruction between the player and the light source
            if (!Physics.Raycast(transform.position, directionToLight, distanceToLight)) {
                isInMoonlight = true;
                break;
            }
        }

        foreach (Light light in artificialLightSources) {
            Vector3 directionToLight = light.transform.position - transform.position;
            float distanceToLight = directionToLight.magnitude;

            // Draw line between player and light source for visualization (artificial color: 255, 223, 128)
            Debug.DrawLine(transform.position, light.transform.position, artificialLightColor);

            // Check if there's an obstruction between the player and the light source
            if (!Physics.Raycast(transform.position, directionToLight, distanceToLight)) {
                isInArtificialLight = true;
                break;
            }
        }

        if(!isInMoonlight) {
            Vector3 gravityForce = Vector3.down * 9.81f;
            rb.AddForce(gravityForce, ForceMode.Force);
        }

        if(isInMoonlight){
            rb.AddForce(Vector3.up * 1f, ForceMode.Acceleration);
            if(Physics.Raycast(transform.position, Vector3.down, 1.3f)){
                rb.AddForce(Vector3.up * 0.2f, ForceMode.Impulse);
            }
        }

        // if (isInMoonlight) {
        //     Debug.Log("Player is in moonlight");
        // }
        // else {
        //     Debug.Log("Player is not in moonlight");
        // }

        // if (isInArtificialLight) {
        //     Debug.Log("Player is in artificial light");
        // }
        // else {
        //     Debug.Log("Player is not in artificial light");
        // }
    }

    private void UpdatePlayerColor() {
        if (isInMoonlight) {
            playerRenderer.material.color = moonlightColor;
        }
        else if (isInArtificialLight) {
            playerRenderer.material.color = artificialLightColor;
        }
        else {
            playerRenderer.material.color = originalColor;
        }
    }

    private void UpdatePlayerFire() {
        if (isInArtificialLight && !isInMoonlight) {
            playerFire.SetActive(true);
            playerFire.transform.position = transform.position; // Match the player's position
        }
        else {
            playerFire.SetActive(false);
        }
    }
}
