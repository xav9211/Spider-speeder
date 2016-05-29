using UnityEngine;
using System.Collections;
using Assets.Scripts;

public class MenuSpider : MonoBehaviour {
    private GameObject leg;

    private float? targetAngle;
    private float? TargetAngle {
        get { return targetAngle; }
        set {
            Physics2D.gravity = new Vector2(0.0f, value == null ? -9.81f : 0.0f);
            targetAngle = value;
        }
    }

    // Use this for initialization
	void Start () {
	    leg = transform.FindChild("BotLeftLeg").gameObject;
	}

    void FixedUpdate() {
        if (targetAngle.HasValue) {
            float angle = leg.transform.rotation.eulerAngles.z;
            float angleDelta = (targetAngle.Value - angle) / 4.0f;

            leg.transform.rotation = Quaternion.Euler(0.0f, 0.0f, angle + angleDelta);
        }
    }

    void PointAtNewGame() {
        TargetAngle = 95.0f;
    }

    void PointAtHighScores() {
        TargetAngle = 105.0f;
    }

    void PointAtExit() {
        TargetAngle = 115.0f;
    }

    void PointAtNothing() {
        TargetAngle = null;
    }
}
