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

    public GameObject creaturePool;
    public Slider energySlider;
    public Text energyText;
    public Text taskTypeText;
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
        showWorkProgressTextsPool.gameObject.SetActive(false);
    }
    /*
    public void SetBaseUnitInfoPanel(BaseUnitInfo bsi)
    {
        DisableAllActionButtons();

        nameText.text = bsi.baseUnitName;
        descriptionText.text = bsi.baseUnitDescription;

        foreach (var type in bsi.actionTypes) ActivateActionButton(type);

        DisableAllShowWorkProgressTexts();
    }
    */
    public void SetBaseUnitInfoPanel(BaseUnit bu)
    {
        BaseUnitInfo bsi = bu.baseUnitInfo;

        gameObject.SetActive(true);

        DisableAllActionButtons();

        nameText.text = bsi.baseUnitName;
        descriptionText.text = bsi.baseUnitDescription;

        foreach (var type in bsi.actionTypes) ActivateActionButton(type);

        showWorkProgressTextsPool.gameObject.SetActive(false);
        creaturePool.SetActive(false);

        switch (bu)
        {
            case PlacedObject po:
                SetPlacedObject(po);
                break;
            case Creature c:
                SetCreature(c);
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
    public void SetPlacedObject(PlacedObject placedObject)
    {
        showWorkProgressTextsPool.gameObject.SetActive(true);

        showWorkProgressTexts[0].text = "Work left:" + placedObject.buildingProgress.ToString() + "/" + placedObject.totalBuildingProgress.ToString();
        showWorkProgressTexts[0].gameObject.SetActive(true);

        int i = 0;
        foreach (var itemType in placedObject.requiredItemTypeToNumber.Keys)
        {
            i++;
            if (placedObject.itemTypeToNumber.ContainsKey(itemType)) showWorkProgressTexts[i].text = itemType.ToString() + ": " + placedObject.itemTypeToNumber[itemType].ToString() + "/" + placedObject.requiredItemTypeToNumber[itemType].ToString();
            else showWorkProgressTexts[i].text = itemType.ToString() + ": " + 0 + "/" + placedObject.requiredItemTypeToNumber[itemType].ToString();
            showWorkProgressTexts[i].gameObject.SetActive(true);
        }
        for (int j = i + 1; j < showWorkProgressTexts.Count; j++) showWorkProgressTexts[j].gameObject.SetActive(false);
    }
    public void SetCreature(Creature creature)
    {
        creaturePool.SetActive(true);
        energySlider.value = creature.energy / 1f;
        float tempEnergy = creature.energy * 100f;
        energyText.text = tempEnergy.ToString("F0");

        taskTypeText.text = creature.task.taskType.ToString();
    }
}
