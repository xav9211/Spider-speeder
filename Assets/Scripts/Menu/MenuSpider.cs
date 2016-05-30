using UnityEngine;
using System.Collections;
using Assets.Scripts;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MenuSpider : MonoBehaviour {
    private GameObject leg;

    private Vector2 savedGravity;
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
	    savedGravity = Physics2D.gravity;
	    leg = transform.FindChild("BotLeftLeg").gameObject;
	}

    void FixedUpdate() {
        if (targetAngle.HasValue) {
            float angle = leg.transform.rotation.eulerAngles.z;
            float angleDelta = (targetAngle.Value - angle) / 4.0f;

            leg.transform.rotation = Quaternion.Euler(0.0f, 0.0f, angle + angleDelta);
        }
    }

    void PointAt(string btnName) {
        string btnPath = "/Canvas/MainMenu/LeftPane/" + btnName;

        Transform bt = GameObject.Find(btnPath).transform;

        Vector2 MAGIC_OFFSET = new Vector2(2.0f, -0.25f);
        Vector2 targetPos = new Vector2(bt.position.x, bt.position.y) + MAGIC_OFFSET;

        Vector3 legPos = leg.transform.position;
        Vector2 dir = new Vector2(legPos.x, legPos.y) - targetPos;

        float angle = (Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg + 450.0f) % 360.0f;
        TargetAngle = angle;
    }

    void PointAtNewGame() {
        PointAt("ButtonContainer/NewGame");
    }

    void PointAtLoadSeed() {
        PointAt("ButtonContainer/LoadSeed");
    }

    void PointAtHighScores() {
        PointAt("ButtonContainer/HighScores");
    }

    void PointAtExit() {
        PointAt("ButtonContainer/Exit");
    }

    void PointAtSeedInputLoad() {
        PointAt("SeedInputPane/Load");
    }

    void PointAtSeedInputBack() {
        PointAt("SeedInputPane/Back");
    }

    void PointAtNothing() {
        TargetAngle = null;
    }

    void OnDestroy() {
        Physics2D.gravity = savedGravity;
    }
}
