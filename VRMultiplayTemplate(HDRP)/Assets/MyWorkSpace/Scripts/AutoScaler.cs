using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoScaler : MonoBehaviour
{
    [SerializeField]
    private float defaultHeight = 1.8f;
    [SerializeField]
    private Camera cam;

    private void Resize()
    {
        float headHeight = cam.transform.localPosition.y;
        float scale = defaultHeight / headHeight;
        transform.localScale = Vector3.one * scale;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))

        {
            Resize();
        }
        
    }
}
