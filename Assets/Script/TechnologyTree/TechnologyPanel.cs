using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class TechnologyPanel : MonoBehaviour
{
    public GameObject technologyNodeContainer;
    public List<TechnologyNode> technologyTreeNodes = new List<TechnologyNode>();

    public GameObject lineContainer;
    public UILine linePrefab;

    public TechnologyNode initialTechnologyNode;
    public TechnologyNode currentTechnologyNode;

    public Button technologyButton;
    public Slider technologyProgressSlider;

    public Text technologyButtonText;
    public Text technologyNameText;

    public GameObject selectionBox;

    private void Awake()
    {
        technologyTreeNodes = new List<TechnologyNode>(technologyNodeContainer.GetComponentsInChildren<TechnologyNode>());

        foreach (var node in technologyTreeNodes)
        {
            foreach (var parent in node.parents)
            {
                UILine line = Instantiate(linePrefab, lineContainer.transform);
                line.SetLine(node.transform.position, parent.transform.position);
            }
        }
    }
    private void Start()
    {
        OnTechnologyNodeClick(initialTechnologyNode);
    }
    public void OnTechnologyNodeClick(TechnologyNode node)
    {
        currentTechnologyNode = node;
        if (node == TechnologyManager.instance.technologyNode) technologyButtonText.text = "Cancel";
        else technologyButtonText.text = "Research";
        technologyNameText.text = node.technologyName;

        selectionBox.transform.SetParent(node.transform);
        selectionBox.transform.localPosition = Vector3.zero;
        selectionBox.transform.localScale = Vector3.one;

        RefreshDisplay();
    }
    public void OnResearchButtonClick()
    {
        if (currentTechnologyNode != TechnologyManager.instance.technologyNode) OnStartResearch();
        else OnCancelResearch();
    }
    public void OnStartResearch()
    {
        if (TechnologyManager.instance.technologyNode != null)
        {
            TechnologyManager.instance.technologyNode.isResearching = false;
            TechnologyManager.instance.technologyNode.InvertColor();
        }
        else TechnologyManager.instance.startResearchEvent.Invoke();

        TechnologyManager.instance.technologyNode = currentTechnologyNode;
        currentTechnologyNode.isResearching = true;
        currentTechnologyNode.InvertColor();

        RefreshDisplay();
    }
    public void OnCancelResearch()
    {
        TechnologyManager.instance.cancelResearchEvent.Invoke();

        currentTechnologyNode.isResearching = false;
        currentTechnologyNode.InvertColor();
        TechnologyManager.instance.technologyNode = null;

        RefreshDisplay();
    }
    public void RefreshDisplay()
    {
        if (currentTechnologyNode == null) return;

        technologyProgressSlider.value = currentTechnologyNode.progress / currentTechnologyNode.totalProgress;

        bool isShowButton = true;
        foreach (var node in currentTechnologyNode.parents) if (!node.isResearched) isShowButton = false;
        if (currentTechnologyNode.isResearched) isShowButton = false;
        technologyButton.gameObject.SetActive(isShowButton);

        if (currentTechnologyNode.isResearching) technologyButtonText.text = "Cancel";
        else technologyButtonText.text = "Research";

    }
}

