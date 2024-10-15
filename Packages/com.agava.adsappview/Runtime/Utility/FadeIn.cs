using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public static class FadeIn
{
    public static IEnumerator FadeInGraphic(Graphic graphicObject, float time)
    {
        graphicObject.enabled = true;

        graphicObject.gameObject.SetActive(true);
        Color color = graphicObject.color;

        if (time > 0)
        {
            color.a = 0;
            float elapsedTime = 0;

            while (elapsedTime < time)
            {
                color.a = Mathf.Lerp(0, 1, elapsedTime / time);
                graphicObject.color = color;
                elapsedTime += Time.unscaledDeltaTime;
                yield return new WaitForEndOfFrame();
            }
        }

        color.a = 1;
        graphicObject.color = color;
    }
}
