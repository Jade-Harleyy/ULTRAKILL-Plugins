using HarmonyLib;
using Sandbox;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using static V3Mod.Extensions;

namespace V3Mod
{
    public class V3 : MonoBehaviour, IAlter, IAlterOptions<bool>, IAlterOptions<float>
    {
        public static List<V3> v3List = [];

        private Animator anim;
        private Rigidbody rb;
        public SkinnedMeshRenderer smr;
        private EnemySimplifier[] ensims;
        public EnemyIdentifier eid;
        private Machine mac;
        public BossHealthBar bhb;
        private int difficulty;
        public float healMultiplier = 1, playerHealDist;
        public float maxHP;

        // Wings
        public Texture[] wingTextures;
        public GameObject wingChangeEffect, currentWingChangeEffect;
        public Color[] patternColors;
        private DragBehind[] drags;

        // Movement
        private float circleTimer = 5;
        private float distancePatience;
        private float slideStopTimer;
        public float dodgeLeft;
        private float stamina = 3, staminaRegen;
        private int wallJumps = 3;
        private float patternCooldown;
        public bool canSlide = true, canJump = true, canSlam = true, canDash = true;
        public bool sliding, jumping, slamming;
        public float jumpPower, wallJumpPower, airAcceleration;
        public GameObject dodgeParticles, slideScrape, slideParticles, slamParticles, slamHitEffect;
        private int prevPattern, currentPattern;
        private NavMeshAgent nma;
        public GameObject jumpSound, dashJumpSound, finalWallJumpSound;
        private int circleDirection;
        public float circleDistance;
        public GroundCheckEnemy gc, wc;
        private float wallHitCooldown;

        public float stepCooldown;  
        public AudioSource stepAud;
        public AudioClip[] stepSounds;

        // Attacks
        private WeaponInfo currentWeapon;
        private float weaponSwapCooldown;
        public WeaponInfo[] weapons;
        public Component overrideTarget;
        private Quaternion targetRot;
        public Transform[] aimAtTarget;
        private float shootableInSightCooldown;
        public float rcCooldown, maxRcCooldown;
        public bool snipeGrenades = true, snipeCoins = true, snipeCannonballs = true, snipeMagnets = true;
        public List<Magnet> activeMagnets;
        public bool rocketsFrozen;
        public float rocketFreezeCharge = 5;
        public bool RocketsFrozen => WeaponCharges.Instance.rocketFrozen || v3List.Any(v3 => v3.rocketsFrozen);

        // Weapons
        public WeaponInfo[] Revolvers => [weapons[0], weapons[1], weapons[2]];
        public bool rev0 = true, rev1 = true, rev2 = true;
        public WeaponInfo[] Shotguns => [weapons[3], weapons[4]];
        public bool stg0 = true, stg1 = true;
        public WeaponInfo[] Nailguns => [weapons[5], weapons[6]];
        public bool nai0 = true, nai1 = true;
        public WeaponInfo[] Railcannons => [weapons[7], weapons[8], weapons[9]];
        public bool rai0 = true, rai1 = true, rai2 = true;
        public WeaponInfo[] RocketLaunchers => [weapons[10], weapons[11]];
        public bool rkt0 = true, rkt1 = true;

        // Target
        public EnemyTarget Target => eid.target;
        public float DistanceToTarget => Vector3.Distance(transform.position, Target.position);
        public float HorizontalDistanceToTarget => Vector3.Distance(transform.position.SetY(0), Target.position.SetY(0));
        public bool TargetHasStuckMagnets => Target.isEnemy ? Target.enemyIdentifier.stuckMagnets.Any() : NewMovement.Instance.transform.GetComponentInChildren<Magnet>();
        public bool TargetIsAirborne => !Target.isOnGround && (Target.isPlayer || !Target.enemyIdentifier.flying);

