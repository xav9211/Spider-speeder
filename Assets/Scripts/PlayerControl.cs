using System;
using UnityEngine;
using System.Collections.Generic;
using Assets;
using Assets.Map;
using UnityEditor;

enum Legs {
    TopRight, TopLeft, BotRight, BotLeft
};
class LegData {
    public Legs legPos;
    public GameObject gameObject;
    public SpringJoint2D spring;
    public float lowerAngle;
    public float upperAngle;
    public LineRenderer lr;

    public LegData(Legs legPos, GameObject gameObject, float angleRange) {
        this.legPos = legPos;
        this.gameObject = gameObject;
        this.lowerAngle = gameObject.transform.localEulerAngles.z - angleRange / 2.0F;
        this.upperAngle = gameObject.transform.localEulerAngles.z + angleRange / 2.0F;
        this.spring = null;
        this.lr = gameObject.GetComponent<LineRenderer>();
    }
}

interface AnalogStick {
    Vector2 Delta { get; }
}

class DefaultAnalogStick: AnalogStick {
    private string axisStringH;
    private string axisStringV;

    public DefaultAnalogStick(int joyIndex,
                              int axisIndex) {
        axisStringH = "J" + joyIndex + "A" + axisIndex + "H";
        axisStringV = "J" + joyIndex + "A" + axisIndex + "V";
    }

    public Vector2 Delta {
        get {
            return new Vector2(-Input.GetAxis(axisStringH),
                               Input.GetAxis(axisStringV));
        }
    }
}

class XboxAnalogStick: AnalogStick {
    private string axisStringH;
    private string axisStringV;

    public XboxAnalogStick(int joyIndex,
                           int axisIndex) {
        axisStringH = "XboxJ" + joyIndex + "A" + axisIndex + "H";
        axisStringV = "XboxJ" + joyIndex + "A" + axisIndex + "V";
    }

    public Vector2 Delta {
        get {
            return new Vector2(-Input.GetAxis(axisStringH),
                               -Input.GetAxis(axisStringV));
        }
    }
}

interface ControlScheme {
    void MoveLeg(LegData leg,
                 Vector2 delta);
}

class RotateControlScheme : ControlScheme {
    public void MoveLeg(LegData leg,
                        Vector2 delta) {
        bool isTopLeg = leg.legPos == Legs.TopLeft
                        || leg.legPos == Legs.TopRight;
        float inverse = isTopLeg ? -1.0f : 1.0f;

        if (!leg.spring && delta.SqrMagnitude() > 0) {
            float force = delta.x * inverse;
            float sensitivity = 3;
            float angle = leg.gameObject.transform.localEulerAngles.z;
            if ((force > 0 && angle - sensitivity*force > leg.lowerAngle) ||
                (force < 0 && angle - sensitivity*force < leg.upperAngle))
                leg.gameObject.transform.Rotate(new Vector3(0, 0, -sensitivity*force));
        }
    }
}

class PointAtTargetControlScheme : ControlScheme {
    public void MoveLeg(LegData leg,
                        Vector2 delta) {
        if (Math.Abs(delta.x) > 0.0f || Math.Abs(delta.y) > 0.0f) {
            float angle = (float)(Math.Atan2(delta.y, delta.x) * 180.0 / Math.PI);
            angle += 90.0f;
            leg.gameObject.transform.rotation = Quaternion.Euler(0.0f, 0.0f, angle);
        }
    }
}

public class PlayerControl: MonoBehaviour {
    class LegAxisMapping {
        public AnalogStick LeftLeg { get; private set; }
        public AnalogStick RightLeg { get; private set; }

        private LegAxisMapping(AnalogStick leftLeg,
                               AnalogStick rightLeg) {
            LeftLeg = leftLeg;
            RightLeg = rightLeg;
        }

        public static LegAxisMapping Default(int joyIndex) {
            return new LegAxisMapping(new DefaultAnalogStick(joyIndex, 1),
                                      new DefaultAnalogStick(joyIndex, 2));
        }

        public static LegAxisMapping Xbox(int joyIndex) {
            return new LegAxisMapping(new DefaultAnalogStick(joyIndex, 1),
                                      new XboxAnalogStick(joyIndex, 2));
        }
    }

    private List<LegAxisMapping> axesMapping = new List<LegAxisMapping>();
    private List<ControlScheme> controlSchemes = new List<ControlScheme>();

    Dictionary<Legs, LegData> legs;
    Transform body;
    Transform camera;
    float angleRange = 90.0F;
    float swingRange = 10000000.0F;

