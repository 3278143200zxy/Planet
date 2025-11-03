using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShowItemNode : MonoBehaviour
{
    public Image image;
    public Text numberText;
    public int number = 0;

    public void SetShowItemNode(Sprite _sprite, Transform _parent)
    {
        image.sprite = _sprite;
        transform.SetParent(_parent);
        transform.localScale = Vector3.one;
    }
    public void AddNumber(int _number)
    {
        number += _number;
        numberText.text = number.ToString();
    }
}
