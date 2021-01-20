using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using Android.Webkit;
using Android.Runtime;
using Xamarin;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace maturita
{
    // Část "ConfigurationChanges=Android.Content.PM.ConfigChanges.Orientation | Android.Content.PM.ConfigChanges.ScreenSize" zajišťuje, aby se po přetočení telefonu nevracela aplikace do úvodního menu, ale aby zůstal aktuální obsah a přetočil se
    [Activity(Label = "maturita", MainLauncher = true, Icon = "@drawable/icon", ConfigurationChanges = Android.Content.PM.ConfigChanges.Orientation | Android.Content.PM.ConfigChanges.ScreenSize)]
    
    public class MainActivity : Activity
    {
        public float odpovediCelkove = 0;
        public float odpovediSpravne = 0;
        public float odpovediSpatne = 0;
        public string zvyraznovani = "0";

        public void literarniKviz()
        {
            String[] lines;
            int count;

            using (StreamReader sr = new StreamReader(this.Assets.Open("kanon_formular.csv"), Encoding.Default)) // Otevře soubor kanon_formular.csv v složce Assets
            {
                lines = sr.ReadToEnd().Split(new char[] { '\n' }); // Rozdělí obsah do pole dle řádků
                count = lines.Count(); // Zjistí počet řádků v souboru
            }
            string dilo = "", jmenoAutora = "";

            using (StreamReader sr2 = new StreamReader(this.Assets.Open("kanon_formular.csv"), Encoding.Default)) // Otevře soubor kanon_formular.csv v složce Assets
            {
                Random r = new Random(); // Zajišťuje možnost následně generovat náhodná čísla
                int rInt = r.Next(1, count); // Generuje náhodné číslo od 3 do count, což je počet řádků v souboru
            
                for (int a = 1; a < rInt; a++) // Provede se tolikrát tolikrát, jak velké je náhodně vygenerované číslo v rozmezí prvního a posledního řádku
                {
                    lines[a] = sr2.ReadLine(); // Přečte další řádek
                    String[] values2 = lines[a].Split(Convert.ToChar(";")); // Rozděluje jednotlivé řádky do dalšího pole na základě oddělení středníkem
                    dilo = values2[4]; // Do stringu dilo dosadí obsah a-tého řádků, 5-tého sloupce
                    jmenoAutora = values2[3]; // Do stringu dilo dosadí obsah a-tého řádků, 4-tého sloupce

                    // Ošetření nesprávného názvu
                    if (dilo.Count() < 4 || jmenoAutora.Count() < 4 || jmenoAutora.Contains("Nevyplněno") || jmenoAutora.Contains("Autor") || dilo.Contains("Nevyplněno") || dilo.Contains("Dílo") || jmenoAutora.Contains("Nezadán"))
                    {                      
                        if (rInt < count - 1) {rInt++;} // Pokud je nevyhovující název a aktuální řádek textu je výš než je pozice posledního řádku, přejde se na další řádek               
                        else {literarniKviz(); }  // Pokud je nevyhovující název a aktuální pozice v textu není výš než na posledním řádku, spustí se znovu metoda literarniKviz()      
                    }
                    //
                }
                LinearLayout literarniKvizLayout = new LinearLayout(this); // Vytvoří LinearLayout
                literarniKvizLayout.LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent); // Nastaví jeho zobrazení na celou obrazovku
                literarniKvizLayout.Orientation = Orientation.Vertical; // Určí orientaci layoutu na vertikální - prvky se přidávají od shora dolů
                literarniKvizLayout.SetGravity(GravityFlags.Center); // Určí zarovnání - do středu

                int rInt2 = r.Next(0, 3); // Vygeneruje další náhodné číslo v rozmezí od 0 do 3

                TextView otazka = new TextView(this); // Vytvoří TextView
                otazka.Text = ("Kdo je autorem díla " + dilo + "?"); // Určí text
                otazka.SetTextSize(Android.Util.ComplexUnitType.FractionParent, 26); // Určí velikost písma jako zlomek velikosti displaye
                otazka.SetPaddingRelative(15, 0, 50, 15); // Určí odsazení
                literarniKvizLayout.AddView(otazka); // Přidá TextView do LinearLayoutu

                // Vytvoří pole 4 RadioButtonů a každému z nich určí parametry a dosadí je to layoutu
                RadioButton[] radioButton = new RadioButton[4];
                int i = 0;
                foreach (Button b in radioButton)
                {
                    radioButton[i] = new RadioButton(this);
                    radioButton[i].Text = "";
                    literarniKvizLayout.AddView(radioButton[i]);
                    radioButton[i].SetPadding(20, 10, 20, 10); // Určí odsazení
                    i++;
                }
                //
                // Vytvoří tlačítko, určí text, dosadí do layoutu
                Button button1 = new Button(this);
                button1.Text = ("Další");
                literarniKvizLayout.AddView(button1);
                //

                // Vytvoří pole 4 TextView a každému z nich určí parametry
                i = 0;
                TextView[] poleTextView = new TextView[4];
                foreach (TextView textView in poleTextView)
                {
                    poleTextView[i] = new TextView(this);
                    poleTextView[i].SetTextSize(Android.Util.ComplexUnitType.FractionParent, 16);
                    poleTextView[i].SetPaddingRelative(25, 10, 25, 10); // Určí odsazení
                    literarniKvizLayout.AddView(poleTextView[i]);
                    i++;
                }
                //

                // Zobrazí počet všech, správných a špatných odpovědí
                poleTextView[0].Text = "Celkový počet odpovědí: " + odpovediCelkove.ToString();
                poleTextView[1].Text = "Z toho správných odpovědí: " + odpovediSpravne.ToString();
                poleTextView[2].Text = "Z toho špatných odpovědí: " + odpovediSpatne.ToString();
                // Výpočet procentuální úspěšnosti, ošetření dělení nulou
                if (odpovediCelkove > 0)
                {
                    poleTextView[3].Text = "Úspěšnost odpovědí: " + (100 / (odpovediCelkove / odpovediSpravne)).ToString() + " %";
                }
                else
                {
                    poleTextView[3].Text = "Úspěšnost odpovědí: 0 %";
                }
                //

                radioButton[rInt2].Text = jmenoAutora; // Za text RadioButtonu, jehož číslo se náhodně vygenerovalo v rozmezí od 0 do 3, dosadí správnou odpověď
                String[] spatneOdpovedi = new string[4]; // Vytvoří 4-prvkové pole stringů
                string jmenoAutora3 = "";
                // 4x Vygeneruje náhodně špatnou odpověď obdobným způsobem, jakým se vygenerovala správná
                for (int z = 0; z < 4; z++)
                {
                    String[] lines3;
                    int count3;
                    using (StreamReader sr = new StreamReader(this.Assets.Open("kanon_formular.csv"), Encoding.Default))
                    {
                        lines3 = sr.ReadToEnd().Split(new char[] { '\n' }); // Rozdělí obsah do pole dle řádků
                        count3 = lines3.Count();
                        sr.Close();
                    }                 
                    using (StreamReader sr3 = new StreamReader(this.Assets.Open("kanon_formular.csv"), Encoding.UTF8))
                    {
                        Random r3 = new Random();
                        int rInt3 = r3.Next(3, count);

                        for (int a = 1; a < rInt3; a++)
                        {
                            lines3[a] = sr3.ReadLine();
                            String[] values3 = lines3[a].Split(Convert.ToChar(";")); // Rozděluje jednotlivé řádky do dalšího pole na základě oddělení středníkem
                            jmenoAutora3 = values3[3];
                        }
                    }
                    spatneOdpovedi[z] = jmenoAutora3; // Přiřadí do pole spatneOdpovedi aktuálně vygenerovanou špatnou odpověď

                    // Ošetření špatného tvaru špatné odpovědi
                    if (jmenoAutora == jmenoAutora3 || (z > 0 && jmenoAutora3 == spatneOdpovedi[z - 1]) || jmenoAutora3.Count() < 4 || jmenoAutora3.Contains("Nevyplněno") || jmenoAutora3.Contains("Autor") || jmenoAutora3.Contains("Nezadán"))
                    {
                        z--; // Pokud je špatný tvar odpovědi, odečte se jednička od proměnné z, takže cyklus pojede o jeden cyklus víc
                    }
                    //
                }
                // Přiřazuje náhodně vybraná jména autorů jako text k těm RadioButtonům, u kterých už není napsán autor správný
                for (int u = 0; u < 4; u++)
                {
                    radioButton[u].Text = spatneOdpovedi[u];
                }
                radioButton[rInt2].Text = jmenoAutora;
                //
               
                // V případě začkrtnutí nějakého RadioButtonu se ostatní odškrtnou
                for (int u = 0; u < 4; u++)
                {                
                        radioButton[u].Id = u;
                        radioButton[u].Click += (sender, e) =>
                        {
                            for (int o = 0; o < 4; o++) {radioButton[o].Checked = false;} 
                            RadioButton rdbtn = (RadioButton)sender;                            
                            radioButton[rdbtn.Id].Checked = true;
                        };                                        
                }        
                //
                // Pokud je při stisknutí tlačítka zaškrtnuto tlačítko, jež se shoduje se správnou odpovědí, objeví se na horní části obrazovky vyskakovací okno a přičte se k celkovému počtu odpovědí a k počtu správných odpovědí
                button1.Click += (sender, e) =>
                {
                    if(radioButton[0].Checked || radioButton[1].Checked || radioButton[2].Checked || radioButton[3].Checked)
                    { 
                    AlertDialog.Builder alert = new AlertDialog.Builder(this);                
                    if (radioButton[rInt2].Checked)
                    {                      
                        alert.SetTitle("Správná odpověď!");
                        odpovediSpravne++;
                    }
                    //
                    // Pokud je při stisknutí tlačítka zaškrtnuto tlačítko, jež se neshoduje se správnou odpovědí, objeví se na horní části obrazovky vyskakovací okno a přičte se k celkovému počtu odpovědí a k počtu špatných odpovědí
                    else
                    {
                        alert.SetTitle("Špatná odpověď.");
                        alert.SetMessage("Autorem díla " + dilo + " je " + jmenoAutora);                        
                        odpovediSpatne++;
                    }
                    odpovediCelkove++;
                    Dialog dialog = alert.Create();
                    dialog.Window.SetGravity(GravityFlags.Top);
                    dialog.Show();
                    //
                    literarniKviz(); // Po vyhodnocení se spouští funkce znovu - přechází se na další otázku
                    }
                };
                SetContentView(literarniKvizLayout);
            }
        }


        public static string odstraneniDiakritiky(string Text) //Metoda převádějící české znaky na znaky bez diakritiky
        {
            string stringFormD = Text.Normalize(NormalizationForm.FormD);
            StringBuilder retVal = new StringBuilder();
            for (int index = 0; index < stringFormD.Length; index++)
            {
                if (System.Globalization.CharUnicodeInfo.GetUnicodeCategory(stringFormD[index]) != System.Globalization.UnicodeCategory.NonSpacingMark)
                    retVal.Append(stringFormD[index]);
            }
            return retVal.ToString().Normalize(NormalizationForm.FormC);
        }


        async public void wiki(string odkaz, string nazev, string nazevBezDiakritiky)
        {
            string html="";
            LinearLayout wikiLayout = new LinearLayout(this); // Vytvoří LinearLayout
            HttpClient httpClient = new HttpClient(); // Vytvoří HTTP klienta
            // Zkusí otevřít přeposlanou Wikipedia stránku, pokud se to nepovede, tak daný výraz zadá do vyhledávání Wikipedie
            try
            {
                // Úprava HTML tagů pro lepší zobrazení - záměna hypertextových odkazů za tučné písmo, zvýraznění dané věci apod.
                html = await httpClient.GetStringAsync(odkaz); // Do stringu dosadí html kód webu z odkazu převedený na typ string
                html = Regex.Replace(html, @"(<img\/?[^>]+>)", "", RegexOptions.IgnoreCase); // Smaže většinu obrázků - html značku <img> nahradí prázdným stringem, zavináč slouží pro upřesňující označení pro string
                html = Regex.Replace(html, @"(<a href\/?[^>]+>)", "<b>", RegexOptions.IgnoreCase); // Značku hypertextu <a href> nahradí značkou pro tučný text <b>
                if (zvyraznovani == "1") // Pokud se string zvyraznovani rovna "1", tak se zobrazuje a stahuje text se zvyraznenym textem tucne a kurzivou
                {
                    html = html.Replace("</a>", "</b>") // Značku </a> nahradí značkou </b>
                                .Replace("<b>", "<b><mark>") // Tučný text zvýrazní
                                .Replace("</b>", "</mark></b>") // Tučný text zvýrazní
                                .Replace("<i>", "<mark><i>") // Text kurzívou zvýrazní
                                .Replace("</i>", "</i></mark>"); // Text kurzívou zvýrazní
                }                
                html = html.Replace("</a>", "</b>") // Značku </a> nahradí značkou </b>
                           .Replace("Navigace", "") // Nahradí text Navigace
                           .Replace("Hledání", "") // Nahradí text Hledání
                           .Replace("Skočit na:", ""); // Nahradí text "Skočit na:"
                if (html.Contains(">Odkazy</span><span class=")){int indexOfSteam = html.IndexOf(">Odkazy</span><span class="); html = html.Remove(indexOfSteam);}
                string regex = "(\\[.*\\])"; // Zjistí všechen obsah v hranatých závorkách
                html = Regex.Replace(html, regex, ""); // Odstraní všechen obsah v hranatých závorkách
                int index = html.IndexOf("Navigační menu");
                if (index > 0){html = html.Substring(0, index);} // V případě že v textu je text "Navigační menu", tak ho odstraní spolu se vším za ním vytvořením substringu
                //
                // Zobrazí WebView a obsah v něm
                WebView zobrazeniWiki = new WebView(this);
                zobrazeniWiki.LoadData(html, "text/html; charset=utf-8", "utf-8");
                wikiLayout.AddView(zobrazeniWiki);
                SetContentView(wikiLayout); 
                //          
                                    
            }
            // Pokud se to nepodaří, obsah stringu obsah tak, aby se název díla nebo autora ve Wikipedii vyhledal místo zobrazení přesné stránky            
            catch
            {
                odkaz = odkaz.Replace("http://cs.wikipedia.org/wiki/", "https://cs.m.wikipedia.org/w/index.php?search=");
                WebView youtube = new WebView(this);
                youtube.LoadUrl(odkaz);
                SetContentView(youtube);
            }
            //

            // Uloží stažený obsah do souboru s odpovídajícím názvem bez diakritiky ve formátu HTML v složce programu
            var path = System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData);
            string fileName = path + "/" + nazevBezDiakritiky + ".html";
            if (!File.Exists(fileName)) // Proveď, pokud neexistuje soubor s daným názvem
            {
                using (var streamWriter = new StreamWriter(fileName, true))
                {
                    streamWriter.Write(html);
                    streamWriter.Close();
                 }
            }
            //
        }
        // Seznam vyjímek - dotazy, u kterých je nutné upřesnění. Pokud se string dilo rovná některému z daných výrazů, potřebně jej změní (potřeba hlavně u jednoslovných názvů)
        public string vyjimkyProCsv(string dilo)
       
        {
            switch (dilo)
            {
                case "Jeptiška":
                    dilo = "Jeptiška_(román)";
                    break;
                case "Nana":
                    dilo = "Nana_(román)";
                    break;
                case "Kytice":
                    dilo = "Kytice_(sbírka)";
                    break;
                case "Babička":
                    dilo = "Babička_(povídka)";
                    break;
                case "F. L. Věk (I. díl)":
                    dilo = "F._L._Věk";
                    break;
                case "Cikáni":
                    dilo = "Cikáni_(Karel_Hynek_Mácha)";
                    break;
                case "Nora":
                    dilo = "Domeček_pro_panenky";
                    break;
                case "Revizor":
                    dilo = "Revizor_(Gogol)";
                    break;
                case "Kvílení":
                    dilo = "Kvílení_a_jiné_básně";
                    break;
                case "Proměna":
                    dilo = "Proměna_(povídka)";
                    break;
                case "Cizinec":
                    dilo = "Cizinec_(román)";
                    break;
                case "1984":
                    dilo = "1984_(román)";
                    break;
                case "Sestra":
                    dilo = "Sestra_(román)";
                    break;
                case "Nesmrtelnost":
                    dilo = "Nesmrtelnost_(kniha)";
                    break;
                case "Osudy dobrého vojáka Švejka za světové války (I. díl)":
                    dilo = "Osudy_dobrého_vojáka_Švejka_za_světové_války";
                    break;
                case "Edison":
                    dilo = "Edison_(báseň)";
                    break;
            }
            return dilo;
            //
        }

        public void csv()
        {
            ScrollView scroll = new ScrollView(this); // Vytvoří ScrollView, což umožní scrollovat
            scroll.LayoutParameters = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent); // Určí parametry - roztažení na celou obrazovku
            LinearLayout kanon = new LinearLayout(this); // Vytvoří LinearLayout
            kanon.Orientation = Orientation.Vertical; // Určí vertikální orientaci layoutu
            kanon.LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent); // Určí parametry - roztažení na celou obrazovku
            scroll.AddView(kanon); // Do ScrollView vloží LinearLayout
            String[] lines; // Vytvoří pole stringů lines
            int count; 
            LinearLayout volne = new LinearLayout(this); // Vytvoří LinearLayout
            volne.LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent); // Určí parametry - roztažení na celou obrazovku
            volne.Orientation = Orientation.Vertical; // Určí vertikální orientaci layoutu
            volne.SetGravity(GravityFlags.Center); // Určí zarovnání - na střed
            
            // Rozdělí obsah souboru na řádky, ty dosadí do pole stringů lines a spočítá jejich počet
            using (StreamReader sr = new StreamReader(this.Assets.Open("kanon_formular.csv"), Encoding.UTF8))
            {
                lines = sr.ReadToEnd().Split(new char[] { '\n' });
                count = lines.Count();
                sr.Close();             
            } 
            //

            using (StreamReader sr2 = new StreamReader(this.Assets.Open("kanon_formular.csv"), Encoding.UTF8))
            {
                // Tolikrát, kolik je počet řádků, provede následující
                for (int a = 0; a < count; a++)
                {
                    lines[a] = sr2.ReadLine();
                    String[] values2 = lines[a].Split(Convert.ToChar(";")); // Rozdělí řádek dle středníků
                    string dilo = values2[4], jmenoAutora = values2[3]; // Vytvoří stringy a dosadí do nich název díla a jméno autora

                    // Oveření správného názvu
                    if (!(dilo.Count() < 4 || jmenoAutora.Count() < 4 || jmenoAutora.Contains("Nevyplněno") || jmenoAutora.Contains("Autor") || dilo.Contains("Nevyplněno") || dilo.Contains("Dílo")))
                    {
                        Button button = new Button(this); // Vytvoří tlačítko
                        button.Text = dilo; // Jako text tlačítka určí název díla
                        kanon.AddView(button); // Přidá tlačítko do layoutu kanon        
                        button.Click += (sender, e) =>
                        {
                            Button[] poleTlacitek = new Button[3];       // Vytvoří jednorozměrné pole Buttonů poleTlacitek a určí jeho velikost počtem souborů
                            int f = 0;
                            // Pro každé tlačítko v poli poleTlacitek vytvoří tlačítko
                            foreach (Button t in poleTlacitek)
                            {
                                poleTlacitek[f] = new Button(this);                 // Jako i-tou položku v poleTlacitek vytvoří nové tlačítko
                                poleTlacitek[f].Id = f; // Jako identifikátor aktuálního tlačítka určí aktuální hodnotu proměnné f
                                volne.AddView(poleTlacitek[f]);              // Přidá i-té tlačítko v poleTlacitek do layoutu                                           
                                f++;
                            }
                            //
                            
                            // Určí text tlačítek
                            poleTlacitek[0].Text = "Audio/Video - " + dilo;
                            poleTlacitek[1].Text = "O díle - " + dilo;
                            poleTlacitek[2].Text = "O autorovi - " + jmenoAutora;
                            //
                            volne.RemoveView(poleTlacitek[1]); // Odstraní z layoutu tlačítko pro dílo, přidá se znovu až následně pod určitou podmínkou

                            // Při kliknutí na první tlačítko se vytvoří a otevře WebView, ve kterém se na YouTube vyhledá jako výraz název díla spolu s autorem a přídavkem "česky"
                            poleTlacitek[0].Click += (sender2, e2) =>
                            {
                                dilo = "https://www.youtube.com/results?search_query=" + dilo + " " + jmenoAutora + " česky";
                                WebView youtube = new WebView(this);
                                youtube.LoadUrl(dilo);
                                SetContentView(youtube);
                            };
                            //

                            dilo = dilo.Replace(" ", "_"); // Pro účely zobrazení konkrétní stránky na Wikipedii nahradí mezery v názvu díla podržítkem
                            string nazev = odstraneniDiakritiky(dilo); // Pomocí funkce odstraneniDiakritiky() odstraní českou diakritiku z důvodu kontroly, jestli už neexistuje soubor pod stejným názvem
                            var path = System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData);
                            string fileName = path + "/" + nazev + ".html";

                            // Pokud soubor už existuje, zabarví text tlačítka do modra, pokud ne, tak do červena
                            if (File.Exists(fileName))
                            {
                                poleTlacitek[1].SetTextColor(Android.Graphics.Color.Blue);
                            }
                            else
                            {
                                poleTlacitek[1].SetTextColor(Android.Graphics.Color.Red);
                            }
                            //

                            if (!(dilo == "Nemilovaná" || dilo == "Měsíce" || dilo == "Katyně" || dilo == "Povídky")) // Vyjímky děl, o kterých na Wikipedii aktuálně neexistuje vhodná stránka
                            {
                                volne.AddView(poleTlacitek[1]); // Přidá zpět tlačítko, jehož většina parametrů je již určena

                                // Po kliknutí na tlačítko s názvem díla se ověří, zda soubor už existuje
                                poleTlacitek[1].Click += (sender2, e2) =>
                                {
                                    if (!File.Exists(fileName)) // Pokud neexistuje, dosadí se jeho upravený název díla spolu s předcházející částí odkazu do stringu odkaz, spustí metodu
                                    {
                                        string odkaz = "http://cs.wikipedia.org/wiki/" + dilo;
                                        wiki(odkaz, dilo, nazev); // Spustí metodu wiki s parametry odkaz, dilo, nazev
                                    }

                                    else // Pokud soubor existuje, otevře už existující soubor
                                    {
                                        LinearLayout wikiLayout = new LinearLayout(this); // Vytvoří LinearLayout
                                        string html = ""; // Vytvoří prázdný string

                                        // Přečte obsah souboru
                                        using (StreamReader sr = new StreamReader(fileName, Encoding.Default))
                                        {
                                            html = sr.ReadToEnd();
                                            sr.Close();
                                        }
                                        //
                                        // Vytvoří WebView a ukáže obsah souboru
                                        WebView zobrazeniWiki = new WebView(this); 
                                        zobrazeniWiki.LoadData(html, "text/html; charset=utf-8", "utf-8");
                                        wikiLayout.AddView(zobrazeniWiki);
                                        SetContentView(wikiLayout);
                                        //
                                    }
                                };
                                //
                            }                           
                            dilo = vyjimkyProCsv(dilo); // Spustí metodu vyjimkyProCsv s parametrem dilo, která případně nahradí nevyhovující název díla
                            jmenoAutora = jmenoAutora.Replace(" ", "_"); // Kvůli Wikipedii nahradí ve jménu autora mezeru podtržítkem

                            string nazev2 = odstraneniDiakritiky(jmenoAutora); // Odstraní diakritiku pomocí metody
                            string fileName2 = path + "/" + nazev2 + ".html";

                            // Pokud soubor už existuje, zabarví text tlačítka do modra, pokud ne, tak do červena
                            if (File.Exists(fileName2))
                            {
                                poleTlacitek[2].SetTextColor(Android.Graphics.Color.Blue);
                            }
                            else
                            {
                                poleTlacitek[2].SetTextColor(Android.Graphics.Color.Red);
                            }
                            //
                            poleTlacitek[2].Click += (sender2, e2) =>
                            {
                                if (!File.Exists(fileName2)) // Pokud neexistuje, dosadí se jeho upravený název autora spolu s předcházející částí odkazu do stringu odkaz, spustí metodu
                                {
                                    string odkaz = "http://cs.wikipedia.org/wiki/" + jmenoAutora; 
                                    wiki(odkaz, jmenoAutora, nazev2); // Spustí metodu wiki s parametry odkaz, dilo, nazev
                                }
                                else // Pokud soubor existuje, otevře už existující soubor
                                {
                                    LinearLayout wikiLayout = new LinearLayout(this);
                                    string html = "";

                                    // Přečte obsah souboru
                                    using (StreamReader sr = new StreamReader(fileName2, Encoding.Default))
                                    {
                                        html = sr.ReadToEnd(); 
                                        sr.Close();
                                    }
                                    //

                                    // Vytvoří WebView a ukáže obsah souboru
                                    WebView zobrazeniWiki = new WebView(this);
                                    zobrazeniWiki.LoadData(html, "text/html; charset=utf-8", "utf-8");
                                    wikiLayout.AddView(zobrazeniWiki);
                                    SetContentView(wikiLayout);
                                    //
                                }                               
                            };
                            Button tlacitkoSmazani = new Button(this);
                            tlacitkoSmazani.Text = "Smazat stažené";
                            tlacitkoSmazani.SetTextColor(Android.Graphics.Color.Black);
                            tlacitkoSmazani.Click += (sender2, e2) => // Po kliknutí na tlačítko smaže už stažené soubory týkající se daného díla a autora
                            {
                                if (File.Exists(fileName)) { File.Delete(fileName); } 
                                if (File.Exists(fileName2)) { File.Delete(fileName2); }
                            };
                            volne.AddView(tlacitkoSmazani);
                            SetContentView(volne);
                        };
                    }
                }
                sr2.Close();
            }
            SetContentView(scroll);
        }

        public void oAplikaci()
        {
            LinearLayout oAplikaciLayout = new LinearLayout(this); // Vytvoří layout
            oAplikaciLayout.Orientation = Orientation.Vertical; // Určí orientaci layoutu na vertikální - prvky se přidávají od shora dolů
            oAplikaciLayout.SetPadding(10, 10, 10, 10); // Určí odsazení
            string oAplikaciText;
            // Přečte soubor v umístění Assets/oAplikaci.html až do konce
            using (StreamReader sr = new StreamReader(this.Assets.Open("oAplikaci.html"), Encoding.Default))
            {
                oAplikaciText = sr.ReadToEnd();
                oAplikaciText.Replace("\n", " \n").Replace("&nbsp;", " "); 
                sr.Close();
            }
            //

            // Vytvoří WebView a zobrazí v něm obsah
            WebView oAplikaciWebView = new WebView(this);
            oAplikaciWebView.LoadData(oAplikaciText, "text/html; charset=utf-8", "utf-8");
            oAplikaciWebView.LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent);
            oAplikaciLayout.AddView(oAplikaciWebView);
            SetContentView(oAplikaciLayout);
            //
        }

        public void autori()
        {
            LinearLayout autoriLayout = new LinearLayout(this);  // Vytvoří layout
            autoriLayout.Orientation = Orientation.Vertical; // Určí orientaci layoutu na vertikální - prvky se přidávají od shora dolů
            autoriLayout.SetGravity(GravityFlags.Center);  // Určí zarovnání na střed
            autoriLayout.SetPadding(10, 10, 10, 10); // Určí odsazení layoutu layoutu v klasických pixelech (Možno upravit pro zachování stejného poměru odsazení (vzledem k rozlišením rozlišení displaye na různých zařízeních) na dp místo px)
            String[] fileListSpisovatele = this.Assets.List("Spisovatele");  // Vytvoří jednorozměrné pole textových řetezců(string) a jako položky řetězce dosadí názvy všech souborů v určené podsložce složky Assets
            Button[] poleTlacitek = new Button[fileListSpisovatele.Count()]; // Vytvoří jednorozměrné pole Buttonů poleTlacitek a určí jeho velikost počtem souborů

            int f = 0;
            // Pro každého autora vytvoří tlačítko s jeho jménem
            foreach (Button t in poleTlacitek)
            {
                poleTlacitek[f] = new Button(this); // Jako i-tou položku v poleTlacitek vytvoří nové tlačítko
                poleTlacitek[f].Id = f; // Jako identifikátor aktuálního tlačítka dosadí aktuální hodnotu proměnné f
                poleTlacitek[f].Text = fileListSpisovatele[f].Replace('_', ' '); // Jako text pro i-té tlačítko v poleTlacitek dosadí i-tou hodnotu z fileList(z listu zjištěných názvů souborů v podsložce)
                autoriLayout.AddView(poleTlacitek[f]); // Přidá i-té tlačítko v poleTlacitek do layoutu               
                poleTlacitek[f].Click += (sender, e) =>
                {
                    Button btn = (Button)sender; // Předá informace o stisknutém tlačítku do tlačítka btn
                    zobrazeniTlacitek(btn.Id); // Spustí funkci zobrazeniTlacitek s parametrem aktuálního sticknutého tlačítka
                };
                f++;
            }
            SetContentView(autoriLayout);                    // Zobrazí layout5
        }
        public void zobrazeniTlacitek(int poradiAutora)
        {            
            LinearLayout layoutZobrazeniVolnehoDila = new LinearLayout(this); // Vytvoří LinearLayout        
            LinearLayout layout5 = new LinearLayout(this); // Vytvoří layout       
            layout5.SetGravity(GravityFlags.Center); // Určí zarovnání na střed
            layout5.LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent); // Určí parametry - roztažení na celou obrazovku
            layout5.Orientation = Orientation.Vertical;         // Určí orientaci layoutu na vertikální - prvky se přidávají od shora dolů
            layout5.SetPadding(10, 10, 10, 10);                 // Určí odsazení layoutu layoutu v klasických pixelech (Možno upravit pro zachování stejného poměru odsazení (vzledem k rozlišením rozlišení displaye na různých zařízeních) na dp místo px)

            ScrollView scroll = new ScrollView(this); // Vytvoří ScrollView, což umožní scrollovat
            scroll.LayoutParameters = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent); // Určí parametry - roztažení horizontálně na celou obrazovku a vertikálně na délku textu - umožní scrollování
            scroll.AddView(layout5); // Do ScrollView vloží LinearLayout

            RelativeLayout rl = new RelativeLayout(this);
            rl.LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent);
            rl.SetGravity(GravityFlags.Center); // Určí zarovnání na střed
            rl.AddView(scroll); // Do rl přidá scroll

            String[] fileListSpisovatele = this.Assets.List("Spisovatele"); // Vytvoří pole string a do něj dosadí jako jednotlivé prvky všechny soubory a složky ve složce Assets/Spisovatele
            String[] fileList = this.Assets.List("Spisovatele/" + fileListSpisovatele[poradiAutora]); // Vytvoří jednorozměrné pole textových řetezců(string) a jako položky řetězce dosadí názvy všech souborů v určené podsložce složky Assets

            // Řazení podle délky textu (včetně HTML značek)

                Button[] poleTlacitek = new Button[fileList.Count()];   // Vytvoří jednorozměrné pole Buttonů poleTlacitek a určí jeho velikost počtem souborů
                String[] content = new string[fileList.Count()];
                int i = 0;

                foreach (Button t in poleTlacitek)
                {
                    poleTlacitek[i] = new Button(this);         // Jako i-tou položku v poleTlacitek vytvoří nové tlačítko
                    poleTlacitek[i].Id = i; // Jako Identifikátor tlačítka poleTlacitek[i] určí proměnnou 
                    poleTlacitek[i].Text = fileList[i].First().ToString().ToUpper() + fileList[i].Substring(1); // Vezme první char ze stringu, zvětší ho a spojí se zbytkem původního textu, z nějž byl původní malý char odstraněn
                    poleTlacitek[i].Text = poleTlacitek[i].Text.Replace('_', ' ').Replace(".html", "").Replace(".pdf", ""); // Jako text pro i-té tlačítko v poleTlacitek dosadí i-tou hodnotu z fileList(z listu zjištěných názvů souborů v podsložce)
                    layout5.AddView(poleTlacitek[i]);           // Přidá i-té tlačítko v poleTlacitek do layoutu 
                    poleTlacitek[i].Click += (sender, e) => // Po kliknutí se přečte obsah následujícího souboru ve složce aktuálního autora
                    {
                        Button btn = (Button)sender; // Předá do Buttonu btn informaci o tom, který Button byl prokliknut                                             
                        using (StreamReader sr = new StreamReader(this.Assets.Open("Spisovatele/" + fileListSpisovatele[poradiAutora] + "/" + fileList[btn.Id]), Encoding.Default))
                        {
                            content[btn.Id] = sr.ReadToEnd(); // Do pole stringů content[] dosadí na pozici odpovídající identifikátoru tlačítka, které bylo právě stisknuto, obsah celého souboru
                            content[btn.Id] = content[btn.Id].Replace("\n", " \n"); // Resi problem s mazanim mezer v HTML textu, ktery je na nasledujicim radku -> prida mezeru na konci radku
                            sr.Close();
                        }
                                              
                        //
                       string font, velikostPisma, barvaPisma; // Definice stringů                     
                        var path2 = System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData);
                        var fileName = Path.Combine(path2 + "/konfiguracniSoubor.txt");
                        predaniKonfiguraceNastaveni(out font, out velikostPisma, out barvaPisma, fileName); // Spustí metodu, která vrátí font,velikostPisma a barvaPisma)
                       //  Úprava textu dle nastavení pomocí HTML tagů
                        string nastaveni = "<style> html *{font-size: " + velikostPisma + " !important; color: " + barvaPisma + " !important; font-family: " + font + ", sarif"+" !important;}"; // Určí parametry stylování HTML                      
                        content[btn.Id] = content[btn.Id].Replace("<style>", nastaveni); // Nahradí tag <style> vlastním tagem <style> s vlastními parametry
                        content[btn.Id] = content[btn.Id].Replace("<STYLE>", nastaveni); // Nahradí tag <STYLE> vlastním tagem <style> s vlastními parametry                     
                        //
                        WebView zobrazeniVolnehoDila = new WebView(this);
                        zobrazeniVolnehoDila.LoadData(content[btn.Id], "text/html; charset=utf-8", "utf-8");
                        layoutZobrazeniVolnehoDila.AddView(zobrazeniVolnehoDila);
                        SetContentView(layoutZobrazeniVolnehoDila);
                    };
                    i++;                    
                }
            SetContentView(rl);
        }


        public void poznamky()
        {
            EditText editText = new EditText(this); // Vytvoří EditText
            LinearLayout layout = new LinearLayout(this); // Vytvoří layout
            
            layout.LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent);
            layout.Orientation = Orientation.Vertical; // Určí orientaci layoutu na vertikální - prvky se přidávají od shora dolů
            layout.SetGravity(GravityFlags.Center); // Určí zarovnání layoutu na střed
            layout.SetPadding(10, 10, 10, 10); // Určí odsazení
            layout.AddView(editText); // Přidá do layoutu EditText

            Button button = new Button(this); // Vytvoří nové tlačítko
            button.Text = "Uložit";
            var path = System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData);
            string fileName = path + "/poznamka.txt";

            // Při kliknutí na tlačítko uložit uloží aktuální obsah editText a následně ho vyprázdní
            button.Click += (sender, e) =>
            {                
                File.Delete(fileName);
                using (var streamWriter = new StreamWriter(fileName, true))
                {
                    streamWriter.WriteLine(editText.Text);
                    streamWriter.Close();
                }
                editText.Text = "";
            };
            //
            Button button2 = new Button(this); // Vytvoří nové tlačítko
            button2.Text = "Otevřít poslední";

            // Po kliknutí na tlačítko "Otevřít poslední" se otevře poslední poznámka a zobrazí se v EditText políčku
            button2.Click += (sender, e) =>
            {                
                using (var streamReader = new StreamReader(fileName, true))
                {
                    editText.Text = streamReader.ReadToEnd();
                    streamReader.Close();
                }
            };
            //
            layout.AddView(button);
            layout.AddView(button2);
            SetContentView(layout);
        }

        public void predaniKonfiguraceNastaveni(out string font, out string velikostPisma, out string barvaPisma, string fileName)

        {
            font = ""; velikostPisma = ""; barvaPisma = "";
            String[] lines3;
            // Přečte obsah konfiguračního souboru a každý řádek dosadí do proměnných, následně první 3 proměnné vrátí
            using (StreamReader sr = new StreamReader(fileName, Encoding.Default))
            {
                lines3 = sr.ReadToEnd().Split(new char[] { '\n' });
            }
            font = lines3[0];
            velikostPisma = lines3[1];
            barvaPisma = lines3[2];
            zvyraznovani = lines3[3];          
            //     
        }

        public void zapisDoKonfiguracnihoSouboru(string font, string velikostPisma, string barvaPisma, string fileName)
        {
            File.WriteAllText(fileName, ""); // Smaže obsah souboru
            // Zapíše do souboru v umístění fileName na 1. řádek font, na 2. řádek velikost písma, na 3. řádek barvu písma v HTML kódu ve tvaru #000000 atd.
            using (var streamWriter = new StreamWriter(fileName, true)) 
            {
                streamWriter.WriteLine(font);
                streamWriter.WriteLine(velikostPisma);
                streamWriter.WriteLine(barvaPisma);
                streamWriter.WriteLine(zvyraznovani);                
                streamWriter.Close();
            }
            //
        }

        public void nastaveni()
        {            
            LinearLayout linearLayout = new LinearLayout(this); // Vytvoří LinearLayout
            linearLayout.LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent);
            linearLayout.Orientation = Orientation.Vertical;         // Určí orientaci layoutu na vertikální - prvky se přidávají od shora dolů
            linearLayout.SetPadding(10, 10, 10, 10);  // Určí odsazení
            LinearLayout linearLayout2 = new LinearLayout(this); // Vytvoří LinearLayout
            linearLayout2.LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent);
            linearLayout2.Orientation = Orientation.Vertical;         // Určí orientaci layoutu na vertikální - prvky se přidávají od shora dolů
            linearLayout2.SetGravity(GravityFlags.Center); // Zarovnání obsahu layoutu na střed
            linearLayout2.SetPadding(10, 10, 10, 10);  // Určí odsazení

            TextView[] textView = new TextView[3]; // Vytvoří pole 3 TextView
            EditText[] poleNastaveni = new EditText[3]; // Vytvoří pole 3 EditView

            int i = 0;
            // 
            foreach (TextView tV in poleNastaveni) // Pro každé TextView v poleNastaveni (3x) provede následující
            {
                poleNastaveni[i] = new EditText(this); // Vytvoří EditView
                textView[i] = new TextView(this);  // Vytvoří TextView
                linearLayout.AddView(textView[i]); // Přidá textView[i] do layoutu
                linearLayout.AddView(poleNastaveni[i]); // Přidá poleNastaveni[i] do layoutu
                poleNastaveni[i].SetTextSize(Android.Util.ComplexUnitType.FractionParent, 20); // Nastaví pro poleNastaveni[i] jako velikost textu poměrnou část z celé velikosti layoutu
                i++; // Navýší proměnnou i o 1
            }
            //
            string font, velikostPisma, barvaPisma; // Vytvoří stringy
            var path = System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData);
            var fileName = Path.Combine(path + "/konfiguracniSoubor.txt"); // Dosadí umístění obsahující konfigurační soubor
            
            predaniKonfiguraceNastaveni(out font, out velikostPisma, out barvaPisma, fileName); // Spustí metodu se 4 parametry a vrátí 3 z nich
                 
             // Určí text     
                textView[0].Text = "Použitý font: ";
                textView[1].Text = "Velikost písma: ";
                textView[2].Text = "Barva písma: ";
                poleNastaveni[0].Text = font;
                poleNastaveni[1].Text = velikostPisma;
                poleNastaveni[2].Text = barvaPisma;
            //
                i = 0;
                Button button = new Button(this);
                button.Text = "Uložit";
                linearLayout.AddView(button);

                Button button2 = new Button(this);
                button2.Text = "Nastavení zvýraznění";
                linearLayout.AddView(button2);

                button2.Click += (sender, e) =>
                {                     
                    Button button22 = new Button(this);
                    linearLayout2.AddView(button22);
                    button22.Text = "Nezvýrazňovat";
                    button22.Click += (sender2, e2) => { zvyraznovani = "0"; zapisDoKonfiguracnihoSouboru(font, velikostPisma, barvaPisma, fileName); };

                    Button button33 = new Button(this);
                    linearLayout2.AddView(button33);
                    button33.Text = "Zvýrazňovat";
                    button33.Click += (sender2, e2) => { zvyraznovani = "1"; zapisDoKonfiguracnihoSouboru(font, velikostPisma, barvaPisma, fileName); };

                    SetContentView(linearLayout2);
                };                

            // Po kliknutí na tlačítko Uložit se do stringů dosadí aktuální obsah EditText polí
            button.Click += (sender, e) =>
                {
                    font = poleNastaveni[0].Text;
                    velikostPisma = poleNastaveni[1].Text;
                    barvaPisma = poleNastaveni[2].Text;
                    zapisDoKonfiguracnihoSouboru(font, velikostPisma, barvaPisma, fileName);                   
                };
            //    

                Button[] poleTlacitek = new Button[3]; // Vytvoří jednorozměrné pole Buttonů poleTlacitek a určí jeho velikost počtem souborů
                int f = 0;
                foreach (Button t in poleTlacitek)
                {
                    poleTlacitek[f] = new Button(this); // Jako i-tou položku v poleTlacitek vytvoří nové tlačítko
                    poleTlacitek[f].Id = f;
                    linearLayout.AddView(poleTlacitek[f]); // Přidá i-té tlačítko v poleTlacitek do layoutu               
                    poleTlacitek[f].Click += (sender, e) =>
                    {
                        Button btn = (Button)sender;
                        Button[] poleTlacitek2 = new Button[6]; // Vytvoří jednorozměrné pole Buttonů poleTlacitek a určí jeho velikost počtem souborů
                        int f2 = 0;                        
                        foreach (Button t2 in poleTlacitek2)
                        {                         
                            poleTlacitek2[f2] = new Button(this); // Jako i-tou položku v poleTlacitek vytvoří nové tlačítko
                            poleTlacitek2[f2].Id = f2;
                            linearLayout2.AddView(poleTlacitek2[f2]); // Přidá i-té tlačítko v poleTlacitek do layoutu                               
                            f2++;
                        }                        
                        switch(btn.Id)
                        {
                            case 0:
                                // Určí text - názvy fontů
                                poleTlacitek2[0].Text = "sans-serif";                                
                                poleTlacitek2[1].Text = "sans-serif-light";
                                poleTlacitek2[2].Text = "sans-serif-condensed";
                                poleTlacitek2[3].Text = "sans-serif-black";
                                poleTlacitek2[4].Text = "sans-serif-thin";
                                poleTlacitek2[5].Text = "sans-serif-medium";
                                //

                                f = 0;
                                
                                foreach (Button b in poleTlacitek2)
                                {                                    
                                    b.Id = f; // Jako identifikátor dosadí aktuální proměnou f
                                    poleTlacitek2[f].Click += (sender2, e2) =>
                                    {
                                        font = poleTlacitek2[b.Id].Text; // Po kliknutí na jakékoliv z tlačítek se do stringu font dosadí text tlačítka - název fontu - právě stisknutého tlačítka
                                    };
                                    f++;
                                }
                                //
                                break;

                            case 1:
                                f = 0;
                                foreach(Button b in poleTlacitek2)
                                {
                                    poleTlacitek2[f].Text = (5 * (f + 1)).ToString(); // Vypočítá číslo dle čísla aktuálního tlačítka (násobky pěti)
                                    b.Id = f; // Jako identifikátor dosadí aktuální proměnou f
                                    poleTlacitek2[f].Click += (sender2, e2) => {velikostPisma = poleTlacitek2[b.Id].Text; }; // Po kliknutí na jakékoliv z tlačítek se do stringu velikostPisma dosadí text tlačítka - velikost písma
                                    f++;
                                }
                                break;
                            case 2:
                                // Určí text tlačítek
                                poleTlacitek2[0].Text = "Červená";
                                poleTlacitek2[1].Text = "Zelená";
                                poleTlacitek2[2].Text = "Modrá";
                                poleTlacitek2[3].Text = "Fialová";
                                poleTlacitek2[4].Text = "Oranžová";
                                poleTlacitek2[5].Text = "Černá";
                                //

                                // Určí, co se má provést po kliknutí - po kliknutí se do stringu barvaPisma dosadí odpovídající HTML kód dané barvy
                                poleTlacitek2[0].Click += (sender2, e2) => { barvaPisma = "#FF0000"; };                                
                                poleTlacitek2[1].Click += (sender2, e2) => { barvaPisma = "#00FF00"; };                                
                                poleTlacitek2[2].Click += (sender2, e2) => { barvaPisma = "#0000FF"; };                               
                                poleTlacitek2[3].Click += (sender2, e2) => { barvaPisma = "#800080"; };                                
                                poleTlacitek2[4].Click += (sender2, e2) => { barvaPisma = "#FFA500"; };                                
                                poleTlacitek2[5].Click += (sender2, e2) => { barvaPisma = "#000000"; };
                                //

                                break;                               
                        }
                        int f3 = 0;

                        // Pokud se klikne na jakékoliv z daných tlačítek, tak se spustí metoda, která uloží obsah stringů do konfiguračního souboru
                        foreach (Button t2 in poleTlacitek2)
                        {
                            poleTlacitek2[f3].Click += (sender2, e2) =>
                            {
                                zapisDoKonfiguracnihoSouboru(font, velikostPisma, barvaPisma, fileName);
                                nastaveni(); // Spuštění metody nastaveni() znova, aby se aktualizovaly hodnoty ve všech EditText
                            };
                            f3++;
                        }
                        //                 
                        SetContentView(linearLayout2);
                    };
                    f++;
                }
                // Určí se text tlačítek
                poleTlacitek[0].Text = "Vybrat font";
                poleTlacitek[1].Text = "Vybrat velikost písma";
                poleTlacitek[2].Text = "Vybrat barvu písma";
                //                            
                SetContentView(linearLayout);               
        }

        protected override void OnCreate(Bundle bundle)
        {
            ActionBar.Hide(); // Skryje normálně viditelný horní panel s logem a názvem aplikace
            base.OnCreate(bundle); // Slouží k inicializaci aktivity, pro dočasné ukládání dat, zabraňuje ztrátě uživatelem zadávaných informací například při otočení zařízení            
            SetContentView(Resource.Layout.Main);       // Zobrazí hlavní layout

            LinearLayout linearLayout = new LinearLayout(this); // Vytvoření layoutu
            linearLayout.Orientation = Orientation.Vertical;  // Určí orientaci layoutu na vertikální - prvky se přidávají od shora dolů
            linearLayout.SetPadding(35, 15, 35, 15); // Určí odsazení
            linearLayout.SetGravity(GravityFlags.Center); // Určení zarovnání na střed

            TextView nazevAplikace = new TextView(this); // Vytvoří TextView
            nazevAplikace.Text = "Maturita 20XY"; // Určí název aplikace
            nazevAplikace.SetTextSize(Android.Util.ComplexUnitType.FractionParent, 33); // Určí velikost písma jako poměr velikosti obrazovky (layoutu)
            nazevAplikace.SetPadding(0, 0, 0, 80); // Určí odsazení
            linearLayout.AddView(nazevAplikace); // Přidá TextView do layoutu linearLayout

            Button[] poleTlacitek = new Button[7]; // Vytvoří pole 7 tlačítek
            int i = 0;

            // Pro každé tlačítko nastaví parametry a přidá ho do layoutu
            foreach (Button t in poleTlacitek)
            {
                poleTlacitek[i] = new Button(this);
                poleTlacitek[i].SetPadding(15, 15, 15, 15);
                linearLayout.AddView(poleTlacitek[i]);
                i++;
            } 
            //

            // Určí text tlačítek
            poleTlacitek[0].Text = "Volná díla";
            poleTlacitek[1].Text = "O aplikaci";
            poleTlacitek[2].Text = "Literární kvíz";
            poleTlacitek[3].Text = "Nastavení";
            poleTlacitek[4].Text = "Kánon literatury";
            poleTlacitek[5].Text = "WIKI";
            poleTlacitek[6].Text = "Poznámky";
            //
            // Určí jednotlivé akce a metody, které se mají spustit po kliknutí na každé z tlačítek
            poleTlacitek[0].Click += (sender, e) => { autori(); };
            poleTlacitek[1].Click += (sender, e) => { oAplikaci(); };          
            poleTlacitek[2].Click += (sender, e) => { literarniKviz(); };
            poleTlacitek[3].Click += (sender, e) => { nastaveni(); };
            poleTlacitek[4].Click += (sender, e) => { csv(); };
            poleTlacitek[5].Click += (sender, e) => // Zobrazí mobilní verzi české verze Wikipedie
            {
                string adresa = "https://cs.m.wikipedia.org/wiki/Hlavn%C3%AD_strana";
                WebView youtube = new WebView(this);
                youtube.LoadUrl(adresa);
                SetContentView(youtube);
            };        
            poleTlacitek[6].Click += (sender, e) => { poznamky(); };

            var path = System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData);
            string fileName = path + "/konfiguracniSoubor.txt"; // Určí adresu konfiguračního souboru
            
            if (!File.Exists(fileName)) // Pokud konfigurační soubor v tomto umístění ještě neexistuje, tak ho vezme z umístění Assets/konfiguracniSoubor.txt a předělá ho do úložiště telefonu, aby byla možná jeho následná úprava
            {             
                string font = "", velikostPisma = "", barvaPisma = ""; // Určí stringy
                String[] lines3;

                // Rozdělí na řádky
                using (StreamReader sr = new StreamReader(this.Assets.Open("konfiguracniSoubor.txt"), Encoding.Default))
                {
                    lines3 = sr.ReadToEnd().Split(new char[] { '\n' });
                }
                //

                // Vezme z každého řádku jeden parametr a ten dosadí do příslušného stringu
                font = lines3[0];
                velikostPisma = lines3[1];
                barvaPisma = lines3[2];
                zvyraznovani = lines3[3];
                //

                zapisDoKonfiguracnihoSouboru(font, velikostPisma, barvaPisma, fileName); // Spustí funkci pro zápis do konfiguračního souboru
            }            
            SetContentView(linearLayout);
        }
        public override void OnBackPressed()
        {
            OnCreate(null);
        }
    }
    // Vytvořil Šimon Horňák
}