        // Sandbox
        public string alterKey => ConstInfo.GUID + ".V3-B";
        public string alterCategoryName => "V3";
        AlterOption<bool>[] IAlterOptions<bool>.options =>
        [
            new AlterOption<bool>
            {
                name = "Slide", key = "slide", value = canSlide,
                callback = delegate(bool value)
                {
                    canSlide = value;
                }
            },
            new AlterOption<bool>
            {
                name = "Jump", key = "jump", value = canJump,
                callback = delegate(bool value)
                {
                    canJump = value;
                }
            },
            new AlterOption<bool>
            {
                name = "Slam", key = "slam", value = canSlam,
                callback = delegate(bool value)
                {
                    canSlam = value;
                }
            },
            new AlterOption<bool>
            {
                name = "Dash", key = "dash", value = canDash,
                callback = delegate(bool value)
                {
                    canDash = value;
                }
            },
            new AlterOption<bool>
            {
                name = "Snipe Grenades", key = "grenades", value = snipeGrenades,
                callback = delegate(bool value)
                {
                    snipeGrenades = value;
                }
            },
            new AlterOption<bool>
            {
                name = "Snipe Coins", key = "coins", value = snipeCoins,
                callback = delegate(bool value)
                {
                    snipeCoins = value;
                }
            },
            new AlterOption<bool>
            {
                name = "Snipe Cannonballs", key = "cannonballs", value = snipeCannonballs,
                callback = delegate(bool value)
                {
                    snipeCannonballs = value;
                }
            },
            new AlterOption<bool>
            {
                name = "Snipe Magnets", key = "magnets", value = snipeMagnets,
                callback = delegate(bool value)
                {
                    snipeMagnets = value;
                }
            },
            new AlterOption<bool>
            {
                name = "Piercer Revolver", key = "rev0", value = rev0,
                callback = delegate(bool value)
                {
                    rev0 = value;
                }
            },
            new AlterOption<bool>
            {
                name = "Marksman Revolver", key = "rev1", value = rev1,
                callback = delegate(bool value)
                {
                    rev1 = value;
                }
            },
            new AlterOption<bool>
            {
                name = "Sharpshooter Revolver", key = "rev2", value = rev2,
                callback = delegate(bool value)
                {
                    rev2 = value;
                }
            },
            new AlterOption<bool>
            {
                name = "Core Eject Shotgun", key = "stg0", value = stg0,
                callback = delegate(bool value)
                {
                    stg0 = value;
                }
            },
            new AlterOption<bool>
            {
                name = "Pump Charge Shotgun", key = "stg1", value = stg1,
                callback = delegate(bool value)
                {
                    stg1 = value;
                }
            },
            new AlterOption<bool>
            {
                name = "Attractor Nailgun", key = "nai0", value = nai0,
                callback = delegate(bool value)
                {
                    nai0 = value;
                }
            },
            new AlterOption<bool>
            {
                name = "Overheat Nailgun", key = "nai1", value = nai1,
                callback = delegate(bool value)
                {
                    nai1 = value;
                }
            },
            new AlterOption<bool>
            {
                name = "Electric Railcannon", key = "rai0", value = rai0,
                callback = delegate(bool value)
                {
                    rai0 = value;
                }
            },
            new AlterOption<bool>
            {
                name = "Screwdriver Railcannon", key = "rai1", value = rai1,
                callback = delegate(bool value)
                {
                    rai1 = value;
                }
            },
            new AlterOption<bool>
            {
                name = "Malicious Railcannon", key = "rai2", value = rai2,
                callback = delegate(bool value)
                {
                    rai2 = value;
                }
            },
            new AlterOption<bool>
            {
                name = "Freezeframe Rocket Launcher", key = "rkt0", value = rkt0,
                callback = delegate(bool value)
                {
                    rkt0 = value;
                }
            },
            new AlterOption<bool>
            {
                name = "S.R.S. Cannon Rocket Launcher", key = "rkt1", value = rkt1,
                callback = delegate(bool value)
                {
                    rkt1 = value;
                }
            }
        ];

        AlterOption<float>[] IAlterOptions<float>.options =>
        [
            new AlterOption<float>
            {
                name = "Rail Cooldown", key = "rcDelay", value = maxRcCooldown,
                callback = delegate(float value)
                {
                    maxRcCooldown = value;
                    if (rcCooldown > value)
                    {
                        rcCooldown = value;
                    }
                }
            },
            new AlterOption<float>
            {
                name = "Healing Factor", key = "healMult", value = healMultiplier,
                callback = delegate(float value)
                {
                    healMultiplier = value;
                }
            }
        ];

        private void Awake()
        {
            anim = GetComponentInChildren<Animator>();
            rb = GetComponent<Rigidbody>();
            gc = GetComponentInChildren<GroundCheckEnemy>();
            nma = GetComponent<NavMeshAgent>();
            eid = GetComponent<EnemyIdentifier>();
            mac = GetComponent<Machine>();
            ensims = GetComponentsInChildren<EnemySimplifier>();
            drags = GetComponentsInChildren<DragBehind>();
            v3List.Add(this);
        }

        private void OnDestroy() => v3List.Remove(this);

