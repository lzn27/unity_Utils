using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class DrawImage : MonoBehaviour
{
    [ContextMenu("GetWhiteImage")]
    public void GetWhiteImage()
    {
        var img = GetComponent<Image>();
        var pixels = img.sprite.texture.GetPixels();
        for (int i = 0; i < pixels.Length; i++)
        {
            var color = pixels[i];
            if (color.a != 0f)
            {
                pixels[i] = Color.white;
            }
        }

        Texture2D te = new Texture2D(img.sprite.texture.width, img.sprite.texture.height);
        te.SetPixels(pixels);
        var fileBytes = te.EncodeToPNG();
        File.WriteAllBytes("E:/log/white.png", fileBytes);
    }
    
    
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
