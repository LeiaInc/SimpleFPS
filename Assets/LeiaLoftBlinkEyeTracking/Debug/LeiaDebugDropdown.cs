using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LeiaDebugDropdown : MonoBehaviour
{
    Dropdown dropdown;
    enum Option { Cube = 0, Rainbow = 1, Cup = 2, GrayPattern = 3, View0White = 4};
    Option optionSelected;

    public GameObject debugUI;

    public LeiaVirtualDisplay leiaVirtualDisplay;
    public GameObject cube;
    public GameObject rainbowPattern;
    public GameObject cup;
    public GameObject grayPattern;
    public GameObject view0White;

    // Start is called before the first frame update
    void Start()
    {
        dropdown = GetComponent<Dropdown>();
    }

    public void OnValueChanged()
    {
        optionSelected = (Option)dropdown.value;
        leiaVirtualDisplay.cameraZaxisMovement = (optionSelected == Option.Cube); //Disable camera z-axis movement for the media viewer objects
        cube.SetActive(optionSelected == Option.Cube);
        debugUI.SetActive(optionSelected == Option.Cube);
        rainbowPattern.SetActive(optionSelected == Option.Rainbow);
        cup.SetActive(optionSelected == Option.Cup);
        grayPattern.SetActive(optionSelected == Option.GrayPattern);
        view0White.SetActive(optionSelected == Option.View0White);
    }
}
