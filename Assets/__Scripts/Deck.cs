using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Deck : MonoBehaviour
{
    [Header("Set In Inspector")]
    //suits
    public Sprite suitClub;
    public Sprite suitDiamond;
    public Sprite suitHeart;
    public Sprite suitSpade;

    public Sprite[] faceSprites;
    public Sprite[] rankSprites;

    public Sprite cardBack;
    public Sprite cardBackGold;
    public Sprite cardFront;
    public Sprite cardFrontGold;

    //prefabs
    public GameObject prefabCard;
    public GameObject prefabSprite;

    [Header("Set Dynamically")]
    public PT_XMLReader xmlr;
    public List<string> cardNames;
    public List<Card> cards;
    public List<Decorator> decorators;
    public List<CardDefinition> cardDefs;
    public Transform deckAnchor;
    public Dictionary<string, Sprite> dictSuits;


    public void InitDeck(string deckXMLText)
    {
        if(GameObject.Find("_Deck") == null)
        {
            GameObject anchorGO = new GameObject("_Deck");
            deckAnchor = anchorGO.transform;
        }
        dictSuits = new Dictionary<string, Sprite>()
        {
            { "C", suitClub},
            { "D", suitDiamond},
            { "H", suitHeart},
            { "S", suitSpade}
        };



        ReadDeck(deckXMLText);


        MakeCards();
    }



    public CardDefinition GetCardDefinitionByRank(int rnk)
    {
        foreach(CardDefinition cd in cardDefs)
        {
            if(cd.rank == rnk)
            {
                return cd;
            }
        }
        return null;
    }

    //make card gameobjects
    public void MakeCards()
    {
        cardNames = new List<string>();
        string[] letters = new string[] { "C", "D", "H", "S" };
        foreach(string s in letters)
        {
            for (int i = 0; i < 13; i++)
            {
                cardNames.Add(s + (i + 1));
            }
        }

        cards = new List<Card>();

        for (int i = 0; i < cardNames.Count; i++)
        {
            cards.Add(MakeCard(i));
        }
    }

    private Card MakeCard(int cNum)
    {
        GameObject cgo = Instantiate(prefabCard) as GameObject;

        cgo.transform.parent = deckAnchor;
        Card card = cgo.GetComponent<Card>();

        //stacks the cards
        cgo.transform.localPosition = new Vector3((cNum % 13) * 3, cNum / 13 * 4, 0);

        //assign basic values
        card.name = cardNames[cNum];
        card.suit = card.name[0].ToString();
        card.rank = int.Parse(card.name.Substring(1));
        if(card.suit == "D" || card.suit == "H")
        {
            card.colS = "Red";
            card.color = Color.red;
        }
        //pull card def
        card.def = GetCardDefinitionByRank(card.rank);

        AddDecorators(card);

        AddPips(card);
        AddFace(card);

        return card;
    }
    //private values used in the helper?
    private Sprite _tSp = null;
    private GameObject _tGO = null;
    private SpriteRenderer _tSR = null;

    private void AddDecorators(Card card)
    {
        //add decorators
        foreach(Decorator deco in decorators)
        {
            if(deco.type == "suit")
            {
                //instantiate
                _tGO = Instantiate(prefabSprite) as GameObject;
                //get spriterend
                _tSR = _tGO.GetComponent<SpriteRenderer>();
                //get suit
                _tSR.sprite = dictSuits[card.suit];
            }
            else
            {
                _tGO = Instantiate(prefabSprite) as GameObject;
                _tSR = _tGO.GetComponent<SpriteRenderer>();
                //get prop srite
                _tSp = rankSprites[card.rank];
                //assign rank
                _tSR.sprite = _tSp;
                //set color
                _tSR.color = card.color;
            }

            //make the deco sprites render above the card
            _tSR.sortingOrder = 1;
            //decor spr child of card
            _tGO.transform.SetParent(card.transform);
            //set local pos off of DeckXML
            _tGO.transform.localPosition = deco.loc;
            //flip the decor if you want to 
            if(deco.flip)
            {
                _tGO.transform.rotation = Quaternion.Euler(0, 0, 180);
            }
            //set the scale to keep decos from being too big
            if(deco.scale != 1)
            {
                _tGO.transform.localScale = Vector3.one * deco.scale;
            }
            //name this gameobject so its easy to see
            _tGO.name = deco.type;
            //add this deco to gameobject to the list card
            card.decoGOs.Add(_tGO);
        }
    }


    private void AddPips(Card card)
    {
        foreach(Decorator pip in card.def.pips)
        {
            _tGO = Instantiate(prefabSprite) as GameObject;
            _tGO.transform.SetParent(card.transform);
            _tGO.transform.localPosition = pip.loc;
            if(pip.scale != 1)
            {
                _tGO.transform.localScale = Vector3.one * pip.scale;
            }
            _tGO.name = "pip";
            _tSR = _tGO.GetComponent<SpriteRenderer>();
            _tSR.sprite = dictSuits[card.suit];
            _tSR.sortingOrder = 1;
            card.pipGos.Add(_tGO);
        }
    }
    private void AddFace(Card card)
    {
        if (card.def.face == "")
        {
            return;
        }

        _tGO = Instantiate(prefabSprite) as GameObject;
        _tSR = _tGO.GetComponent<SpriteRenderer>();

        _tSp = GetFace(card.def.face + card.suit);
        _tSR.sprite = _tSp;
        _tSR.sortingOrder = 1;
        _tGO.transform.SetParent(card.transform);
        _tGO.transform.localPosition = Vector3.zero;
        _tGO.name = "face";
    }

    private Sprite GetFace(string faceS)
    {
        foreach(Sprite _tSP in faceSprites)
        {
            if(_tSP.name == faceS)
            {
                return _tSP;
            }

        }
        return null;
    }

    public void ReadDeck(string deckXMLText)
    {
        xmlr = new PT_XMLReader();
        xmlr.Parse(deckXMLText);


        string s = "xml[0] decoator[0]";
        s += "type=" + xmlr.xml["xml"][0]["decorator"][0].att("type");
        s += "x=" + xmlr.xml["xml"][0]["decorator"][0].att("x");
        s += "y=" + xmlr.xml["xml"][0]["decorator"][0].att("y");
        s += "scale=" + xmlr.xml["xml"][0]["decorator"][0].att("scale");

        //print(s);




        decorators = new List<Decorator>();
        //grab an PT)XMLHashList of all the decorators in the xml fle
        PT_XMLHashList xDecos = xmlr.xml["xml"][0]["decorator"];
        Decorator deco;

        for (int i = 0; i < xDecos.Count; i++)
        {
            deco = new Decorator();
            deco.type = xDecos[i].att("type");
            deco.flip = (xDecos[i].att("flip") == "1");
            deco.scale = float.Parse(xDecos[i].att("scale"));
            //vector3 loc
            deco.loc.x = float.Parse(xDecos[i].att("x"));
            deco.loc.y = float.Parse(xDecos[i].att("y"));
            deco.loc.z = float.Parse(xDecos[i].att("z"));


            //add the temp to the list
            decorators.Add(deco);



        }

        //read pip locations for the card
        cardDefs = new List<CardDefinition>();

        PT_XMLHashList xCardDefs = xmlr.xml["xml"][0]["card"];

        for(int i = 0; i < xCardDefs.Count; i++)
        {
            //create new card def
            CardDefinition cDef = new CardDefinition();
            //parse
            cDef.rank = int.Parse(xCardDefs[i].att("rank"));
            //grab
            PT_XMLHashList xPips = xCardDefs[i]["pip"];
            if(xPips != null)
            {
                for(int j = 0; j < xPips.Count; j++)
                {
                    deco = new Decorator();
                    //pips on the card are handled via the decorator
                    deco.type = "pip";
                    deco.flip = (xPips[j].att("flip") == "1");
                    deco.loc.x = float.Parse(xPips[j].att("x"));
                    deco.loc.y = float.Parse(xPips[j].att("y"));
                    deco.loc.z = float.Parse(xPips[j].att("z"));
                    if(xPips[j].HasAtt("scale"))
                    {
                        deco.scale = float.Parse(xPips[j].att("scale"));
                    }
                    cDef.pips.Add(deco);
                }
            }
            cardDefs.Add(cDef);
        }

    }
}
