using HarmonyLib;
using System.Linq;
using ULTRAKILL.Cheats;
using UnityEngine;
using static UnityEngine.Object;
using System.Collections.Generic;

namespace V3Mod
{
    public class WeaponInfo : MonoBehaviour
    {
        private V3 v3;

        private GameObject Object => transform.GetChild(0).gameObject;

        public IEnemyWeapon EnemyWeapon => Object.GetComponent<IEnemyWeapon>();

        public bool chargingAlt;
        public float drawTime, drawTimeAlt, prepareTimeAlt;

        public bool altBlocksNormal, normalBlocksAlt;
        public bool resetCooldownOnSwap, resetCooldownOnSwapAlt;

        public float cooldown, cooldownAlt;
        public int maxCharges, maxChargesAlt;
        public float chargeIncrease, chargeIncreaseAlt;
        public float charge, chargeAlt;

        public float projectileSpeed, projectileSpeedAlt;
        public bool useGravity, useGravityAlt, aimAtHead, aimAtHeadAlt;
        public float PredictAmount => Predict(projectileSpeed);
        public float AltPredictAmount => Predict(projectileSpeedAlt);

        public System.Func<V3, float> weightCalc, weightCalcAlt;
        public float Weight => (!Object.activeSelf && resetCooldownOnSwap || timeSinceFired >= cooldown) && (!altBlocksNormal || !Object.activeSelf && resetCooldownOnSwapAlt || timeSinceFiredAlt >= cooldownAlt) && charge >= 1 && PredictAmount >= 0 ? weightCalc.Invoke(v3) : 0;
        public float AltWeight => (!Object.activeSelf && resetCooldownOnSwapAlt || timeSinceFiredAlt >= cooldownAlt) && (!normalBlocksAlt || !Object.activeSelf && resetCooldownOnSwap || timeSinceFired >= cooldown) && chargeAlt >= 1 && AltPredictAmount >= 0 ? weightCalcAlt.Invoke(v3) : 0;
        public float TotalWeight => Weight + AltWeight;

        private TimeSince? timeSinceEnabled;
        public TimeSince timeSinceFired, timeSinceFiredAlt;

        public bool Ready => timeSinceEnabled > drawTime && !chargingAlt && timeSinceFired >= cooldown && (!altBlocksNormal || timeSinceFiredAlt >= cooldownAlt) && charge >= 1;
        public bool ReadyAlt => timeSinceEnabled > drawTimeAlt && !chargingAlt && timeSinceFiredAlt >= cooldownAlt && (!normalBlocksAlt || timeSinceFired >= cooldown) && chargeAlt >= 1;

        private void Awake() => v3 = GetComponentInParent<V3>();

        private void Update()
        {
            if (maxCharges < 0)
            {
                charge = 1;
            }
            else if (charge < maxCharges)
            {
                charge = Mathf.MoveTowards(charge, maxCharges, chargeIncrease * Time.deltaTime);
            }

            if (maxChargesAlt < 0)
            {
                chargeAlt = 1;
            }
            else if (chargeAlt < maxChargesAlt)
            {
                chargeAlt = Mathf.MoveTowards(chargeAlt, maxChargesAlt, chargeIncreaseAlt * Time.deltaTime);
            }
        }

        private float Predict(float speed)
        {
            if (speed < 0 || v3.Target == null)
            {
                return 0;
            }

            Vector3 diff = (v3.overrideTarget?.transform.position ?? v3.Target.position) - v3.aimAtTarget[1].position;
            Vector3 vel = v3.overrideTarget ? Vector3.zero : v3.Target.GetVelocity();

            float a = Vector3.Dot(vel, vel) - speed * speed;
            float b = Vector3.Dot(vel, diff) * 2;
            float c = Vector3.Dot(diff, diff);

            float disc = b * b - 4 * a * c;

            return disc > 0 ? 2 * c / Mathf.Sqrt(disc - b) : -1;
        }

        public void SetActive(bool value)
        {
            if (value == Object.activeSelf)
            {
                return;
            }

            EnemyWeapon.CancelAltCharge();
            Object.SetActive(value);
            timeSinceEnabled = value ? 0 : null;

            if (value)
            {
                if (resetCooldownOnSwap)
                {
                    timeSinceFired = cooldown;
                }

                if (resetCooldownOnSwapAlt)
                {
                    timeSinceFiredAlt = cooldownAlt;
                }
            }
        }

        public void UpdateBuffs() => Object.transform.SendMessage("UpdateBuffs", v3.eid);

        public void Fire(bool fake = false)
        {
            if (!Ready)
            {
                return;
            }

            if (!fake)
            {
                EnemyWeapon.UpdateTarget(v3.Target);
                EnemyWeapon.Fire();
            }
            timeSinceFired = 0;
            charge--;
        }

