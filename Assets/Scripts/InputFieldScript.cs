using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InputFieldScript : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        gameObject.GetComponent<InputField>().onValueChanged.AddListener(UpdateState);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void RestartSimulation()
	{
        SendMessageUpwards("LoadState", state_);
	}

    public void UpdateState(string state)
	{
        state_ = state;
	}

    string state_ = "";
}
