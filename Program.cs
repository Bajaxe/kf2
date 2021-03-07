using System;
using System.Linq;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Collections.Generic;


namespace kf2
{
    //  logic for selecting specific option
    public class Menu
    {
        public Menu(IEnumerable<string> items)
        {
            Items = items.ToArray();
        }


        public IReadOnlyList<string> Items { get; }

        public int SelectedIndex { get; private set; } = -1; // nothing selected

        public string SelectedOption => SelectedIndex != -1 ? Items[SelectedIndex] : null;


        public void MoveUp() => SelectedIndex = Math.Max(SelectedIndex - 1, 0);

        public void MoveDown() => SelectedIndex = Math.Min(SelectedIndex + 1, Items.Count - 1);
    }


    // logic for drawing menu list
    public class ConsoleMenuPainter
    {
        readonly Menu menu;

        public ConsoleMenuPainter(Menu menu)
        {
            this.menu = menu;
        }

        public void Paint(int x, int y)
        {
            for (int i = 0; i < menu.Items.Count; i++)
            {
                Console.SetCursorPosition(x, y + i);

                var color = menu.SelectedIndex == i ? ConsoleColor.Yellow : ConsoleColor.Gray;

                Console.ForegroundColor = color;
                Console.WriteLine(menu.Items[i]);
            }
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var Data = ConfigurationManager.AppSettings;

            string login = Data.Get("Key0");
            string password = Data.Get("Key1");
            int LvlGoal = int.Parse(Data.Get("Key3"));
            string CurrentChar = Data.Get("Key2");
            ReadOnlyCollection<IWebElement> chars;

            Console.WriteLine("Type your login(e-mail):");
            Console.WriteLine(login + "? Press Enter or type new.");
            login = Console.ReadLine();
            if (login.Equals("")) login = Data.Get("Key0");

            //string NewLogin = Console.ReadLine(login);

            Console.WriteLine("Your password:");
            password = Console.ReadLine();

            Console.WriteLine("Do you want to save credentials? Y/N  Press Enter to skip");
            
            string CredentChck = Console.ReadLine();

            bool saveChck = false;

            if (CredentChck.Equals("Y"))
                saveChck = true;

            else if (CredentChck.Equals("yes"))
                saveChck = true;

            else
                saveChck = false;
            
            IWebDriver driver = new ChromeDriver();
            // logowanie
            driver.Navigate().GoToUrl("https://www.kf2.pl");
            driver.FindElement(By.Name("email")).SendKeys(login);
            driver.FindElement(By.Name("password")).SendKeys(password);
            driver.FindElement(By.ClassName("submit-button")).Click();

            //save credentials
            if (saveChck.Equals(true))
                Data.Set("Key0", login);
                Data.Set("Key1", password);

            //get account data
            chars = driver.FindElements(By.XPath("//div[@class = 'charName']"));
            Console.Clear();
            Console.WriteLine("Characters avaiable:");

            
            List<string> charlist = new List<string>();
            for (int N = 0;N <= chars.Count-3; N++ ) // -3 > new char; creat char; count from 0
            {
               charlist.Add (N + 1 + ". " + chars[N].Text);       
            }

            //stwórz menu wyboru
            var menu = new Menu(charlist);
            var menuPainter = new ConsoleMenuPainter(menu);
            bool done = false;

            do
            {
                menuPainter.Paint(8, 5);

                var keyInfo = Console.ReadKey();

                switch (keyInfo.Key)
                {
                    case ConsoleKey.UpArrow: menu.MoveUp(); break;
                    case ConsoleKey.DownArrow: menu.MoveDown(); break;
                    case ConsoleKey.Enter: done = true; break;
                }
            }
            while (!done);

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("Selected option: " + (menu.SelectedOption ?? "(nothing)"));

            CurrentChar = menu.SelectedOption;

            string[] tempCharID = CurrentChar.Split('[');
            string CharID = tempCharID[1].Remove(tempCharID[1].Length - 1, 1);

            driver.FindElement(By.XPath("//div[contains(@onclick,'"+ CharID +"')]")).Click();
            //driver.FindElement(By.XPath("//div[contains(text()'Roan')]")).Click(); not working ;c
            //driver.FindElement(By.XPath("//div[text()'" + CurrentChar + "']")).Click();
            Data.Set("Key2", CurrentChar); //save last used char
            //  driver.FindElement(By.XPath("//div[contains(@onclick,'821')]")).Click();

            //expienie
            string level = driver.FindElement(By.Id("ajax_level")).Text;
            int lvl = int.Parse(level);

            Console.WriteLine("Default level to be achived is 100. Go to Settings if you need to adjust.");

            while (lvl < LvlGoal)
            {
                driver.Navigate().GoToUrl("https://www.kf2.pl/podziemia");

                // select lvl
                var comboBox = driver.FindElement(By.Name("level"));
                try
                {
                    new SelectElement(comboBox).SelectByText("89"); //best lvl to use
                }
                catch 
                {
                    new SelectElement(comboBox).SelectByIndex(0); //if not avaiable go for highest one
                }
          
                {
                    //zacznij walke
                    driver.FindElement(By.XPath("//input[contains(@value,'Zbadaj')]")).Click();

                    // poczekaj aż załaduje skille
                    DefaultWait<IWebDriver> fluentWait = new DefaultWait<IWebDriver>(driver);
                    fluentWait.Timeout = TimeSpan.FromSeconds(5);
                    fluentWait.PollingInterval = TimeSpan.FromMilliseconds(250);
                    /* Ignore the exception - NoSuchElementException that indicates that the element is not present */
                    fluentWait.IgnoreExceptionTypes(typeof(NoSuchElementException));
                    fluentWait.Message = "Element to be searched not found";

                    WebDriverWait wait1 = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
                    IWebElement element1 = wait1.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(By.XPath("//img[contains(@ref,'obszarowe')]")));
                    element1.GetHashCode();

                    //counter skilli
       
                    int pics = driver.FindElements(By.XPath("//div[@id = 'fight_rightside']//div[@class = 'fighter_actions']/img[2]")).Count;
                    Console.WriteLine(pics);

                    //golem
                    driver.FindElement(By.XPath("//img[contains(@ref,'nieumarłego')]")).Click();

                    while (pics != 0)
                    {

                        try
                        {
                            driver.FindElement(By.XPath("//img[contains(@ref,'obszarowe')]")).Click();
                        }
                        catch (OpenQA.Selenium.StaleElementReferenceException)
                        {
                           
                        }

                        catch (OpenQA.Selenium.NoSuchElementException)
                        {
                           
                        }

                        //select hit target
                        IWebElement ele = driver.FindElement(By.XPath("//div[contains(@id,'hpBody')]"));
                        IJavaScriptExecutor executor = (IJavaScriptExecutor)driver;
                        executor.ExecuteScript("arguments[0].click();", ele);

                        //count avaiable enemies again
                        pics = driver.FindElements(By.XPath("//div[@id = 'fight_rightside']//div[@class = 'fighter_actions']/img[2]")).Count;
                    }

                    //koniec walki

                    try
                    {
                        driver.FindElement(By.ClassName("end-fight")).Click();
                    }
                    catch (OpenQA.Selenium.ElementClickInterceptedException)
                    {
                        driver.Navigate().GoToUrl("https://www.kf2.pl/podziemia");

                    }

                    //hp check
                    String visibility = driver.FindElement(By.XPath("//div[@id = 'main_hp_bar_data']")).Text;
                    Console.WriteLine(visibility);

                    var HP = visibility.Split('/').Select(x => int.Parse(x)).ToList();
                    // Convert.ToInt32(HP);

                    if (HP[0] < HP[1])
                    {
                        driver.Navigate().GoToUrl("https://www.kf2.pl/lokacja/zobacz/lecznica");
                        driver.FindElement(By.XPath("//div[contains(@onclick,'healMe')]")).Click();
                    }

                }
            }
           
        }
    }
}
