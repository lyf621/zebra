using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class StatManager : MonoBehaviour
{
    [SerializeField] private int Balance = 5;
    [SerializeField] private int MaxStat = 10;
    [Header("Resources")]
    [SerializeField] private int GoldCoin;
    [SerializeField] private int PublicOpinion, MilitaryStrength, AuthorityLevel;
    [Header("Reputation")]
    [SerializeField] private int TheKing;
    [SerializeField] private int TheChurch, TheAristocrats;
    [Header("TemporaryTurnResource")]
    [SerializeField] private int Majesty = 0;
    [SerializeField] private int Fight = 0;
    /*[Header("TempUI")]
    [SerializeField] private TMP_Text GC;
    [SerializeField] private TMP_Text PO, KR, CR, AR, MS, AL;*/
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        /*GC.text = "Gold Coin: " + GoldCoin;
        PO.text = "PO: " + PublicOpinion;
        MS.text = "MS: " + MilitaryStrength;
        AL.text = "AL: " + AuthorityLevel;

        KR.text = "TheKing: " + TheKing;
        CR.text = "TheChurch: " + TheChurch;
        AR.text = "TheAristocrats: " + TheAristocrats;*/
    }

    public void UpdateGold(int gold)
    {
        GoldCoin += gold;
        if(GoldCoin < 0) GoldCoin = 0;
    }
    public void UpdateResource(int po, int ms, int al)
    {
        PublicOpinion += po;
        if(PublicOpinion > MaxStat) PublicOpinion = MaxStat;
        if(PublicOpinion < 0) PublicOpinion = 0;
        MilitaryStrength += ms;
        if(MilitaryStrength > MaxStat) MilitaryStrength = MaxStat;
        if(MilitaryStrength < 0) MilitaryStrength = 0;
        AuthorityLevel += al;
        if(AuthorityLevel > MaxStat) AuthorityLevel = MaxStat;
        if(AuthorityLevel < 0) AuthorityLevel = 0;
    }
    public void UpdateReputation(int kr, int cr, int ar)
    {
        TheKing += kr;
        if(TheKing > MaxStat) TheKing = MaxStat;
        if(TheKing < 0) TheKing = 0;
        TheChurch += cr;
        if(TheChurch > MaxStat) TheChurch = MaxStat;
        if(TheChurch < 0) TheChurch = 0;
        TheAristocrats += ar;
        if(TheAristocrats > MaxStat) TheAristocrats = MaxStat;
        if(TheAristocrats < 0) TheAristocrats = 0;
    }

    public int GetGold() {return GoldCoin;}
    public int GetPO() {return PublicOpinion;}
    public int GetMS() {return MilitaryStrength;}
    public int GetAL() {return AuthorityLevel;}

    public int GetKR() {return TheKing;}
    public int GetCR() {return TheChurch;}
    public int GetAR() {return TheAristocrats;}

    public void ReturnToBalance() 
    {
        if(PublicOpinion > Balance) PublicOpinion --;
        if(PublicOpinion < Balance) PublicOpinion ++;
        if(MilitaryStrength > Balance) MilitaryStrength --;
        if(MilitaryStrength <Balance) MilitaryStrength ++;
        if(AuthorityLevel > Balance) AuthorityLevel --;
        if(AuthorityLevel < Balance) AuthorityLevel ++;

        if(TheKing > Balance) TheKing --;
        if(TheKing < Balance) TheKing ++;
        if(TheChurch > Balance) TheChurch --;
        if(TheChurch < Balance) TheChurch ++;
        if(TheAristocrats > Balance) TheAristocrats --;
        if(TheAristocrats < Balance) TheAristocrats ++;
    }

    public int KingRel() 
    {
        if(TheKing < 2) return 1;
        if(TheKing < 4) return 2;
        if(TheKing < 7) return 3;
        if(TheKing < 9) return 4;
        return 5;
    }
    public int ChurchRel() 
    {
        if(TheChurch < 2) return 1;
        if(TheChurch < 4) return 2;
        if(TheChurch < 7) return 3;
        if(TheChurch < 9) return 4;
        return 5;
    }
    public int AristocratRel() 
    {
        if(TheAristocrats < 2) return 1;
        if(TheAristocrats < 4) return 2;
        if(TheAristocrats < 7) return 3;
        if(TheAristocrats < 9) return 4;
        return 5;
    }

    public void TempClear()
    {
        UpdateGold(-100);
        UpdateReputation(-10, -10, -10);
        UpdateResource(-10, -10, -10);
    }

    public int GetMajesty() { return Majesty; }
    public int GetFight() { return Fight; }

    public void UpdateMajesty(int x) { Majesty += x; }
    public void UpdateFight(int x) { Fight += x; }
    public void ClearTurnStat() { Majesty = 0; Fight = 0; }
}