        private void Start()
        {
            difficulty = PrefsManager.Instance.GetInt("difficulty");
            SwitchPattern(currentPattern);
            foreach (WeaponInfo weapon in weapons)
            {
                weapon.UpdateBuffs();
                weapon.SetActive(false);
            }

            rcCooldown = maxRcCooldown;

            // Piercer Revolver
            weapons[0].weightCalc = (v3) => v3.rev0 ? 2 / 3 : 0;
            weapons[0].weightCalcAlt = (v3) => v3.rev0 ? 1 : 0;
            // Marksman Revolver
            weapons[1].weightCalc = (v3) => v3.rev1 ? 2 / 3 : 0;
            weapons[1].weightCalcAlt = (v3) => v3.rev1 && v3.snipeCoins ? 5 : 0;
            // Sharpshooter Revolver
            weapons[2].weightCalc = (v3) => v3.rev2 ? 2 / 3 : 0;
            weapons[2].weightCalcAlt = (v3) => v3.rev2 ? 1 : 0;
            // Core Eject Shotgun
            weapons[3].weightCalc = (v3) => v3.stg0 && v3.DistanceToTarget <= 20 ? 2 : 0;
            weapons[3].weightCalcAlt = (v3) => v3.stg0 && v3.DistanceToTarget is > 6 and <= 50 ? 1 : 0;
            // Pump Charge Shotgun
            weapons[4].weightCalc = (v3) => v3.stg1 && v3.DistanceToTarget <= 20 ? 4 : 0;
            weapons[4].weightCalcAlt = (v3) => v3.stg1 ? 0 : 0; // TODO
            // Attractor Nailgun
            weapons[5].weightCalc = (v3) => v3.nai0 && v3.DistanceToTarget <= 75 && !v3.eid.stuckMagnets.Any() ? 4 : 0;
            weapons[5].weightCalcAlt = (v3) => v3.nai0 && v3.DistanceToTarget <= 50 && !TargetHasStuckMagnets ? 5 : 0;
            // Overheat Nailgun
            weapons[6].weightCalc = (v3) => v3.nai1 ? 0 : 0; // TODO
            weapons[6].weightCalcAlt = (v3) => v3.nai1 ? 0 : 0; // TODO
            // Electric Railcannon
            weapons[7].weightCalc = (v3) => v3.rai0 && v3.rcCooldown <= 0 ? 20 : 0;
            weapons[7].weightCalcAlt = (v3) => 0;
            // Screwdriver Railcannon
            weapons[8].weightCalc = (v3) => v3.rai1 && v3.rcCooldown <= 0 && v3.DistanceToTarget < 40 ? 40 : 0;
            weapons[8].weightCalcAlt = (v3) => 0;
            // Malicious Railcannon
            weapons[9].weightCalc = (v3) => v3.rai2 && v3.rcCooldown <= 0 && v3.DistanceToTarget > 13.5f ? 20 : 0;
            weapons[9].weightCalcAlt = (v3) => 0;
            // Freezeframe Rocket Launcher
            weapons[10].weightCalc = (v3) => v3.rkt0 && v3.DistanceToTarget > (v3.TargetIsAirborne ? 12 : 6) && v3.DistanceToTarget <= 30 && !v3.eid.stuckMagnets.Any() && (!v3.RocketsFrozen || v3.TargetHasStuckMagnets) ? 1 + (v3.TargetHasStuckMagnets ? 2 : 0) + (v3.TargetIsAirborne ? 3 : 0) : 0;
            weapons[10].weightCalcAlt = (v3) => v3.rkt0 && !v3.RocketsFrozen && v3.rocketFreezeCharge > 0 && ObjectTracker.Instance.grenadeList.Any(g => g.rocket && !g.enemy) && !v3.TargetHasStuckMagnets ? 10 : 0;
            // S.R.S. Cannon Rocket Launcher
            weapons[11].weightCalc = (v3) => v3.rkt1 && v3.DistanceToTarget > (v3.TargetIsAirborne ? 12 : 6) && v3.DistanceToTarget <= 30 && !v3.eid.stuckMagnets.Any() && (!v3.RocketsFrozen || v3.TargetHasStuckMagnets) ? 1 + (v3.TargetHasStuckMagnets ? 2 : 0) + (v3.TargetIsAirborne ? 3 : 0) : 0;
            weapons[11].weightCalcAlt = (v3) => v3.rkt1 && v3.DistanceToTarget <= 75 ? 5 : 0;

            SwitchWeapon(weapons[0]);

            staminaRegen = difficulty switch
            {
                0 => 1.4f,
                1 => 1.05f,
                _ => 0.7f
            };
        }

