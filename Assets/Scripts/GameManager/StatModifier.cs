using System;

[Serializable]
public struct StatModifier
{
    public int gold;
    public int po;
    public int ms;
    public int al;
    public int kr;
    public int cr;
    public int ar;

    public void ApplyTo(StatManager stats)
    {
        stats.UpdateGold(gold);
        stats.UpdateResource(po, ms, al);
        stats.UpdateReputation(kr, cr, ar);
    }
}