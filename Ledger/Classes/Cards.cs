using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ledger.Classes
{
    public class ImageUris
    {
        public string Small { get; set; }
        public string Normal { get; set; }
        public string Large { get; set; }
        public string Png { get; set; }
        public string Art_crop { get; set; }
        public string Border_crop { get; set; }
    }

    public class CardFace
    {
        public string Object { get; set; }
        public string Name { get; set; }
        public string Mana_cost { get; set; }
        public string Type_line { get; set; }
        public string Oracle_text { get; set; }
        public string Watermark { get; set; }
        public string Artist { get; set; }
        public string Artist_id { get; set; }
        public string Illustration_id { get; set; }
    }

    public class AllParts
    {
        public string Object { get; set; }
        public string Id { get; set; }
        public string Component { get; set; }
        public string Name { get; set; }
        public string Type_line { get; set; }
        public string Uri { get; set; }
    }

    public class Legalities
    {
        public string Standard { get; set; }
        public string Future { get; set; }
        public string Historic { get; set; }
        public string Pioneer { get; set; }
        public string Modern { get; set; }
        public string Legacy { get; set; }
        public string Pauper { get; set; }
        public string Vintage { get; set; }
        public string Penny { get; set; }
        public string Commander { get; set; }
        public string Brawl { get; set; }
        public string Duel { get; set; }
        public string Oldschool { get; set; }
    }

    public class Prices
    {
        public string Usd { get; set; }
        public string Usd_foil { get; set; }
        public string Eur { get; set; }
        public string Tix { get; set; }
    }

    public class RelatedUris
    {
        public string Gatherer { get; set; }
        public string Tcgplayer_decks { get; set; }
        public string Edhrec { get; set; }
        public string Mtgtop8 { get; set; }
    }

    public class PurchaseUris
    {
        public string Tcgplayer { get; set; }
        public string Cardmarket { get; set; }
        public string Cardhoarder { get; set; }
    }

    public class Cards
    {
        public string Object { get; set; }
        public string Id { get; set; }
        public string Oracle_id { get; set; }
        public List<int> Multiverse_ids { get; set; }
        public int Mtgo_id { get; set; }
        public int Arena_id { get; set; }
        public int Tcgplayer_id { get; set; }
        public string Name { get; set; }
        public string Lang { get; set; }
        public string Released_at { get; set; }
        public string Uri { get; set; }
        public string Scryfall_uri { get; set; }
        public string Layout { get; set; }
        public bool Highres_image { get; set; }
        public ImageUris Image_uris { get; set; }
        public string Mana_cost { get; set; }
        public double Cmc { get; set; }
        public string Type_line { get; set; }
        public List<string> Colors { get; set; }
        public List<string> Color_identity { get; set; }
        public List<AllParts> All_parts { get; set; }
        public List<CardFace> Card_faces { get; set; }
        public Legalities Legalities { get; set; }
        public List<string> Games { get; set; }
        public bool Reserved { get; set; }
        public bool Foil { get; set; }
        public bool Nonfoil { get; set; }
        public bool Oversized { get; set; }
        public bool Promo { get; set; }
        public bool Reprint { get; set; }
        public bool Variation { get; set; }
        public string Set { get; set; }
        public string Set_name { get; set; }
        public string Set_type { get; set; }
        public string Set_uri { get; set; }
        public string Set_search_uri { get; set; }
        public string Scryfall_set_uri { get; set; }
        public string Rulings_uri { get; set; }
        public string Prints_search_uri { get; set; }
        public string Collector_number { get; set; }
        public bool Digital { get; set; }
        public string Rarity { get; set; }
        public string Card_back_id { get; set; }
        public string Artist { get; set; }
        public List<string> Artist_ids { get; set; }
        public string Illustration_id { get; set; }
        public string Border_color { get; set; }
        public string Frame { get; set; }
        public bool Full_art { get; set; }
        public bool Textless { get; set; }
        public bool Booster { get; set; }
        public bool Story_spotlight { get; set; }
        public int Edhrec_rank { get; set; }
        public Prices Prices { get; set; }
        public RelatedUris Related_uris { get; set; }
        public PurchaseUris Purchase_uris { get; set; }
    }

    public class CardList
    {
        public string Object { get; set; }
        public int Total_cards { get; set; }
        public bool Has_more { get; set; }
        public string Next_page { get; set; }
        public List<Cards> Data { get; set; }
    }
}
