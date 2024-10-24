using UnityEngine;

namespace ProceduralMapNodeSystem
{
    [CreateAssetMenu(menuName = "Map/NodeType")]
    public class NodeType : ScriptableObject
    {
        public string TypeName;
        
        public float MapNodeScale = 1f;
    }
}