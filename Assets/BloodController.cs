using UnityEngine;

public class BloodController : MonoBehaviour, IPlaceable
{
    public PlacementType placement;

    public PlacementType GetPlacement()
    {
        return placement;
    }

    // Start is called before the first frame update
    void Start()
    { }

    // Update is called once per frame
    void Update()
    { }
}
