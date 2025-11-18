using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class BaseUnitButtonNode
{
    public ActionType actionType;
    public ActionButton actionButton;
}

public class BaseUnitInfoPanel : MonoBehaviour
{
    public Text nameText;
    public Text descriptionText;

    public List<BaseUnitButtonNode> baseUnitButtonNodes = new List<BaseUnitButtonNode>();
    public Dictionary<ActionType, ActionButton> actionTypeToButton = new Dictionary<ActionType, ActionButton>();

    public GameObject activeActionButtonPool;
    public List<ActionButton> activeActionButtons = new List<ActionButton>();
    public GameObject disabledActionButtonPool;
    public List<ActionButton> disabledActionButtons = new List<ActionButton>();

    public Transform showWorkProgressTextsPool;
    public List<Text> showWorkProgressTexts = new List<Text>();
    private void Awake()
    {
        foreach (BaseUnitButtonNode bubn in baseUnitButtonNodes)
        {
            actionTypeToButton[bubn.actionType] = bubn.actionButton;
            disabledActionButtons.Add(bubn.actionButton);
            bubn.actionButton.transform.SetParent(disabledActionButtonPool.transform);
            bubn.actionButton.actionType = bubn.actionType;
        }
        gameObject.SetActive(false);

        showWorkProgressTexts = new List<Text>(showWorkProgressTextsPool.GetComponentsInChildren<Text>());
        DisableAllShowWorkProgressTexts();
    }
    public void SetBaseUnitInfoPanel(BaseUnitInfo bsi)
    {
        DisableAllActionButtons();

        nameText.text = bsi.baseUnitName;
        descriptionText.text = bsi.baseUnitDescription;

        foreach (var type in bsi.actionTypes) ActivateActionButton(type);

        DisableAllShowWorkProgressTexts();
    }
    public void SetBaseUnitInfoPanel(BaseUnitInfo bsi, BaseUnit bu)
    {
        DisableAllActionButtons();
        DisableAllShowWorkProgressTexts();

        nameText.text = bsi.baseUnitName;
        descriptionText.text = bsi.baseUnitDescription;

        foreach (var type in bsi.actionTypes) ActivateActionButton(type);

        DisableAllShowWorkProgressTexts();
        switch (bu)
        {
            case PlacedObject po:
                ActivateShowWorkProgressTexts(po.requiredItemTypeToNumber.Count + 1);
                showWorkProgressTexts[0].text = "Work left:" + po.buildingProgress.ToString() + "/" + po.totalBuildingProgress.ToString();
                int i = 0;
                foreach (var itemType in po.requiredItemTypeToNumber.Keys)
                {
                    i++;
                    if (po.itemTypeToNumber.ContainsKey(itemType)) showWorkProgressTexts[i].text = itemType.ToString() + ": " + po.itemTypeToNumber[itemType].ToString() + "/" + po.requiredItemTypeToNumber[itemType].ToString();
                    else showWorkProgressTexts[i].text = itemType.ToString() + ": " + 0 + "/" + po.requiredItemTypeToNumber[itemType].ToString();

                }
                break;
            default:
                break;
        }
    }
    public void ActivateActionButton(ActionType type)
    {
        ActionButton button = actionTypeToButton[type];
        if (activeActionButtons.Contains(button)) return;
        disabledActionButtons.Remove(button);
        activeActionButtons.Add(button);
        button.gameObject.transform.SetParent(activeActionButtonPool.transform);
    }
    public void DisableActionButton(ActionType type)
    {
        ActionButton button = actionTypeToButton[type];
        if (disabledActionButtons.Contains(button)) return;
        activeActionButtons.Remove(button);
        disabledActionButtons.Add(button);
        button.gameObject.transform.SetParent(disabledActionButtonPool.transform);
    }
    public void DisableAllActionButtons()
    {
        foreach (ActionType type in System.Enum.GetValues(typeof(ActionType))) DisableActionButton(type);
    }
    public void DisableAllShowWorkProgressTexts()
    {
        foreach (var text in showWorkProgressTexts) text.gameObject.SetActive(false);

    }
    public void ActivateShowWorkProgressTexts(int number)
    {
        for (int i = 0; i < number; i++) showWorkProgressTexts[i].gameObject.SetActive(true);
    }
}
