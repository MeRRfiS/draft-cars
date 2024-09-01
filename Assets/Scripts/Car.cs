using UnityEngine;
using UnityEngine.InputSystem;

namespace DriftCar.Car
{
    public class Car : MonoBehaviour
    {
        [SerializeField] private Wheel[] _wheels;

        [Header("Stats")]
        public float maxMotorTorque = 1500f;
        public float maxSteeringAngle = 30f;
        public float brakeForce = 3000f;
        public float steeringReturnSpeed = 10f;
        public float decelerationRate = 10f;
        public float driftFactor = 0.95f;
        public float reverseTorqueFactor = 0.5f;
        public float reverseThresholdSpeed = 1f;
        public Rigidbody rb;
        public Transform respawnPoint;

        private CarAction inputActions;
        private float steeringInput;
        private float accelerationInput;
        private float brakeAndReverseInput;
        private bool isDrifting;
        private float downforce = 50f;
        private float driftCorrectionFactor = 0.9f;
        private float stuckTimer = 0f;
        private float stuckThreshold = 5f; // Час, після якого вважається, що машина застрягла
        private float minSpeedThreshold = 0.5f; // Мінімальна швидкість, нижче якої машина вважається застряглою

        void Awake()
        {
            inputActions = new CarAction();
            //// �������� ��䳿 ��� ��������� ���������
            //inputActions.Mobile.Tilt.performed += ctx => OnTilt(ctx.ReadValue<Vector2>());
            //inputActions.Mobile.TouchPress.started += ctx => OnTouchPress(ctx);
            //inputActions.Mobile.TouchPosition.performed += ctx => OnTouchPosition(ctx.ReadValue<Vector2>());

            //// �������� ��䳿 ��� �� ���������
            inputActions.PC.Steering.performed += ctx => steeringInput = ctx.ReadValue<float>();
            inputActions.PC.Steering.canceled += ctx => steeringInput = 0f;

            inputActions.PC.Accelerate.started += ctx => accelerationInput = 1f;
            inputActions.PC.Accelerate.canceled += ctx => accelerationInput = 0f;

            inputActions.PC.Brake.started += ctx => brakeAndReverseInput = 1f;
            inputActions.PC.Brake.canceled += ctx => brakeAndReverseInput = 0f;
        }

        private void Start()
        {
            foreach (var wheel in _wheels)
            {
                wheel.ConfigureWheelStats(maxMotorTorque,
                                          maxSteeringAngle,
                                          brakeForce,
                                          steeringReturnSpeed,
                                          decelerationRate,
                                          reverseTorqueFactor,
                                          reverseThresholdSpeed);
            }
        }

        void OnEnable()
        {
            inputActions.Enable();
            //wheelColliders = GetComponentsInChildren<WheelCollider>();

            // ������������ ������ ���� ���������
            rb.centerOfMass = new Vector3(0, -0.9f, 0);
        }

        void OnDisable()
        {
            inputActions.Disable();
        }

        private void Update()
        {
            foreach (var wheel in _wheels)
            {
                wheel.ConfigureWheelStats(maxMotorTorque,
                                          maxSteeringAngle,
                                          brakeForce,
                                          steeringReturnSpeed,
                                          decelerationRate,
                                          reverseTorqueFactor,
                                          reverseThresholdSpeed);
            }
        }

        private void FixedUpdate()
        {
            foreach (var wheel in _wheels)
            {
                wheel.ApplySteering(steeringInput);
                wheel.ApplyMotorTorque(accelerationInput, brakeAndReverseInput);
                wheel.SmoothDeceleration(accelerationInput, brakeAndReverseInput);
                wheel.RotateWheels();
                wheel.ApplyVisualSteering();
            }

            CheckDrift();
            CheckIfStuck();
        }

        private void CheckIfStuck()
        {
            if (rb.velocity.magnitude < minSpeedThreshold && (accelerationInput >= 0 || brakeAndReverseInput >= 0))
            {
                stuckTimer += Time.deltaTime;
            }
            else
            {
                stuckTimer = 0f;
            }

            if (stuckTimer > stuckThreshold)
            {
                RespawnCar();
            }
        }

        private void RespawnCar()
        {
            // Повернення машини на трасу
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            transform.position = respawnPoint.position;
            transform.rotation = respawnPoint.rotation;
            stuckTimer = 0f;
        }

        private void CheckDrift()
        {
            // Визначаємо кут між напрямком руху автомобіля і його вектором швидкості
            Vector3 velocity = rb.velocity;
            float angle = Vector3.Angle(transform.forward, velocity);

            bool isMovingForward = Vector3.Dot(transform.forward, velocity) > 0;

            // Якщо кут великий і автомобіль рухається, вважаємо, що автомобіль у стані дрифту
            isDrifting = isMovingForward && angle > 30f && velocity.magnitude > 0.9f;

            rb.AddForce(-transform.up * downforce * rb.linearVelocity.magnitude);

            // Якщо автомобіль дрифтує, зменшуємо контроль над рухом автомобіля
            if (isDrifting)
            {
                Debug.Log("I Drifting");
                rb.AddForce(-transform.right * velocity.magnitude * (1f - driftFactor) * driftCorrectionFactor, ForceMode.Acceleration);
            }
        }

        private void OnTilt(Vector2 tilt)
        {
            steeringInput = tilt.x; // ������������� ����� �� �� X ��� �������� ���������
        }

        private void OnTouchPress(InputAction.CallbackContext context)
        {
            // ��������������� ��� ��������� ������������ ��� ������������
            if (context.started)
            {
                accelerationInput = 1f; // ����������� ��� ��������
            }
            else if (context.canceled)
            {
                accelerationInput = 0f; // ³��������� ��� �����������
            }
        }

        private void OnTouchPosition(Vector2 touchPosition)
        {
            // �������� ������� ��� ���������� ��������� �������� �� ������, ���� ���������
        }

        void OnCollisionEnter(Collision collision)
        {
            if (collision.relativeVelocity.magnitude > 5f)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
        }
    } 
}
