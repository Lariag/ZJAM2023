using System;
using System.Collections;
using System.Collections.Generic;

using TMPro;

using UnityEngine;
using UnityEngine.SceneManagement;

public class GrandmaNeeds : MonoBehaviour
{

    public float AngrynessTotal { get { return Angryness; } }
    public float Angryness = 0.0f;
    public float MaxAngryness = 100.0f;

    public float StatHunger;
    public float StatFun;
    public float StatTemp;

    public float current_cooldown;
    public float cooldown = 3.0f;
    public GameObject TextBubble;
    public GameObject GeneratedTextBubble;

    public int MaxConcurrentRequests; // Maximo 3, uno por cada tipo de stat.
    public float MinTimeBetweenRequests;
    public float MaxTimeBetweenRequests;
    float TimeNextRequest;

    public EnumObjectTypes[] ObjectsForHunger;
    public EnumObjectTypes[] ObjectsForFun;
    public EnumObjectTypes[] ObjectsForTemp;

    public string[] TextsForHunger;
    [HideInInspector]
    public string[] TextsForHungerReminder;
    public string[] TextsForFun;
    [HideInInspector]
    public string[] TextsForFunReminder;
    public string[] TextsForTemp;
    [HideInInspector]
    public string[] TextsForTempReminder;
    public FontStyles textStyle;

    public Sprite Food;
    public Sprite Hot;
    public Sprite Cold;
    public Sprite Fun;
    public Sprite Light;

    public Sprite Fan_icon;
    public Sprite Jukebox_icon;

    public Sprite Lamp_icon;
    public Sprite Oven_icon;
    public Sprite Tea_icon;
    public Sprite TV_icon;

    public Sprite Vitro_icon;

    public Sprite Window_icon;


    Dictionary<EnumObjectTypes, bool> ObjectStatus = new Dictionary<EnumObjectTypes, bool>();


    List<Request> CurrentRequests = new List<Request>();

    EnumStatType[] AllStatTypes;
    class Request
    {
        public Request(EnumStatType StatType, EnumObjectTypes ObjectType)
        {
            this.StatType = StatType;
            this.ObjectType = ObjectType;
            TimeRequested = Time.time;
            RemindersGiven = 0;
        }
        public EnumStatType StatType;
        public EnumObjectTypes ObjectType;
        public float TimeRequested;
        public int RemindersGiven;
    }

    private void Awake()
    {
        AllStatTypes = (EnumStatType[])Enum.GetValues(typeof(EnumStatType));

        EnumObjectTypes[] allObjectTypes = (EnumObjectTypes[])Enum.GetValues(typeof(EnumObjectTypes));
        for (int i = 0; i < allObjectTypes.Length; i++)
            ObjectStatus.Add(allObjectTypes[i], false);
    }
    void Start()
    {
        GameEvents.Ins.OnObjectSwitched += OnObjectSwitched;
        GameEvents.Ins.OnEventHappened += OnEventHappened;
        current_cooldown = UnityEngine.Random.Range(0.0f, 200.0f) / 100;

        TextsForHungerReminder = TextsForHunger;
        TextsForFunReminder = TextsForFun;
        TextsForTempReminder = TextsForTemp;

        TimeNextRequest = Time.time + 1;
    }

    // Update is called once per frame
    void Update()
    {
        if (TimeNextRequest < Time.time)
        {
            if (!TryAddRequest()) RemindCurrentRequest();
            TimeNextRequest = Time.time + Mathf.Lerp(MinTimeBetweenRequests, MaxTimeBetweenRequests, UnityEngine.Random.Range(0f, 1f));
        }
        AddAngrynessFromRequests();
    }

    void AddAngrynessFromRequests()
    {
        int accumulatedAttempts = 0;
        for (int i = 0; i < CurrentRequests.Count; i++)
        {
            accumulatedAttempts += CurrentRequests[i].RemindersGiven + 1;
        }
        Angryness += accumulatedAttempts * Time.deltaTime;

        if (GameEvents.Ins.OnScoreChanged != null) GameEvents.Ins.OnScoreChanged(EnumScoreType.GrannyAnger, this.AngrynessTotal);
    }

    private void SetNeedSprite(Request req)
    {
        BubbleText bubble = GeneratedTextBubble.GetComponent<BubbleText>();
        Sprite selectedSprite = null;

        if (UnityEngine.Random.Range(0.0f, 100.0f) > 20.0f)
        {
            if (req.StatType == EnumStatType.Hunger)
            {
                selectedSprite = Food;
            }
            else if (req.StatType == EnumStatType.Entretainment)
            {
                selectedSprite = Fun;
            }
            else if (req.StatType == EnumStatType.Temperature)
            {
                selectedSprite = Cold;
            }
        }
        else
        {
            switch (req.ObjectType)
            {
                case EnumObjectTypes.Fan:
                    selectedSprite = Fan_icon;
                    break;
                case EnumObjectTypes.Jukebox:
                    selectedSprite = Jukebox_icon;
                    break;
                case EnumObjectTypes.Oven:
                    selectedSprite = Oven_icon;
                    break;
                case EnumObjectTypes.Tea:
                    selectedSprite = Tea_icon;
                    break;
                case EnumObjectTypes.Vitro:
                    selectedSprite = Vitro_icon;
                    break;
                case EnumObjectTypes.TV:
                    selectedSprite = TV_icon;
                    break;
                case EnumObjectTypes.Lamp:
                    selectedSprite = Lamp_icon;
                    break;
                case EnumObjectTypes.Window:
                    selectedSprite = Window_icon;
                    break;
                case EnumObjectTypes.AC:
                    selectedSprite = Hot;
                    break;
            }
        }


        bubble.SetSprite(selectedSprite, req.RemindersGiven);
    }

