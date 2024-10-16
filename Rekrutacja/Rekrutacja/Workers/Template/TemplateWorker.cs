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
            public int a { get; set; }
            [Caption("B")]
            public int b { get; set; }
            [Caption("Operacja")]
            public string operacja { get; set; }
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
            /*Pracownik pracownicy = null;
            if (this.Cx.Contains(typeof(Pracownik)))
            {
                pracownicy = (Pracownik)this.Cx[typeof(Pracownik)];
            }*/
            var pracownicy = (Pracownik[])this.Cx[typeof(Pracownik[])] as IEnumerable<Pracownik>;
           /* if (!this.Cx.Contains(typeof(Pracownik)))
            {
                throw new InvalidOperationException("Brak zaznaczonych pracowników.");
            }*/
            

            if (pracownicy == null || !pracownicy.Any())
            {
                throw new InvalidOperationException("Nie znaleziono zaznaczonych pracowników.");
            }

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
            //Modyfikacja danych
            //Aby modyfikować dane musimy mieć otwartą sesję, któa nie jest read only
            using (Session nowaSesja = this.Cx.Login.CreateSession(false, false, "ModyfikacjaPracownika"))
            {
                //Otwieramy Transaction aby można było edytować obiekt z sesji
                using (ITransaction trans = nowaSesja.Logout(true))
                {
                    //Pobieramy obiekt z Nowo utworzonej sesji
                   // var pracownikZSesja = nowaSesja.Get(pracownik);
                    //Features - są to pola rozszerzające obiekty w bazie danych, dzięki czemu nie jestesmy ogarniczeni to kolumn jakie zostały utworzone przez producenta
                    //pracownikZSesja.Features["DataObliczen"] = this.Parametry.DataObliczen;
                    //Zatwierdzamy zmiany wykonane w sesji
                    
                    foreach (var pracownik in pracownicy)
                    {
                        var pracownikZSesji = nowaSesja.Get(pracownik);

                        // Zapisujemy wynik w polu "Wynik"
                        pracownikZSesji.Features["Wynik"] = wynik;

                        // Zapisujemy datę obliczeń w polu "DataObliczen"
                        pracownikZSesji.Features["DataObliczen"] = this.Parametry.DataObliczen;
                    }
                    trans.CommitUI();
                }
                //Zapisujemy zmiany
                nowaSesja.Save();
            }
        }
    }
}