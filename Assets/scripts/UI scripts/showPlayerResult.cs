using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;
using Image = UnityEngine.UI.Image;

public class showPlayerResult : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject iconTexture;
    public GameObject playerName;
    public GameObject playerPlacement;
    public GameObject playerDecision;
    
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void showResult(GameObject player, int placement, string playerNick, GameObject contentHolder)
    {
        gameObject.name = player.name + "-result";
        Texture2D playerTexture = player.GetComponent<SpriteRenderer>().sprite.texture;
        Sprite playerSprite = player.GetComponent<SpriteRenderer>().sprite;
        Vector2 spriteSize = playerSprite.bounds.size;
        float spriteMultiplier = 30 / spriteSize.y;
        float width = spriteSize.x * spriteMultiplier;
        iconTexture.GetComponent<RectTransform>().sizeDelta = new Vector2(width, 30);
        iconTexture.GetComponent<Image>().sprite = playerSprite;
        TextMeshProUGUI place = playerPlacement.GetComponent<TextMeshProUGUI>();
        place.SetText(placement + ".");
        
        TextMeshProUGUI name = playerName.GetComponent<TextMeshProUGUI>();
        name.SetText(playerNick);

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
