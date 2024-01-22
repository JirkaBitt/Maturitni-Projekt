using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

/*public class SendGameobjects : MonoBehaviourPunCallbacks, IPunObservable
{
    public GameObject[] Objects;

    public string[] objectNames;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            //we only need to send position because rotation is the same
            for (int i = 0; i < Objects.Length; i++)
            {
                GameObject curObj = Objects[i];
                Texture2D tex = curObj.GetComponent<SpriteRenderer>().sprite.texture;
                Vector3 sizeTex = new Vector3(tex.width, tex.height, 0);
                PolygonCollider2D coll = curObj.GetComponent<PolygonCollider2D>();
                Vector2[][] paths = new Vector2[][coll.pathCount];
                for (int j = 0; j < coll.pathCount; j++)
                {
                    paths[j] = coll.GetPath(j);
                }
                Byte[] bytes = tex.EncodeToPNG();
                
                stream.SendNext(objectNames[i]);
                stream.SendNext(bytes);
                stream.SendNext(sizeTex);
                stream.SendNext(paths);
            }
        }
        else
        {
            //1) name
            //2) texture
            //3) texture size
            //4) collider points

            string name = (string)stream.ReceiveNext();
            Byte[] texBytes = (Byte[])stream.ReceiveNext();
            Vector3 texSize = (Vector3)stream.ReceiveNext();
            Vector2[][] paths = (Vector2[][])stream.ReceiveNext();
            
        }
    }
}
*/