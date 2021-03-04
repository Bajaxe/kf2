using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;

namespace kf2
{
    class Program
    {
        static void Main(string[] args)
        {

            IWebDriver driver = new ChromeDriver();
            // logowanie
            driver.Navigate().GoToUrl("https://www.kf2.pl");
            driver.FindElement(By.Name("email")).SendKeys("las111@wp.pl");
            driver.FindElement(By.Name("password")).SendKeys("las12342");
            driver.FindElement(By.ClassName("submit-button")).Click();

            driver.FindElement(By.XPath("//div[contains(@onclick,'821')]")).Click();

            //expienie
            string level = driver.FindElement(By.Id("ajax_level")).Text;
            int lvl = int.Parse(level);

            while (lvl < 100)
            {
                driver.Navigate().GoToUrl("https://www.kf2.pl/podziemia");

                // select lvl
                var comboBox = driver.FindElement(By.Name("level"));
               // new SelectElement(comboBox).SelectByText("89");
                new SelectElement(comboBox).SelectByIndex(0);

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

                        IWebElement ele = driver.FindElement(By.XPath("//div[contains(@id,'hpBody')]"));
                        IJavaScriptExecutor executor = (IJavaScriptExecutor)driver;
                        executor.ExecuteScript("arguments[0].click();", ele);

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
