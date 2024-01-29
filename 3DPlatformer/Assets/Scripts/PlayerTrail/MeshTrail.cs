using System.Collections;
using UnityEngine;

public class MeshTrail : MonoBehaviour
{
    //public float activeTime = 2F;
    public float meshRefreshRate = 0.1F;
    public float meshDestroyDelay = 3F;
    public Transform positionToSpawn;
    public Material mat;
    public string shaderVarRef;
    public float shaderVarRate = 0.1F;
    public float shaderVarRefreshRate = 0.05F;

    private bool isTrailActive;
    private SkinnedMeshRenderer[] skinnedMeshRenderers;


    public void StartTrail(float time)
    {
        if (!isTrailActive)
        {
            isTrailActive = true;
            StartCoroutine(ActivateTrail(time));
        }
    }

    IEnumerator ActivateTrail(float timeActive)
    {
        skinnedMeshRenderers ??= GetComponentsInChildren<SkinnedMeshRenderer>();

        while (timeActive > 0)
        {
            timeActive -= meshRefreshRate;

            foreach (var skinnedMeshRenderer in skinnedMeshRenderers)
            {
                GameObject obj = new GameObject();
                obj.transform.SetPositionAndRotation(positionToSpawn.position, positionToSpawn.rotation);

                var mr = obj.AddComponent<MeshRenderer>();
                var mf = obj.AddComponent<MeshFilter>();

                var mesh = new Mesh();
                skinnedMeshRenderer.BakeMesh(mesh);
                mf.mesh = mesh;
                mr.material = mat;
                mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

                StartCoroutine(AnimateMaterialFloat(mr.material, 0, shaderVarRate, shaderVarRefreshRate));

                Destroy(obj, meshDestroyDelay);
            }

            yield return new WaitForSeconds(meshRefreshRate);
        }

        isTrailActive = false;
    }

    IEnumerator AnimateMaterialFloat(Material mat, float goal, float rate, float refreshRate)
    {
        float valueToAnimate = mat.GetFloat(shaderVarRef);

        while (valueToAnimate > goal)
        {
            valueToAnimate -= rate;
            mat.SetFloat(shaderVarRef, valueToAnimate);

            yield return new WaitForSeconds(refreshRate);
        }
    }
}
