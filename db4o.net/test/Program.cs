﻿using System;

using Db4objects.Db4o;
using System.Linq;
using Db4objects.Db4o.Linq;
using Db4objects.Db4o.Query;
using Db4objects.Db4o.CS;
using Db4objects.Db4o.Ext;
using System.Threading;

namespace db40
{
    public class Person
    {

        public Person(string v)
        {
            Name = v;
        }
        //Fields, not properties.
        public string Name;
        public long Age = 9;
    }
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            IObjectContainer db;
            IObjectServer server = null;

            bool embed = false;
            string dbpath = "/tmp/x.db";

            if (embed)
            {
                db = Db4oEmbedded.OpenFile(dbpath);
            }
            else
            {
                var config = Db4oClientServer.NewServerConfiguration();
                //config.Common.ObjectClass(typeof(Person)).ObjectField("Age").Indexed(true);
                //config.Common.ObjectClass(typeof(Person)).ObjectField("Name").Indexed(true);
                //config.Common.Diagnostic.AddListener(new Db4objects.Db4o.Diagnostic.DiagnosticToConsole());
                server = Db4oClientServer.OpenServer(config, dbpath, 7881);
                server.GrantAccess("user", "password");
                IObjectContainer client =
                        Db4oClientServer.OpenClient("localhost", 7881, "user", "password");
                db = client;
            }

            try
            {

                // Store a few Person objects

                db.Store(new Person("Petra"));

                db.Store(new Person("Gallad"));

                // Retrieve the Person
                Person p;
                {
                    Console.WriteLine("001");
                    var results = db.Query<Person>(x => x.Name == "Petra");
                    p = results.First();
                    Console.WriteLine(p.Name);
                }
                {
                    Console.WriteLine("002");
                    var result2 = from Person tp in db
                                  where tp.Name == "Petra"
                                  select tp;
                    p = result2.First();
                    Console.WriteLine(p.Name);
                }

                {
                    Console.WriteLine("003");
                    var result2 = from Person tp in db
                                  where tp.Name.StartsWith("Petr")
                                  select tp;
                    p = result2.First();
                    Console.WriteLine(p.Name);
                }

                {
                    Console.WriteLine("004");
                    var result2 = from Person tp in db
                                  where tp.Age == 9 && tp.Name == "Petra"
                                  select tp;
                    p = result2.First();
                    Console.WriteLine(p.Name);
                }
                {
                    Console.WriteLine("005");
                    var uid = db.Ext().GetObjectInfo(p).GetInternalID();
                    p = (Person)db.Ext().GetByID(uid);
                    Console.WriteLine(p.Name);
                }
                p.Name = "Peter";
                db.Store(p);


                // Delete the person
                db.Delete(p);

                // Don't forget to commit!
                db.Commit();
                Console.WriteLine("Commited " + db.Query<Person>().Count(x => x.Age >= 0));

            }

            catch (Exception ex)
            {
                db.Rollback();
                Console.WriteLine(ex.ToString());
                throw ex;
            }

            finally
            {
                // Close the db cleanly
                db.Close();
                Environment.Exit(0);

            }


        }
    }
}