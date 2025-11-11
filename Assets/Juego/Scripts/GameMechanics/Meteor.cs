using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Meteor : MonoBehaviour
{
    public float directFallSpeed = 0f;
    public float lifetimeSeconds = 10f;
    public float despawnY = -6f;
    public int dodgePoints = 1;

    Rigidbody body;
    bool collidedWithPlayer;
    bool isProcessingCollision;

    void Awake()
    {
        body = GetComponent<Rigidbody>();
    }

    void Start()
    {
        startTime = Time.time;
        if (directFallSpeed > 0f)
        {
            body.isKinematic = false;
            body.useGravity = false;
            body.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
            body.interpolation = RigidbodyInterpolation.Interpolate;
            body.linearVelocity = Vector3.down * directFallSpeed;
        }
    }

    void FixedUpdate()
    {
        if (directFallSpeed > 0f)
        {
            body.linearVelocity = Vector3.down * directFallSpeed;
        }

        // Destruir si está fuera de la pantalla o ha pasado mucho tiempo
        if (transform.position.y <= despawnY || Time.time - startTime > lifetimeSeconds)
        {
            if (!collidedWithPlayer)
            {
                ScoreManager.Instance?.AddPoints(dodgePoints);
                GameSession.Instance?.RegisterMeteorDodged();
            }
            Destroy(gameObject);
        }
    }

    private float startTime;

    void OnCollisionEnter(Collision collision)
    {
        if (isProcessingCollision) return;
        isProcessingCollision = true;

        if (collision.collider.CompareTag("Player"))
        {
            collidedWithPlayer = true;
            if (GameManager.Instance != null)
            {
                GameManager.Instance.HandlePlayerHit();
            }
        }
        Destroy(gameObject);
    }
}