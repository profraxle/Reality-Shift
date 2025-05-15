using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpatialAnchorManager : MonoBehaviour
{
    
    public OVRSpatialAnchor anchorPrefab;

    private List<OVRSpatialAnchor> anchors = new List<OVRSpatialAnchor>();
    private OVRSpatialAnchor lastCreatedAnchor;
    
    public const string numUuidsPlayerPref = "numUuids"; 
    
    AnchorLoader anchorLoader;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void CreateSpatialAnchor()
    {
        Vector3 anchorPosition = new Vector3(0.436f, -0.363f, 0);
        
        OVRSpatialAnchor newAnchor = Instantiate(anchorPrefab,anchorPosition, Quaternion.identity);
        
        StartCoroutine(AnchorCreated(newAnchor));
    }

    public IEnumerator AnchorCreated(OVRSpatialAnchor newAnchor)
    {
        while (!newAnchor.Created && !newAnchor.Localized)
        {
            yield return new WaitForEndOfFrame();
        }
        Guid anchorGuid = newAnchor.Uuid;
        anchors.Add(newAnchor);
        lastCreatedAnchor = newAnchor;
    }

    private void SaveLastCreatedAnchor()
    {
        lastCreatedAnchor.Save((lastCreatedAnchor, success) =>
        {
            if (success)
            {
                Debug.Log("Saved last anchor");
            }
        });
    }

    void SaveUuidToPlayerPrefs(Guid uuid)
    {
        if(!PlayerPrefs.HasKey(numUuidsPlayerPref))
        {
            PlayerPrefs.SetInt(numUuidsPlayerPref,0);
        }
        
        int playerNumUuids = PlayerPrefs.GetInt(numUuidsPlayerPref);
        PlayerPrefs.SetString("uuid"+playerNumUuids,uuid.ToString());
        PlayerPrefs.SetInt(numUuidsPlayerPref, ++playerNumUuids);
    }
}
