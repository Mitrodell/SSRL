using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public sealed class LaserBeamVFX : MonoBehaviour
{
    [Header("Visual")]
    [SerializeField] private float baseWidth = 0.08f;
    [Header("Impact FX (optional)")]
    [SerializeField] private ParticleSystem impactFx;

    private LineRenderer lr;

    private void Awake()
    {
        lr = GetComponent<LineRenderer>();
        lr.positionCount = 2;
        lr.enabled = false;
    }

    public void SetActive(bool on)
    {
        if (lr != null) lr.enabled = on;
        if (!on && impactFx != null) impactFx.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    }

    public void SetBeam(Vector3 start, Vector3 end, bool impact)
    {
        if (lr == null) return;

        lr.SetPosition(0, start);
        lr.SetPosition(1, end);

        lr.startWidth = baseWidth;
        lr.endWidth = baseWidth;

        if (impactFx != null)
        {
            if (impact)
            {
                impactFx.transform.position = end;
                if (!impactFx.isPlaying) impactFx.Play();
            }
            else
            {
                if (impactFx.isPlaying) impactFx.Stop();
            }
        }
    }
}
