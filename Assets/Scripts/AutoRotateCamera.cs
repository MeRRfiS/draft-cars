using Unity.Cinemachine;
using UnityEngine;

public class AutoRotateCamera : MonoBehaviour
{
    [SerializeField] private float _rotationSpeed = 10f;

    private CinemachineOrbitalFollow _orbital;

    private void Start()
    {
        // �������� ��������� Orbital Transposer � ������
        _orbital = GetComponent<CinemachineOrbitalFollow>();
    }

    private void Update()
    {
        // ������� �������� �� X ��� ��������� ������� ��'����
        _orbital.HorizontalAxis.Value += _rotationSpeed * Time.deltaTime;
    }
}
