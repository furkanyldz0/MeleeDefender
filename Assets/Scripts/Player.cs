using System;
using System.Collections;
using UnityEngine;

public class Player : MonoBehaviour
{
    public event EventHandler OnAttack;

    public static Player Instance { get; private set; }
    
    private Rigidbody rb;
    private float moveSpeed = 2f;
    private int currentXDirection = 0;

    private bool canDash = true;
    private bool isDashing;
    private float dashingPower = 30f;
    private float dashingTime = 0.05f;
    private float dashingCooldown = 0f;

    private void Awake() {
        if(Instance != null) {
            Debug.LogError("Sahnede birden fazla Player var!");
        }
        Instance = this;
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Update() {
        //Input taramasýný her karede, dash durumundan baðýmsýz olarak en baþta yap
        TrackHorizontalInput();

        if (isDashing) {
            return;
        }

        HandleMovement();
        HandleAttack();
        LookAtMouse();

        if(Input.GetKeyDown(KeyCode.LeftShift) && canDash) {
            StartCoroutine(Dash());
        }
    }

    private void LookAtMouse() {
        Ray mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        Plane plane = new Plane(Vector3.up, transform.position);
        //ray'in sadece düzlem ile çarpýþmasýna bakýyor
        if (plane.Raycast(mouseRay, out float hitDist)) { //hitdist ray'in uzunluðu, getpoint ile çarptýðý yerin kordinatýný alýyoruz
            Vector3 hitPoint = mouseRay.GetPoint(hitDist);

            Vector3 lookDirection = hitPoint - transform.position;
            lookDirection.y = 0f;

            float rotationLimit = 1f;
            //konuma göre farenin konumunu kontrol ediyor, karakterin arkasýna düþmesi durumunda
            if (lookDirection.z < rotationLimit) {
                //karakterin tam arkaya dönmemesi için
                lookDirection.z = rotationLimit + 0.001f;
            }

            if (lookDirection != Vector3.zero) {
                // Yönü rotasyona çevir ve Rigidbody'e "Dön" de
                Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
                rb.MoveRotation(targetRotation);
            }
        }
    }

    //private void LookAtMouse() {
    //    Ray mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
    //    Plane plane = new Plane(Vector3.up, transform.position);
    //    //ray'in sadece düzlem ile çarpýþmasýna bakýyor
    //    if(plane.Raycast(mouseRay, out float hitDist)) {
    //        Vector3 hitPoint = mouseRay.GetPoint(hitDist); //hitdist ray'in uzunluðu, getpoint ile çarptýðý yerin kordinatýný alýyoruz
    //        transform.LookAt(hitPoint);
    //    }
    //}

    private IEnumerator Dash() {
        Vector2 inputVector = GetMovementVector2Normalized();
        Vector3 dashDir = new Vector3(inputVector.x, 0, inputVector.y);

        canDash = false;
        isDashing = true;
        //rb.useGravity = false; //
        rb.linearVelocity = dashDir * dashingPower;
        yield return new WaitForSeconds(dashingTime);
        isDashing = false;
        rb.linearVelocity = Vector3.zero;
        yield return new WaitForSeconds(dashingCooldown);
        canDash = true;
        //rb.useGravity = true;
    }

    private void HandleAttack() {
        if (Input.GetMouseButtonDown(0)) {
            OnAttack?.Invoke(this, EventArgs.Empty);
        }
    }

    private void HandleMovement() {
        //çarpýþmadan kaynaklý sürtünme ve istenmeyen hareketleri engellemek için
        //dash atarken metot okunmadýðý için sýkýntý yok
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        Vector2 inputVector = GetMovementVector2Normalized();
        Vector3 moveDir = new Vector3(inputVector.x, 0, inputVector.y);
        rb.MovePosition(rb.position + moveDir * moveSpeed * Time.fixedDeltaTime); //fixeddelta'da yazýcam 
    }

    //a ve d'ye birlikte basýmlarda sýkýntý çýkabiliyor, onun için yazýldý
    private void TrackHorizontalInput() {
        // Yeni bir tuþa basýldýysa yönü doðrudan ona eþitle
        if (Input.GetKeyDown(KeyCode.A)) currentXDirection = -1;
        if (Input.GetKeyDown(KeyCode.D)) currentXDirection = 1;

        // Bir tuþtan el çekildiðinde, diðer tuþ hala basýlýysa yönü ona çevir
        if (Input.GetKeyUp(KeyCode.A) && Input.GetKey(KeyCode.D)) currentXDirection = 1;
        if (Input.GetKeyUp(KeyCode.D) && Input.GetKey(KeyCode.A)) currentXDirection = -1;

        // Hiçbir tuþa basýlmýyorsa sýfýrla
        if (!Input.GetKey(KeyCode.A) && !Input.GetKey(KeyCode.D)) currentXDirection = 0;
    }

    private Vector2 GetMovementVector2Normalized() {
        //Vector2 inputVector = new Vector2(0, 0);

        //if (Input.GetKey(KeyCode.A)) {
        //    inputVector.x += -1;
        //}
        //if (Input.GetKey(KeyCode.D)) {
        //    inputVector.x += 1; 
        //}
        Vector2 inputVector = new Vector2(currentXDirection, 0);

        inputVector = inputVector.normalized;

        return inputVector;
    }
}
