using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class DoorController : MonoBehaviour, IInteractable
{
    public UnityAction<IInteractable> OnInteractionComplete { get; set; }

    [SerializeField] private BoxCollider _doorCollider;
    [SerializeField] private GameObject _door;
    [SerializeField] private Transform _sideA;
    [SerializeField] private Transform _sideB;
    [SerializeField] private float _openTime = 3f;
    [SerializeField] private float _openSpeed = 1.5f;


    private bool _isOpen = false;
    private Quaternion _closedRotation;

    private void Start()
    {
        _closedRotation = _door.transform.localRotation;
    }
    public void Interact(Interactor interactor, out bool interactSuccessful)
    {
        if (_isOpen)
        {
            interactSuccessful = false;
            return;
        }

        float distA = Vector3.Distance(interactor.transform.position, _sideA.position);
        float distB = Vector3.Distance(interactor.transform.position, _sideB.position);
        float angle = distA < distB ? 90f : -90f;

        StartCoroutine(OpenAndCloseDoor(angle));
        interactSuccessful = true;
    }

    private IEnumerator OpenAndCloseDoor(float angle)
    {
        _isOpen = true;
        _doorCollider.enabled = false;

        Quaternion openRotation = _closedRotation * Quaternion.Euler(0, angle, 0);

        float t = 0;
        while (t < 1f)
        {
            t += Time.deltaTime * _openSpeed;
            _door.transform.localRotation = Quaternion.Lerp(_closedRotation, openRotation, t);
            yield return null;
        }

        yield return new WaitForSeconds(_openTime);

        t = 0;
        while (t < 1f)
        {
            t += Time.deltaTime * _openSpeed;
            _door.transform.localRotation = Quaternion.Lerp(openRotation, _closedRotation, t);
            yield return null;
        }

        _isOpen = false;
        _doorCollider.enabled = true;
    }

    public void EndInteraction() { }
}