        private void Update()
        {
            if (rcCooldown > 0)
            {
                rcCooldown = Mathf.MoveTowards(rcCooldown, 0, Time.deltaTime);
            }

            activeMagnets.RemoveAll(magnet => !magnet);
            Nailguns[0].maxChargesAlt = 3 - activeMagnets.Count;

            if (rocketsFrozen && rocketFreezeCharge > 0)
            {
                rocketFreezeCharge = Mathf.MoveTowards(rocketFreezeCharge, 0, Time.deltaTime);
                if (rocketFreezeCharge <= 0)
                {
                    PrepareSpecialAlt("ToggleRocketFreeze");
                }
            }
            else if (!rocketsFrozen && rocketFreezeCharge < 5)
            {
                rocketFreezeCharge = Mathf.MoveTowards(rocketFreezeCharge, 5, Time.deltaTime / 2);
            }

            if (bhb || TryGetComponent(out bhb))
            {
                if (!bhb.secondaryBar)
                {
                    Destroy(bhb);
                    bhb = gameObject.AddComponent<BossHealthBar>();
                    bhb.bossName = "V3-B";
                    bhb.secondaryBar = true;
                }
                else
                {
                    bhb.SetSecondaryBarColor(rcCooldown > 0 ? ColorBlindSettings.Instance.railcannonChargingColor : ColorBlindSettings.Instance.railcannonFullColor);
                    bhb.UpdateSecondaryBar(maxRcCooldown > 0 ? (maxRcCooldown - rcCooldown) / maxRcCooldown : 1);
                }
            }

            if (!sliding && stamina < 3)
            {
                stamina = Mathf.MoveTowards(stamina, 3, staminaRegen * Time.deltaTime);
            }

            anim.SetBool("InAir", !gc.onGround || dodgeLeft > 0);

            foreach (DragBehind drag in drags)
            {
                drag.active = !gc.onGround || dodgeLeft > 0 || sliding;
            }

            if ((rb.velocity.SetY(0).magnitude > 0 || nma.velocity.SetY(0).magnitude > 0) && dodgeLeft <= 0 && gc.onGround && !sliding)
            {
                float num = Quaternion.Angle(anim.transform.rotation, transform.rotation);

                anim.SetLayerWeight(1, 1);
                anim.SetLayerWeight(2, (num <= 90 ? num : num - 180) / 90);

                anim.SetBool("RunningLeft", anim.transform.rotation.eulerAngles.y > transform.rotation.eulerAngles.y);
                anim.SetBool("RunningBack", num > 90);

                if (stepCooldown > 0)
                {
                    stepCooldown = Mathf.MoveTowards(stepCooldown, 0, Time.deltaTime);
                }
                else
                {
                    stepAud.pitch = Random.Range(0.9f, 1.1f);
                    stepAud.PlayOneShot(stepSounds[Random.Range(0, stepSounds.Length)]);
                    stepCooldown = 0.4f;
                }
            }
            else
            {
                anim.SetLayerWeight(1, 0);
                anim.SetLayerWeight(2, 0);
                stepCooldown = 0.4f;
            }

            if (Target == null || nma.enabled)
            {
                SetSlide(false);
            }
            else
            {
                if (wallHitCooldown > 0)
                {
                    wallHitCooldown = Mathf.MoveTowards(wallHitCooldown, 0, Time.deltaTime);
                }

                if (patternCooldown > 0)
                {
                    patternCooldown = Mathf.MoveTowards(patternCooldown, 0, Time.deltaTime);
                }

                if (!sliding)
                {
                    targetRot = Quaternion.LookRotation((Target.position - transform.position).SetY(0), Vector3.up);

                    if (dodgeLeft <= 0 && currentPattern != 0)
                    {
                        if (currentPattern == 3)
                        {
                            transform.rotation = Quaternion.LookRotation((transform.position - Target.position).SetY(0), Vector3.up);
                        }
                        else if (currentPattern == 1 || HorizontalDistanceToTarget < circleDistance)
                        {
                            Quaternion rotation = targetRot;
                            rotation.eulerAngles += new Vector3(0, circleDirection * Mathf.Min(180 * HorizontalDistanceToTarget / circleDistance - 180, 0), 0);
                            transform.rotation = rotation;
                        }
                        else
                        {
                            transform.rotation = targetRot;
                            if (gc.onGround && !jumping && !slamming && HorizontalDistanceToTarget > 20)
                            {
                                SetSlide(true);
                            }
                        }
                    }

                    anim.transform.rotation = Quaternion.RotateTowards(anim.transform.rotation, targetRot, Time.deltaTime * 10 * Quaternion.Angle(anim.transform.rotation, targetRot));
                }
                else
                {
                    Quaternion a = Quaternion.LookRotation(transform.forward, Vector3.up);
                    Quaternion b = Quaternion.LookRotation(Target.position - transform.position, Vector3.up);
                    if (Quaternion.Angle(a, b) > 90 || distancePatience >= 5 && Quaternion.Angle(a, b) > 45)
                    {
                        if (slideStopTimer > 0)
                        {
                            slideStopTimer = Mathf.MoveTowards(slideStopTimer, 0, Time.deltaTime);
                        }
                        else
                        {
                            SetSlide(false);
                        }
                    }
                }

                if (dodgeLeft <= 0)
                {
                    CheckPattern();

                    if (!jumping && Physics.Raycast(transform.position + Vector3.up, Vector3.down, out RaycastHit hit, Mathf.Infinity, LayerMask.GetMask("Environment", "Outdoors")) && hit.distance > 25)
                    {
                        Slam();
                    }
                    else if (wc.onGround && wallHitCooldown <= 0)
                    {
                        if (currentPattern == 1 || Vector3.Distance(transform.position, Target.position) < 10)
                        {
                            circleDirection = -circleDirection;
                        }

                        if (currentPattern == 3 || !gc.onGround)
                        {
                            Jump();
                        }

                        switch (currentPattern)
                        {
                            case 0:
                                if (Physics.Linecast(transform.position, wc.ClosestPoint(), out RaycastHit hit2, LayerMask.GetMask("Environment", "Outdoors")))
                                {
                                    ChangeDirection(transform.position + Vector3.Reflect(wc.ClosestPoint() - transform.position, hit2.normal));
                                }
                                break;
                            case 1:
                                break;
                            case 2:
                            case 3:
                                ChangeDirection(Target.position);
                                break;
                        }

                        wallHitCooldown = 1;
                    }

                    if (DistanceToTarget > 15)
                    {
                        if (distancePatience < 12)
                        {
                            distancePatience = Mathf.MoveTowards(distancePatience, 12, Time.deltaTime);
                        }

                        if ((distancePatience >= 4 || DistanceToTarget > 30) && currentPattern != 2)
                        {
                            SwitchPattern(2);
                        }
                    }
                    else if (distancePatience > 0)
                    {
                        distancePatience = Mathf.MoveTowards(distancePatience, 0, Time.deltaTime * 2);
                    }
                }

                if (currentPattern == 1 || DistanceToTarget < 10)
                {
                    if (circleTimer > 0)
                    {
                        circleTimer = Mathf.MoveTowards(circleTimer, 0, Time.deltaTime * (currentPattern == 1 ? 1 : 1.5f));
                    }
                    else if (dodgeLeft <= 0)
                    {
                        circleTimer = 1;
                        Dodge(Target.position, true);
                        if (currentPattern is not 3 and not 1)
                        {
                            SwitchPattern(3);
                        }
                    }
                }
                else
                {
                    if (circleTimer < 5)
                    {
                        circleTimer = Mathf.MoveTowards(circleTimer, 5, Time.deltaTime);
                    }

                    if (currentPattern == 3 && circleTimer > 2)
                    {
                        CheckPattern();
                        SwitchPattern(prevPattern);
                    }
                }

                List<Coin> visibleCoins = CoinList.Instance.revolverCoinsList.FindAll(c
                    => !Physics.Linecast(aimAtTarget[1].position, c.transform.position, LayerMask.GetMask("Environment", "Outdoors"))
                    && Vector3.Distance(c.transform.position, transform.position) <= 50
                    && c.GetComponent<Rigidbody>());

                List<Grenade> visibleGrenades = ObjectTracker.Instance.grenadeList.FindAll(g
                    => !Physics.Linecast(aimAtTarget[1].position, g.transform.position, LayerMask.GetMask("Environment", "Outdoors"))
                    && !IsSafeFromExplosion(g.rocket ? g.explosion : g.superExplosion, true, g.frozen ? 1.5f : 1)
                    && IsSafeFromExplosion(g.rocket ? g.explosion : g.superExplosion, false, g.frozen ? 1.5f : 1));

                List<Cannonball> visibleCannonballs = ObjectTracker.Instance.cannonballList.FindAll(c
                    => !Physics.Linecast(aimAtTarget[1].position, c.transform.position, LayerMask.GetMask("Environment", "Outdoors"))
                    && !IsSafeFromExplosion(Traverse.Create(c).Field("interruptionExplosion").GetValue<GameObject>(), true)
                    && IsSafeFromExplosion(Traverse.Create(c).Field("interruptionExplosion").GetValue<GameObject>(), false)
                    && !c.sisy);


                List<Magnet> visibleMagnets = [];/*MagnetPatch.magnets.FindAll(m
                    => !Physics.Linecast(aimAtTarget[1].position, m.transform.position, LayerMask.GetMask("Environment", "Outdoors"))
                    && (Vector3.Distance(transform.position, m.transform.position) <= 50 || Nailguns[0].chargeAlt <= 0)
                    && !m.onEnemy
                    && (m.sawblades.Any(saw => saw && saw.TryGetComponent(out Nail component) && component.heated)
                        || m.sawblades.Count > 5
                        || Traverse.Create(m).Field("affectedRbs").GetValue<List<Rigidbody>>().Count > 20));*/

                overrideTarget =
                    visibleGrenades.FirstOrDefault(g => snipeGrenades && Target.isPlayer && g.playerRiding)
                    ?? visibleCoins.FirstOrDefault(c => snipeCoins && !c.shot)
                    ?? visibleCannonballs.FirstOrDefault(_ => snipeCannonballs)
                    ?? visibleGrenades.FirstOrDefault(_ => snipeGrenades)
                    ?? visibleMagnets.FirstOrDefault(_ => snipeMagnets)
                    ?? null as Component;

                if (overrideTarget && (rai2 || rev0 || rev1 || rev2))
                {
                    if (shootableInSightCooldown > 0)
                    {
                        shootableInSightCooldown = Mathf.MoveTowards(shootableInSightCooldown, 0, Time.deltaTime * eid.totalSpeedModifier);
                    }
                    else
                    {
                        if (rai2 && overrideTarget is Grenade { rocket: false } && rcCooldown <= 0 && currentWeapon != Railcannons[2])
                        {
                            SwitchWeapon(Railcannons[2]);
                        }
                        else if ((rev0 || rev1 || rev2) && !Revolvers.Contains(currentWeapon))
                        {
                            List<WeaponInfo> revs = [];
                            if (rev0)
                            {
                                revs.Add(Revolvers[0]);
                            }
                            if (rev1)
                            {
                                revs.Add(Revolvers[1]);
                            }
                            if (rev2)
                            {
                                revs.Add(Revolvers[2]);
                            }

                            SwitchWeapon(revs[Random.Range(0, revs.Count)]);
                        }

                        if (currentWeapon.Ready)
                        {
                            currentWeapon.Fire();
                            weaponSwapCooldown = 0;
                            if (Railcannons.Contains(currentWeapon))
                            {
                                rcCooldown = maxRcCooldown;
                            }
                        }
                    }
                }
                else
                {
                    shootableInSightCooldown = 0.25f;

                    if (weaponSwapCooldown > 0)
                    {
                        weaponSwapCooldown = Mathf.MoveTowards(weaponSwapCooldown, 0, Time.deltaTime * (currentWeapon.Weight <= 0 ? 2 : 1) * (currentWeapon.AltWeight <= 0 ? 2 : 1));
                    }
                    else if (!currentWeapon.chargingAlt)
                    {
                        float rand = Random.Range(0, weapons.Sum(w => w.TotalWeight));
                        foreach (WeaponInfo weapon in weapons.Where(w => w.TotalWeight > 0))
                        {
                            if (rand < weapon.TotalWeight)
                            {
                                weaponSwapCooldown = weapon == currentWeapon ? 2.5f : 7.5f;
                                SwitchWeapon(weapon);
                                break;
                            }
                            rand -= weapon.TotalWeight;
                        }
                    }

                    if (Target.isEnemy || !Traverse.Create(NewMovement.Instance).Field("hurting").GetValue<bool>())
                    {
                        if (currentWeapon.ReadyAlt && Random.Range(0, currentWeapon.TotalWeight) < currentWeapon.AltWeight)
                        {
                            if (currentWeapon == Revolvers[1])
                            {
                                PrepareSpecialAlt("ThrowCoin");
                            }
                            else if (currentWeapon == Nailguns[0])
                            {
                                PrepareSpecialAlt("ShootMagnet");
                            }
                            else if (currentWeapon == RocketLaunchers[0])
                            {
                                PrepareSpecialAlt("ToggleRocketFreeze");
                            }
                            else if (currentWeapon == RocketLaunchers[1])
                            {
                                PrepareSpecialAlt("ShootCannonball");
                            }
                            else
                            {
                                currentWeapon.PrepareAltFire();
                            }
                        }
                        else if (currentWeapon.Ready)
                        {
                            if (currentWeapon == Railcannons[1])
                            {
                                ShootScrew();
                            }
                            else
                            {
                                currentWeapon.Fire();
                            }

                            if (Railcannons.Contains(currentWeapon))
                            {
                                rcCooldown = maxRcCooldown;
                                if (rcCooldown > 0)
                                {
                                    weaponSwapCooldown = 0;
                                }
                            }
                            else if (RocketLaunchers.Contains(currentWeapon))
                            {
                                RocketLaunchers.Do(wi => wi.timeSinceFired = 0);
                            }
                        }
                    }
                }
            }
        }

