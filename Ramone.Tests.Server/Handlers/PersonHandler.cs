﻿using Ramone.Tests.Common;
using System.Web;


namespace Ramone.Tests.Server.Handlers
{
  public class PersonHandler
  {
    public object Get(string name)
    {
      return new Person
      {
        Name = name,
        Address = "At home"
      };
    }
  }
}