namespace CS2Cheat.Utils;

public abstract class Offsets
{
    #region offsets

    public const float WeaponRecoilScale = 2f;
    public const nint m_nCurrentTickThisFrame = 0x34;

    // Offsets from offsets.json (client.dll)
    public static int dwLocalPlayerPawn = 39233864;
    public static int dwEntityList = 41024440;
    public static int dwViewMatrix = 39256656;
    public static int dwViewAngles = 39325848;
    public static int dwLocalPlayerController = 39069000;
    public static int dwPlantedC4 = 38603632;
    public static int dwGlobalVars = 35868936;

    // Offsets from client_dll.json

    // C_BaseEntity
    public static int m_fFlags = 1012;
    public static int m_lifeState = 852;
    public static int m_iHealth = 844;
    public static int m_iTeamNum = 999;
    public static int m_pGameSceneNode = 816;
    public static int m_vecAbsVelocity = 1016;

    // C_BasePlayerPawn
    public static int m_vOldOrigin = 5284;

    // C_BaseModelEntity
    public static int m_vecViewOffset = 3936;

    // C_CSPlayerPawn
    public static int m_AimPunchAngle = 0;
    public static int m_AimPunchCache = 0;
    public static int m_pAimPunchServices = 5528;
    public static int m_iIDEntIndex = 14044;
    public static int m_iShotsFired = 7864;
    public static int m_entitySpottedState = 7816;

    // CPlayer_CameraServices
    public static int m_vecCsViewPunchAngle = 72;

    // CSkeletonInstance
    public static int m_modelState = 320;

    // CGameSceneNode
    public static int m_bDormant = 259;

    // CBasePlayerController
    public static int m_hPawn = 1724;

    // C_AttributeContainer
    public static int m_Item = 80;

    // C_CSPlayerPawnBase
    public static int m_pClippingWeapon = 0;
    public static int m_bIsScoped = 0;
    public static int m_flFlashDuration = 5392;

    // C_EconEntity
    public static int m_AttributeManager = 4752;

    // C_EconItemView
    public static int m_iItemDefinitionIndex = 442;

    // CBasePlayerController
    public static int m_iszPlayerName = 1788;

    // C_PlantedC4
    public static int m_nBombSite = 4748;
    public static int m_bBombDefused = 4828;
    public static int m_flDefuseCountDown = 4824;
    public static int m_flC4Blow = 4792;
    public static int m_bBeingDefused = 4804;

    public static readonly Dictionary<string, int> Bones = new()
    {
        { "head", 7 },
        { "neck_0", 6 },
        { "spine_1", 8 },
        { "spine_2", 3 },
        { "pelvis", 1 },
        { "arm_upper_L", 9 },
        { "arm_lower_L", 10 },
        { "hand_L", 11 },
        { "arm_upper_R", 13 },
        { "arm_lower_R", 14 },
        { "hand_R", 15 },
        { "leg_upper_L", 17 },
        { "leg_lower_L", 18 },
        { "ankle_L", 19 },
        { "leg_upper_R", 20 },
        { "leg_lower_R", 21 },
        { "ankle_R", 22 }
    };

    #endregion
}