using HarmonyLib;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using static UnityEngine.Object;
using static V3Mod.Extensions;
using static UnityEngine.AddressableAssets.Addressables;

namespace V3Mod
{
    public static class V3Assets
    {
        public static readonly float maxHP = 75;

        public static GameObject BuildV3Object()
        {
            WeaponInfo weapon;
            AudioSource aud;
            Transform tf;
            Shader shad = LoadAssetAsync<Shader>("Assets/Shaders/Main/ULTRAKILL-unlit-emissive.shader").WaitForCompletion();

            // Create Prefab
            GameObject newPrefab = InstantiateInactive(LoadAssetAsync<GameObject>("Assets/Prefabs/Enemies/V2 Green Arm Variant.prefab").WaitForCompletion());
            Destroy(newPrefab.transform.GetChild(0).gameObject);

            V2 v2 = newPrefab.GetComponent<V2>();
            V3 v3 = newPrefab.AddComponent<V3>();
            v3.aimAtTarget = v2.aimAtTarget;
            v3.dashJumpSound = v2.dashJumpSound;

            v3.dodgeParticles = InstantiateInactive(v2.dodgeEffect, newPrefab.transform);
            ParticleSystem.MainModule main = v3.dodgeParticles.GetComponent<ParticleSystem>().main;
            main.playOnAwake = true;

            v3.gc = v2.gc;
            v3.jumpSound = v2.jumpSound;

            v3.finalWallJumpSound = InstantiateInactive(v2.jumpSound);
            v3.finalWallJumpSound.GetComponent<AudioSource>().clip = NewMovement.Instance.finalWallJump;

            v3.slamHitEffect = NewMovement.Instance.impactDust;

            v3.slamParticles = InstantiateInactive(NewMovement.Instance.fallParticle, newPrefab.transform);
            v3.slamParticles.transform.localScale = new(2, 1, 2);

            v3.slideParticles = InstantiateInactive(NewMovement.Instance.slideParticle, newPrefab.transform);
            v3.slideParticles.transform.localPosition = new(0, 1, 2);
            v3.slideParticles.transform.localScale = new(0.75f, 0.25f, 0.75f);

            v3.slideScrape = InstantiateInactive(v2.slideEffect, newPrefab.transform);

            v3.smr = v2.smr;

            v3.stepAud = v3.gameObject.AddComponent<AudioSource>();
            PrepareAudio(v3.stepAud);
            v3.stepAud.volume = 0.5f;
            v3.stepAud.playOnAwake = false;

            v3.stepSounds = NewMovement.Instance.anim.GetComponent<PlayerAnimations>().footsteps;

            v3.wc = v2.wc;

            WeaponInfo v2Revolver = v2.weapons[0].AddComponent<WeaponInfo>();
            WeaponInfo v2Shotgun = v2.weapons[1].AddComponent<WeaponInfo>();
            WeaponInfo v2Nailgun = v2.weapons[2].AddComponent<WeaponInfo>();

            v3.wingChangeEffect = v2.wingChangeEffect;
            v3.wingTextures = v2.wingTextures;
            Destroy(v2);

            EnemyIdentifier eid = newPrefab.GetComponent<EnemyIdentifier>();
            eid.weaknesses = ["revolver", "nail", "saw", "drill", "shotgun", "explosion"];
            eid.weaknessMultipliers = [1.5f, 1.5f, 1.25f, 1.25f, 0.8f, 0.5f];

            Machine mac = newPrefab.GetComponent<Machine>();
            mac.health = maxHP;
            mac.specialDeath = false;
            mac.simpleDeath = true;

            NavMeshAgent nma = newPrefab.GetComponent<NavMeshAgent>();
            nma.speed = 15;

            newPrefab.GetComponentsInChildren<TrailRenderer>().Do(Destroy);

            // Piercer Revolver
            v3.weapons[0] = Instantiate(v2Revolver.gameObject, v2Revolver.transform.parent).GetComponentInChildren<WeaponInfo>();
            weapon = v3.weapons[0];
            weapon.name = "Piercer Revolver";
            Revolver v1Piercer = GunSetter.Instance.revolverPierce[0].ToAsset().GetComponent<Revolver>();
            EnemyRevolver v3Piercer = weapon.EnemyWeapon as EnemyRevolver;
            v3Piercer.shootPoint.GetChild(0).GetChild(0).transform.localScale = Vector3.one * 0.8f;
            v3Piercer.GetComponent<MeshRenderer>().enabled = false;
            Transform v3PiercerObj = Instantiate(v1Piercer.transform.GetChild(0), v3Piercer.transform);
            v3PiercerObj.GetChild(0).gameObject.SetActive(false);
            v3PiercerObj.GetChild(1).gameObject.layer = LayerMask.GetMask("Default");
            v3PiercerObj.GetChild(2).gameObject.layer = LayerMask.GetMask("Default");
            v3PiercerObj.GetChild(3).gameObject.SetActive(false);
            v3PiercerObj.GetComponent<Animator>().enabled = false;
            v3PiercerObj.GetComponent<RevolverAnimationReceiver>().enabled = false;
            v3PiercerObj.GetComponentsInChildren<GunColorGetter>().Do(gcg => gcg.enabled = false);
            v3PiercerObj.GetComponentsInChildren<SkinnedMeshRenderer>().Do(smr => smr.material.shader = shad);
            v3PiercerObj.localPosition = new(6.3f, -2.8f, 0);
            v3PiercerObj.localRotation = Quaternion.Euler(340, 270, 0);
            v3PiercerObj.localScale = Vector3.one * 0.65f;
            v3Piercer.bullet = PrepareBeam(v1Piercer.revolverBeam, weapon.gameObject);
            v3Piercer.altBullet = PrepareBeam(v1Piercer.revolverBeamSuper, weapon.gameObject);
            v3Piercer.muzzleFlash = v3Piercer.bullet.transform.GetChild(0).gameObject;
            v3Piercer.muzzleFlash.transform.GetChild(0).gameObject.layer = LayerMask.GetMask("Default");
            aud = v3Piercer.muzzleFlash.AddComponent<AudioSource>();
            PrepareAudio(aud);
            aud.clip = v1Piercer.gunShots.First();
            aud.volume = 0.55f;
            Destroy(v3Piercer.shootPoint.GetChild(0).GetComponent<AudioLowPassFilter>());
            v3Piercer.muzzleFlashAlt = v3Piercer.altBullet.transform.GetChild(0).gameObject;
            v3Piercer.muzzleFlashAlt.transform.GetChild(0).gameObject.layer = LayerMask.GetMask("Default");
            aud = v3Piercer.muzzleFlashAlt.AddComponent<AudioSource>();
            PrepareAudio(aud);
            aud.clip = v1Piercer.superGunSound.clip;
            aud.volume = 0.5f;
            weapon.drawTime = 0.3642f;
            weapon.drawTimeAlt = 0.3642f;
            weapon.cooldown = 0.5f;
            weapon.cooldownAlt = 0.5f;
            weapon.prepareTimeAlt = 1 / 1.75f;
            weapon.maxCharges = -1;
            weapon.maxChargesAlt = 1;
            weapon.chargeIncrease = -1;
            weapon.chargeIncreaseAlt = 0.4f;
            weapon.normalBlocksAlt = true;
            weapon.altBlocksNormal = true;
            weapon.resetCooldownOnSwap = true;
            weapon.resetCooldownOnSwapAlt = true;
            weapon.projectileSpeed = -1;
            weapon.projectileSpeedAlt = -1;
            weapon.aimAtHead = true;
            weapon.aimAtHeadAlt = true;
            weapon.useGravity = false;
            weapon.useGravityAlt = false;

            // Marksman Revolver
            v3.weapons[1] = Instantiate(v3.weapons[0].gameObject, v3.weapons[0].transform.parent).GetComponentInChildren<WeaponInfo>();
            weapon = v3.weapons[1];
            weapon.name = "Marksman Revolver";
            Revolver v1Marksman = GunSetter.Instance.revolverRicochet[0].ToAsset().GetComponent<Revolver>();
            EnemyRevolver v3Marksman = weapon.EnemyWeapon as EnemyRevolver;
            v3Marksman.bullet = PrepareBeam(v1Marksman.revolverBeam, weapon.gameObject);
            v3Marksman.altBullet = v1Marksman.coin;
            v3Marksman.muzzleFlash = v3Marksman.bullet.transform.GetChild(0).gameObject;
            v3Marksman.muzzleFlash.transform.GetChild(0).gameObject.layer = LayerMask.GetMask("Default");
            aud = v3Marksman.muzzleFlash.AddComponent<AudioSource>();
            PrepareAudio(aud);
            aud.clip = v1Marksman.gunShots.First();
            aud.volume = 0.55f;
            v3Marksman.muzzleFlashAlt = null;
            weapon.drawTime = 0.3642f;
            weapon.drawTimeAlt = 0;
            weapon.cooldown = 0.5f;
            weapon.cooldownAlt = 1 / 4.8f;
            weapon.prepareTimeAlt = 0;
            weapon.maxCharges = -1;
            weapon.maxChargesAlt = 4;
            weapon.chargeIncrease = -1;
            weapon.chargeIncreaseAlt = 0.25f;
            weapon.normalBlocksAlt = false;
            weapon.altBlocksNormal = false;
            weapon.resetCooldownOnSwap = true;
            weapon.resetCooldownOnSwapAlt = true;
            weapon.projectileSpeed = -1;
            weapon.projectileSpeedAlt = 1;
            weapon.aimAtHead = true;
            weapon.aimAtHeadAlt = false;
            weapon.useGravity = false;
            weapon.useGravityAlt = true;

            // Sharpshooter Revolver
            v3.weapons[2] = Instantiate(v3.weapons[0].gameObject, v3.weapons[0].transform.parent).GetComponentInChildren<WeaponInfo>();
            weapon = v3.weapons[2];
            weapon.name = "Sharpshooter Revolver";
            Revolver v1Sharpshooter = GunSetter.Instance.revolverTwirl[0].ToAsset().GetComponent<Revolver>();
            EnemyRevolver v3Sharpshooter = weapon.EnemyWeapon as EnemyRevolver;
            v3Sharpshooter.bullet = PrepareBeam(v1Sharpshooter.revolverBeam, weapon.gameObject);
            v3Sharpshooter.altBullet = PrepareBeam(v1Sharpshooter.revolverBeamSuper, weapon.gameObject);
            v3Sharpshooter.altBullet.GetComponent<RevolverBeam>().ricochetAmount = 3;
            v3Sharpshooter.muzzleFlash = v3Sharpshooter.bullet.transform.GetChild(0).gameObject;
            v3Sharpshooter.muzzleFlash.transform.GetChild(0).gameObject.layer = LayerMask.GetMask("Default");
            aud = v3Sharpshooter.muzzleFlash.AddComponent<AudioSource>();
            PrepareAudio(aud);
            aud.clip = v1Sharpshooter.gunShots.First();
            aud.volume = 0.55f;
            Transform chargeEffect = v3Sharpshooter.shootPoint.GetChild(0);
            aud = chargeEffect.GetComponent<AudioSource>();
            aud.clip = v1Sharpshooter.chargeEffect.GetComponent<AudioSource>().clip;
            aud.volume = 0.5f;
            Destroy(chargeEffect.GetComponent<AudioLowPassFilter>());
            Destroy(chargeEffect.GetComponent<Light>());
            chargeEffect.GetChild(0).GetChild(0).localScale = Vector3.zero;
            v3Sharpshooter.muzzleFlashAlt = v3Sharpshooter.altBullet.transform.GetChild(0).gameObject;
            v3Sharpshooter.muzzleFlashAlt.transform.GetChild(0).gameObject.layer = LayerMask.GetMask("Default");
            aud = v3Sharpshooter.muzzleFlashAlt.AddComponent<AudioSource>();
            PrepareAudio(aud);
            aud.clip = v1Sharpshooter.superGunSound.clip;
            aud.volume = 0.5f;
            aud = v3Sharpshooter.muzzleFlashAlt.AddComponent<AudioSource>();
            PrepareAudio(aud);
            aud.clip = v1Sharpshooter.twirlShotSound.GetComponent<AudioSource>().clip;
            aud.volume = 0.75f;
            weapon.drawTime = 0.3642f;
            weapon.drawTimeAlt = 0.3642f;
            weapon.cooldown = 0.5f;
            weapon.cooldownAlt = 0;
            weapon.prepareTimeAlt = 1;
            weapon.maxCharges = -1;
            weapon.maxChargesAlt = 3;
            weapon.chargeIncrease = -1;
            weapon.chargeIncreaseAlt = 0.15f;
            weapon.normalBlocksAlt = true;
            weapon.altBlocksNormal = true;
            weapon.resetCooldownOnSwap = true;
            weapon.resetCooldownOnSwapAlt = true;
            weapon.projectileSpeed = -1;
            weapon.projectileSpeedAlt = -1;
            weapon.aimAtHead = true;
            weapon.aimAtHeadAlt = true;
            weapon.useGravity = false;
            weapon.useGravityAlt = false;

            // Core Eject Shotgun
            v3.weapons[3] = Instantiate(v2Shotgun.gameObject, v2Shotgun.transform.parent).GetComponentInChildren<WeaponInfo>();
            weapon = v3.weapons[3];
            weapon.name = "Core Eject Shotgun";
            Shotgun v1CoreEject = GunSetter.Instance.shotgunGrenade[0].ToAsset().GetComponent<Shotgun>();
            EnemyShotgun v3CoreEject = weapon.EnemyWeapon as EnemyShotgun;
            tf = Instantiate(v1CoreEject.transform.GetChild(2), v3CoreEject.transform);
            v3CoreEject.transform.GetChild(0).gameObject.SetActive(false);
            v3CoreEject.transform.GetChild(1).gameObject.SetActive(false);
            v3CoreEject.transform.GetChild(2).gameObject.SetActive(false);
            tf.GetComponent<Animator>().enabled = false;
            tf.GetComponent<ShotgunAnimationReceiver>().enabled = false;
            tf.GetChild(0).gameObject.SetActive(false);
            tf.GetChild(1).gameObject.SetActive(false);
            tf.GetChild(2).gameObject.SetActive(false);
            tf.GetChild(3).gameObject.SetActive(false);
            tf.GetChild(4).gameObject.layer = LayerMask.GetMask("Default");
            tf.GetChild(4).GetComponent<GunColorGetter>().enabled = false;
            tf.GetChild(4).GetComponent<SkinnedMeshRenderer>().material.shader = shad;
            tf.localScale = Vector3.one;
            tf.localRotation = Quaternion.Euler(90, 0, 0);
            tf.localPosition = new(0, -7.8f, 11);
            v3CoreEject.bullet = PrepareProjectile(v1CoreEject.bullet, weapon.gameObject);
            v3CoreEject.grenade = PrepareGrenade(v1CoreEject.grenade, weapon.gameObject);
            v3CoreEject.muzzleFlash = CreateClone(v1CoreEject.muzzleFlash, weapon.gameObject);
            v3CoreEject.muzzleFlash.transform.GetChild(0).GetChild(0).gameObject.layer = LayerMask.GetMask("Default");
            weapon.drawTime = 0.4496f;
            weapon.drawTimeAlt = 0.4496f;
            weapon.cooldown = 1.3315f;
            weapon.cooldownAlt = 3.0685f;
            weapon.prepareTimeAlt = 1;
            weapon.maxCharges = -1;
            weapon.maxChargesAlt = -1;
            weapon.chargeIncrease = -1;
            weapon.chargeIncreaseAlt = -1;
            weapon.normalBlocksAlt = true;
            weapon.altBlocksNormal = true;
            weapon.resetCooldownOnSwap = true;
            weapon.resetCooldownOnSwapAlt = true;
            weapon.projectileSpeed = 75;
            weapon.projectileSpeedAlt = 70;
            weapon.aimAtHead = false;
            weapon.aimAtHeadAlt = false;
            weapon.useGravity = false;
            weapon.useGravityAlt = true;

            // Pump Charge Shotgun
            v3.weapons[4] = Instantiate(v3.weapons[3].gameObject, v3.weapons[3].transform.parent).GetComponentInChildren<WeaponInfo>();
            weapon = v3.weapons[4];
            weapon.name = "Pump Charge Shotgun";
            Shotgun v1PumpCharge = GunSetter.Instance.shotgunPump[0].ToAsset().GetComponent<Shotgun>();
            EnemyShotgun v3PumpCharge = weapon.EnemyWeapon as EnemyShotgun;
            v3PumpCharge.bullet = PrepareProjectile(v1PumpCharge.bullet, weapon.gameObject);
            v3PumpCharge.grenade = null;
            v3PumpCharge.muzzleFlash = CreateClone(v1PumpCharge.muzzleFlash, weapon.gameObject);
            v3PumpCharge.muzzleFlash.transform.GetChild(0).GetChild(0).gameObject.layer = LayerMask.GetMask("Default");
            weapon.drawTime = 0.4496f;
            weapon.drawTimeAlt = 0.4496f;
            weapon.cooldown = 0.9899f;
            weapon.cooldownAlt = 0;
            weapon.prepareTimeAlt = 0.636f;
            weapon.maxCharges = -1;
            weapon.maxChargesAlt = -1;
            weapon.chargeIncrease = -1;
            weapon.chargeIncreaseAlt = -1;
            weapon.normalBlocksAlt = true;
            weapon.altBlocksNormal = true;
            weapon.resetCooldownOnSwap = true;
            weapon.resetCooldownOnSwapAlt = true;
            weapon.projectileSpeed = 75;
            weapon.projectileSpeedAlt = -1;
            weapon.aimAtHead = false;
            weapon.aimAtHeadAlt = false;
            weapon.useGravity = false;
            weapon.useGravityAlt = false;

            // Attractor Nailgun
            v3.weapons[5] = Instantiate(v2Nailgun.gameObject, v2Nailgun.transform.parent).GetComponentInChildren<WeaponInfo>();
            weapon = v3.weapons[5];
            weapon.name = "Attractor Nailgun";
            weapon.transform.localScale = Vector3.one;
            Nailgun v1Attractor = GunSetter.Instance.nailMagnet[0].ToAsset().GetComponent<Nailgun>();
            EnemyNailgun v3Attractor = weapon.EnemyWeapon as EnemyNailgun;
            v3Attractor.toIgnore = [];
            v3Attractor.shootPoint.localScale = Vector3.one * 0.4f;
            v3Attractor.transform.localScale = Vector3.one;
            tf = Instantiate(v1Attractor.transform.GetChild(0), v3Attractor.transform);
            v3Attractor.transform.GetChild(0).gameObject.SetActive(false);
            v3Attractor.transform.GetChild(1).gameObject.SetActive(false);
            tf.GetComponent<Animator>().enabled = false;
            tf.GetComponent<NailgunAnimationReceiver>().enabled = false;
            tf.GetChild(0).GetChild(0).GetChild(0).gameObject.layer = LayerMask.GetMask("Default");
            tf.GetChild(0).GetChild(0).GetChild(0).GetComponent<GunColorGetter>().enabled = false;
            tf.GetChild(0).GetChild(0).GetChild(0).GetComponent<MeshRenderer>().material.shader = shad;
            tf.GetChild(0).GetChild(0).GetChild(0).GetChild(0).gameObject.SetActive(false);
            tf.GetChild(0).GetChild(0).GetChild(1).gameObject.layer = LayerMask.GetMask("Default");
            tf.GetChild(0).GetChild(0).GetChild(1).GetComponent<GunColorGetter>().enabled = false;
            tf.GetChild(0).GetChild(0).GetChild(1).GetComponent<MeshRenderer>().material.shader = shad;
            tf.GetChild(0).GetChild(0).GetChild(1).GetChild(0).gameObject.SetActive(false);
            tf.GetChild(0).GetChild(0).GetChild(2).gameObject.SetActive(false);
            tf.GetChild(0).GetChild(0).GetChild(3).gameObject.SetActive(false);
            tf.GetChild(0).GetChild(0).GetChild(4).gameObject.SetActive(false);
            tf.GetChild(1).gameObject.layer = LayerMask.GetMask("Default");
            tf.GetChild(1).GetComponent<GunColorGetter>().enabled = false;
            tf.GetChild(1).GetComponent<SkinnedMeshRenderer>().material.shader = shad;
            tf.GetChild(2).gameObject.SetActive(false);
            tf.GetChild(3).gameObject.SetActive(false);
            tf.localScale = Vector3.one * 0.5f;
            tf.localPosition = new(0, 0.01f, 0.01f);
            v3Attractor.nail = PrepareNail(v1Attractor.nail, weapon.gameObject);
            v3Attractor.altNail = CreateClone(v1Attractor.magnetNail, weapon.gameObject);
            v3Attractor.muzzleFlash = CreateClone(v1Attractor.muzzleFlash, weapon.gameObject);
            v3Attractor.muzzleFlash.transform.GetChild(0).GetChild(0).gameObject.layer = LayerMask.GetMask("Default");
            aud = v3Attractor.muzzleFlash.GetComponent<AudioSource>();
            PrepareAudio(aud);
            weapon.drawTime = 0.4463f;
            weapon.drawTimeAlt = 0.4463f;
            weapon.cooldown = 0.04f;
            weapon.cooldownAlt = 0.5f;
            weapon.prepareTimeAlt = 0;
            weapon.maxCharges = 100;
            weapon.maxChargesAlt = 3;
            weapon.chargeIncrease = 5;
            weapon.chargeIncreaseAlt = 3;
            weapon.normalBlocksAlt = false;
            weapon.altBlocksNormal = true;
            weapon.resetCooldownOnSwap = true;
            weapon.resetCooldownOnSwapAlt = true;
            weapon.projectileSpeed = 200;
            weapon.projectileSpeedAlt = 100;
            weapon.aimAtHead = false;
            weapon.aimAtHeadAlt = false;
            weapon.useGravity = true;
            weapon.useGravityAlt = true;

            // Overheat Nailgun
            v3.weapons[6] = Instantiate(v3.weapons[5].gameObject, v3.weapons[5].transform.parent).GetComponentInChildren<WeaponInfo>();
            weapon = v3.weapons[6];
            weapon.name = "Overheat Nailgun";
            Nailgun v1Overheat = GunSetter.Instance.nailOverheat[0].ToAsset().GetComponent<Nailgun>();
            EnemyNailgun v3Overheat = weapon.EnemyWeapon as EnemyNailgun;
            v3Overheat.toIgnore = [];
            v3Overheat.shootPoint.localScale = Vector3.one * 0.4f;
            v3Overheat.nail = PrepareNail(v1Overheat.nail, weapon.gameObject);
            v3Overheat.altNail = PrepareNail(v1Overheat.heatedNail, weapon.gameObject);
            v3Overheat.muzzleFlash = CreateClone(v1Overheat.muzzleFlash, weapon.gameObject);
            v3Overheat.muzzleFlash.transform.GetChild(0).GetChild(0).gameObject.layer = LayerMask.GetMask("Default");
            aud = v3Overheat.muzzleFlash.GetComponent<AudioSource>();
            PrepareAudio(aud);
            weapon.drawTime = 0.4463f;
            weapon.drawTimeAlt = 0.4463f;
            weapon.cooldown = 0.04f;
            weapon.cooldownAlt = 0;
            weapon.prepareTimeAlt = 0;
            weapon.maxCharges = -1;
            weapon.maxChargesAlt = 2;
            weapon.chargeIncrease = -1;
            weapon.chargeIncreaseAlt = 0.125f;
            weapon.normalBlocksAlt = false;
            weapon.altBlocksNormal = false;
            weapon.resetCooldownOnSwap = false;
            weapon.resetCooldownOnSwapAlt = false;
            weapon.projectileSpeed = 200;
            weapon.projectileSpeedAlt = 200;
            weapon.aimAtHead = false;
            weapon.aimAtHeadAlt = false;
            weapon.useGravity = true;
            weapon.useGravityAlt = true;

            // Electric Railcannon
            v3.weapons[7] = Instantiate(v2Revolver.gameObject, v2Revolver.transform.parent).GetComponentInChildren<WeaponInfo>();
            weapon = v3.weapons[7];
            weapon.name = "Electric Railcannon";
            Railcannon v1Electric = GunSetter.Instance.railCannon[0].ToAsset().GetComponent<Railcannon>();
            EnemyRevolver v3Electric = weapon.EnemyWeapon as EnemyRevolver;
            v3Electric.GetComponent<MeshRenderer>().enabled = false;
            GameObject v3ElectricObj = Instantiate(LoadAssetAsync<GameObject>("Assets/Models/Weapons/Railcannon/Railgun.fbx").WaitForCompletion(), v3Electric.transform);
            v3ElectricObj.transform.GetChild(0).GetChild(0).gameObject.SetActive(false);
            Material v3ElectricMat = LoadAssetAsync<Material>("Assets/Models/Weapons/Railcannon/Railcannon.mat").WaitForCompletion();
            v3ElectricMat.shader = shad;
            v3ElectricObj.GetComponentsInChildren<SkinnedMeshRenderer>().Do(smr => smr.material = v3ElectricMat);
            v3ElectricObj.transform.localPosition = new(9, 9, 0f);
            v3ElectricObj.transform.localRotation = Quaternion.Euler(0, 180, 100);
            v3ElectricObj.transform.localScale = Vector3.one * 10;
            v3Electric.bullet = PrepareBeam(v1Electric.beam, weapon.gameObject);
            v3Electric.altBullet = null;
            v3Electric.muzzleFlash = v3Electric.bullet.transform.GetChild(0).gameObject;
            v3Electric.muzzleFlash.transform.GetChild(0).gameObject.layer = LayerMask.GetMask("Default");
            aud = v3Electric.muzzleFlash.AddComponent<AudioSource>();
            PrepareAudio(aud);
            aud.clip = v1Electric.fireSound.GetComponent<AudioSource>().clip;
            aud.volume = 0.65f;
            v3Electric.muzzleFlashAlt = null;
            aud = v3Electric.gameObject.AddComponent<AudioSource>();
            PrepareAudio(aud);
            aud.clip = v1Electric.fullCharge.GetComponent<AudioSource>().clip;
            aud.volume = 0.35f;
            aud.loop = true;
            weapon.drawTime = 1;
            weapon.drawTimeAlt = -1;
            weapon.cooldown = 0;
            weapon.cooldownAlt = -1;
            weapon.prepareTimeAlt = -1;
            weapon.maxCharges = -1;
            weapon.maxChargesAlt = -1;
            weapon.chargeIncrease = -1;
            weapon.chargeIncreaseAlt = -1;
            weapon.normalBlocksAlt = false;
            weapon.altBlocksNormal = false;
            weapon.resetCooldownOnSwap = false;
            weapon.resetCooldownOnSwapAlt = false;
            weapon.projectileSpeed = -1;
            weapon.projectileSpeedAlt = -1;
            weapon.aimAtHead = false;
            weapon.aimAtHeadAlt = false;
            weapon.useGravity = false;
            weapon.useGravityAlt = false;

            // Screwdriver Railcannon
            v3.weapons[8] = Instantiate(v3.weapons[7].gameObject, v3.weapons[7].transform.parent).GetComponentInChildren<WeaponInfo>();
            weapon = v3.weapons[8];
            weapon.name = "Screwdriver Railcannon";
            Railcannon v1Screwdriver = GunSetter.Instance.railHarpoon[0].ToAsset().GetComponent<Railcannon>();
            EnemyRevolver v3Screwdriver = weapon.EnemyWeapon as EnemyRevolver;
            v3Screwdriver.bullet = v1Screwdriver.beam;
            v3Screwdriver.altBullet = null;
            v3Screwdriver.muzzleFlash = v3Screwdriver.bullet.transform.GetChild(0).gameObject;
            aud = v3Screwdriver.muzzleFlash.AddComponent<AudioSource>();
            PrepareAudio(aud);
            aud.clip = v1Screwdriver.fireSound.GetComponent<AudioSource>().clip;
            aud.volume = 0.65f;
            v3Screwdriver.muzzleFlashAlt = null;
            aud = v3Screwdriver.gameObject.GetComponent<AudioSource>();
            aud.clip = v1Screwdriver.fullCharge.GetComponent<AudioSource>().clip;
            weapon.drawTime = 1;
            weapon.drawTimeAlt = -1;
            weapon.cooldown = 0;
            weapon.cooldownAlt = -1;
            weapon.prepareTimeAlt = -1;
            weapon.maxCharges = -1;
            weapon.maxChargesAlt = -1;
            weapon.chargeIncrease = -1;
            weapon.chargeIncreaseAlt = -1;
            weapon.normalBlocksAlt = false;
            weapon.altBlocksNormal = false;
            weapon.resetCooldownOnSwap = false;
            weapon.resetCooldownOnSwapAlt = false;
            weapon.projectileSpeed = 250;
            weapon.projectileSpeedAlt = -1;
            weapon.aimAtHead = false;
            weapon.aimAtHeadAlt = false;
            weapon.useGravity = true;
            weapon.useGravityAlt = false;

            // Malicious Railcannon
            v3.weapons[9] = Instantiate(v3.weapons[7].gameObject, v3.weapons[7].transform.parent).GetComponentInChildren<WeaponInfo>();
            weapon = v3.weapons[9];
            weapon.name = "Malicious Railcannon";
            Railcannon v1Malicious = GunSetter.Instance.railMalicious[0].ToAsset().GetComponent<Railcannon>();
            EnemyRevolver v3Malicious = weapon.EnemyWeapon as EnemyRevolver;
            v3Malicious.bullet = PrepareBeam(v1Malicious.beam, weapon.gameObject);
            v3Malicious.altBullet = null;
            v3Malicious.muzzleFlash = v3Malicious.bullet.transform.GetChild(0).gameObject;
            v3Malicious.muzzleFlash.transform.GetChild(0).gameObject.layer = LayerMask.GetMask("Default");
            aud = v3Malicious.muzzleFlash.AddComponent<AudioSource>();
            PrepareAudio(aud);
            aud.clip = v1Malicious.fireSound.GetComponent<AudioSource>().clip;
            aud.pitch = 3;
            v3Malicious.muzzleFlashAlt = null;
            aud = v3Malicious.gameObject.GetComponent<AudioSource>();
            aud.clip = v1Malicious.fullCharge.GetComponent<AudioSource>().clip;
            aud.pitch = 2;
            weapon.drawTime = 1;
            weapon.drawTimeAlt = -1;
            weapon.cooldown = 0;
            weapon.cooldownAlt = -1;
            weapon.prepareTimeAlt = -1;
            weapon.maxCharges = -1;
            weapon.maxChargesAlt = -1;
            weapon.chargeIncrease = -1;
            weapon.chargeIncreaseAlt = -1;
            weapon.normalBlocksAlt = false;
            weapon.altBlocksNormal = false;
            weapon.resetCooldownOnSwap = false;
            weapon.resetCooldownOnSwapAlt = false;
            weapon.projectileSpeed = -1;
            weapon.projectileSpeedAlt = -1;
            weapon.aimAtHead = false;
            weapon.aimAtHeadAlt = false;
            weapon.useGravity = false;
            weapon.useGravityAlt = false;

            // Freezeframe Rocket Launcher
            v3.weapons[10] = Instantiate(v2Revolver.gameObject, v2Revolver.transform.parent).GetComponentInChildren<WeaponInfo>();
            weapon = v3.weapons[10];
            weapon.name = "Freezeframe Rocket Launcher";
            RocketLauncher v1Freezeframe = GunSetter.Instance.rocketBlue[0].ToAsset().GetComponent<RocketLauncher>();
            EnemyRevolver v3Freezeframe = weapon.EnemyWeapon as EnemyRevolver;
            v3Freezeframe.GetComponent<MeshRenderer>().enabled = false;
            GameObject v3FreezeframeObj = Instantiate(LoadAssetAsync<GameObject>("Assets/Models/Weapons/RocketLauncher/RocketLauncher.fbx").WaitForCompletion(), v3Freezeframe.transform);
            v3FreezeframeObj.transform.GetChild(0).gameObject.SetActive(false);
            Material v3FreezeframeMat = LoadAssetAsync<Material>("Assets/Models/Weapons/RocketLauncher/RocketLauncher Unlit CustomColor.mat").WaitForCompletion();
            v3FreezeframeMat.shader = shad;
            v3FreezeframeObj.GetComponentsInChildren<SkinnedMeshRenderer>().Do(smr => smr.material = v3FreezeframeMat);
            v3FreezeframeObj.transform.localPosition = new(8.5f, -5.5f, 0);
            v3FreezeframeObj.transform.localRotation = Quaternion.Euler(0, 180, 125);
            v3FreezeframeObj.transform.localScale = Vector3.one * 10;
            v3Freezeframe.bullet = PrepareGrenade(v1Freezeframe.rocket, weapon.gameObject);
            v3Freezeframe.altBullet = null;
            v3Freezeframe.muzzleFlash = CreateClone(v1Freezeframe.muzzleFlash, weapon.gameObject);
            aud = v3Freezeframe.muzzleFlash.AddComponent<AudioSource>();
            PrepareAudio(aud);
            aud.clip = v1Freezeframe.GetComponent<AudioSource>().clip;
            aud.volume = 0.85f;
            v3Freezeframe.muzzleFlashAlt = null;
            weapon.drawTime = 0.25f;
            weapon.drawTimeAlt = 0;
            weapon.cooldown = 1;
            weapon.cooldownAlt = 0;
            weapon.prepareTimeAlt = 0;
            weapon.maxCharges = -1;
            weapon.maxChargesAlt = -1;
            weapon.chargeIncrease = -1;
            weapon.chargeIncreaseAlt = -1;
            weapon.normalBlocksAlt = false;
            weapon.altBlocksNormal = false;
            weapon.resetCooldownOnSwap = false;
            weapon.resetCooldownOnSwapAlt = false;
            weapon.projectileSpeed = 100;
            weapon.projectileSpeedAlt = -1;
            weapon.aimAtHead = false;
            weapon.aimAtHeadAlt = false;
            weapon.useGravity = false;
            weapon.useGravityAlt = false;

            // S.R.S. Cannon Rocket Launcher
            v3.weapons[11] = Instantiate(v3.weapons[10].gameObject, v3.weapons[10].transform.parent).GetComponentInChildren<WeaponInfo>();
            weapon = v3.weapons[11];
            weapon.name = "S.R.S. Cannon Rocket Launcher";
            RocketLauncher v1SRSCannon = GunSetter.Instance.rocketGreen[0].ToAsset().GetComponent<RocketLauncher>();
            EnemyRevolver v3SRSCannon = weapon.EnemyWeapon as EnemyRevolver;
            v3SRSCannon.bullet = PrepareGrenade(v1SRSCannon.rocket, weapon.gameObject);
            v3SRSCannon.altBullet = PrepareCannonball(v1SRSCannon.cannonBall.gameObject, weapon.gameObject);
            v3SRSCannon.muzzleFlash = CreateClone(v1SRSCannon.muzzleFlash, weapon.gameObject);
            aud = v3SRSCannon.muzzleFlash.AddComponent<AudioSource>();
            PrepareAudio(aud);
            aud.clip = v1SRSCannon.GetComponent<AudioSource>().clip;
            aud.volume = 0.85f;
            v3SRSCannon.muzzleFlashAlt = CreateClone(v1SRSCannon.muzzleFlash, weapon.gameObject);
            aud = v3SRSCannon.muzzleFlashAlt.AddComponent<AudioSource>();
            aud.clip = v1SRSCannon.GetComponent<AudioSource>().clip;
            aud.volume = 0.85f;
            aud.pitch = 0.7f;
            PrepareAudio(aud);
            weapon.drawTime = 0.25f;
            weapon.drawTimeAlt = 0.25f;
            weapon.cooldown = 1;
            weapon.cooldownAlt = 1;
            weapon.prepareTimeAlt = 1;
            weapon.maxCharges = -1;
            weapon.maxChargesAlt = 1;
            weapon.chargeIncrease = -1;
            weapon.chargeIncreaseAlt = 0.125f;
            weapon.normalBlocksAlt = true;
            weapon.altBlocksNormal = true;
            weapon.resetCooldownOnSwap = false;
            weapon.resetCooldownOnSwapAlt = false;
            weapon.projectileSpeed = 100;
            weapon.projectileSpeedAlt = 150;
            weapon.aimAtHead = false;
            weapon.aimAtHeadAlt = false;
            weapon.useGravity = false;
            weapon.useGravityAlt = true;

            v2Revolver.SetActive(false);
            v2Shotgun.SetActive(false);
            v2Nailgun.SetActive(false);
            
            static GameObject CreateClone(GameObject original, GameObject newParent)
            {
                GameObject cloneParent = InstantiateInactive(new GameObject(original.name + "(Parent)"), newParent.transform);
                return Instantiate(original, cloneParent.transform);
            }

            static GameObject PrepareBeam(GameObject beam, GameObject parent)
            {
                GameObject clone = CreateClone(beam, parent);
                RevolverBeam component = clone.GetComponent<RevolverBeam>();
                component.beamType = BeamType.Enemy;
                component.ignoreEnemyType = EnemyType.V2;
                component.noMuzzleflash = true;
                component.sourceWeapon = parent;
                return clone;
            }

            static GameObject PrepareProjectile(GameObject proj, GameObject parent)
            {
                GameObject clone = CreateClone(proj, parent);
                Projectile component = clone.GetComponent<Projectile>();
                component.damage *= 25;
                component.friendly = false;
                component.playerBullet = false;
                component.safeEnemyType = EnemyType.V2;
                component.sourceWeapon = parent;
                return clone;
            }

            static GameObject PrepareGrenade(GameObject grenade, GameObject parent)
            {
                GameObject clone = CreateClone(grenade, parent);
                Grenade component = clone.GetComponent<Grenade>();
                component.enemy = true;
                component.sourceWeapon = parent;
                return clone;
            }

            static GameObject PrepareNail(GameObject nail, GameObject parent)
            {
                GameObject clone = CreateClone(nail, parent);
                Nail component = clone.GetComponent<Nail>();
                component.enemy = true;
                component.safeEnemyType = EnemyType.V2;
                component.sourceWeapon = parent;
                return clone;
            }

            static GameObject PrepareCannonball(GameObject cb, GameObject parent)
            {
                GameObject clone = CreateClone(cb, parent);
                Cannonball component = clone.GetComponent<Cannonball>();
                component.sourceWeapon = parent;
                return clone;
            }

            static void PrepareAudio(AudioSource aud)
            {
                aud.rolloffMode = AudioRolloffMode.Linear;
                aud.spatialBlend = 1;
                aud.velocityUpdateMode = AudioVelocityUpdateMode.Fixed;
            }

            newPrefab.name = "V3-B";
            newPrefab.hideFlags = HideFlags.HideAndDontSave;
            DontDestroyOnLoad(newPrefab);
            return newPrefab;
        }

        public static SpawnableObject BuildV3Spawnable()
        {
            SpawnableObject spawnable = LoadAssetAsync<SpawnableObject>("V3Spawnable.asset").WaitForCompletion();
            spawnable.gameObject = BuildV3Object();
            return spawnable;
        }
    }

    [HarmonyPatch]
    internal static class InjectMenus
    {
        private static bool spawnerInjected, shopInjected;
        private static readonly SpawnableObject spawnable = LoadAssetAsync<SpawnableObject>("V3Spawnable.asset").WaitForCompletion(); //V3Assets.BuildV3Spawnable();

        [HarmonyPatch(typeof(SpawnMenu), "Awake"), HarmonyPrefix]
        private static void InjectSpawnerArm(ref SpawnableObjectsDatabase ___objects)
        {
            if (spawnerInjected) return;
            AddToArray(ref ___objects.enemies, spawnable, 13);
            spawnerInjected = true;
        }

        [HarmonyPatch(typeof(EnemyInfoPage), "Start"), HarmonyPrefix]
        private static void InjectShop(ref SpawnableObjectsDatabase ___objects)
        {
            if (shopInjected) return;
            AddToArray(ref ___objects.enemies, spawnable, 18);
            shopInjected = true;
        }
    }
}