        private void PrepareSpecialAlt(string methodName)
        {
            currentWeapon.PrepareAltFire(true);
            Invoke(methodName, currentWeapon.prepareTimeAlt);
        }

        private void ThrowCoin() => Instantiate((currentWeapon.EnemyWeapon as EnemyRevolver).altBullet, aimAtTarget[0].position, aimAtTarget[0].rotation).GetComponent<Rigidbody>().AddForce(aimAtTarget[0].forward * 20 + Vector3.up * 15 + rb.velocity, ForceMode.VelocityChange);

        private void ShootMagnet()
        {
            EnemyNailgun weapon = currentWeapon.EnemyWeapon as EnemyNailgun;
            GameObject obj = Instantiate(weapon.altNail, weapon.shootPoint.position, weapon.shootPoint.rotation);
            obj.GetComponent<Rigidbody>().AddForce(aimAtTarget[1].up * 100, ForceMode.VelocityChange);
            activeMagnets.Add(obj.GetComponentInChildren<Magnet>());

            NoCollide(obj.GetComponent<Collider>());
        }

        private void ShootCannonball()
        {
            EnemyRevolver weapon = currentWeapon.EnemyWeapon as EnemyRevolver;
            GameObject obj = Instantiate(weapon.altBullet, weapon.shootPoint.position + weapon.shootPoint.forward, weapon.shootPoint.rotation);
            obj.GetComponent<Rigidbody>().AddForce(aimAtTarget[1].up * 150, ForceMode.VelocityChange);

            NoCollide(obj.GetComponent<Collider>());
        }

