using System;
using UnityEngine;

namespace BoardDefence.Services
{
    public sealed class LevelContainer : IDisposable
    {
        private GameObject _root;
        private Transform _boardContainer;
        private Transform _enemiesContainer;
        private Transform _defencesContainer;
        private Transform _projectilesContainer;

        public Transform Root => _root?.transform;
        public Transform Board => _boardContainer;
        public Transform Enemies => _enemiesContainer;
        public Transform Defences => _defencesContainer;
        public Transform Projectiles => _projectilesContainer;
        public bool IsActive => _root != null;

        public void Create(int levelIndex)
        {
            Dispose();

            _root = new GameObject($"[Level_{levelIndex}]");

            _boardContainer = new GameObject("Board").transform;
            _boardContainer.SetParent(_root.transform);

            _enemiesContainer = new GameObject("Enemies").transform;
            _enemiesContainer.SetParent(_root.transform);

            _defencesContainer = new GameObject("Defences").transform;
            _defencesContainer.SetParent(_root.transform);

            _projectilesContainer = new GameObject("Projectiles").transform;
            _projectilesContainer.SetParent(_root.transform);
        }

        public void Dispose()
        {
            if (_root != null)
            {
                UnityEngine.Object.Destroy(_root);
                _root = null;
                _boardContainer = null;
                _enemiesContainer = null;
                _defencesContainer = null;
                _projectilesContainer = null;
            }
        }
    }
}
