using UnityEngine;

namespace Enemies
{

/// <summary>
/// Lo que necesitan los estados de ataque de Ira (puño, bolas de fuego...).
/// Lo implementan BossAngry y BossFinal, así los mismos estados sirven para los dos.
/// </summary>
public interface IAngryBoss
{
    float DistanceToFloor { get; }
    float FireballSpeed { get; }
    float PunchYSpeed { get; }
    float PunchXSpeed { get; }
    int FireballCount { get; }
    float FireballInterval { get; }
    float FireballMinAngle { get; }
    float FireballMaxAngle { get; }
    Vector2 FireballSpawnOffset { get; }
    GameObject FireballPrefab { get; }
    GameObject PunchPrefab { get; }
}

/// <summary>
/// Lo que necesitan los estados de ataque de Tristeza (carámbanos, radial, bola de agua).
/// Lo implementan BossSad y BossFinal.
/// </summary>
public interface ISadBoss
{
    float DistanceToFloor { get; }
    float IcicleSpeed { get; }
    float WaterBallSpeed { get; }
    float RadialSpeed { get; }
    GameObject IciclePrefab { get; }
    GameObject WaterBallPrefab { get; }
}

/// <summary>
/// Lo que necesitan los estados de ataque de Miedo (X de garras y tazas).
/// Lo implementan BossFear y BossFinal.
/// </summary>
public interface IFearBoss
{
    float CupSpeed { get; }
    int CupCount { get; }
    float CupInterval { get; }
    GameObject CupPrefab { get; }
    GameObject ClawPrefab { get; }
    float ClawOffsetX { get; }
}
}
