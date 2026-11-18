using System;
using System.Collections.Generic;
using UnityEngine;
using player;
using Orbs;

namespace Managers
{

/// <summary>
/// Datos de la partida: jefes derrotados, botones, orbes y último checkpoint.
/// Se guardan en disco (PlayerPrefs, como JSON) cada vez que se activa un checkpoint
/// o se cambian los orbes equipados en uno.
/// </summary>
public static class PlayerDataManager
{
    [Serializable]
    public class ButtonStatusData
    {
        public bool red;
        public bool blue;
        public bool green;
    }

    [Serializable]
    public class GameData
    {
        public ButtonStatusData buttonStatus = new ButtonStatusData();
        public List<string> defeatedBosses = new List<string>();

        // orbes
        public List<string> collectedOrbs = new List<string>();     // ids de los orbes recogidos
        public string[] equippedOrbs = new string[2] { "", "" };    // ids de los 2 equipados ("" = vacío)
        public int activeOrbIndex = 0;

        // último checkpoint
        public bool hasRespawn = false;
        public float respawnX;
        public float respawnY;
    }

    private const string SaveKey = "HellFrontiers.Save";

    public static GameData data = new GameData();

    // ---------------- Jefes ----------------

    /// <summary>
    /// marca un jefe como derrotado
    /// </summary>
    public static void KillBoss(string bossName)
    {
        if (string.IsNullOrEmpty(bossName)) return;
        if (!data.defeatedBosses.Contains(bossName)) data.defeatedBosses.Add(bossName);
    }

    /// <summary>
    /// comprueba si un jefe ya fue derrotado
    /// </summary>
    public static bool IsBossDefeated(string bossName)
    {
        return !string.IsNullOrEmpty(bossName) && data.defeatedBosses.Contains(bossName);
    }

    // ---------------- Reset ----------------

    /// <summary>
    /// borra el progreso en memoria (nueva partida / reiniciar). NO borra la partida guardada en disco:
    /// esa se sobrescribe al activar el primer checkpoint de la nueva partida.
    /// </summary>
    public static void ResetData()
    {
        data = new GameData();
    }

    // ---------------- Guardado en disco ----------------

    /// <summary>
    /// ¿hay una partida guardada con la que continuar?
    /// </summary>
    public static bool HasSave
    {
        get
        {
            if (!PlayerPrefs.HasKey(SaveKey)) return false;
            try { return JsonUtility.FromJson<GameData>(PlayerPrefs.GetString(SaveKey)).hasRespawn; }
            catch { return false; }
        }
    }

    public static void Save()
    {
        PlayerPrefs.SetString(SaveKey, JsonUtility.ToJson(data));
        PlayerPrefs.Save();
    }

    /// <summary>
    /// carga la partida guardada en memoria. Devuelve false si no hay.
    /// </summary>
    public static bool Load()
    {
        if (!PlayerPrefs.HasKey(SaveKey)) return false;
        try
        {
            var loaded = JsonUtility.FromJson<GameData>(PlayerPrefs.GetString(SaveKey));
            if (loaded == null) return false;
            if (loaded.equippedOrbs == null || loaded.equippedOrbs.Length < 2) loaded.equippedOrbs = new string[2] { "", "" };
            data = loaded;
            return true;
        }
        catch (Exception e)
        {
            Debug.LogWarning("[PlayerDataManager] Guardado corrupto: " + e.Message);
            return false;
        }
    }

    public static void DeleteSave()
    {
        PlayerPrefs.DeleteKey(SaveKey);
        PlayerPrefs.Save();
    }

    // ---------------- Checkpoint ----------------

    /// <summary>
    /// guarda posición de reaparición + orbes del jugador + jefes + botones
    /// </summary>
    public static void SaveCheckpoint(Player p, Vector2 respawn)
    {
        data.hasRespawn = true;
        data.respawnX = respawn.x;
        data.respawnY = respawn.y;
        SaveOrbs(p);
    }

    /// <summary>
    /// guarda los orbes (recogidos y equipados) y escribe a disco
    /// </summary>
    public static void SaveOrbs(Player p)
    {
        if (p != null)
        {
            data.collectedOrbs.Clear();
            foreach (var orb in p.orbs)
                if (orb != null) data.collectedOrbs.Add(orb.OrbId);

            for (int i = 0; i < 2; i++)
                data.equippedOrbs[i] = (p.equippedOrbs[i] != null) ? p.equippedOrbs[i].OrbId : "";

            data.activeOrbIndex = p.activeOrbIndex;
        }
        Save();
    }

    public static bool IsCurrentRespawn(Vector2 position, float tolerance = 0.1f)
    {
        return data.hasRespawn && Vector2.Distance(new Vector2(data.respawnX, data.respawnY), position) < tolerance;
    }

    /// <summary>
    /// aplica la partida cargada al jugador: posición, orbes recogidos y equipados, vida
    /// </summary>
    public static void RestorePlayer(Player p)
    {
        if (p == null) return;

        if (data.hasRespawn)
        {
            p.transform.position = new Vector3(data.respawnX, data.respawnY, p.transform.position.z);
            p.SetVelocity(0, 0);
        }

        p.health = p.maxHealth;

        // los orbes se buscan en la escena por id (los orbes ya recogidos se esconden)
        BaseOrb[] all = UnityEngine.Object.FindObjectsByType<BaseOrb>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        BaseOrb Find(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            foreach (var o in all) if (o.OrbId == id) return o;
            return null;
        }

        foreach (string id in data.collectedOrbs)
        {
            BaseOrb orb = Find(id);
            if (orb == null) { Debug.LogWarning("[PlayerDataManager] No encuentro el orbe guardado: " + id); continue; }
            orb.MarkCollected();
            if (!p.orbs.Contains(orb)) p.orbs.Add(orb);
        }

        p.SetEquippedOrbs(Find(data.equippedOrbs[0]), Find(data.equippedOrbs[1]), data.activeOrbIndex);
    }
}
}
