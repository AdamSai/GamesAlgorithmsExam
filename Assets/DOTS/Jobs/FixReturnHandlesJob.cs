// using Unity.Burst;
// using Unity.Entities;
// using DOTS.Components;
//
// namespace DOTS.Jobs
// {
//     [BurstCompile]
//     public partial struct FixReturnHandlesJob : IJobEntity
//     {
//         
//         
//         public void Execute(ref BezierPathComponent path)
//         {
//             for (int i = 0; i <= path.points.Length - 1; i++)
//             {
//                 BezierPoint _currentPoint = _RETURN_POINTS[i];
//                 if (i == 0)
//                 {
//                     _currentPoint.SetHandles(_RETURN_POINTS[1].location - _currentPoint.location);
//                 }
//                 else if (i == total_outboundPoints - 1)
//                 {
//                     _currentPoint.SetHandles(_currentPoint.location - _RETURN_POINTS[i - 1].location);
//                 }
//                 else
//                 {
//                     _currentPoint.SetHandles(_RETURN_POINTS[i + 1].location - _RETURN_POINTS[i - 1].location);
//                 }
//             }
//
//             bezierPath.MeasurePath();
//         }
//     }
// }