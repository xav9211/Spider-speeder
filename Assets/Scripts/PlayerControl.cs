﻿using System;
using System.Collections.Generic;
using System.Linq;
using Assets;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Assertions;

namespace Assets.Scripts {
    public class DamageInfo {
		public Collider2D Collider { get; set; }
        public float Damage { get; set; }
        public Vector3 SourcePosition { get; private set; }
        public Vector3 TargetPosition { get; private set; }
        public int? PlayerNumber { get; private set; }

        public DamageInfo(float damage,
                          Vector3 sourcePosition,
                          Vector3 targetPosition,
                          int? playerNumber,
						  Collider2D collider) {
			Collider = collider;
            Damage = damage;
            SourcePosition = sourcePosition;
            TargetPosition = targetPosition;
            PlayerNumber = playerNumber;

            GameStatistics.AddDamage(this);
        }
    }

    enum Legs {
        TopRight, TopLeft, BotRight, BotLeft
    };

    class LegData {
        public Legs legPos;
        public GameObject gameObject;
        public SpringJoint2D spring;
        public LineRenderer lr;
        public RaycastHit2D webHitInfo;
        public Vector2 angleLimits; // ranges of leg movement x - lower limit, y -upper limit
        public float currentAngle; // added because in unity angle 361degrees is saved as 1 degree (361 number is relevant)
        public bool swingCollision = false;
        public float height = 0.5F; //leg size
        public float maxFrameDeltaAngle = 15.0F; // by how many degrees can leg change in single frame

        public readonly float startMaxRange; // beggining value (in degrees) of maximum possible range
        public readonly float arctanRotation; //rotation value (in degrees) which should be performed for proper arctan values

        private Vector2 originalLocalPos;

        public int PlayerNumber {
            get { return (legPos == Legs.TopLeft || legPos == Legs.TopRight) ? 1 : 2; }
        }

        public LegData(Legs legPos, GameObject gameObject, float angleRange, float startMaxRange, float arctanRotation) {
            this.legPos = legPos;
            this.gameObject = gameObject;
            this.currentAngle = gameObject.transform.localEulerAngles.z;
            this.angleLimits = new Vector2 (currentAngle - angleRange / 2.0F, currentAngle + angleRange / 2.0F);
            this.startMaxRange = startMaxRange;
            this.arctanRotation = arctanRotation;
            this.lr = gameObject.GetComponent<LineRenderer>();
            this.spring = null;

            originalLocalPos = gameObject.transform.localPosition;
        }

