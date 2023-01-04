using Unity.Entities;


//Use this component for systems that keep running in the background
//Just a simple check instead of doing structural changes
namespace DOTS.Components
{
    public struct EnableComponent : IComponentData
    {
        public bool value;
    }
}