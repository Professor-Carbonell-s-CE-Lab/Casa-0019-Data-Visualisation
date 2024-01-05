using UnityEngine;
using System.Collections.Generic;
using Newtonsoft.Json;
using XCharts.Runtime;
using Unity.VisualScripting;
using System;
using UnityEngine.UI;

public class mqttController : MonoBehaviour
{
    [Tooltip("Optional name for the controller")]
    public string nameController = "Controller 1";
    public string tag_mqttManager = ""; //to be set on the Inspector panel. It must match one of the mqttManager.cs GameObject
    [Header("   Case Sensitive!!")]
    [Tooltip("the topic to subscribe must contain this value. !!Case Sensitive!! ")]
    public string topicSubscribed = ""; //the topic to subscribe, it need to match a topic from the mqttManager
    private float pointerValue = 0.0f;
    private float secondValue = 0.0f;
    private float thirdValue = 0.0f;
    private float fourValue = 0.0f;
    [Space]
    [Space]
    public GameObject objectToControl; //pointer of the gauge, or other 3D models
    [Tooltip("Select the rotation axis of the object to control")]
    public enum State { X, Y, Z };
    public State rotationAxis;
    [Space]
    [Tooltip("Direction Rotation")]
    public bool CW = true; //CW True = 1; CW False = -1
    private int rotationDirection = 1;
    [Space]
    [Space]
    [Tooltip("minimum value on the dial")]
    public float startValue = 0f; //start value of the gauge
    [Tooltip("maximum value on the dial")]
    public float endValue = 180f; // end value of the gauge
    [Tooltip("full extension of the gauge in EulerAngles")]
    public float fullAngle = 180f; // full extension of the gauge in EulerAngles

    [Tooltip("Adjust the origin of the scale. negative values CCW; positive value CW")]
    public float adjustedStart = 0f; // negative values CCW; positive value CW
    [Space]
    public mqttManager _eventSender;

    public LineChart lineChart;

    public ScatterChart scatterChart;
    public RingChart ringChart1;
    public RingChart ringChart2;
    public RingChart ringChart3;
    public RingChart ringChart4;
    public AudioSource audioSourceA;
    public AudioSource audioSourceB;
    private bool audioAPlayed = false;
    private bool audioBPlayed = false;
    public GameObject model;
    private bool modelActivated = false;

    public GameObject popupPanel;
    private bool isPopupShown = false;
    static public GameObject dispopupPanel;

    int count = 0;


    void Awake() 
    {
        dispopupPanel=GameObject.Find("popup");
        //popupPanel=GameObject.FindGameObjectWithTag("popup");
        if (GameObject.FindGameObjectsWithTag(tag_mqttManager).Length > 0)
        {
            _eventSender = GameObject.FindGameObjectsWithTag(tag_mqttManager)[0].gameObject.GetComponent<mqttManager>();
            _eventSender.Connect(); //Connect tha Manager when the object is spawned
        }
        else
        {
            Debug.LogError("At least one GameObject with mqttManager component and Tag == tag_mqttManager needs to be provided");
        }
    }

    void OnEnable()
    {
        _eventSender.OnMessageArrived += OnMessageArrivedHandler;

    }

    private void OnDisable()
    {
        _eventSender.OnMessageArrived -= OnMessageArrivedHandler;
    }
    public void ClosePopup()
    {
    dispopupPanel.gameObject.SetActive(false); 
        
        Debug.Log("Closing popup");
        Debug.Log(popupPanel.activeSelf);
        isPopupShown = false; 
    }
    
