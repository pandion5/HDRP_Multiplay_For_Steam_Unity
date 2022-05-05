using UnityEngine.XR;
using Photon.Pun;

public class LeaveRoomOnInput : MonoBehaviourPunCallbacks
{
    public UnityEngine.XR.Interaction.Toolkit.InputHelpers.Button inputHelpers = UnityEngine.XR.Interaction.Toolkit.InputHelpers.Button.MenuButton;
    public XRNode controller = XRNode.LeftHand;
    
    void Update()
    {
        UnityEngine.XR.Interaction.Toolkit.InputHelpers.IsPressed(InputDevices.GetDeviceAtXRNode(controller), inputHelpers, out bool isPressed);

        if (isPressed)
        {
            PhotonNetwork.Disconnect();
            PhotonNetwork.LoadLevel(0);
        }
    }
}