        public void PrepareAltFire(bool fake = false)
        {
            if (!ReadyAlt)
            {
                return;
            }

            chargingAlt = true;
            if (prepareTimeAlt > 0)
            {
                Object.transform.SendMessage("PrepareAltFire");
                Invoke(fake ? "FakeAltFire" : "AltFire", prepareTimeAlt);
            }
            else if (fake)
            {
                FakeAltFire();
            }
            else
            {
                AltFire();
            }
        }

        public void AltFire()
        {
            EnemyWeapon.UpdateTarget(v3.Target);
            EnemyWeapon.AltFire();
            timeSinceFiredAlt = 0;
            chargingAlt = false;
            chargeAlt--;
        }

        public void CancelAltCharge()
        {
            CancelInvoke("AltFire");
            EnemyWeapon.CancelAltCharge();
            chargingAlt = false;
        }

        public void FakeAltFire()
        {
            timeSinceFiredAlt = 0;
            chargingAlt = false;
            chargeAlt--;
        }
    }

    /*
    [HarmonyPatch(typeof(Coin))]
    internal class CoinPatch
    {
        [HarmonyPatch("DelayedReflectRevolver"), HarmonyPrefix]
        private static bool DelayedReflectRevolver(GameObject beam, Coin __instance)
        {
            if (beam && beam.TryGetComponent(out RevolverBeam component) && component.beamType == BeamType.Enemy && component.ignoreEnemyType == EnemyType.V2)
            {
                __instance.customTarget = component.sourceWeapon.GetComponentInParent<EnemyIdentifier>().target;
                __instance.DelayedEnemyReflect();
                return false;
            }
            else
            {
                return true;
            }
        }
    }

    [HarmonyPatch(typeof(Cannonball))]
    internal class CannonballPatch
    {
        [HarmonyPatch("OnTriggerEnter"), HarmonyPrefix]
        private static bool OnTriggerEnter(Collider other, Cannonball __instance, ref bool ___checkingForBreak, Rigidbody ___rb)
        {
            if (__instance.sourceWeapon?.transform.GetComponentInParent<EnemyIdentifier>() && other.CompareTag("Player"))
            {
                ___checkingForBreak = true;
                NewMovement.Instance.GetHurt(Mathf.RoundToInt(Mathf.Min(__instance.damage, ___rb.velocity.magnitude * 0.15f) * 10), true);
                NewMovement.Instance.GetComponent<Rigidbody>().AddForce(___rb.velocity, ForceMode.VelocityChange);
                if (!NewMovement.Instance.gc.onGround)
                {
                    NewMovement.Instance.GetComponent<Rigidbody>().AddForce(Vector3.up, ForceMode.VelocityChange);
                }

                if (___rb.velocity.magnitude < 15)
                {
                    __instance.Break();
                }
                else
                {
                    Traverse.Create(__instance).Method("Bounce").GetValue();
                }

                return false;
            }
            else
            {
                return true;
            }
        }
    }

    [HarmonyPatch(typeof(Nail))]
    internal class NailPatch
    {
        [HarmonyPatch("MagnetCaught"), HarmonyPostfix]
        private static void MagnetCaught(Magnet mag, Nail __instance)
        {
            if (mag.transform.parent.parent && mag.transform.parent.parent.CompareTag("Player"))
            {
                __instance.enemy = true;
            }
        }

        [HarmonyPatch("FixedUpdate"), HarmonyPostfix]
        private static void FixedUpdate(Nail __instance)
        {
            if (!__instance.enemy && __instance.magnets.Any(mag => mag != null && mag.transform && mag.transform.parent && mag.transform.parent.parent && mag.transform.parent.parent.CompareTag("Player")))
            {
                __instance.enemy = true;
            }
        }
    }

    [HarmonyPatch(typeof(Nailgun))]
    internal class NailgunPatch
    {
        [HarmonyPatch("ShootMagnet"), HarmonyPrefix]
        private static bool ShootMagnet(Nailgun __instance, CameraController ___cc, CameraFrustumTargeter ___targeter, Animator ___anim, ref float ___fireCooldown, WeaponCharges ___wc)
        {
            Traverse.Create(__instance).Method("UpdateAnimationWeight").GetValue();
            GameObject gameObject = Instantiate(__instance.magnetNail, ___cc.transform.position, __instance.transform.rotation);
            gameObject.transform.forward = __instance.transform.forward;
            if (___targeter.CurrentTarget && ___targeter.IsAutoAimed)
            {
                gameObject.transform.LookAt(___targeter.CurrentTarget.bounds.center);
            }

            gameObject.GetComponent<Rigidbody>().AddForce(gameObject.transform.forward * 100, ForceMode.VelocityChange);
            ___anim.SetTrigger("Shoot");
            if (___fireCooldown < 10)
            {
                ___fireCooldown = 10;
            }

            Magnet componentInChildren = gameObject.GetComponentInChildren<Magnet>();
            if ((bool)componentInChildren)
            {
                ___wc.magnets.Add(componentInChildren);
            }

            ___wc.naiMagnetCharge -= 1;
            MonoSingleton<RumbleManager>.Instance.SetVibration(RumbleProperties.Magnet);

            foreach (Collider col in gameObject.GetComponentsInChildren<Collider>())
            {
                Physics.IgnoreCollision(col, NewMovement.Instance.GetComponent<Collider>());
            }

            return false;
        }
    }

    [HarmonyPatch(typeof(Railcannon))]
    internal class RailcannonPatch
    {
        [HarmonyPatch("Shoot"), HarmonyPrefix]
        private static bool Shoot(Railcannon __instance, CameraController ___cc, CameraFrustumTargeter ___targeter, GunControl ___gc, Animator ___anim)
        {
            GameObject gameObject = Instantiate(__instance.beam, ___cc.GetDefaultPos(), ___cc.transform.rotation);
            if (___targeter.CurrentTarget && ___targeter.IsAutoAimed)
            {
                gameObject.transform.LookAt(___targeter.CurrentTarget.bounds.center);
            }

            if (__instance.variation != 1)
            {
                if (gameObject.TryGetComponent<RevolverBeam>(out RevolverBeam component))
                {
                    component.sourceWeapon = ___gc.currentWeapon;
                    component.alternateStartPoint = __instance.shootPoint.position;
                }
            }
            else
            {
                gameObject.GetComponent<Rigidbody>().AddForce(gameObject.transform.forward * 250, ForceMode.VelocityChange);
            }

            Instantiate(__instance.fireSound);
            ___anim.SetTrigger("Shoot");
            ___cc.CameraShake(2);
            MonoSingleton<RumbleManager>.Instance.SetVibration(RumbleProperties.GunFireStrong);

            foreach (Collider col in gameObject.GetComponentsInChildren<Collider>())
            {
                Physics.IgnoreCollision(col, NewMovement.Instance.GetComponent<Collider>());
            }

            return false;
        }
    }

    [HarmonyPatch(typeof(Harpoon))]
    internal class HarpoonPatch
    {
        [HarmonyPatch("Start"), HarmonyPrefix]
        private static void Start(Harpoon __instance, ref AudioSource ___aud) => ___aud = __instance.GetComponent<AudioSource>();

        [HarmonyPatch("FixedUpdate"), HarmonyPrefix]
        private static bool FixedUpdate(Harpoon __instance, bool ___stopped, bool ___drilling, ref float ___drillCooldown, AudioSource ___currentDrillSound, ref int ___drillHitsLeft, int ___drillHits)
        {
            if (__instance.GetComponentInParent<NewMovement>())
            {
                if (___stopped && ___drilling)
                {
                    if (___drillCooldown > 0)
                    {
                        ___drillCooldown = Mathf.MoveTowards(___drillCooldown, 0, Time.deltaTime);
                    }
                    else
                    {
                        ___drillCooldown = 0.08f;
                        NewMovement.Instance.GetHurt(1, false, hardDamageMultiplier: 0, ignoreInvincibility: true);

                        if (___currentDrillSound)
                        {
                            ___currentDrillSound.pitch = 1.5f - ___drillHitsLeft / (float)___drillHits / 2;
                        }

                        if (___drillHitsLeft > 0)
                        {
                            ___drillHitsLeft--;
                        }
                        else if (!PauseTimedBombs.Paused)
                        {
                            Destroy(__instance.gameObject);
                        }
                    }
                }

                return false;
            }
            else
            {
                return true;
            }
        }

        [HarmonyPatch("OnTriggerEnter"), HarmonyPrefix]
        private static bool OnTriggerEnter(Collider other, Harpoon __instance, ref bool ___hit, ref float ___damageLeft, Rigidbody ___rb, ref bool ___stopped, ref bool ___drilling, ref AudioSource ___currentDrillSound, ref int ___drillHitsLeft, TrailRenderer ___tr, Magnet ___magnet, ref AudioSource ___aud)
        {
            if (!___hit && other.gameObject.CompareTag("Player"))
            {
                ___hit = true;
                NewMovement.Instance.GetHurt(Mathf.RoundToInt(___damageLeft * 10), false);

                ___rb.velocity = Vector3.zero;
                ___rb.useGravity = false;
                ___rb.constraints = RigidbodyConstraints.FreezeAll;
                __instance.transform.position = other.transform.position - (__instance.GetComponent<Collider>().bounds.center - __instance.transform.position);
                __instance.transform.SetParent(other.transform, true);

                ___stopped = true;
                if (__instance.drill)
                {
                    ___drilling = true;
                    ___currentDrillSound = Instantiate(__instance.drillSound, __instance.transform.position, __instance.transform.rotation);
                    ___currentDrillSound.transform.SetParent(__instance.transform, true);

                    __instance.drillHits = 72;
                    ___drillHitsLeft = 72;
                }

                ___rb.collisionDetectionMode = CollisionDetectionMode.Discrete;
                ___tr.emitting = false;

                __instance.GetComponent<TimeBomb>().StartCountdown();

                if (___magnet)
                {
                    //___magnet.onEnemy = true;
                    ___magnet.transform.position = other.bounds.center;

                    foreach (Breakable component in __instance.GetComponentsInChildren<Breakable>())
                    {
                        Physics.IgnoreCollision(component.GetComponent<Collider>(), NewMovement.Instance.gc.GetComponent<Collider>());
                    }
                }

                if (!___aud)
                {
                    ___aud = __instance.GetComponent<AudioSource>();
                }

                ___aud.clip = __instance.enemyHitSound;
                ___aud.pitch = Random.Range(0.9f, 1.1f);
                ___aud.volume = 0.4f;
                ___aud.Play();

                return false;
            }
            else
            {
                return true;
            }
        }
    }

    [HarmonyPatch(typeof(Magnet))]
    internal class MagnetPatch
    {
        public static List<Magnet> magnets = [];

        [HarmonyPatch("Start"), HarmonyPostfix]
        private static void Start(Magnet __instance) => magnets.Add(__instance);

        [HarmonyPatch("OnDestroy"), HarmonyPostfix]
        private static void OnDestroy(Magnet __instance) => magnets.Remove(__instance);

        [HarmonyPatch("OnTriggerEnter"), HarmonyPostfix]
        private static void OnTriggerEnter(Collider other, Magnet __instance)
        {
            if ((__instance.transform.parent.parent?.CompareTag("Player") ?? false) && other.gameObject.layer == 14 && other.gameObject.CompareTag("Metal") && other.attachedRigidbody.TryGetComponent(out Grenade component))
            {
                component.enemy = true;
            }
        }
    }

    [HarmonyPatch(typeof(Punch))]
    internal class PunchPatch
    {
        [HarmonyPatch("ActiveFrame"), HarmonyPrefix]
        private static void ActiveFrame(Punch __instance, CameraController ___cc, StyleHUD ___shud, float ___screenShakeMultiplier)
        {
            Traverse tv = Traverse.Create(__instance);

            Harpoon[] harpoons = NewMovement.Instance.GetComponentsInChildren<Harpoon>();
            if (harpoons.Any() && ___cc.transform.rotation.eulerAngles.x < 90)
            {
                Harpoon component = harpoons.OrderBy(h => h.drill).First();

                if (__instance.type == FistType.Standard)
                {
                    if (!__instance.parriedSomething && component.drill)
                    {
                        TimeController.Instance.HitStop(0.1f);
                        ___cc.CameraShake(0.5f * ___screenShakeMultiplier);
                        __instance.anim.Play("Hook", 0, 0.065f);
                        TimeController.Instance.ParryFlash();
                        ___shud.AddPoints(100, $"<color=green>SCREW OFF</color>");

                        int hitsLeft = Traverse.Create(component).Field("drillHitsLeft").GetValue<int>();
                        NewMovement.Instance.GetHurt(hitsLeft, false, 0.5f, hardDamageMultiplier: 0, ignoreInvincibility: true);
                        NewMovement.Instance.GetHurt(40, true, 0, ignoreInvincibility: true);
                        component.drillHits = 115;

                        component.transform.forward = ___cc.transform.forward;
                        component.transform.position = ___cc.GetDefaultPos();
                        component.Punched();

                        __instance.hitSomething = true;
                        __instance.parriedSomething = true;
                    }
                    else if (!__instance.hitSomething && !component.drill)
                    {
                        Destroy(component.transform.gameObject);
                        __instance.hitSomething = true;
                    }
                }
                else if (!__instance.hitSomething)
                {
                    Destroy(component.transform.gameObject);
                    __instance.hitSomething = true;
                }
            }
        }
    }

    [HarmonyPatch(typeof(Grenade))]
    internal class GrenadePatch
    {
        [HarmonyPatch("frozen", MethodType.Getter), HarmonyPostfix]
        private static void GetFrozen(ref bool __result)
        {
            if (!__result && V3.v3List.Any(v3 => v3.rocketsFrozen))
            {
                __result = true;
            }
        }
    }
    */
}
