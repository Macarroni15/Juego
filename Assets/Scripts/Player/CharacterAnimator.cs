using UnityEngine;

public class CharacterAnimator : MonoBehaviour
{
    public Transform legL;
    public Transform legR;
    public Transform armL;
    public Transform armR;

    public float swingAmount = 20f;
    public float swingSpeed = 10f;

    private Rigidbody rb;
    private float timer;

    private void Start()
    {
        rb = GetComponentInParent<Rigidbody>();
        
        // Find limbs if not assigned
        Transform visuals = transform;
        if (legL == null) legL = visuals.Find("LegL");
        if (legR == null) legR = visuals.Find("LegR");
        if (armL == null) armL = visuals.Find("ArmL");
        if (armR == null) armR = visuals.Find("ArmR");
    }

    private void Update()
    {
        float speed = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z).magnitude;
        
        if (speed > 0.1f)
        {
            timer += Time.deltaTime * speed * swingSpeed;
            float angle = Mathf.Sin(timer) * swingAmount;

            if (legL) legL.localRotation = Quaternion.Euler(angle, 0, 0);
            if (legR) legR.localRotation = Quaternion.Euler(-angle, 0, 0);
            if (armL) armL.localRotation = Quaternion.Euler(-angle, 0, 0);
            if (armR) armR.localRotation = Quaternion.Euler(angle, 0, 0);
        }
        else
        {
            // Reset to resting position
            timer = 0;
            if (legL) legL.localRotation = Quaternion.Slerp(legL.localRotation, Quaternion.identity, Time.deltaTime * 5f);
            if (legR) legR.localRotation = Quaternion.Slerp(legR.localRotation, Quaternion.identity, Time.deltaTime * 5f);
            if (armL) armL.localRotation = Quaternion.Slerp(armL.localRotation, Quaternion.identity, Time.deltaTime * 5f);
            if (armR) armR.localRotation = Quaternion.Slerp(armR.localRotation, Quaternion.identity, Time.deltaTime * 5f);
        }
    }
}