        public void ResetJoint() {
            gameObject.transform.localPosition = originalLocalPos;
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
                if ((force > 0 && angle - sensitivity*force > leg.angleLimits.x) ||
                    (force < 0 && angle - sensitivity*force < leg.angleLimits.y))
                    leg.gameObject.transform.Rotate(new Vector3(0, 0, -sensitivity*force));
            }
        }
    }

    class PointAtTargetControlScheme : ControlScheme {
        public void MoveLeg(LegData leg,
                            Vector2 delta) {
            if (!leg.spring && (Math.Abs(delta.x) > 0.0f || Math.Abs(delta.y) > 0.0f)) {
                delta = Quaternion.Euler (0, 0, leg.arctanRotation) * (-delta);
                float diffAngle = Mathf.Rad2Deg * Mathf.Atan2(delta.y, delta.x);
                if (diffAngle < 0)
                    diffAngle = diffAngle < -90.0F ? 180.0F : 0.0F;
			

                diffAngle = (leg.startMaxRange + diffAngle) - leg.currentAngle;
                if (Mathf.Abs(diffAngle) > leg.maxFrameDeltaAngle)
                    diffAngle = Mathf.Sign(diffAngle) * leg.maxFrameDeltaAngle;

                float finalAngle;


                if (leg.currentAngle + diffAngle > leg.angleLimits.y)
                    finalAngle = leg.angleLimits.y;
                else if (leg.currentAngle + diffAngle < leg.angleLimits.x)
                    finalAngle = leg.angleLimits.x;
                else
                    finalAngle = leg.currentAngle + diffAngle;

                leg.currentAngle = finalAngle;
                leg.gameObject.transform.localEulerAngles = new Vector3(0, 0, finalAngle);
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

        private BuffStack buffs;
        private EffectContainer effects;

        public Vector3 Position { get { return transform.position; } }

        public float BaseDamage { get; private set; }
        public float Damage {
            get { return buffs.Apply(BaseDamage, Statistic.Damage); }
        }

        public float Health { get; private set; }
        public float BaseMaxHealth { get; private set; }
        public float MaxHealth {
            get { return buffs.Apply(BaseMaxHealth, Statistic.MaxHealth); }
        }

        public float BaseWebSwingRange { get; private set; }
        public float WebSwingRange { get { return buffs.Apply(BaseWebSwingRange, Statistic.WebSwingRange); } }

        private bool isImmune = false;
        public bool IsImmune {
            get { return isImmune; }
            private set {
                isImmune = value;
                Color color = isImmune ? new Color(1.0f, 1.0f, 1.0f, 0.5f) : Color.white;
                foreach (SpriteRenderer spriteRenderer in GetComponentsInChildren<SpriteRenderer>()) {
                    spriteRenderer.color = color;
                }
            }
        }

        public Item SelectedItem;

        private GameObject gameOverOverlay;

        private void AttachCamera() {
            GameObject camera = GameObject.FindGameObjectWithTag("MainCamera");
            Assert.IsNull(camera.transform.parent, "there should be only one PlayerControl instance");
            camera.transform.SetParent(transform);
            camera.transform.localPosition = new Vector3(0.0f, 0.0f, camera.transform.localPosition.z);
        }

        private void DetachCamera() {
            GameObject camera = GameObject.FindGameObjectWithTag("MainCamera");
            camera.transform.SetParent(null);
            camera.transform.position = new Vector3(transform.position.x,
                                                    transform.position.y,
                                                    camera.transform.position.z);
        }

        // Use this for initialization
        void Start() {
            AttachCamera();
            AudioUtils.BackgroundMusic.Play();
            gameOverOverlay = GameObject.FindGameObjectWithTag("GameOverOverlay");
            gameOverOverlay.SetActive(false);

            buffs = GetComponent<BuffStack>();
            effects = GetComponent<EffectContainer>();

            BaseDamage = 50.0f;
            BaseMaxHealth = 100.0f;
            Health = BaseMaxHealth;
            BaseWebSwingRange = 20.0f;

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

            float angleRange = 180.0F;
            legs = new Dictionary<Legs, LegData>();
            legs.Add(Legs.TopRight, new LegData(Legs.TopRight, transform.FindChild("TopRightLeg").gameObject, angleRange, 225.0F, 45.0F));
            legs.Add(Legs.TopLeft, new LegData(Legs.TopLeft, transform.FindChild("TopLeftLeg").gameObject, angleRange, -45.0F, -45.0F));
            legs.Add(Legs.BotRight, new LegData(Legs.BotRight, transform.FindChild("BotRightLeg").gameObject, angleRange, 135.0F, 135.0F));
            legs.Add(Legs.BotLeft, new LegData(Legs.BotLeft, transform.FindChild("BotLeftLeg").gameObject, angleRange, 45.0F, -135.0F));
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

            // Button 7 == Start (Win/Lin), (Mac: D-pad left, lol)
            // http://wiki.unity3d.com/index.php?title=Xbox360Controller
            if (Input.GetKeyDown(KeyCode.Joystick1Button7)) {
                ToggleControlScheme(0);
            }
            if (Input.GetKeyDown(KeyCode.Joystick2Button7)) {
                ToggleControlScheme(1);
            }
            if (Input.GetKeyDown(KeyCode.F2)) {
                Die(100.0f);
            }

            if ((Input.GetKeyDown(KeyCode.Joystick1Button0) || Input.GetKeyDown(KeyCode.Joystick2Button0)) && SelectedItem) {
                if (Input.GetKeyDown(KeyCode.Joystick1Button0)) {
                    GameStatistics.AddItemPickup(1, SelectedItem);
                }
                if (Input.GetKeyDown(KeyCode.Joystick2Button0)) {
                    GameStatistics.AddItemPickup(2, SelectedItem);
                }

                ApplyItem(SelectedItem);
                Destroy(SelectedItem.gameObject);
                SelectedItem = null;
            }
        }

        private void ApplyItem(Item item) {
            if (item.RestoreHealth.HasValue) {
                Health = Mathf.Min(Health + item.RestoreHealth.Value, MaxHealth);
                AudioUtils.Play("HealUp", 7.0f);
            }
            if (item.ExplosionForce.HasValue) {
                ExplosionFactory.Create(item.Position, item.ExplosionForce.Value);
            }
            if (item.ApplyBuff != null) {
                buffs.Add(item.ApplyBuff);
            }
            if (item.ApplyEffect != null) {
                effects.Add(item.ApplyEffect);
            }
        }

        void StopImmunity() {
            IsImmune = false;
        }

        public void StopMovement() {
            Rigidbody2D body = GetComponent<Rigidbody2D>();
            foreach (Rigidbody2D childBody in body.GetComponentsInChildren<Rigidbody2D>()) {
                childBody.velocity = Vector3.zero;
                childBody.angularVelocity = 0.0f;
            }
        }

        void Die(float damage) {
            if (IsImmune) {
                return;
            }

            Health -= damage;

            const float IMMUNITY_TIME_S = 1.0f;
            IsImmune = true;
            Invoke("StopImmunity", IMMUNITY_TIME_S);

            if (Health <= 0.0f) {
                AudioUtils.BackgroundMusic.Stop();
                ExplosionFactory.Create(transform.position, 3.0f);

                DetachCamera();
                AudioUtils.Play("GameOver", 15.0f);
                Destroy(this.gameObject);
            }
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

            legs[Legs.TopLeft].ResetJoint();
            legs[Legs.TopRight].ResetJoint();
            legs[Legs.BotLeft].ResetJoint();
            legs[Legs.BotRight].ResetJoint();
        }

        void WebControl(KeyCode key, LegData leg) {
            if (Input.GetKeyDown(key)) {
                if (!leg.spring) {
                    GameObject legObject = leg.gameObject;
                    Transform legTransform = legObject.transform;
                    LayerMask layerMask = ~(1 << LayerMask.NameToLayer("Player"));
                    // shoot web from the tip of the leg
                    Vector2 legEnd = legTransform.position + (legTransform.up * leg.height);
                    RaycastHit2D hitInfo = Physics2D.Raycast(legEnd, legTransform.up, WebSwingRange, layerMask);
                    if (hitInfo.collider) {
                        if (hitInfo.collider.tag == "Enemy") {
                            DamageInfo dmgSource = new DamageInfo(Damage,
                                                                  Position,
                                                                  hitInfo.collider.gameObject.transform.position,
                                                                  leg.PlayerNumber,
																  hitInfo.collider);
                            hitInfo.rigidbody.SendMessage ("Die", dmgSource);
                        }

                        // if out leg is inside the wall don't create spring
                        if (!legEnd.Equals (hitInfo.point)) {
                            leg.webHitInfo = hitInfo;
                            SpringJoint2D spring = legObject.AddComponent<SpringJoint2D> ();
                            spring.autoConfigureDistance = false;
                            spring.distance = 0;
                            spring.dampingRatio = 0.9F;
                            spring.frequency = 0.3F;
                            // it should be up because joint start should be attached to the tip of the leg, but due to
                            // hingejoint bugs its atm connected to the body
                            spring.anchor = Vector2.zero;
                            spring.connectedAnchor = hitInfo.point;
                            leg.spring = spring;
                            leg.swingCollision = true;
                            leg.lr.SetPosition (1, leg.spring.connectedAnchor);
                            leg.lr.enabled = true;
                        }
                    } else {
                        // did not hit anything, change web color to red with the end at the maximum range
                        leg.swingCollision = false;
                        Vector2 reachedPoint = legTransform.position + legTransform.up * (leg.height + WebSwingRange);
                        leg.lr.SetPosition (1, reachedPoint);
                        leg.lr.enabled = true;
                    }

                    AudioUtils.Play("Laser");
                }
            } else if (Input.GetKeyUp(key)) {
                leg.lr.enabled = false;
                SpringJoint2D spring = leg.spring;
                if (spring) {
                    Destroy (spring);
                    leg.webHitInfo = new RaycastHit2D();
                }
            }

            if (leg.lr.enabled) {
                if (leg.webHitInfo.collider && !leg.webHitInfo.collider.name.ToLower().Contains("enemy")) { // if we're connected to (not enemy)
                    // shoot ray again to check if the string should be destroyed
                    LayerMask layerMask = ~(1 << LayerMask.NameToLayer("Player"));
                    Transform legTransform = leg.gameObject.transform;
                    Vector2 legEnd = legTransform.position + (legTransform.up * leg.height);
                    RaycastHit2D hitInfo = Physics2D.Raycast(legEnd, leg.webHitInfo.point - legEnd, WebSwingRange, layerMask);
                    if (Vector2.SqrMagnitude(hitInfo.point - leg.webHitInfo.point) > 0.001F) {
                        leg.lr.enabled = false;
                        SpringJoint2D spring = leg.spring;
                        Destroy (spring);
                        leg.webHitInfo = new RaycastHit2D();

                        AudioUtils.Play("FabricTorn", hitInfo.point);
                        return;
                    }
                }
				
                Vector2 legTip = leg.gameObject.transform.position + Quaternion.Euler(0, 0, leg.gameObject.transform.eulerAngles.z) * new Vector2(0, leg.height);
                if (leg.swingCollision) {
                    leg.lr.SetPosition (0, legTip);
                    leg.lr.SetColors (Color.white, Color.white);
                } else {
                    leg.lr.SetColors (Color.red, Color.red);
                    leg.lr.SetPosition (0, legTip);
                }
            }
        }

        void OnDestroy() {
            if (gameOverOverlay != null) {
                gameOverOverlay.SetActive(true);
            }
        }
    }
}