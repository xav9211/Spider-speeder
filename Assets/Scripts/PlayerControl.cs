using UnityEngine;
using System.Collections;
using System.Collections.Generic;

enum Legs { TopRight, TopLeft, BotRight, BotLeft};
class LegData
{
    public GameObject gameObject;
    public SpringJoint2D spring;
    public float lowerAngle;
    public float upperAngle;
    public LineRenderer lr;

    public LegData(GameObject gameObject, float angleRange)
    {
        this.gameObject = gameObject;
        this.lowerAngle = gameObject.transform.localEulerAngles.z - angleRange / 2.0F;
        this.upperAngle = gameObject.transform.localEulerAngles.z + angleRange / 2.0F;
        this.spring = null;
        this.lr = gameObject.GetComponent<LineRenderer>();
    }
    
}

public class PlayerControl : MonoBehaviour {

    Dictionary<Legs, LegData> legs;
    Transform body;
    float angleRange = 90.0F;
    float swingRange = 100.0F;

    // Use this for initialization
    void Start ()
    {
        legs = new Dictionary<Legs, LegData>();
        legs.Add(Legs.TopRight, new LegData(GameObject.Find("TopRightLeg"), angleRange));
        legs.Add(Legs.TopLeft, new LegData(GameObject.Find("TopLeftLeg"), angleRange));
        legs.Add(Legs.BotRight, new LegData(GameObject.Find("BotRightLeg"), angleRange));
        legs.Add(Legs.BotLeft, new LegData(GameObject.Find("BotLeftLeg"), angleRange));
        body = transform.Find("SpiderBody");
    }
	
	// Update is called once per frame
	void Update ()
    {
        Move();
	}

    void Move()
    {
        WebControl(KeyCode.Joystick1Button4, legs[Legs.TopLeft]);
        WebControl(KeyCode.Joystick1Button5, legs[Legs.TopRight]);
        WebControl(KeyCode.Joystick2Button4, legs[Legs.BotLeft]);
        WebControl(KeyCode.Joystick2Button5, legs[Legs.BotRight]);

        MoveLeg("J1A1H", legs[Legs.TopLeft], 3, 1);
        MoveLeg("J1A2H", legs[Legs.TopRight], 3, 1);
        MoveLeg("J2A1H", legs[Legs.BotLeft], 3, -1);
        MoveLeg("J2A2H", legs[Legs.BotRight], 3, -1);
    }

    /**
     * inverse should be either 1 or -1 
     */
    void MoveLeg(string inputAxis, LegData leg, float sensitivity, float inverse)
    {
        if (!leg.spring && System.Math.Abs(Input.GetAxis(inputAxis)) > 0)
        {
            float force = Input.GetAxis(inputAxis);
            float angle = leg.gameObject.transform.localEulerAngles.z;
            if ((inverse * force > 0 && angle - inverse * sensitivity * force > leg.lowerAngle) || 
                (inverse * force < 0 && angle -  inverse * sensitivity * force < leg.upperAngle))
                leg.gameObject.transform.Rotate(new Vector3(0, 0, -inverse * sensitivity * force));
        }
    }

    void WebControl(KeyCode key, LegData leg)
    {
        if (Input.GetKeyDown(key))
        {
            if (!leg.spring)
            {
                GameObject legObject = leg.gameObject;
                Transform legTransform = legObject.transform;
                float angle = -(Mathf.PI / 180) * legTransform.transform.eulerAngles.z;
                Vector2 pointingDirection = new Vector2(Mathf.Sin(angle), Mathf.Cos(angle));
                LayerMask layerMask = ~(1 << LayerMask.NameToLayer("Player"));
                RaycastHit2D hitInfo = Physics2D.Raycast(legTransform.position, pointingDirection, swingRange, layerMask);
                if (hitInfo.collider)
                {
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
        }else if (Input.GetKeyUp(key))
        {
            SpringJoint2D spring = leg.spring;
            if (spring)
            {
                leg.lr.enabled = false;
                Destroy(spring);
            }
        }

        if (leg.spring)
        {
            Vector2 v = leg.gameObject.transform.position + Quaternion.Euler(0, 0, leg.gameObject.transform.eulerAngles.z) * new Vector2(0, 0.5F);
            leg.lr.SetPosition(0, v);
            leg.lr.SetPosition(1, leg.spring.connectedAnchor);
        }
    }
}
