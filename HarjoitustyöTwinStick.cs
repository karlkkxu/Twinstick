using System;
using System.Collections.Generic;
using Jypeli;
using Jypeli.Assets;
using Jypeli.Controls;
using Jypeli.Widgets;

// @author Kim Karlsson
// @version 9.12.19 'naytto'


public class HarjoitustyöTwinStick : PhysicsGame
{

    /// <summary>
    /// Physicsobject jolla on määritelty määrä elämää, tuhoutuu kun elamaLaskuri = 0
    /// </summary>
    public class Alus : PhysicsObject
    {
        public Timer ammu = new Timer();
        public IntMeter elamaLaskuri = new IntMeter(0, 0, 10);
        public IntMeter ElamaLaskuri { get { return elamaLaskuri; } }

        public Alus(double leveys, double korkeus, int elama, int speed, int aivotyyppi, double ampuuko)
            : base(leveys, korkeus)
        {

            Shape = Shape.Triangle;
            // Color = RandomGen.NextColor();
            Color = Color.Red;
            elamaLaskuri.AddValue(elama);
            elamaLaskuri.LowerLimit += delegate { this.Destroy(); };
            CollisionIgnoreGroup = 2;

            if (ampuuko > 0)
                ammu.Interval = ampuuko;

            ammu.Start();

            elamaLaskuri.LowerLimit += delegate { ammu.Stop(); };

            switch (aivotyyppi)
            {
                case 1:
                    RandomMoverBrain randomAivot = new RandomMoverBrain(speed)
                    {
                        Active = true,
                        TurnWhileMoving = true,
                        ChangeMovementSeconds = 3
                    };
                    Brain = randomAivot;
                    break;
            }

        }

    }


    /// <summary>
    /// Physicsobject jolle on määritelty puoli, puoli otetaan huomioon törmäystilanteissa
    /// </summary>
    class Luoti : PhysicsObject
    {

        public Luoti(string puoli)
            : base(10, 10)
        {
            Shape = Shape.Circle;
            Mass = 0.01;
            if (puoli == "pelaaja")
                Color = Color.Purple;
            else
                // Color = RandomGen.NextColor();
                Color = Color.Red;

            if (puoli == "pelaaja")
                CollisionIgnoreGroup = 1;
            else
                CollisionIgnoreGroup = 2;

            LifetimeLeft = TimeSpan.FromSeconds(3.0);
        }

    }


    public int kierrosmaara;
    /// <summary>
    /// Luo ohjelman ikkunalle koon, kentälle reunat (esteettisistä syistä) ja antaa pelaajalle mahdollisuuden aloittaa
    /// uuden pelin (AloitaPeli) tai lopettavan ohjelman
    /// </summary>
    public override void Begin()
    {
        SetWindowSize(1024, 768, false);
        Level.CreateBorders();

        MultiSelectWindow alkuValikko = new MultiSelectWindow("",
"Aloita peli", "Ohjeet", "Lopeta");
        Add(alkuValikko);
        alkuValikko.AddItemHandler(0, AloitaPeli);
        alkuValikko.AddItemHandler(1, DisplayOhjeet);
        alkuValikko.AddItemHandler(2, Exit);

    }


    /// <summary>
    /// Näyttää pelaajalle ohjeet miten peliä ajetaan
    /// </summary>
    private void DisplayOhjeet()
    {

        PeliLoppu();
        MessageWindow ikkuna = new MessageWindow("Avaruusaluksesi on kaapattu alieneiden toimesta, ja sinut pakotetaan taistelemaan hengestäsi heidän " +
            "loppumattomalla areenallaan!" +
            "\n" +
            "\n" +
            "Liiku WASD-näppäimillä ja ammu välilyönnillä" +
            "\n" +
            "Voit kutsua lisää vihollisia Enterillä" +
            "\n" +
            "\n" +
            "Karataksesi areenasta ja voittaaksesi pelin, pelaa niin kauan että pelin tekijä jaksaa ohjelmoida kunnollisen lopun pelille");
        Add(ikkuna);


    }


    /// <summary>
    /// Luo pelin ikkunan ja kentän, kutsuu sinne pelaajan aluksen, pistelaskurin, elämäpalkin, ja alustaa kierrosmäärän
    /// </summary>
    private void AloitaPeli()
    {
        ClearAll();
        IsPaused = false;
        Level.CreateBorders();
        LuoPistelaskuri();

        kierrosmaara = 1;

        Alus pelaajaAlus = CreatePlayerShip();
        CheckPlayerMovement(pelaajaAlus);
        CheckPlayerHealth(pelaajaAlus);

        Level.CreateBorders();
    }


