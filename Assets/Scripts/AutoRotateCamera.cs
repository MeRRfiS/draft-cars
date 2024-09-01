using Unity.Cinemachine;
using UnityEngine;

public class AutoRotateCamera : MonoBehaviour
{
    [SerializeField] private float _rotationSpeed = 10f;

    private CinemachineOrbitalFollow _orbital;

    private void Start()
    {
        // Отримуємо компонент Orbital Transposer з камери
        _orbital = GetComponent<CinemachineOrbitalFollow>();
    }

    private void Update()
    {
        // Змінюємо значення осі X для обертання навколо об'єкта
        _orbital.HorizontalAxis.Value += _rotationSpeed * Time.deltaTime;
    }
}