    // Use this for initialization
    void Start() {
        var joystickNames = Input.GetJoystickNames();
        for (int i = 0; i < joystickNames.Length; ++i) {
            if (joystickNames[i].ToLower().Contains("xbox")) {
                axesMapping.Add(LegAxisMapping.Xbox(i + 1));
                controlSchemes.Add(new PointAtTargetControlScheme());
            } else {
                axesMapping.Add(LegAxisMapping.Default(i + 1));
                controlSchemes.Add(new RotateControlScheme());
            }
        }

        Map map = GameObject.Find("Map").GetComponent<Map>();
        Point2i startPos = map.GetSpiderStartPos();

        transform.position = new Vector3(startPos.x, startPos.y);
        body = transform.Find("SpiderBody");
        camera = transform.Find("Main Camera");

        legs = new Dictionary<Legs, LegData>();
        legs.Add(Legs.TopRight, new LegData(Legs.TopRight, GameObject.Find("TopRightLeg"), angleRange));
        legs.Add(Legs.TopLeft, new LegData(Legs.TopLeft, GameObject.Find("TopLeftLeg"), angleRange));
        legs.Add(Legs.BotRight, new LegData(Legs.BotRight, GameObject.Find("BotRightLeg"), angleRange));
        legs.Add(Legs.BotLeft, new LegData(Legs.BotLeft, GameObject.Find("BotLeftLeg"), angleRange));
    }

    private void ToggleControlScheme(int playerIndex) {
        if (controlSchemes[playerIndex] is RotateControlScheme) {
            controlSchemes[playerIndex] = new PointAtTargetControlScheme();
        } else {
            controlSchemes[playerIndex] = new RotateControlScheme();
        }
    }

    // Update is called once per frame
    void Update() {
        Move();
        camera.position = new Vector3(body.position.x, body.position.y, camera.position.z);

        // Button 7 == Start (Win/Lin), (Mac: D-pad left, lol)
        // http://wiki.unity3d.com/index.php?title=Xbox360Controller
        if (Input.GetKeyDown(KeyCode.Joystick1Button7)) {
            ToggleControlScheme(0);
        }
        if (Input.GetKeyDown(KeyCode.Joystick2Button7)) {
            ToggleControlScheme(1);
        }
    }
    
    void Die()
    {
        Destroy(this.gameObject);
    }

    void Move() {
        WebControl(KeyCode.Joystick1Button4, legs[Legs.TopLeft]);
        WebControl(KeyCode.Joystick1Button5, legs[Legs.TopRight]);
        WebControl(KeyCode.Joystick2Button4, legs[Legs.BotLeft]);
        WebControl(KeyCode.Joystick2Button5, legs[Legs.BotRight]);

        if (axesMapping.Count > 0) {
            controlSchemes[0].MoveLeg(legs[Legs.TopLeft], axesMapping[0].LeftLeg.Delta);
            controlSchemes[0].MoveLeg(legs[Legs.TopRight], axesMapping[0].RightLeg.Delta);

            if (axesMapping.Count > 1) {
                controlSchemes[1].MoveLeg(legs[Legs.BotLeft], axesMapping[1].LeftLeg.Delta);
                controlSchemes[1].MoveLeg(legs[Legs.BotRight], axesMapping[1].RightLeg.Delta);
            }
        }
    }

    void MoveLeg(LegData leg,
                 Vector2 delta) {
        if (Math.Abs(delta.x) > 0.0f || Math.Abs(delta.y) > 0.0f) {
            float angle = (float)(Math.Atan2(delta.y, delta.x) * 180.0 / Math.PI);
            angle += 90.0f;
            leg.gameObject.transform.rotation = Quaternion.Euler(0.0f, 0.0f, angle);
        }
    }

    void WebControl(KeyCode key, LegData leg) {
        if (Input.GetKeyDown(key)) {
            if (!leg.spring) {
                GameObject legObject = leg.gameObject;
                Transform legTransform = legObject.transform;
                float angle = -(Mathf.PI / 180) * legTransform.transform.eulerAngles.z;
                Vector2 pointingDirection = new Vector2(Mathf.Sin(angle), Mathf.Cos(angle));
                LayerMask layerMask = ~(1 << LayerMask.NameToLayer("Player"));
                RaycastHit2D hitInfo = Physics2D.Raycast(legTransform.position, pointingDirection, swingRange, layerMask);
                if (hitInfo.collider.name == "Enemy(Clone)")
                {
                    hitInfo.rigidbody.SendMessage("Die");
                }
                if (hitInfo.collider) {
                    SpringJoint2D spring = legObject.AddComponent<SpringJoint2D>();
                    spring.anchor.Set(0, 1);
                    spring.autoConfigureDistance = false;
                    spring.distance = 0;
                    spring.dampingRatio = 0.9F;
                    spring.frequency = 0.3F;
                    spring.connectedAnchor = hitInfo.point;
                    leg.spring = spring;
                }

                leg.lr.enabled = true;
            }
        } else if (Input.GetKeyUp(key)) {
            SpringJoint2D spring = leg.spring;
            if (spring) {
                leg.lr.enabled = false;
                Destroy(spring);
            }
        }

        if (leg.spring) {
            Vector2 v = leg.gameObject.transform.position + Quaternion.Euler(0, 0, leg.gameObject.transform.eulerAngles.z) * new Vector2(0, 0.5F);
            leg.lr.SetPosition(0, v);
            leg.lr.SetPosition(1, leg.spring.connectedAnchor);
        }
    }
}
