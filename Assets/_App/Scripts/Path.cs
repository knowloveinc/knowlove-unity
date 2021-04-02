
using System.Collections.Generic;
using UnityEngine;

namespace Knowlove
{
    public class Path : MonoBehaviour
    {
        public List<PathNode> nodes = new List<PathNode>();

        public PathNode GetPathNode(int index)
        {
            if (index < nodes.Count)
            {
                return nodes[index];
            }

            return null;
        }

        public List<Vector3> NodesAsVector3()
        {
            List<Vector3> list = new List<Vector3>();
            for(int i = 0; i < nodes.Count; i++)
            {
                list.Add(nodes[i].transform.position);
            }

            return list;
        }

        [ContextMenu("Get Nodes")]
        public void GetNodesFromChildren()
        {
            if (Application.isEditor)
            {
                nodes.Clear();

                int i = 1;
                foreach(Transform child in transform)
                {

                    PathNode node = child.GetComponent<PathNode>();
                    if(node != null)
                    {
                        node.gameObject.name = "NODE: " + i;
                        nodes.Add(node);
                        i++;
                    }


                }
            }
        }
    }
}