using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class AttemptsReturnOverhaul : MonoBehaviour
{
    public GameObject TextPrefab;
    public Color LetterColor;
    public Color WordColor;
    public Color SeparationColor;
    public Transform AttemptsSpawnParent;
    public List<TextMeshProUGUI> Texts = new List<TextMeshProUGUI>();

    public void SpawnNewText(string context,bool IsWord)
    {
        int CurrentIndexTarget;
        //spawn the text
        Texts.Add(Instantiate(TextPrefab, Vector3.zero, Quaternion.identity, AttemptsSpawnParent).GetComponent<TextMeshProUGUI>());
        CurrentIndexTarget = Texts.Count - 1;
        Texts[CurrentIndexTarget].text = $" {context}";
        Texts[CurrentIndexTarget].rectTransform.sizeDelta = new Vector2(Texts[CurrentIndexTarget].preferredWidth, 50);

        //differences between letters/words
        if (!IsWord)
        {
            Texts[CurrentIndexTarget].color = LetterColor;
        }
        else
        {
            Texts[CurrentIndexTarget].color = WordColor;
        }

        //spawn separation slash betweem texts
        Texts.Add(Instantiate(TextPrefab, Vector3.zero, Quaternion.identity, AttemptsSpawnParent).GetComponent<TextMeshProUGUI>());
        CurrentIndexTarget = Texts.Count - 1;
        Texts[CurrentIndexTarget].color = SeparationColor;
        Texts[CurrentIndexTarget].text = $"/";
        Texts[CurrentIndexTarget].rectTransform.sizeDelta = new Vector2(Texts[CurrentIndexTarget].preferredWidth, 50);

        CheckAndDeleteOverflow();
    }
    public void CheckAndDeleteOverflow()
    {
        for (int i = 0; i < Texts.Count; i++)
        {
            float comparingValue = Texts[i].transform.localPosition.x;
            if (comparingValue > -900 && comparingValue < -550)
            {
                GameObject destroyTarget0 = Texts[i].transform.gameObject;
                GameObject destroyTarget1 = Texts[i + 1].transform.gameObject;
                Texts.RemoveRange(i,2);
                Destroy(destroyTarget0);
                Destroy(destroyTarget1);
                i = 0;
            }
        }
    }
}
