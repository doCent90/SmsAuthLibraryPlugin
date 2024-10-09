/*
UniGif
Copyright (c) 2015 WestHillApps (Hironari Nishioka)
This software is released under the MIT License.
http://opensource.org/licenses/mit-license.php
*/

using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UniGifTest : MonoBehaviour
{
    [SerializeField]
    private InputField m_inputField;
    [SerializeField]
    private UniGifImage m_uniGifImage;

    public IEnumerator ViewGifCoroutine()
    {
        yield return StartCoroutine(m_uniGifImage.SetGifFromUrlCoroutine(m_inputField.text));
    }
}
