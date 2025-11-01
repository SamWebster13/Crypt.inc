using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class PortalTrigger : MonoBehaviour
{
    Portal _portal;
    float _lastSide = 0f;
    bool _tracking = false;

    void Awake()
    {
        _portal = GetComponent<Portal>();
        var bc = GetComponent<BoxCollider>();
        bc.isTrigger = true;
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        _tracking = true;
        _lastSide = _portal.SignedSide(other.transform.position);
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        _tracking = false;
    }

    void OnTriggerStay(Collider other)
    {
        if (!_tracking || !other.CompareTag("Player")) return;

        float side = _portal.SignedSide(other.transform.position);
        // crossed the plane if sign changed and moved sufficiently
        if (Mathf.Sign(side) != Mathf.Sign(_lastSide) && Mathf.Abs(side) > 0.02f)
        {
            other.TryGetComponent<CharacterController>(out var cc);
            _portal.Teleport(other.transform, cc);
            _lastSide = _portal.SignedSide(other.transform.position); // reset from new side
        }
        else
        {
            _lastSide = side;
        }
    }
}
