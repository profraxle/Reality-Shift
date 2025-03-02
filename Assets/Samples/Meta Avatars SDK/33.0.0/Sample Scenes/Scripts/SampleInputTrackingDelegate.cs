#nullable enable

#if USING_XR_MANAGEMENT && USING_XR_SDK_OCULUS && !OVRPLUGIN_UNSUPPORTED_PLATFORM
#define USING_XR_SDK
#endif

using Oculus.Avatar2;
using UnityEngine;
using Node = UnityEngine.XR.XRNode;

/*
 *
 */
public class SampleInputTrackingDelegate : OvrAvatarInputTrackingDelegate
{

#if USING_XR_SDK
    private OVRCameraRig? _ovrCameraRig = null;
#endif

#if USING_XR_SDK
    public SampleInputTrackingDelegate(OVRCameraRig? ovrCameraRig)
    {
        _ovrCameraRig = ovrCameraRig;
    }
#endif
    public override bool GetRawInputTrackingState(out OvrAvatarInputTrackingState inputTrackingState)
    {
        inputTrackingState = default;
#if USING_XR_SDK
        bool leftControllerActive = true;
        bool rightControllerActive = true;
        if (OVRInput.GetActiveController() != OVRInput.Controller.Hands)
        {
            leftControllerActive = OVRInput.GetControllerOrientationTracked(OVRInput.Controller.LHand);
            
            rightControllerActive = OVRInput.GetControllerOrientationTracked(OVRInput.Controller.RHand);
        }

        if (_ovrCameraRig is not null)
        {
            inputTrackingState.headsetActive = true;
            inputTrackingState.leftControllerActive = leftControllerActive;
            inputTrackingState.rightControllerActive = rightControllerActive;
            inputTrackingState.leftControllerVisible = false;
            inputTrackingState.rightControllerVisible = false;
            inputTrackingState.headset = (CAPI.ovrAvatar2Transform)_ovrCameraRig.centerEyeAnchor;
            inputTrackingState.leftController = (CAPI.ovrAvatar2Transform)_ovrCameraRig.leftHandAnchor;
            inputTrackingState.rightController = (CAPI.ovrAvatar2Transform)_ovrCameraRig.rightHandAnchor;
            return true;
        }
        else if (OVRNodeStateProperties.IsHmdPresent())
        {
            inputTrackingState.headsetActive = true;
            inputTrackingState.leftControllerActive = leftControllerActive;
            inputTrackingState.rightControllerActive = rightControllerActive;
            inputTrackingState.leftControllerVisible = true;
            inputTrackingState.rightControllerVisible = true;

            if (OVRNodeStateProperties.GetNodeStatePropertyVector3(Node.CenterEye, NodeStatePropertyType.Position,
                OVRPlugin.Node.EyeCenter, OVRPlugin.Step.Render, out var headPos))
            {
                inputTrackingState.headset.position = headPos;
            }
            else
            {
                inputTrackingState.headset.position = Vector3.zero;
            }

            if (OVRNodeStateProperties.GetNodeStatePropertyQuaternion(Node.CenterEye, NodeStatePropertyType.Orientation,
                OVRPlugin.Node.EyeCenter, OVRPlugin.Step.Render, out var headRot))
            {
                inputTrackingState.headset.orientation = headRot;
            }
            else
            {
                inputTrackingState.headset.orientation = Quaternion.identity;
            }

            inputTrackingState.headset.scale = Vector3.one;

            inputTrackingState.leftController.position = OVRInput.GetLocalControllerPosition(OVRInput.Controller.LHand);
            inputTrackingState.rightController.position = OVRInput.GetLocalControllerPosition(OVRInput.Controller.RHand);
            Quaternion leftOrientation = OVRInput.GetLocalControllerRotation(OVRInput.Controller.LHand);
            inputTrackingState.leftController.orientation = Quaternion.Euler(leftOrientation.eulerAngles.x,
                leftOrientation.eulerAngles.y, leftOrientation.eulerAngles.z-90);
            Quaternion rightOrientation = OVRInput.GetLocalControllerRotation(OVRInput.Controller.RHand);
            inputTrackingState.rightController.orientation = Quaternion.Euler(rightOrientation.eulerAngles.x,
                rightOrientation.eulerAngles.y, rightOrientation.eulerAngles.z+90);
            inputTrackingState.leftController.scale = Vector3.one;
            inputTrackingState.rightController.scale = Vector3.one;
            return true;
        }
#endif

        return false;
    }
}
