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
    }
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
    public void SetBaseUnitInfoPanel(BaseUnitInfo bsi)
    {
        nameText.text = bsi.baseUnitName;

        foreach (var button in activeActionButtons)
        {
            button.transform.SetParent(disabledActionButtonPool.transform);
            disabledActionButtons.Add(button);
        }
        activeActionButtons.Clear();

        foreach (var type in bsi.actionTypes)
        {
            ActionButton button = actionTypeToButton[type];
            button.transform.SetParent(activeActionButtonPool.transform);
            disabledActionButtons.Remove(button);
            activeActionButtons.Add(button);
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
}
