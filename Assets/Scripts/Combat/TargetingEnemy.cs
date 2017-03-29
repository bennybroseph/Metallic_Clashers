﻿using UnityEngine;

namespace Combat
{
    using System.Linq;

    using CustomInput;
    using CustomInput.Information;

    public class TargetingEnemy : MonoBehaviour
    {
        private EnemyMono m_CurrentEnemyMono;
        private bool m_Rising = true;
        private bool m_Selecting;
        private float m_MaxY;
        private float m_MinY;
        private Transform m_Marker;
        [SerializeField]
        private float m_MaxTime;
        private float m_CurrentTime;

        public GameObject markerPrefab;

        private void Start ()
        {
            m_CurrentTime = m_MaxTime;
            m_CurrentEnemyMono = EnemyManager.self.currentEnemy;           // Get current enemy

            m_Marker = Instantiate(markerPrefab, SetMarkerToCurrent(),
                markerPrefab.transform.rotation).transform;
            
            CombatManager.self.onCombatUpdate.AddListener(OnCombatUpdate);
            InputManager.self.onPress.AddListener(OnPress); // Set Listener for input
        }

        private void OnCombatUpdate()
        {
            if (m_Selecting)
            {
                CombatCamera.isAnimating = false;
                m_CurrentTime -= Time.deltaTime;
                if (m_CurrentTime <= 0f)
                {
                    m_CurrentTime = m_MaxTime;
                    m_Selecting = false;
                    CombatCamera.isAnimating = true;
                }
            }

            if (m_Rising)
            {
                m_Marker.position += new Vector3(0, Time.deltaTime, 0);
            }

            else
            {
                m_Marker.position -= new Vector3(0, Time.deltaTime, 0);
            }

            if (m_Marker.position.y < m_MinY)
                m_Rising = true;

            if (m_Marker.position.y > m_MaxY)
                m_Rising = false;

            // If the enemy is not null or there are no enemies, return
            if (m_CurrentEnemyMono != null || EnemyManager.self.enemies.Count == 0)
                return;

            // If current enemy is null and there are enemies

            m_CurrentEnemyMono = EnemyManager.self.enemies.First();    // Find the first guy

            m_Marker.position = SetMarkerToCurrent();

            EnemyManager.self.currentEnemy = m_CurrentEnemyMono;
        }

        private void OnPress(TouchInformation touchInfo)
        {
            if (CombatCamera.isAnimating)   // if the camera is animating, do nothing
                return;

            // Else shoot ray from touch
            var ray = Camera.main.ScreenPointToRay(touchInfo.position);
            var hit = new RaycastHit();

            try
            {
                Physics.Raycast(ray.origin, ray.direction, out hit);
            }
            catch { }

            if (hit.transform == null)                  // Did the ray hit something
                return;

            var tempObject = hit.transform.gameObject;  //Store gameobject temperarily.
            var gameOb = tempObject;                    // This will be the final result.


            while(true)
            {
                if (tempObject.GetComponent<EnemyMono>())   // Does it have an EnemyMono
                {
                    gameOb = tempObject;
                    break;
                }
                if (tempObject.transform.parent != null 
                    && !tempObject.GetComponent<EnemyMono>())
                    tempObject = tempObject.transform.parent.gameObject;

                // If no parent, reached end, return function.
                else
                {
                    return;
                }
            }
            
            // EnemyMono same as the current target
            if (!gameOb.GetComponent<EnemyMono>())
                return;

            CombatCamera.isAnimating = false;
            m_Selecting = true;

            Camera.main.transform.localPosition = new Vector3(0, 0, -5f);
            m_CurrentEnemyMono = gameOb.GetComponent<EnemyMono>();

            m_Marker.position = SetMarkerToCurrent();   // Set position of marker

            EnemyManager.self.currentEnemy = m_CurrentEnemyMono;   // Set enemy

            CombatCamera.isAnimating = true;            // Turn combat Camera back on
        }

        private Vector3 SetMarkerToCurrent()
        {
            var currenyEnemyBounds = m_CurrentEnemyMono.GetComponent<MeshRenderer>().bounds;
            m_MinY = currenyEnemyBounds.center.y + currenyEnemyBounds.extents.y;
            m_MaxY = m_MinY + 1;

            var newPosition = currenyEnemyBounds.center +
                new Vector3(0, currenyEnemyBounds.extents.y + .5f, 0);
            return newPosition;
        }
    }
}