using DOTS.Components;
using DOTS.Components.Train;
using DOTS.Utility;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace DOTS.Jobs
{
    public partial struct UpdateCarriagesJob : IJobEntity
    {
        public EntityCommandBuffer ECB;
        public NativeArray<Entity> trains;
        public NativeArray<Entity> metroLines;
        public ComponentLookup<TrainPositionComponent> tPos;
        public ComponentLookup<TrainIDComponent> trainIDs;
        public ComponentLookup<MetroLineComponent> MetroLineComponents;
        public ComponentLookup<BezierPathComponent> BezierPaths;
        public BufferLookup<DOTS.BezierPoint> bezierPoints;

        public void Execute(Entity ent, CarriageIDComponent carriageIDComponent, ref CarriagePositionComponent carriagePos, in CarriageAheadOfMeComponent carriageAheadOfMeComp)
        {
            Entity trainEntity = default;
            Entity metroLine = default;

            // Find the correct Train
            for (var i = 0; i < trains.Length; i++)
            {
                if (trainIDs.GetRefRO(trains[i]).ValueRO.LineIndex == carriageIDComponent.lineIndex &&
                    trainIDs.GetRefRO(trains[i]).ValueRO.TrainIndex == carriageIDComponent.trainIndex)
                {
                    trainEntity = trains[i];
                    break;
                }
            }

            // Find the correct MetroLine
            for (var i = 0; i < metroLines.Length; i++)
            {
                if (MetroLineComponents.GetRefRO(metroLines[i]).ValueRO.MetroLineID == carriageIDComponent.lineIndex)
                {
                    metroLine = metroLines[i];
                    break;
                }
            }

            var bezierBuffer = bezierPoints[metroLine];

            // UpdateCarriages
            // Update position on the bezier

            if (carriageAheadOfMeComp.Value == Entity.Null)
            {
                //Move carriage relative to the carriage ahead of me
                var trainPos = tPos.GetRefRO(trainEntity).ValueRO;
            }

            var bezier = BezierPaths.GetRefRO(metroLine).ValueRO;
            float carriageOffset = 3f;
            float pos = tPos[trainEntity].value - carriageIDComponent.id * carriageOffset / bezier.distance;


            if (pos >= 1f)
                pos %= 1f;

            carriagePos.value = pos;

            var posOnRail = BezierUtility.Get_Position(pos, bezier.distance, bezierBuffer);
            var rotOnRail = BezierUtility.Get_NormalAtPosition(pos, bezier.distance, bezierBuffer);
            // Debug.Log(carriageIDComponent.lineIndex + ":" + carriageIDComponent.id + ": " + carriageIDComponent.id * carriageOffset);

            // Set rotation and position
            var transform = LocalTransform.FromPosition(posOnRail);
            var rot = Quaternion.LookRotation(transform.Position - (transform.Position - rotOnRail), Vector3.up);
            transform.Rotation = rot;
            ECB.SetComponent(ent, transform);
        }
    }
}