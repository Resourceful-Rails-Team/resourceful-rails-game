using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Rails.ScriptableObjects {
  [CreateAssetMenu(fileName = "Train", menuName = "ScriptableObjects/Map/TrainData", order = 3)]
  public class TrainData : ScriptableObject {
    [SerializeField]
    public int move_points;
    [SerializeField]
    public int goods_total;
    [SerializeField]
    public GameObject model;
  }
}