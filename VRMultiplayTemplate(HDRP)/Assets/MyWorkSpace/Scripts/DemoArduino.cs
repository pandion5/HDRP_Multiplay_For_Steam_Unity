using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO.Ports;

public class DemoArduino : MonoBehaviour
{

    public enum PortNumber
    {
        COM1, COM2, COM3, COM4,
        COM5, COM6, COM7, COM8,
        COM9, COM10, COM11, COM12,
        COM13, COM14, COM15, COM16
    }

    private SerialPort serial;
    private string fanvalue;
    [SerializeField]
    private PortNumber portNumber = PortNumber.COM5;
    [SerializeField]
    private string baudRate = "9600";

    // Start is called before the first frame update
    void Start()
    {
        serial = new SerialPort(portNumber.ToString(), int.Parse(baudRate));
        if (!serial.IsOpen)
        {
            serial.Open();
        }
    }

    private void Update()
    {
        if (!serial.IsOpen)
        {
            serial.Open();
        }
    }

    // Update is called once per frame

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.CompareTag("Tree")) // 나무향
        {
            Debug.Log("충돌");
            fanvalue = "4";
            serial.Write(fanvalue);
            if (!serial.IsOpen)
            {
                fanvalue = "5";
                serial.Write(fanvalue);
            }
            /*            servoValue = "u";
                        serial.Write(servoValue);*/
        }
        if (other.gameObject.CompareTag("Flower")) // 꽃향
        {
            fanvalue = "4";
            serial.Write(fanvalue);
            /*            servoValue = "u";
                        serial.Write(servoValue);*/
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Tree")) //나무향
        {
            Debug.Log("충돌 끝");
            fanvalue = "5";
            serial.Write(fanvalue);
        }
        if (other.gameObject.CompareTag("Flower")) //나무향
        {
            Debug.Log("충돌 끝");
            fanvalue = "5";
            serial.Write(fanvalue);
        }
    }

    private void OnApplicationQuit()
    {
        Debug.Log("작동종료");
        fanvalue = "5";
        serial.Write(fanvalue);
    }
}