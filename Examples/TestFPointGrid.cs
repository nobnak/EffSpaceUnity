using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EffSpace.Examples {

    public class TestFPointGrid : MonoBehaviour {

        private void OnEnable() {
            var c = Camera.main;

        }

        [System.Serializable]
        public class Tuner {
            public int grid = 10;
        }
    }

}