        private void ShootScrew()
        {
            currentWeapon.Fire(true);
            EnemyRevolver weapon = currentWeapon.EnemyWeapon as EnemyRevolver;
            GameObject obj = Instantiate(weapon.bullet, weapon.shootPoint.position, weapon.shootPoint.rotation);
            obj.GetComponent<Rigidbody>().AddForce(aimAtTarget[1].up * 250, ForceMode.VelocityChange);

            NoCollide(obj.GetComponent<Collider>());
        }

        private void ToggleRocketFreeze() => rocketsFrozen = !rocketsFrozen;

        private void NoCollide(Collider other)
        {
            Collider[] cols = aimAtTarget[1].GetComponentsInChildren<Collider>();
            foreach (Collider col in cols)
            {
                Physics.IgnoreCollision(col, other, true);
            }
        }

        private bool IsSafeFromExplosion(GameObject explosion, bool target, float sizeMultiplier = 1)
            => !explosion || explosion.GetComponentsInChildren<Explosion>().Any(e => !e.harmless
            && e.canHit != (target && Target.isPlayer ? AffectedSubjects.PlayerOnly : AffectedSubjects.EnemiesOnly)
            && Vector3.Distance(e.transform.position, target ? Target.position + Target.rigidbody.velocity : transform.position + rb.velocity) > e.maxSize * sizeMultiplier);

        private void FixedUpdate()
        {
            if (gc.onGround)
            {
                if (!jumping)
                {
                    wallJumps = 3;
                }

                if (slamming)
                {
                    slamParticles.SetActive(false);
                    Instantiate(slamHitEffect, transform.position, Quaternion.Euler(90, 0, 0)).SetActive(true);
                    slamming = false;
                }
            }

            if (Target != null)
            {
                if (Physics.Linecast(aimAtTarget[0].position, Target.position, LayerMask.GetMask("Environment", "Outdoors")))
                {
                    nma.enabled = true;
                    nma.SetDestination(Target.position);
                    SetSlide(false);
                }
                else
                {
                    nma.enabled = false;
                    rb.isKinematic = false;

                    if (dodgeLeft > 0)
                    {
                        rb.useGravity = false;
                        rb.velocity = transform.forward * nma.speed * 3;
                        dodgeLeft = Mathf.MoveTowards(dodgeLeft, 0, Time.deltaTime);
                    }
                    else if (sliding)
                    {
                        rb.useGravity = true;
                        rb.velocity = (transform.forward * nma.speed * 1.5f).SetY(rb.velocity.y);
                        slideScrape.SetActive(gc.onGround);
                    }
                    else if (gc.onGround)
                    {
                        rb.useGravity = false;
                        rb.velocity = (transform.forward * nma.speed).SetY(rb.velocity.y);
                    }
                    else if (slamming)
                    {
                        rb.useGravity = false;
                        rb.velocity = new(0, -100, 0);
                    }
                    else
                    {
                        rb.useGravity = true;
                        rb.AddForce(transform.forward.SetY(0).normalized * airAcceleration * Time.deltaTime);
                    }
                }
            }
            else
            {
                rb.velocity = gc.onGround ? Vector3.zero : Vector3.zero.SetY(rb.velocity.y);
            }
        }