    IntMeter pisteLaskuri;
    /// <summary>
    /// Luo uuden labelin ja sitoo sen pistelaskuriin
    /// </summary>
    void LuoPistelaskuri()
    {
        pisteLaskuri = new IntMeter(0);
        pisteLaskuri.Value = 0;

        Label pisteNaytto = new Label
        {
            X = 350,
            Y = 350,
            TextColor = Color.Black,
            Color = Color.White
        };

        pisteNaytto.BindTo(pisteLaskuri);
        Add(pisteNaytto);
    }


    /// <summary>
    /// Luo elämäpalkin taulukosta pelaajan elämien perusteella
    /// </summary>
    /// <param name="pelaajaAlus">Pelaajan alus</param>
    public void CheckPlayerHealth(Alus pelaajaAlus)
    {
        GameObject[] elamaPalkki = new GameObject[11];

        for (int i = pelaajaAlus.elamaLaskuri; i <= pelaajaAlus.elamaLaskuri.MaxValue; i++)
        {
            elamaPalkki[i] = new GameObject(10, 10)
            {
                Shape = Shape.Rectangle,
                Color = Color.Red,
                X = -100 + i * 10,
                Y = 350
            };

            Add(elamaPalkki[i]);
        }

        for (int i = 1; i <= pelaajaAlus.elamaLaskuri; i++)
        {
            elamaPalkki[i] = new GameObject(10, 10)
            {
                Shape = Shape.Rectangle,
                Color = Color.Green,
                X = -100 + i * 10,
                Y = 350
            };

            Add(elamaPalkki[i]);
        }

        if (pelaajaAlus.IsDestroyed)
            PeliLoppu();
        
    }


    /// <summary>
    /// Pysäyttää pelin toiminnan ja antaa vaihtoehtoina aloittaa kentän alusta (AloitaPeli) tai lopettaa ohjelman
    /// </summary>
    private void PeliLoppu()
    {
        IsPaused = true;
        MultiSelectWindow kuolinValikko = new MultiSelectWindow("",
"Aloita uusi peli", "Ohjeet", "Lopeta");
        Add(kuolinValikko);
        kuolinValikko.AddItemHandler(0, AloitaPeli);
        kuolinValikko.AddItemHandler(1, DisplayOhjeet);
        kuolinValikko.AddItemHandler(2, Exit);
    }


    /// <summary>
    /// Laskee kierrosten lukumäärästä seuraavalle kierrokselle sopivan vaikeustason ja luo sen perusteella vihollisia
    /// </summary>
    void UusiKierros()
    {

        int vaikeustaso = kierrosmaara * 100;
        int yhteisvaikeus = 0;

        while (yhteisvaikeus < vaikeustaso)
        {
            yhteisvaikeus += Convert.ToInt32(LuoVihollinen());
        }

        kierrosmaara++;

    }


    /// <summary>
    /// Generoi yhden vihollisen ominaisuudet, lisää sen kentälle ja palauttaa sen vaikeusarvon. Lisää vaikeusarvon pisteisiin
    /// </summary>
    /// <param name="vaikeustaso">Laskettu kierrosten lukumäärästä, toimii kattona vihollisten määrälle ja vaikeudelle</param>
    /// <returns></returns>
    private double LuoVihollinen()
    {

        int vihunElama = RandomGen.NextInt(1, 5);

        double ampuuko;
        if (RandomGen.NextBool() == true)
            ampuuko = RandomGen.NextDouble(0.5, 2.0);
        else
            ampuuko = 0;

        int speed = RandomGen.NextInt(50, 300);

        int brainType = RandomGen.NextInt(1, 1);

        double vihunVaikeus = vihunElama * 10 + ampuuko * 100 + speed;
        double vihuScore = vihunVaikeus;

        Alus vihu = new Alus(25 * vihunElama * 0.8, 25 * vihunElama * 0.8, vihunElama, speed, brainType, ampuuko);
        {
            vihu.X = RandomGen.NextInt(-300, 300);
            if (RandomGen.NextBool() == true)
                vihu.Y = 300;
            else
                vihu.Y = -300;
            vihu.Tag = "vihu";
        }

        vihu.ammu.Timeout += delegate { LuoLuoti(vihu, "vihu"); };
            
        pisteLaskuri.AddOverTime(Convert.ToInt32(vihuScore), 3);

        Add(vihu);
        return vihunVaikeus;

    }


