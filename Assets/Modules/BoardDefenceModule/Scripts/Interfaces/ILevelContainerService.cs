using Strada.Core.Patterns.Interfaces;
using UnityEngine;

namespace BoardDefence.Services
{
    public interface ILevelContainerService : IService
    {
        Transform Board { get; }
        Transform Enemies { get; }
        Transform Defences { get; }
        Transform Projectiles { get; }
        bool IsActive { get; }
    }
}