        private void LateUpdate()
        {
            if (Target != null)
            {
                Vector3 position;
                Vector3 gravity = Sqrt(Physics.gravity);
                Vector3 velocity = (currentWeapon.chargingAlt ? currentWeapon.useGravityAlt : currentWeapon.useGravity) ? -gravity : Vector3.zero;
                if (shootableInSightCooldown <= 0 && overrideTarget)
                {
                    position = overrideTarget.transform.position;
                    velocity += (overrideTarget.GetComponent<Rigidbody>()?.velocity ?? Vector3.zero) + gravity;
                }
                else
                {
                    position = (currentWeapon.chargingAlt ? currentWeapon.aimAtHeadAlt : currentWeapon.aimAtHead) ? Target.headPosition : Target.position;
                    velocity += (Target.rigidbody?.velocity ?? Vector3.zero) + (!Target.isOnGround && (!Target.enemyIdentifier || !Target.enemyIdentifier.flying) ? gravity : Vector3.zero);
                }

                aimAtTarget[0].LookAt(Target.headPosition);
                aimAtTarget[1].LookAt(position + velocity * (currentWeapon.chargingAlt ? currentWeapon.AltPredictAmount : currentWeapon.PredictAmount));
                aimAtTarget[1].Rotate(Vector3.right, 90, Space.Self);
                aimAtTarget[1].Rotate(Vector3.up, 180, Space.Self);
            }
        }

        public void Jump()
        {
            if (canJump && !jumping)
            {
                if (gc.onGround)
                {
                    jumping = true;
                    SetSlide(false);
                    Invoke("NotJumping", 0.25f);
                    anim.SetTrigger("Jump");
                    anim.SetLayerWeight(1, 1);
                    anim.SetLayerWeight(2, 0);
                    if (sliding)
                    {
                        Instantiate(jumpSound, transform.position, Quaternion.identity);
                        rb.AddForce(Vector3.up * jumpPower * 2);
                    }
                    else if (dodgeLeft > 0 && stamina >= 1)
                    {
                        Instantiate(dashJumpSound);
                        rb.AddForce(Vector3.up * jumpPower * 1.5f);
                        stamina--;
                    }
                    else
                    {
                        Instantiate(jumpSound, transform.position, Quaternion.identity);
                        rb.AddForce(Vector3.up * jumpPower * 2.5f);
                    }
                }
                else if (wc.onGround && wallJumps > 0)
                {
                    jumping = true;
                    SetSlide(false);
                    Invoke("NotJumping", 0.25f);
                    Instantiate(jumpSound, transform.position, Quaternion.identity).GetComponent<AudioSource>().pitch = 1.75f - 0.25f * wallJumps;
                    if (wallJumps == 1)
                    {
                        Instantiate(finalWallJumpSound, transform.position, Quaternion.identity);
                    }
                    CheckPattern();
                    rb.velocity = Vector3.zero;
                    rb.AddForce((transform.position - wc.ClosestPoint()).normalized.SetY(1) * wallJumpPower);
                    wallJumps--;
                }
            }
        }

        private void CheckPattern()
        {
            if (patternCooldown <= 0 && distancePatience < 4 && currentPattern != 3)
            {
                int num = Random.Range(0, 3);
                if (num == currentPattern)
                {
                    patternCooldown = Random.Range(0.5f, 1f);
                    if (currentPattern == 1)
                    {
                        circleTimer++;
                    }
                }
                else
                {
                    patternCooldown = Random.Range(2, 5);
                    SwitchPattern(num);
                }

                if (currentPattern == 1 || DistanceToTarget < 10)
                {
                    circleDirection = Random.Range(0, 1) > 0.5f ? -1 : 1;
                }
            }
        }

        private void ChangeDirection(Vector3 other)
        {
            Quaternion rotation = anim.transform.rotation;
            transform.LookAt(other.SetY(transform.position.y));
            anim.transform.rotation = rotation;
        }

        public void Slam()
        {
            if (canSlam && !slamming && !gc.onGround)
            {
                SetSlide(false);
                slamming = true;
                slamParticles.SetActive(true);
            }
        }

        public void Dodge(Vector3 direction, bool away)
        {
            if (canDash && stamina >= 1)
            {
                SetSlide(false);
                slamming = false;
                slamParticles.SetActive(false);
                stamina--;
                dodgeLeft = 0.25f;
                eid.Bless();
                Invoke("UnBless", dodgeLeft);
                Vector3 vector = away ? transform.position - direction : direction - transform.position;
                ChangeDirection(transform.position + vector);
                Instantiate(dodgeParticles, transform).SetActive(true);
                anim.SetTrigger("Jump");
            }
        }

        private void UnBless() => eid.Unbless();

        private void NotJumping() => jumping = false;

        private void SetSlide(bool state)
        {
            if (canSlide && sliding != state)
            {
                sliding = state;
                anim.SetBool("Sliding", state);
                slideParticles.SetActive(state);
                if (state)
                {
                    slideStopTimer = 0.2f;
                }
                else
                {
                    slideScrape.SetActive(false);
                    CheckPattern();
                }
            }
        }

