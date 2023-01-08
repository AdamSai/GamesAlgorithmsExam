using System.Collections;
using Unity.Entities;
using UnityEngine;

namespace Assets.DOTS.Systems
{
    public partial struct EnterTrainSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {

        }

        public void OnDestroy(ref SystemState state)
        {

        }

        public void OnUpdate(ref SystemState state)
        {

        }
    }

    public partial struct EnterTrainJob : IJobEntity
    {
        public void Execute(in Entity entity, ref PassengerComponent passenger)
        {

        }
    }
}