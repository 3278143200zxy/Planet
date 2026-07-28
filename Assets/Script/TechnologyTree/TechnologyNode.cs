using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
public class TechnologyNode : MonoBehaviour
{
    public string technologyName = "Technology";
    public string technologyDedscription;

    public Text technologyNameText;
    public Image backgroundImage;
    public Image researchedBackground;

    public List<TechnologyNode> parents = new List<TechnologyNode>();

    public float totalProgress;
    public float progress;

    public bool isResearching = false;
    public bool isResearched = false;

    public List<Building> buildings = new List<Building>();

    private void OnValidate()
    {
        technologyNameText.text = technologyName;
    }
    public void AddProgress(float p)
    {
        progress += p;
        if (progress > totalProgress) OnTechnologyResearched();
    }
    public void OnClick()
    {
        TechnologyManager.instance.technologyPanel.OnTechnologyNodeClick(this);
    }
    public void InvertColor()
    {
        if (isResearching)
        {
            backgroundImage.color = Color.black;
            technologyNameText.color = Color.white;
        }
        else
        {
            backgroundImage.color = Color.white;
            technologyNameText.color = Color.black;
        }
    }
    public void OnTechnologyResearched()
    {
        isResearched = true;
        TechnologyManager.instance.technologyPanel.OnCancelResearch();
        TechnologyManager.instance.OnTechnologyResearched(this);
        researchedBackground.gameObject.SetActive(true);
    }
    private void OnDrawGizmos()
    {
        foreach (var parent in parents)
        {
            if (parent != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawLine(transform.position, parent.transform.position);
            }
        }
    }
}