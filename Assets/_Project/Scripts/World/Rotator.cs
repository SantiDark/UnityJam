using UnityEngine;

public class Rotator : MonoBehaviour
{
    [SerializeField] private float _rotationSpeed = 5f;

    // Update is called once per frame
    void Update()
    {
        this.transform.Rotate(0, _rotationSpeed * Time.deltaTime, 0);
    }
}
