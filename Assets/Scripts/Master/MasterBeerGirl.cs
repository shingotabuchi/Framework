using System;
using UnityEngine;
using Fwk.Master;

[Serializable]
public class MasterBeerGirl : MasterData<MasterBeerGirl>
{
    [SerializeField] private string _name;

    public string Name => _name;
}