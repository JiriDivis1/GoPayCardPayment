using GoPay.Common;
using GoPay.Model.Payment;
using GoPay.Model.Payments;
using Mono.TextTemplating;
using System.Text.RegularExpressions;
using static GoPay.Model.Payments.Payment;

namespace GoPayCardPayment.Models
{
    // Globální proměnné pro tuto aplikaci
    public class GlobalVariables
    {
        // true -> po spuštění se aplikace přesměruje na NGROK veřejnou URL (pokud je NGROK zapnutý -> Tools - Start ngrok Tunnel).
        // automaticky se nastaví returnURL a notificationURL
        public static bool usingNGROK = true;
        // Connection string k Postgres
        public static readonly string connectionString = "" +
            "Server=; port=; user id=; password=; database=";     
        public static readonly string sandboxURL = "https://gw.sandbox.gopay.com/api";      // URL pro sandbox                  
        public static readonly string goID = "8408381003";                      // Jedinečný identifikátor eshopu v systému platební brány
        public static readonly string secureKey = "z9dcyJnNuLW8Lh2VJGYAZWza";           
        public static readonly string clientID = "1625054156";                  // testovací Client ID
        public static readonly string clientSecret = "UTUJR4vs";                // testovací Client Secret
        public static readonly string userName = "testUser8408381003";
        public static readonly string password = "P2170798";
        public static string returnURL = "";                        // Adresa, na kterou bude zákazník přesměrován po opuštění platební brány
        public static string notificationURL = "";                  // Adresa, na kterou budou posílány notifikace o stavu prováděných plateb z GoPay

    }

    /*
    SELECT*
    FROM payment p1
    WHERE p1.State = 'PAID'
    AND NOT EXISTS (
        SELECT 1            // SELECT 1 znamená, že pouze chceme zjistit, zda nějaký záznam existuje
        FROM payment p2
        WHERE p2.paymentID = p1.paymentID
        AND p2.State = 'REFUNDED'
    );



    SELECT paymentID
    FROM payment
    GROUP BY paymentID
    HAVING SUM(CASE WHEN State = 'PAID' THEN 1 ELSE 0 END) > 0
    AND SUM(CASE WHEN State = 'REFUNDED' THEN 1 ELSE 0 END) = 0;
    */
}
