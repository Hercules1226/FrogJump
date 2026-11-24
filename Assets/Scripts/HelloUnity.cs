using UnityEngine;

public class HelloUnity : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Debug.Log("반갑습니다");
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetMouseButtonDown(0))
        {
            Debug.Log("마우스 주 버튼이 눌렸습니다.");
        }

        if (Input.GetMouseButtonDown(1))
        {
            Debug.Log("마우스 보조 버튼이 눌렸습니다");
        }

        if (Input.GetMouseButtonDown(2))
        {
            Debug.Log("마우스 가운데 버튼이 눌렸습니다");
        }
    }
}
