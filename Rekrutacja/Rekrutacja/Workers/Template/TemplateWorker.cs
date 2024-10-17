using Soneta.Business;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Soneta.Kadry;
using Soneta.KadryPlace;
using Soneta.Types;
using Rekrutacja.Workers.Template;

//Rejetracja Workera - Pierwszy TypeOf określa jakiego typu ma być wyświetlany Worker, Drugi parametr wskazuje na jakim Typie obiektów będzie wyświetlany Worker
[assembly: Worker(typeof(TemplateWorker), typeof(Pracownicy))]
namespace Rekrutacja.Workers.Template
{
    public class TemplateWorker
    {
        //Aby parametry działały prawidłowo dziedziczymy po klasie ContextBase
        public class TemplateWorkerParametry : ContextBase
        {
            [Caption("Data obliczeń")]
            public Date DataObliczen { get; set; }
            [Caption("A")]
            public string a { get; set; }
            [Caption("B")]
            public string b { get; set; }
            [Caption("Operacja")]
            //public string operacja { get; set; }
            public enum figura
            {
                kwadrat,
                prostokat,
                kolo,
                trojkat
            }
            [Caption("Figura")]
            public figura WybranaFigura { get; set; } 
            
            public TemplateWorkerParametry(Context context) : base(context)
            {
                this.DataObliczen = Date.Today;
            }
        }
        //Obiekt Context jest to pudełko które przechowuje Typy danych, aktualnie załadowane w aplikacji
        //Atrybut Context pobiera z "Contextu" obiekty które aktualnie widzimy na ekranie
        [Context]
        public Context Cx { get; set; }
        //Pobieramy z Contextu parametry, jeżeli nie ma w Context Parametrów mechanizm sam utworzy nowy obiekt oraz wyświetli jego formatkę
        [Context]
        public TemplateWorkerParametry Parametry { get; set; }
        //Atrybut Action - Wywołuje nam metodę która znajduje się poniżej
        [Action("Kalkulator",
           Description = "Prosty kalkulator ",
           Priority = 10,
           Mode = ActionMode.ReadOnlySession,
           Icon = ActionIcon.Accept,
           Target = ActionTarget.ToolbarWithText)]
        public void WykonajAkcje()
        {
            //Włączenie Debug, aby działał należy wygenerować DLL w trybie DEBUG
            DebuggerSession.MarkLineAsBreakPoint();
            //Pobieranie danych z Contextu
            var pracownicy = (Pracownik[])this.Cx[typeof(Pracownik[])] as IEnumerable<Pracownik>;
           
            if (pracownicy == null || !pracownicy.Any())
            {
                throw new InvalidOperationException("Nie znaleziono zaznaczonych pracowników.");
            }

            // double wynik = wynikKalkulator();
            int a = stringToInt(this.Parametry.a);
            int b = stringToInt(this.Parametry.b);
            double polePowierzchni = obliczPolePowierzchni(a,b);
            
            //Modyfikacja danych
            //Aby modyfikować dane musimy mieć otwartą sesję, któa nie jest read only
            using (Session nowaSesja = this.Cx.Login.CreateSession(false, false, "ModyfikacjaPracownika"))
            {
                //Otwieramy Transaction aby można było edytować obiekt z sesji
                using (ITransaction trans = nowaSesja.Logout(true))
                {
                    //Pobieramy obiekt z Nowo utworzonej sesji

                    //Features - są to pola rozszerzające obiekty w bazie danych, dzięki czemu nie jestesmy ogarniczeni to kolumn jakie zostały utworzone przez producenta
                  
                    foreach (var pracownik in pracownicy)
                    {
                        var pracownikZSesji = nowaSesja.Get(pracownik);

                        //Zapisujemy wynik w polu "Wynik"
                        //pracownikZSesji.Features["Wynik"] = wynik;
                        pracownikZSesji.Features["Wynik"] = polePowierzchni;

                        // Zapisujemy datę obliczeń w polu "DataObliczen"
                        pracownikZSesji.Features["DataObliczen"] = this.Parametry.DataObliczen;
                    }
                    //Zatwierdzamy zmiany wykonane w sesji
                    trans.CommitUI();
                }
                //Zapisujemy zmiany
                nowaSesja.Save();
            }
        }

        private double obliczPolePowierzchni(int a, int b)
        {
            double pole = 0;
            switch (this.Parametry.WybranaFigura)
            {
                case TemplateWorkerParametry.figura.kolo:
                    pole = (int)(Math.PI * a * a);
                    break;
                case TemplateWorkerParametry.figura.trojkat:
                    pole = (a * b)/2;
                    break;
                case TemplateWorkerParametry.figura.prostokat:
                    pole = a * b;
                    break;
                case TemplateWorkerParametry.figura.kwadrat:
                    pole = a * a;
                    break;
                default:
                    throw new InvalidOperationException("Nieprawidłowa operacja.");
            }


            return pole;
        }

        /*public double wynikKalkulator()
        {
            double wynik = 0;
            switch (this.Parametry.operacja)
            {
                case "+":
                    wynik = this.Parametry.a + this.Parametry.b;
                    break;
                case "-":
                    wynik = this.Parametry.a - this.Parametry.b;
                    break;
                case "*":
                    wynik = this.Parametry.a * this.Parametry.b;
                    break;
                case "/":
                    if (this.Parametry.b == 0)
                        throw new DivideByZeroException("Nie można dzielić przez zero.");
                    wynik = (double)this.Parametry.a / this.Parametry.b;
                    break;
                default:
                    throw new InvalidOperationException("Nieprawidłowa operacja.");
            }
            return wynik;
        }*/

        public int stringToInt(string value)
        {
            if (string.IsNullOrEmpty(value))
                throw new ArgumentException("String nie może być pusty");

            bool isNegative = false;
            int result = 0;

            int startIndex = 0;
            if (value[0] == '-')
            {
                isNegative = true;
                startIndex = 1; 
            }

            for (int i = startIndex; i < value.Length; i++)
            {
                char currentChar = value[i];

                if (currentChar < '0' || currentChar > '9')
                    throw new FormatException($"Niepoprawny znak '{currentChar}'.");

                
                int digitValue = currentChar - '0'; 

         
                result = result * 10 + digitValue;
            }

            if (isNegative)
            {
                return -result;
            }
            else
            {
                return result;
            }
        }
    }
}