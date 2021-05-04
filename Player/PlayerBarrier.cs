using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

   public class PlayerBarrier : MonoBehaviour
   {
       public float barrierHP = 150f;
       private float hpdrain = 15f;
       
       void Start()
       {
           gameObject.tag = "PBarrier";
       }
       
       void FixedUpdate()
       {
            Invoke("HPDrain", 1f);
       }
       
       void HPDrain()
       {
           barrierHP = barrierHP - hpdrain;
       }
       
       void Die()
       {
           Destroy(gameObject);
       }
   }



