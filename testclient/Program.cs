using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text;
using System.Web;
using Newtonsoft.Json;
using NUnit.Framework;
using System.Collections.Generic;

namespace testclient
{
    [TestFixture]
    public class Program
    {
        //public Dictionary<string,object> DICT;
        [TestCase]
        public static async Task tc1()
        {
            using var client = new HttpClient();
            Console.WriteLine("tc1 find pets not in store ...");
            //query pending
            var builder = new UriBuilder("https://petstore.swagger.io/v2/pet/findByStatus");
            var query = HttpUtility.ParseQueryString(builder.Query);
            query["status"] = "pending";
            builder.Query = query.ToString();
            builder.Port = -1;
            string url = builder.ToString();
            var response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();
            var responseBody = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            //get pet by id
            var dictl = JsonConvert.DeserializeObject<List<Dictionary<string,object>>>(responseBody);            
            Console.WriteLine("Pets not in store by id [pending]:");
            foreach (var dl in dictl) {
                builder = new UriBuilder("https://petstore.swagger.io/v2/pet/"+dl["id"]);
                builder.Port = -1;
                url = builder.ToString();
                try {
                    response = await client.GetAsync(url);
                }
                catch {
                }
                finally {
                    Console.WriteLine(dl["id"]);
                    Assert.That(false);
                }
            }

           //query sold
            query["status"] = "sold";
            builder.Query = query.ToString();
            builder.Port = -1;
            url = builder.ToString();
            response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();
            responseBody = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            //get pet by id
            var d = JsonConvert.DeserializeObject<Dictionary<string,object>>(responseBody);            
            Console.WriteLine("Pets not in store by id [sold]:");
            builder = new UriBuilder("https://petstore.swagger.io/v2/pet/"+d["id"]);
            builder.Port = -1;
            url = builder.ToString();
            try {
                response = await client.GetAsync(url);
            }
            catch {
            }
            finally {
                Console.WriteLine(d["id"]);
                Assert.That(false);                    
            }

            Assert.That(true);
        }

        [TestCase]
        public static async Task tc2()
        {
            Console.WriteLine("tc2 Add a new(unique) pet to the store ...");
            using var client = new HttpClient();            
            //query 0 pet from list
            var builder = new UriBuilder("https://petstore.swagger.io/v2/pet/findByStatus");
            var query = HttpUtility.ParseQueryString(builder.Query);
            query["status"] = "available";
            builder.Query = query.ToString();
            builder.Port = -1;
            string url = builder.ToString();
            var response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();
            var responseBody = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            
            //change a bit exiting record and POST it back
            var dictl = JsonConvert.DeserializeObject<List<Dictionary<string,object>>>(responseBody);  
            //Console.WriteLine(dictl[0]);
            builder = new UriBuilder("https://petstore.swagger.io/v2/pet");
            Random random = new Random();
            Console.WriteLine("Change existing record");            
            dictl[0]["name"] = dictl[0]["name"] + random.ToString(); // now differ from original should be added new unique pet record
            var json = JsonConvert.SerializeObject(dictl[0]);
            //Console.WriteLine(json);
            builder.Port = -1;
            url = builder.ToString();
            response = await client.PostAsync(url, new StringContent(json, Encoding.UTF8, "application/json"));
            if (response.IsSuccessStatusCode){       
                Console.WriteLine("Pet posted");
            }
            else{ 
                Console.WriteLine("Pet not posted");               
            }
            
            //check: get pet by id
            Console.WriteLine("Checking POSTed");
            builder = new UriBuilder("https://petstore.swagger.io/v2/pet/"+dictl[0]["id"]);
            builder.Port = -1;
            url = builder.ToString();
            response = await client.GetAsync(url);
            if (response.IsSuccessStatusCode){       
                Console.WriteLine("Get OK");                
            }
            else{ 
                Console.WriteLine("Get not OK");                               
            }

            responseBody = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            Console.WriteLine(responseBody);

            Assert.That(false);
            
        }

        [TestCase]
        public static async Task tc3()
        {         
            Console.WriteLine("tc3 Negative scenario of updating a pet ...");
            using var client = new HttpClient();            
            //query 0 pet from list
            var builder = new UriBuilder("https://petstore.swagger.io/v2/pet/findByStatus");
            var query = HttpUtility.ParseQueryString(builder.Query);
            query["status"] = "available";
            builder.Query = query.ToString();
            builder.Port = -1;
            string url = builder.ToString();
            var response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();
            var responseBody = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            
            //spoil exiting record and PUT it back
            var dictl = JsonConvert.DeserializeObject<List<Dictionary<string,object>>>(responseBody);  
            //Console.WriteLine(dictl[0]);
            builder = new UriBuilder("https://petstore.swagger.io/v2/pet");
            Console.WriteLine("Spoil existing record");            
            dictl[0]["tags"] = "[]"; 
            var json = JsonConvert.SerializeObject(dictl[0]);
            Console.WriteLine(json);
            builder.Port = -1;
            url = builder.ToString();
            response = await client.PutAsync(url, new StringContent(json, Encoding.UTF8, "application/json"));
            if (response.IsSuccessStatusCode){       
                Console.WriteLine("Pet updated");
            }
            else{ 
                Console.WriteLine("Pet not updated");               
            }        

            Assert.That(false);           
        }        
    }
}