    private void SetDialogText(string newText)
    {
        if (GameEvents.Ins.OnNewTextForMainDialog != null) GameEvents.Ins.OnNewTextForMainDialog(string.Empty, newText, textStyle);
    }

    // Intenta a�adir una nueva peticion.
    bool TryAddRequest()
    {
        if (CurrentRequests.Count >= MaxConcurrentRequests) return false; // Por ahora solo permitimos uno de cada a la vez.

        // Comprueba que stattypes no han sido ya pedidos.
        List<EnumStatType> statTypes = new List<EnumStatType>(AllStatTypes);
        for (int i = 0; i < CurrentRequests.Count; i++)
        {
            statTypes.Remove(CurrentRequests[i].StatType);
        }

        // Agrega aleatoriamente un statype que no haya pedido ya.
        if (statTypes.Count > 0)
        {
            // Elige el tipo de stat que queire satisfacer.
            EnumStatType newStatType = statTypes[UnityEngine.Random.Range(0, statTypes.Count)];

            // Elige el objeto que necesitas usar segun el tipo de stat.
            EnumObjectTypes[] objectTypeSelected = { };
            switch (newStatType)
            {
                case EnumStatType.Entretainment:
                    objectTypeSelected = ObjectsForFun; break;
                case EnumStatType.Hunger:
                    objectTypeSelected = ObjectsForHunger; break;
                case EnumStatType.Temperature:
                    objectTypeSelected = ObjectsForTemp; break;
            }

            // Crea nuevo request eligiendo aleatoriamente el objeto que lo satisfacira (provisional).
            Request newReq = new Request(newStatType, objectTypeSelected[UnityEngine.Random.Range(0, objectTypeSelected.Length)]);
            CurrentRequests.Add(newReq);

            // Bubble & Dialog
            {
                GenerateBubble(newReq);
                SetDialogText(GetTextForRequest(newReq.StatType, false));
            }

            SayText($"Abuelita: Quiero que uses {newReq.ObjectType.ToString()}, apresurate");
            return true;
        }
        return false;
    }

    // Te recuerda una peticion ya realizada, elevando el nivel de impaciencia.
    void RemindCurrentRequest()
    {
        if (CurrentRequests.Count > 0)
        {
            Request toRemind = CurrentRequests[UnityEngine.Random.Range(0, CurrentRequests.Count)];
            toRemind.RemindersGiven++;
            GenerateBubble(toRemind);
            SayText($"Abuelita: Recuerda usar el {toRemind.ObjectType.ToString()}, ya te lo he dicho {toRemind.RemindersGiven} veces");
            SetDialogText(GetTextForRequest(toRemind.StatType, true));
        }
    }

    void GenerateBubble(Request req)
    {
        if (GeneratedTextBubble != null) Destroy(GeneratedTextBubble);
        GeneratedTextBubble = Instantiate(TextBubble, new Vector3(transform.position.x + 1.5f, transform.position.y + 0.75f, -9.0f), Quaternion.identity);
        SetNeedSprite(req);
    }

    string GetTextForRequest(EnumStatType stat, bool isReminder)
    {
        string[] possibleTexts = null;
        switch (stat)
        {
            case EnumStatType.Entretainment:
                possibleTexts = isReminder ? TextsForFunReminder : TextsForFun; break;
            case EnumStatType.Hunger:
                possibleTexts = isReminder ? TextsForHungerReminder : TextsForHunger; break;
            case EnumStatType.Temperature:
                possibleTexts = isReminder ? TextsForTempReminder : TextsForTemp; break;
        }

        if (possibleTexts == null) return string.Empty;
        return possibleTexts[UnityEngine.Random.Range(0, possibleTexts.Length)];
    }


    void SayText(string text = default(string))
    {
        // TODO: Show text in the bubble.
        Debug.Log(text);
    }


    #region Recepci�n de Eventos

    void OnObjectSwitched(bool status, EnumObjectTypes objectType, int times)
    {
        // TODO: Updatear el valor de ObjectStatus
        ObjectStatus[objectType] = status;
    }

    void OnEventHappened(EnumEventTypes eventType, EnumObjectTypes objectType)
    {
        if (eventType != EnumEventTypes.ObjectReady) return;

        EnumStatType statType = GetStatTypeFromObjectType(objectType);

        for (int i = 0; i < CurrentRequests.Count; i++)
        {
            if (CurrentRequests[i].StatType == statType)
            {
                // Reducir enfado por objetivo completado.
                Request req = CurrentRequests[i];

                // Placeholder: Reduce un porcentaje fijo de enfado.
                this.Angryness = this.Angryness * 0.85f;
                if (GameEvents.Ins.OnScoreChanged != null) GameEvents.Ins.OnScoreChanged(EnumScoreType.GrannyAnger, this.AngrynessTotal);
                Destroy(GeneratedTextBubble);
                CurrentRequests.RemoveAt(i);
                break;
            }
        }
    }

    EnumStatType GetStatTypeFromObjectType(EnumObjectTypes objectType)
    {
        for (int i = 0; i < ObjectsForFun.Length; i++)
            if (ObjectsForFun[i] == objectType) return EnumStatType.Entretainment;
        for (int i = 0; i < ObjectsForTemp.Length; i++)
            if (ObjectsForTemp[i] == objectType) return EnumStatType.Temperature;
        for (int i = 0; i < ObjectsForHunger.Length; i++)
            if (ObjectsForHunger[i] == objectType) return EnumStatType.Hunger;

        return default(EnumStatType);
    }

    private void OnDestroy()
    {
        GameEvents.Ins.OnEventHappened -= OnEventHappened;
    }

    #endregion Recepci�n de Eventos
}