    private void OnMessageArrivedHandler(mqttObj mqttObject) //the mqttObj is defined in the mqttManager.cs
    {
        
        //We need to check the topic of the message to know where to use it 
        if (mqttObject.topic.Contains(topicSubscribed))
        {
        
            var jsonData = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, float>>(mqttObject.msg);

            
            if (jsonData != null)
            {
                if (jsonData.ContainsKey("/UCL Virtual ES/IOT-mqtt/1PS/107/Room/CDS/Value"))
                {
                    pointerValue = jsonData["/UCL Virtual ES/IOT-mqtt/1PS/107/Room/CDS/Value"];
                    lineChart.AddData(0, count, pointerValue);
                    if (!modelActivated)
                    {
                        model.SetActive(true);
                        modelActivated = true;
                    }
                    
                    if (pointerValue < 600 && !audioAPlayed) 
                    {
                        audioSourceA.Play();
                        audioAPlayed = true;
                        audioBPlayed = false; 
                        
                    }

                    else if (pointerValue >= 600 && !audioBPlayed)
                    {
                        audioSourceB.Play();
                        audioBPlayed = true;
                        audioAPlayed = false; 
                        popupPanel.SetActive(true);
                        
                    }
                }
                

                if (jsonData.ContainsKey("/UCL Virtual ES/IOT-mqtt/1PS/107/Room/TPS/Value"))
                {
                    float temfour = Convert.ToSingle(jsonData["/UCL Virtual ES/IOT-mqtt/1PS/107/Room/TPS/Value"]);
                    Double formattedValue4 = Convert.ToDouble(temfour.ToString("F2")); 
                    scatterChart.AddData(0, temfour, pointerValue);
                    ringChart1.ClearData();
                    ringChart1.AddData(0, formattedValue4, 40);
                }




                if (jsonData.ContainsKey("/UCL Virtual ES/IOT-mqtt/1PS/004/Room/CDS/Value"))
                {
                    secondValue = jsonData["/UCL Virtual ES/IOT-mqtt/1PS/004/Room/CDS/Value"];
                    // Add data to the second line, assuming it's at index 1
                    lineChart.AddData(1, count, secondValue);
                }


                if (jsonData.ContainsKey("/UCL Virtual ES/IOT-mqtt/1PS/004/Room/TPS/Value"))
                {

                    float temfour = Convert.ToSingle(jsonData["/UCL Virtual ES/IOT-mqtt/1PS/004/Room/TPS/Value"]);
                    Double formattedValue3 = Convert.ToDouble(temfour.ToString("F2")); 
                    scatterChart.AddData(0, temfour, secondValue);
                    ringChart2.ClearData();
                    ringChart2.AddData(0, formattedValue3, 40);
                }


                if (jsonData.ContainsKey("/UCL Virtual ES/IOT-mqtt/1PS/201/NextToOffice201A/CDS/Value"))
                {
                    thirdValue = jsonData["/UCL Virtual ES/IOT-mqtt/1PS/201/NextToOffice201A/CDS/Value"];
                    // Add data to the second line, assuming it's at index 1
                    lineChart.AddData(2, count, thirdValue);
                }

                if (jsonData.ContainsKey("/UCL Virtual ES/IOT-mqtt/1PS/201/NextToOffice201A/TPS/Value"))
                {
                    float temfour = Convert.ToSingle(jsonData["/UCL Virtual ES/IOT-mqtt/1PS/201/NextToOffice201A/TPS/Value"]);
                    Double formattedValue2 = Convert.ToDouble(temfour.ToString("F2")); 
                    scatterChart.AddData(0, temfour, thirdValue);
                    ringChart3.ClearData();
                    ringChart3.AddData(0, formattedValue2, 40);
                }



                if (jsonData.ContainsKey("/UCL Virtual ES/IOT-mqtt/1PS/313/Room/CDS/Value"))
                {
                    fourValue = jsonData["/UCL Virtual ES/IOT-mqtt/1PS/313/Room/CDS/Value"];
                    // Add data to the second line, assuming it's at index 1
                    lineChart.AddData(3, count, fourValue);
                }

                if (jsonData.ContainsKey("/UCL Virtual ES/IOT-mqtt/1PS/313/Room/TPS/Value"))
                {
                    float temfour = Convert.ToSingle(jsonData["/UCL Virtual ES/IOT-mqtt/1PS/313/Room/TPS/Value"]);
                    Double formattedValue1 = Convert.ToDouble(temfour.ToString("F2")); 
                    scatterChart.AddData(0, temfour, fourValue);
                    ringChart4.ClearData();
                    ringChart4.AddData(0, formattedValue1, 40);
                }
                count++;

            }




        }

    }

    private void Update()
    {
        
        float step = 1.5f * Time.deltaTime;
        // ternary conditional operator https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/operators/conditional-operator
        rotationDirection = CW ? 1 : -1;

        if (pointerValue >= startValue)
        {
            Vector3 rotationVector = new Vector3();
            //If the rotation Axis is X
            if (rotationAxis == State.X)
            {
                rotationVector = new Vector3(
                (rotationDirection * ((pointerValue - startValue) * (fullAngle / (endValue - startValue)))) - adjustedStart,
                objectToControl.transform.localEulerAngles.y,
                objectToControl.transform.localEulerAngles.z);
            }
            //If the rotation Axis is Y
            else if (rotationAxis == State.Y)
            {
                rotationVector = new Vector3(
                objectToControl.transform.localEulerAngles.x,
                (rotationDirection * ((pointerValue - startValue) * (fullAngle / (endValue - startValue)))) - adjustedStart,
                objectToControl.transform.localEulerAngles.z);

            }
            //If the rotation Axis is Z
            else if (rotationAxis == State.Z)
            {
                rotationVector = new Vector3(
                objectToControl.transform.localEulerAngles.x,
                objectToControl.transform.localEulerAngles.y,
                (rotationDirection * ((pointerValue - startValue) * (fullAngle / (endValue - startValue)))) - adjustedStart);
            }
            objectToControl.transform.localRotation = Quaternion.Lerp(
                    objectToControl.transform.localRotation,
                    Quaternion.Euler(rotationVector),
                    step);
        }
    }
}
