using UnityEngine;

public class VectorPractice : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Vector2 moveHere = new Vector3(0f, 0f);
        moveHere.x -= 4.0f;
        moveHere.y += 2.0f;
        Debug.Log(moveHere);
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetMouseButtonDown(0))
        {
            Debug.Log("MouseClick");
        }
    }
}