        public void SwitchWeapon(WeaponInfo weapon)
        {
            if (currentWeapon != weapon)
            {
                currentWeapon = weapon;
                foreach (WeaponInfo wi in weapons)
                {
                    wi.SetActive(false);
                }
                weapon.SetActive(true);
            }
        }

        public void SwitchPattern(int pattern)
        {
            if (currentPattern != pattern)
            {
                prevPattern = currentPattern;
                currentPattern = pattern;
                if (currentWingChangeEffect != null)
                {
                    Destroy(currentWingChangeEffect);
                }
                EnemySimplifier[] array = ensims;
                foreach (EnemySimplifier enemySimplifier in array)
                {
                    if (enemySimplifier.matList != null && enemySimplifier.matList.Length > 1)
                    {
                        enemySimplifier.matList[1].mainTexture = wingTextures[pattern];
                    }
                }
                currentWingChangeEffect = Instantiate(wingChangeEffect, transform);
                currentWingChangeEffect.GetComponent<Light>().color = patternColors[pattern];
                switch (pattern)
                {
                    case 0:
                        currentWingChangeEffect.GetComponent<AudioSource>().pitch = 1.5f;
                        break;
                    case 1:
                        currentWingChangeEffect.GetComponent<AudioSource>().pitch = 1.25f;
                        break;
                }
            }
        }

        public void Heal(float hp)
        {
            if (mac.health < maxHP)
            {
                mac.health = Mathf.MoveTowards(mac.health, maxHP, hp / 10 / eid.totalHealthModifier * healMultiplier);
            }
        }
    }

    /*
    [HarmonyPatch(typeof(BulletCheck))]
    internal class BulletCheckPatch
    {
        [HarmonyPatch("OnTriggerEnter"), HarmonyPrefix]
        private static bool OnTriggerEnter(BulletCheck __instance, Collider other)
        {
            if (__instance.GetComponentInParent<V3>() is { Target: not null, eid.blessed: false } v3)
            {
                switch (other.gameObject.layer)
                {
                    case 10 or 12: // Enemies
                        if (other.TryGetComponent(out EnemyIdentifier eid) && eid != v3.eid || other.TryGetComponent(out EnemyIdentifierIdentifier eidid) && eidid.eid != v3.eid)
                        {
                            v3.Dodge(other.transform.position, true);
                        }
                        break;
                    case 14: // Projectiles
                        if (other.GetComponentInParent<PhysicalShockwave>() is PhysicalShockwave shockwave)
                        {
                            v3.Dodge(shockwave.transform.position, false);
                        }
                        else if ((!other.TryGetComponent(out Projectile projectile) || projectile.safeEnemyType != EnemyType.V2) && (!other.TryGetComponent(out Nail nail) || nail.safeEnemyType != EnemyType.V2))
                        {
                            v3.Dodge(v3.transform.position + (Vector3.SignedAngle(v3.transform.forward, other.transform.position, Vector3.up) >= 0 ? -v3.transform.right : v3.transform.right), false);
                        }
                        break;
                    case 23: // Explosions
                        if (other.TryGetComponent(out Explosion explosion) && !explosion.harmless)
                        {
                            v3.Dodge(other.transform.position, true);
                        }
                        break;
                }

                return false;
            }

            return true;
        }

        [HarmonyPatch("ForceDodge"), HarmonyPrefix]
        private static bool ForceDodge(BulletCheck __instance)
        {
            if (__instance.GetComponentInParent<V3>() is V3 v3 && v3.eid.blessed)
            {
                v3.Dodge(v3.transform.position + Random.insideUnitSphere, false);
                return true;
            }

            return false;
        }
    }

    [HarmonyPatch(typeof(Bloodsplatter))]
    internal class BloodsplatterPatch
    {
        [HarmonyPatch("Collide"), HarmonyPostfix]
        private static void Collide(Collider other, Bloodsplatter __instance, int ___eidID, bool ___canCollide)
        {
            if (__instance.ready && __instance.bsm && ___canCollide && other.TryGetComponent(out V3 v3) && ___eidID != v3.eid.GetInstanceID())
            {
                Traverse.Create(__instance).Method("DisableCollider").GetValue();
                v3.Heal(__instance.hpAmount);
            }
        }

        [HarmonyPatch("CreateBloodstain"), HarmonyPostfix]
        private static void CreateBloodstain(ref RaycastHit hit, Bloodsplatter __instance, int ___eidID)
        {
            if (__instance.ready && __instance.hpOnParticleCollision && hit.collider.TryGetComponent(out V3 v3) && ___eidID != v3.eid.GetInstanceID())
            {
                v3.Heal(3);
            }
        }
    }

    [HarmonyPatch(typeof(NewMovement))]
    internal class NewMovementPatch
    {
        [HarmonyPatch("GetHurt"), HarmonyPrefix]
        private static void GetHurt_Prefix(NewMovement __instance, out int __state) => __state = __instance.hp;

        [HarmonyPatch("GetHurt"), HarmonyPostfix]
        private static void GetHurt_Postfix(NewMovement __instance, int __state)
        {
            IEnumerable<V3> v3List = V3.v3List.Where(v3 => Vector3.Distance(v3.transform.position, __instance.transform.position) < v3.playerHealDist);
            if (__instance.hp < __state && v3List.Any())
            {
                v3List.First().Heal(__state - __instance.hp);
            }
        }
    }
    */
}