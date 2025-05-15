using System;
using UnityEngine;

public class AnchorLoader : MonoBehaviour
{
    private OVRSpatialAnchor anchorPrefab;
    private SpatialAnchorManager spatialAnchorManager;
    
    Action<OVRSpatialAnchor.UnboundAnchor,bool> _onLoadAnchor;

    void Awake()
    {
        spatialAnchorManager = GetComponent<SpatialAnchorManager>();
        anchorPrefab = spatialAnchorManager.anchorPrefab;
        _onLoadAnchor = OnLocalized;
    }

    public void LoadAnchorsByUuid()
    {
        if (!PlayerPrefs.HasKey(SpatialAnchorManager.numUuidsPlayerPref))
        {
            PlayerPrefs.SetInt(SpatialAnchorManager.numUuidsPlayerPref, 0);
        }
        var playerUuidCount = PlayerPrefs.GetInt(SpatialAnchorManager.numUuidsPlayerPref);
        
        if (playerUuidCount == 0)
            return;
        
        var uuids = new Guid[playerUuidCount];
        for (int i = 0; i < playerUuidCount; i++)
        {
            var uuidKey = "uuid" + i;
            var currentUuid = PlayerPrefs.GetString(uuidKey); 
            uuids[i] = new Guid(currentUuid);
        }
        
        Load(new OVRSpatialAnchor.LoadOptions
        {
            Timeout = 0,
            StorageLocation = OVRSpace.StorageLocation.Local,
            Uuids = uuids,
        });
    }

    private async void Load(OVRSpatialAnchor.LoadOptions options)
    {
        var oVRTask = await OVRSpatialAnchor.LoadUnboundAnchorsAsync(options);

        if (oVRTask == null)
        {
            return;
        }

        foreach (var anchor in oVRTask)
        {
            if (anchor.Localized)
            {
                _onLoadAnchor(anchor, true);
            }
            else if (!anchor.Localizing)
            {
                _onLoadAnchor(anchor, await anchor.LocalizeAsync());
            }
        }
    }

    private void OnLocalized(OVRSpatialAnchor.UnboundAnchor anchor, bool localized)
    {
        if (!localized) return;

        var pose = anchor.Pose;
        var spatialAnchor = Instantiate(anchorPrefab,pose.position, pose.rotation);
        anchor.BindTo(spatialAnchor);
    }
}

