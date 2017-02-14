﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

using Random = UnityEngine.Random;

[Serializable]
public class CombatCamera : MonoBehaviour
{
    [SerializeField]
    private List<TransformAnimation> m_Animations = new List<TransformAnimation>();

    private IEnumerator m_AnimationEnumerator;

    public List<TransformAnimation> animations { get { return m_Animations; } }

    private void Start()
    {
        m_AnimationEnumerator = Animate();

        CombatManager.self.onCombatUpdate.AddListener(OnCombatUpdate);
    }

    private void OnCombatUpdate()
    {
        if (m_AnimationEnumerator.MoveNext()) { }
    }

    private IEnumerator Animate()
    {
        Random.InitState(DateTime.Now.Millisecond);

        var originalPosition = transform.localPosition;
        var originalEulerAngles = transform.localEulerAngles;
        var originalZoom = transform.GetChild(0).localPosition;

        TransformAnimation currentAnimation = null;
        while (true)
        {
            transform.localPosition = originalPosition;
            transform.localEulerAngles = originalEulerAngles;
            transform.GetChild(0).localPosition = originalZoom;
            
            var tempList = m_Animations.ToList();
            if (currentAnimation != null)
                tempList.Remove(currentAnimation);

            var randomIndex = Random.Range(0, tempList.Count);
            currentAnimation = tempList[randomIndex];

            //TODO: Do something based on TargetType
            var currentEnumerator = currentAnimation.Animate(transform, null);

            while (currentEnumerator.MoveNext()) { yield return null; }
        }
    }
}