using System;
using System.Collections;
using UnityEngine;

public class Player : MonoBehaviour
{
    public event EventHandler OnAttack;
    public event EventHandler OnSkillUsed;
    public event Action OnDash;

    public static Player Instance { get; private set; }
    public int CurrentSkillPoint { get; set; } = 0;
    public int MaxSkillPoint => PlayerStats.Instance != null ? PlayerStats.Instance.SkillPointsRequired : 20;
    public bool IsDashing => isDashing;

    [SerializeField] private Melee melee;
    [SerializeField] private TrailRenderer trail;

    private Rigidbody rb;
    private float moveSpeed = 5f;
    private int currentXDirection = 0;

    private bool canDash = true;
    private bool isDashing;
    private float dashingPower = 40f;
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
        Cursor.lockState = CursorLockMode.Confined; //bunu sonra bi config dosyasının içine al yeri burası değil
        rb = GetComponent<Rigidbody>();

        DisableTrail();

        // Kalıcı geliştirme: yetenek çubuğu kısmen dolu başlar
        CurrentSkillPoint = Mathf.FloorToInt(MaxSkillPoint * PlayerStats.Instance.StartingSkillCharge);
    }

    private void Update() {
        //Input taramasını her karede, dash durumundan bağımsız olarak en başta yap
        TrackHorizontalInput();

        // Menüde, duraklatmada veya seviye atlama ekranındayken saldırı/yetenek alma
        if (isDashing || !CanAct()) {
            return;
        }

        if (Input.GetMouseButtonDown(0)) {
            Attack();
        }
        if (Input.GetKeyDown(KeyCode.LeftShift) && canDash) {
            StartCoroutine(Dash());
        }
        if (Input.GetMouseButtonDown(1) && CurrentSkillPoint >= MaxSkillPoint) {
            CurrentSkillPoint = 0;
            OnSkillUsed?.Invoke(this, EventArgs.Empty);
            //özel skill
        }

    }

    private void FixedUpdate() {
        if (isDashing) {
            return;
        }

        if (!CanAct()) {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            return;
        }

        HandleMovement();
        LookAtMouse();
        //CalculateRotationSpeed();
    }

    private bool CanAct() {
        return GameFlow.Instance == null || GameFlow.Instance.IsPlaying;
    }

    public void AddSkillPoint(int amount) {
        CurrentSkillPoint += amount;

        if(CurrentSkillPoint > MaxSkillPoint) {
            CurrentSkillPoint = MaxSkillPoint;
        }
    }

    private void LookAtMouse() {
        Ray mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        Plane plane = new Plane(Vector3.up, transform.position);
        //ray'in sadece düzlem ile çarpışmasına bakıyor
        if (plane.Raycast(mouseRay, out float hitDist)) { //hitdist ray'in uzunluğu, getpoint ile çarptığı yerin kordinatını alıyoruz
            Vector3 hitPoint = mouseRay.GetPoint(hitDist);

            Vector3 lookDirection = hitPoint - transform.position;
            lookDirection.y = 0f;

            float rotationLimit = 1f;
            //konuma göre farenin konumunu kontrol ediyor, karakterin arkasına düşmesi durumunda
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

    private IEnumerator Dash() {
        Vector2 inputVector = GetMovementVector2Normalized();
        Vector3 dashDir = new Vector3(inputVector.x, 0, inputVector.y);
        if (dashDir == Vector3.zero) {
            yield break;
        }
            

        canDash = false;
        isDashing = true;
        OnDash?.Invoke();
        //rb.useGravity = false; //
        EnableTrail();
        rb.linearVelocity = dashDir * dashingPower;
        yield return new WaitForSeconds(dashingTime);

        isDashing = false;
        DisableTrail();
        rb.linearVelocity = Vector3.zero;
        yield return new WaitForSeconds(dashingCooldown);

        canDash = true;
        //rb.useGravity = true;
    }

    private void Attack() {
        OnAttack?.Invoke(this, EventArgs.Empty);
    }

    private void HandleMovement() {
        //çarpışmadan kaynaklı sürtünme ve istenmeyen hareketleri engellemek için
        //dash atarken metot okunmadığı için sıkıntı yok
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        Vector2 inputVector = GetMovementVector2Normalized();
        Vector3 moveDir = new Vector3(inputVector.x, 0, inputVector.y);
        float speed = moveSpeed * PlayerStats.Instance.MoveSpeedMultiplier;
        rb.MovePosition(rb.position + moveDir * speed * Time.fixedDeltaTime); //fixeddelta'da yazıcam 
    }

    //a ve d'ye birlikte basımlarda sıkıntı çıkabiliyor, onun için yazıldı
    private void TrackHorizontalInput() {
        // Yeni bir tuşa basıldıysa yönü doğrudan ona eşitle
        if (Input.GetKeyDown(KeyCode.A)) currentXDirection = -1;
        if (Input.GetKeyDown(KeyCode.D)) currentXDirection = 1;

        // Bir tuştan el çekildiğinde, diğer tuş hala basılıysa yönü ona çevir
        if (Input.GetKeyUp(KeyCode.A) && Input.GetKey(KeyCode.D)) currentXDirection = 1;
        if (Input.GetKeyUp(KeyCode.D) && Input.GetKey(KeyCode.A)) currentXDirection = -1;

        // Hiçbir tuşa basılmıyorsa sıfırla
        if (!Input.GetKey(KeyCode.A) && !Input.GetKey(KeyCode.D)) currentXDirection = 0;
    }

    private void EnableTrail() {
        trail.emitting = true;
    }

    private void DisableTrail() {
        trail.emitting = false;
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

    public Melee GetMelee() {
        return melee;
    }


    //private void CalculateRotationSpeed() {
    //    // Sadece zaman akıyorsa hesaplama yap (Hata vermemesi için)
    //    if (Time.fixedDeltaTime > 0f) {
    //        float angleDifference = Quaternion.Angle(lastFrameRotation, rb.rotation);
    //        float rawTurnSpeed = angleDifference / Time.fixedDeltaTime;

    //        CurrentTurnSpeed = Mathf.Clamp(rawTurnSpeed, 0f, 1500f);
    //    }

    //    lastFrameRotation = rb.rotation;
    //}

    //private void LookAtMouse() {
    //    Ray mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
    //    Plane plane = new Plane(Vector3.up, transform.position);
    //    //ray'in sadece düzlem ile çarpışmasına bakıyor
    //    if(plane.Raycast(mouseRay, out float hitDist)) {
    //        Vector3 hitPoint = mouseRay.GetPoint(hitDist); //hitdist ray'in uzunluğu, getpoint ile çarptığı yerin kordinatını alıyoruz
    //        transform.LookAt(hitPoint);
    //    }
    //}
}
