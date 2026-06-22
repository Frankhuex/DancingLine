using UnityEngine;

public class HelicopterAutoFlightController : MonoBehaviour
{
    [Header("Flight Configuration")]
    public Vector3 StartPosition = new Vector3(0f, 10f, 0f);
    public Vector3 EndPosition = new Vector3(100f, 20f, 100f);
    public float EngineForce = 5f;
    public float FlightSpeed = 10f;

    [Header("References")]
    public HelicopterController HelicopterController;
    public Rigidbody HelicopterRigidbody;

    private bool isFlying;
    private bool hasReachedDestination;

    void Start()
    {
        if (HelicopterController == null)
            HelicopterController = GetComponent<HelicopterController>();
        if (HelicopterRigidbody == null)
            HelicopterRigidbody = GetComponent<Rigidbody>();

        transform.position = StartPosition;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
            StartFlight();
        if (Input.GetKeyDown(KeyCode.G))
            StopFlight();
    }

    void FixedUpdate()
    {
        if (!isFlying || hasReachedDestination) return;

        float distance = Vector3.Distance(transform.position, EndPosition);
        if (distance < 1f)
        {
            hasReachedDestination = true;
            HelicopterController.EngineForce = 0f;
            return;
        }

        HelicopterController.EngineForce = EngineForce;

        Vector3 direction = (EndPosition - transform.position).normalized;
        Vector3 movement = direction * FlightSpeed * Time.fixedDeltaTime;
        HelicopterRigidbody.MovePosition(transform.position + movement);

        Quaternion targetRotation = Quaternion.LookRotation(direction);
        HelicopterRigidbody.MoveRotation(Quaternion.Slerp(transform.rotation, targetRotation, Time.fixedDeltaTime * 5f));
    }

    public void StartFlight()
    {
        isFlying = true;
        hasReachedDestination = false;
        HelicopterController.EngineForce = EngineForce;
    }

    public void StopFlight()
    {
        isFlying = false;
        HelicopterController.EngineForce = 0f;
    }
}