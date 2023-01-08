using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public struct PassengerComponent : IComponentData
{
    public Entity currentCarriage; // The current carriage that the passenger is on
    public Entity carriageSeat; // Index to the position where passenger will be standing in metro
    public Entity currentTrain;
}