using UnityEngine;
using HexasphereGrid;
using System.Collections;

namespace HexasphereGrid_Demos {

    public class CapsuleMover : MonoBehaviour {

        public float duration = 2;

        Hexasphere hexa;
        float startTime;

        void Start() {
            hexa = Hexasphere.GetInstance("Hexasphere");
            // event handler -> trigger capsule movement when clicking on a tile
            hexa.OnTileClick += Hexa_OnTileClick;

            // ensure the capsule is aligned to the surface on start
            AlignToSurface();
        }

        private void OnDestroy() {
            // remove event handler
            hexa.OnTileClick -= Hexa_OnTileClick;
        }

        private void Hexa_OnTileClick(int tileIndex) {
            StopAllCoroutines();
            // get start and end positions and annotate that we're moving the capsule
            Vector3 endPosition = hexa.GetTileCenter(tileIndex, worldSpace: false);
            StartCoroutine(MoveCapsule(endPosition));
        }

        // Update is called once per frame
        IEnumerator MoveCapsule(Vector3 endPosition) {

            startTime = Time.time;
            Vector3 startPosition = transform.localPosition;
            Debug.Log("Moving from " + startPosition + " to " + endPosition);

            float t = 0;
            while (t < 1f) {

                // Compute next position
                t = (Time.time - startTime) / duration;
                t = Mathf.Clamp01(t);
                Vector3 surfacePosition = Vector3.Lerp(startPosition, endPosition, t).normalized * 0.5f;
                // clamp to surface including extrusion
                transform.localPosition = hexa.GetExtrudedPosition(surfacePosition, worldSpace: false);

                // Adjust rotation so it keeps aligned to hexasphere surface
                Vector3 lookPosition = hexa.transform.TransformPoint(endPosition);
                Vector3 up = (transform.position - hexa.transform.position).normalized;
                transform.LookAt(lookPosition, up);

                yield return null;
            }
        }

        void AlignToSurface() {
            transform.LookAt(hexa.transform.position);
            transform.Rotate(-90, 0, 0, Space.Self);
        }
    }

}