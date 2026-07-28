using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;

public class MiningDrill : MonoBehaviour
{
    public Building building;
    private Planet planet;

    public float drillMoveSpeed;
    public GameObject drill;
    public Transform drillPoint;
    public GameObject dustParticle;

    public Transform fixedPoint;
    public Transform drillContactPoint;
    public GameObject rope;

    public float mineInterval;
    private float mineTimer;
    public float progressPreMineStone;

    public Cell lastCurrentCell;
    public Stone stone;

    private Vector3 dir;
    private void Awake()
    {
        planet = building.planet;

        dir = transform.position - planet.transform.position;
        dir.z = 0;
        dir = dir.normalized;

        dustParticle.SetActive(false);

        lastCurrentCell = planet.PosToCell(drillPoint.position);
        stone = lastCurrentCell.neighbourCellNodes[1].cell.building.GetComponent<Stone>();
    }

    // Update is called once per frame
    void Update()
    {
        Cell currentCell = planet.PosToCell(drillPoint.position);
        Cell belowCell = currentCell.neighbourCellNodes[1].cell;
        float distanceToGround = Vector2.Distance(drillPoint.transform.position, planet.transform.position) - planet.CellRadiusDistance(belowCell.radiusIdx) - planet.CellHeight(belowCell.radiusIdx) / 2f;

        if (lastCurrentCell != currentCell)
        {
            lastCurrentCell = currentCell;
            if (belowCell.building != null) stone = belowCell.building.GetComponent<Stone>();
        }

        if (distanceToGround < 0.05f && stone != null)
        {
            mineTimer += TimeManager.deltaTime;
            if (mineTimer >= mineInterval)
            {
                bool flag = false;
                flag = stone.stoneMineProgress < progressPreMineStone;
                stone.MineStone(progressPreMineStone);
                if (flag) stone = null;
                mineTimer = 0;
            }

            if (!dustParticle.activeInHierarchy) dustParticle.SetActive(true);
        }
        else
        {
            mineTimer = 0;

            drill.transform.position -= drillMoveSpeed * dir * TimeManager.deltaTime;

            if (dustParticle.activeInHierarchy) dustParticle.SetActive(false);
        }

        rope.transform.position = (fixedPoint.transform.position + drillContactPoint.transform.position) / 2;
        rope.transform.localScale = new Vector3(rope.transform.localScale.x, Vector2.Distance(fixedPoint.transform.position, drillContactPoint.transform.position), 1);
    }
}