    /// <summary>
    /// Kuuntelee näppäimistöä ja suorittaa pelaajan aluksen liikekomennot
    /// </summary>
    /// <param name="pelaajaAlus">Pelaajan kontrolloima alus</param>
    private void CheckPlayerMovement(PhysicsObject pelaajaAlus)
    {
        Keyboard.Listen(Key.W, ButtonState.Down, Liiku, null, pelaajaAlus, 1000, false);
        Keyboard.Listen(Key.S, ButtonState.Down, Liiku, null, pelaajaAlus, -100, true);
        Keyboard.Listen(Key.D, ButtonState.Down, Kaanna, null, pelaajaAlus, 5);
        Keyboard.Listen(Key.A, ButtonState.Down, Kaanna, null, pelaajaAlus, -5);
        Keyboard.Listen(Key.Space, ButtonState.Pressed, LuoLuoti, null, pelaajaAlus, "pelaaja");
        Keyboard.Listen(Key.Enter, ButtonState.Pressed, UusiKierros, null);
        Keyboard.Listen(Key.Escape, ButtonState.Pressed, ConfirmExit, "Lopeta peli");
    }


    /// <summary>
    /// Luo kentälle parametrina tuodusta aluksesta luodin joka voi tuhota vastakkaisia aluksia
    /// </summary>
    /// <param name="alus">Mistä aluksesta luoti luodaan</param>
    /// <param name="puoli">Onko luoti "pelaaja"n vai vihollisten luoma</param>
    public void LuoLuoti(PhysicsObject alus, string puoli)
    {

        Luoti luoti = new Luoti(puoli)
        {
            X = alus.X,
            Y = alus.Y,
            Velocity = 500 * Vector.FromAngle(alus.Angle)
            
        };

        AddCollisionHandler<Luoti, Alus>(luoti, LuotiOsui);
        Add(luoti);
    }


    /// <summary>
    /// Collision Handler luodille ja alukselle, vahingoittaa alusta ja tuhoaa luodin
    /// </summary>
    /// <param name="collidingObject"></param>
    /// <param name="otherObject"></param>
    private void LuotiOsui(Luoti collidingObject, Alus otherObject)
    {
        otherObject.elamaLaskuri.AddValue(-1);
        collidingObject.Destroy();
        if (otherObject.Tag == "pelaaja")
            CheckPlayerHealth(otherObject);
    }


    /// <summary>
    /// Työntää pelaajan alusta joko eteen, tai taaksepäin. Jos taaksepäin, lopettaa myös kaiken pyörimisliikkeen
    /// </summary>
    /// <param name="pelaajaAlus">Pelaajan alus</param>
    /// <param name="suunta">Positiivisella eteenpäin</param>
    /// <param name="jarru">Pysäyttääkö pyörimisliikkeen</param>
    private void Liiku(PhysicsObject pelaajaAlus, int suunta, bool jarru)
    {
        Vector pelaajanSuunta = Vector.FromLengthAndAngle(suunta, pelaajaAlus.Angle);
        pelaajaAlus.Push(pelaajanSuunta);
        if (jarru) 
            pelaajaAlus.StopAngular();
    }


    /// <summary>
    /// Muuttaa pelaajan aluksen kulmaa
    /// </summary>
    /// <param name="pelaajaAlus">Pelaajan alus</param>
    /// <param name="suunta">Kääntyykö vasemmalle vai oikealle</param>
    private void Kaanna(PhysicsObject pelaajaAlus, int suunta)
    {
        pelaajaAlus.Angle += Angle.FromDegrees(suunta);
    }


    /// <summary>
    /// Määrittelee pelaajan aluksen parametrit ja luo sen kentälle
    /// </summary>
    /// <returns>Pelaajan alus</returns>
    public Alus CreatePlayerShip()
    {
        int x = 90;
        int y = 40;
        int elama = 10;

        Alus pelaajaAlus = new Alus(x, y, elama, 0, -1, 0)
        {
            Color = Color.Purple,
            Shape = Shape.Pentagon,
            Angle = Angle.FromDegrees(90),
            Mass = 1,
            Restitution = 0.5,
            LinearDamping = 0.97,
            CollisionIgnoreGroup = 1,
            Tag = "pelaaja"
        };

        AddCollisionHandler<Alus, Alus>(pelaajaAlus, "vihu", AlusTormays);
        Add(pelaajaAlus);

        return pelaajaAlus;
    }


    /// <summary>
    /// Tuhoaa vihollisen aluksen ja vähentää sen elämämäärän pelaajan aluksesta
    /// </summary>
    /// <param name="pelaajaAlus">Pelaajan alus</param>
    /// <param name="vihuAlus">Vihollisen alus</param>
    private void AlusTormays(Alus pelaajaAlus, Alus vihuAlus)
    {
        pelaajaAlus.elamaLaskuri.AddValue(-1 * vihuAlus.elamaLaskuri);
        CheckPlayerHealth(pelaajaAlus);
        vihuAlus.elamaLaskuri.AddValue(-1 * vihuAlus.elamaLaskuri);
    }
}
