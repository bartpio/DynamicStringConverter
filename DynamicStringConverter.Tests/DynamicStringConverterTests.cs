using Microsoft.VisualStudio.TestTools.UnitTesting;

using DynamicStringConverter;
using System.Linq;
using System.ComponentModel;
using System.Collections.Generic;
using System;
using System.Globalization;

namespace DynamicStringConverter.Tests
{
    /// <summary>
    /// dynamic string converter tests
    /// </summary>
    [TestClass]
    public class DynamicStringConverterTests
    {
        /// <summary>
        /// simple dyn text
        /// </summary>
        [TestMethod]
        public void Simpledyntest()
        {
            var numbers = Enumerable.Range(0, 5000);
            var pairs = numbers.ToDictionary(x => "K" + x.ToString(), x =>  x.ToString());
            dynamic ds = new DynamicStrings(pairs);
            var coo = (int)ds.K999;
            Assert.IsTrue(coo.GetType() == typeof(int));  //A needless tautology
            Assert.AreEqual(999, coo);
        }


        /// <summary>
        /// test casting for what it's worth
        /// </summary>
        [TestMethod]
        public void Castertest()
        {
            var numbers = Enumerable.Range(0, 5000);
            var pairs = numbers.ToDictionary(x => "K" + x.ToString(), x => x.ToString());
            dynamic ds = new DynamicStrings(pairs);
            var coo = (int)ds.K999;
            Assert.AreEqual(999, coo);

            IDictionary<string, string> id = ds;
            DynamicStrings dss = ds;

            Assert.IsTrue(id.ContainsKey("K111"), "we should have had key 111");
            Assert.ThrowsException<NotSupportedException>(() =>
            {
                //we don't support mutability etc.
                id.Add("somekey", "someval");
            });

            var memnames = dss.GetDynamicMemberNames().ToDictionary(x => x, x => true);
            Assert.AreEqual(pairs.Count, memnames.Count, "count of GetDynamicMemberNames");
        }

        /// <summary>
        /// test various types
        /// includes DateTimeOffsetConverter special case
        /// </summary>
        [TestMethod]
        public void Multidyntest()
        {
            var utcnow = DateTime.UtcNow;
            var dto = DateTimeOffset.UtcNow;

            var pairs = new Dictionary<string, string>()
            {
                ["Someint"] = "123456789",
                ["Somestring"] = "yes just a string",
                ["Somedate"] = utcnow.ToString("o"),
                ["Somedto"] = dto.ToString(),
                ["Somebool"] = "TruE",
                ["Somenull"] = null,
                ["Anotherint"] = "123"
            };

            dynamic ds = new DynamicStrings(pairs,
                StringComparer.OrdinalIgnoreCase,  //let's ignore case
                new TypeConverter[] { new DateTimeOffsetConverter() }); //datetimeoffset seems to need an explicit converter


            // vv Here are the casts from dynamic vv \\
            int someint = ds.Someint;
            string somestring = ds.Somestring;
            var somedate = ((DateTime)(ds.Somedate)).ToUniversalTime();
            DateTimeOffset somedto = ds.Somedto;
            bool somebool = ds.Somebool;
            object somenull = ds.Somenull;

            //Nullables currently NOT supported\\
            //int? anotherint = ds.Anotherint;  //Nullable test; non null case
            //int? anothernull = ds.Somenull; //nullable. null case

            Assert.AreEqual(123456789, someint);
            Assert.AreEqual("yes just a string", somestring);
            Assert.AreEqual(utcnow, somedate);
            Assert.IsTrue(Math.Abs((dto - somedto).TotalMilliseconds) < 2000, "was expecting dto and somedto essentially the same");
            Assert.AreEqual(true, somebool);
            Assert.IsNull(somenull, "somenull");

            //Nullables currently NOT supported\\
            //Assert.AreEqual(123, anotherint);
            //Assert.IsNull(anothernull, "anothernull");
        }

        [TestMethod]
        public void Customconvtest()
        {
            var cust = new SampleCustomTypeConverter();

            var numbers = Enumerable.Range(0, 5000);
            //evens are normal, odds get an odd CUSTOM! prefix on the value portion
            var pairs = numbers.ToDictionary(x => "K" + x.ToString(), x => ((x % 2 == 0) ? "" : "CUSTOM!") + x.ToString());
            dynamic ds = new DynamicStrings(pairs, null, new TypeConverter[] { cust });
            var coo = (SampleCustomType)ds.K999;
            Assert.AreEqual("CUSTOM!999", coo.Somestring);


            var smally = (short)ds.K888;
            Assert.AreEqual(888, smally);
        }
    }
}
