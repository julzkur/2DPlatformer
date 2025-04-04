using UnityEngine;

public class DropDown : MonoBehaviour
{
    public string oneWayPlatformLayerName = "OneWayLayer";
    public string playerLayerName = "player";

    void Update()
    {
        // Allow the player to drop through the platform
        if (Input.GetAxis("Vertical") < 0)
        {
            Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer(playerLayerName), LayerMask.NameToLayer(oneWayPlatformLayerName), true);
        }
        else
        {
            Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer(playerLayerName), LayerMask.NameToLayer(oneWayPlatformLayerName), false);
        }
    }
}
