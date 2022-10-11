
using UnityEngine;
using UnityEngine.UI;

public class AutoContentSize : ContentSizeFitter
{
    public Vector2 startSize;
    public Image image;
    public bool limit= false;
    public Vector2 limitSize = Vector2.one * 100; 

    protected override void OnRectTransformDimensionsChange()
    {
        base.OnRectTransformDimensionsChange();
        SetImageSize();
    }

    public void SetImageSize()
    {
        if (image == null)
            return;

        Vector2 size = startSize + GetComponent<RectTransform>().sizeDelta;

        image.GetComponent<RectTransform>().sizeDelta = size;
        if (!limit)
            return;
        SetLimit(size);
        
    }

    private void SetLimit(Vector2 size)
    {
        if (limitSize.x > 0)
        {
            if (size.x >= limitSize.x)
            {
                if (horizontalFit == FitMode.PreferredSize)
                {
                    horizontalFit = FitMode.Unconstrained;
                    OnRectTransformDimensionsChange();
                    GetComponent<RectTransform>().sizeDelta = new Vector2(limitSize.x,size.y);
                    
                }

            }
        }
    }

}