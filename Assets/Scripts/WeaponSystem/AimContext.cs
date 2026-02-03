using UnityEngine;

public struct AimContext
{
    public Transform owner;       // игрок
    public Transform muzzle;      // точка выстрела (shootPoint)
    public Camera cam;            // камера
    public Vector3 aimPoint;      // точка прицела (raycast из центра экрана)
}
