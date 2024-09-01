using UnityEngine;

namespace DriftCar.Car
{
    public class Wheel : MonoBehaviour
    {
        public enum WheelPlace
        {
            Forward = 1,
            Backward = 2
        }

        private float _maxMotorTorque;
        private float _maxSteeringAngle;
        private float _brakeForce;
        private float _steeringReturnSpeed;
        private float _decelerationRate;
        private float _reverseTorqueFactor;
        private float _reverseThresholdSpeed;
        private Rigidbody _rbCar;
        private WheelCollider _collider;

        private float _currentBrakeForce;

        [SerializeField] private WheelPlace _wheelPlace;
        [SerializeField] private Transform _wheelTransform;

        private void Awake()
        {
            _rbCar = transform.GetComponentInParent<Rigidbody>();
            _collider = GetComponent<WheelCollider>();
            ConfigureWheelFriction(_collider);
        }

        private void ConfigureWheelFriction(WheelCollider wheelCollider)
        {
            // Forward Friction
            WheelFrictionCurve forwardFriction = wheelCollider.forwardFriction;
            forwardFriction.extremumSlip = 0.2f;  // Точка ковзання, після якої починається ковзання колеса
            forwardFriction.extremumValue = 1f;   // Максимальне тертя до досягнення ковзання
            forwardFriction.asymptoteSlip = 2f; // Точка ковзання, після якої колеса починають значно втрачати зчеплення
            forwardFriction.asymptoteValue = 0.5f; // Тертя під час ковзання
            forwardFriction.stiffness = 1.2f;      // Жорсткість шини, регулює загальний вплив тертя
            wheelCollider.forwardFriction = forwardFriction;

            // Sideways Friction
            WheelFrictionCurve sidewaysFriction = wheelCollider.sidewaysFriction;
            sidewaysFriction.extremumSlip = 0.2f;
            sidewaysFriction.extremumValue = 1f;
            sidewaysFriction.asymptoteSlip = 0.5f;
            sidewaysFriction.asymptoteValue = 0.75f;
            sidewaysFriction.stiffness = 1f;
            wheelCollider.sidewaysFriction = sidewaysFriction;
        }

        public void ConfigureWheelStats(float maxMotorTorque,
                                        float maxSteeringAngle,
                                        float brakeForce,
                                        float steeringReturnSpeed,
                                        float decelerationRate,
                                        float reverseTorqueFactor,
                                        float reverseThresholdSpeed)
        {
            _maxMotorTorque = maxMotorTorque;
            _maxSteeringAngle = maxSteeringAngle;
            _brakeForce = brakeForce;
            _steeringReturnSpeed = steeringReturnSpeed;
            _decelerationRate = decelerationRate;
            _reverseTorqueFactor = reverseTorqueFactor;
            _reverseThresholdSpeed = reverseThresholdSpeed;
        }

        public void ApplyMotorTorque(float accelerationInput, float brakeAndReverseInput)
        {
            if (accelerationInput > 0)
            {
                _collider.motorTorque = _maxMotorTorque * accelerationInput;
            }
            else if (brakeAndReverseInput > 0)
            {
                if (_rbCar.linearVelocity.magnitude > _reverseThresholdSpeed &&
                    Vector3.Dot(_rbCar.linearVelocity, transform.forward) > 0)
                {
                    // Гальмуємо, якщо автомобіль рухається вперед
                    _collider.brakeTorque = _brakeForce;
                }
                else
                {
                    // Рухаємося назад, якщо швидкість мала або автомобіль уже зупинився
                    _collider.brakeTorque = 0f;
                    _collider.motorTorque = -_maxMotorTorque * _reverseTorqueFactor;
                }
            }
            else
            {
                // Якщо нічого не натиснуто, обнуляємо крутний момент на колесах
                _collider.motorTorque = 0f;
                _collider.brakeTorque = 0f;
            }
        }

        public void ApplySteering(float steeringInput)
        {
            if (_wheelPlace != WheelPlace.Forward) return;

            float speedFactor = _rbCar.velocity.magnitude / 10f; // Нелінійний коефіцієнт для контролю повороту
            float adjustedSteeringAngle = Mathf.Lerp(_maxSteeringAngle, _maxSteeringAngle * 0.5f, speedFactor);

            if (steeringInput == 0)
            {
                _collider.steerAngle = Mathf.Lerp(_collider.steerAngle, 0, Time.deltaTime * _steeringReturnSpeed * 1.5f);
            }
            else
            {
                float steering = adjustedSteeringAngle * steeringInput;
                _collider.steerAngle = steering;

                if (Mathf.Abs(steeringInput) < 0.5f)
                {
                    _collider.steerAngle *= 0.7f;
                }
            }
        }

        public void SmoothDeceleration(float accelerationInput, float brakeAndReverseInput)
        {
            if (accelerationInput == 0 && brakeAndReverseInput == 0)
            {
                _currentBrakeForce = Mathf.Lerp(_currentBrakeForce, _brakeForce, Time.deltaTime * _decelerationRate);
                _collider.brakeTorque = _currentBrakeForce;
            }
            else if (brakeAndReverseInput == 0)
            {
                _currentBrakeForce = 0f; // Скидаємо плавне гальмування, коли натиснута кнопка прискорення або гальма
                _collider.brakeTorque = _currentBrakeForce;
            }
        }

        public void RotateWheels()
        {
            Vector3 position;
            Quaternion rotation;
            _collider.GetWorldPose(out position, out rotation);
            _wheelTransform.position = position;
            _wheelTransform.rotation = rotation;
        }

        public void ApplyVisualSteering()
        {
            if (_wheelPlace != WheelPlace.Forward) return;

            Vector3 localEulerAngles = _collider.transform.localEulerAngles;
            localEulerAngles.y = _collider.steerAngle; // Поворот навколо осі Y
            _collider.transform.localEulerAngles = localEulerAngles;
        }
    } 
}
