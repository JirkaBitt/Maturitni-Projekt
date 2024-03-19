using TMPro;
using UnityEngine;
using Image = UnityEngine.UI.Image;

public class ShowPlayerResult : MonoBehaviour
{
    public GameObject iconTexture;
    public GameObject playerName;
    public GameObject playerPlacement;
    public GameObject playerDecision;
    public void showResult(GameObject player, int placement, string playerNick, GameObject contentHolder)
    {
        //instantiate and adjust the prefab to display the player result with his icon and his decision if he wants to play again
        gameObject.name = player.name + "-result";
        PlayerStats stats = player.GetComponent<PlayerStats>();
        Sprite playerSprite = player.GetComponent<SpriteRenderer>().sprite;
        Vector2 spriteSize = playerSprite.bounds.size;
        float spriteMultiplier = 30 / spriteSize.y;
        float width = spriteSize.x * spriteMultiplier;
        iconTexture.GetComponent<RectTransform>().sizeDelta = new Vector2(width, 30);
        iconTexture.GetComponent<Image>().sprite = playerSprite;
        TextMeshProUGUI place = playerPlacement.GetComponent<TextMeshProUGUI>();
        place.SetText(placement + ".");
        TextMeshProUGUI name = playerName.GetComponent<TextMeshProUGUI>();
        name.SetText(playerNick + " - Score: " + stats.score);
        gameObject.transform.parent = contentHolder.transform;
        gameObject.transform.localPosition = new Vector3(155, -25 - (placement - 1) * 40);
        gameObject.transform.localScale = Vector3.one;
    }
    public void voteYes()
    {
        playerDecision.GetComponent<UnityEngine.UI.Image>().color = Color.green;
    }

    public void voteNo()
    {
        playerDecision.GetComponent<UnityEngine.UI.Image>().color = Color.red;
    }
}
