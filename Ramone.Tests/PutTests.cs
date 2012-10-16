﻿using System.ServiceModel.Syndication;
using NUnit.Framework;
using Ramone.Tests.Common.CMS;
using System.Net;


namespace Ramone.Tests
{
  [TestFixture]
  public class PutTests : TestHelper
  {
    Dossier MyDossier = new Dossier
    {
      Id = 15,
      Title = "A new dossier"
    };

    Request DossierReq;


    protected override void SetUp()
    {
      base.SetUp();
      DossierReq = Session.Bind(DossierTemplate, new { id = MyDossier.Id });
    }


    [Test]
    public void CanPutAndIgnoreReturnedBody()
    {
      // Act
      using (Response response = DossierReq.Put(MyDossier))
      {
        // Assert
        Assert.IsNotNull(response);
      }
    }


    [Test]
    public void CanPutAndGetResult()
    {
      // Act
      using (Response<Dossier> response = DossierReq.Put<Dossier>(MyDossier))
      {
        Dossier newDossier = response.Body;

        // Assert
        Assert.IsNotNull(newDossier);
      }
    }


    [Test]
    public void CanPutAndGetResultWithAccept()
    {
      // Act
      using (var newDossier = DossierReq.Accept<Dossier>().Put(MyDossier))
      {
        // Assert
        Assert.IsNotNull(newDossier.Body);
      }
    }


    [Test]
    public void CanPutAndGetResultWithAcceptMediaType()
    {
      // Act
      using (var newDossier = DossierReq.Accept<Dossier>(CMSConstants.CMSMediaType).Put(MyDossier))
      {
        // Assert
        Assert.IsNotNull(newDossier.Body);
      }
    }


    [Test]
    public void CanPutEmptyBody_Typed()
    {
      // Arrange
      Request request = Session.Bind(AnyEchoTemplate);

      // Act
      using (Response<string> response = request.Accept("text/plain").ContentType("application/x-www-url-formencoded").Put<string>())
      {
        // Assert
        Assert.AreEqual(null, response.Body);
      }
    }


    [Test]
    public void CanPutEmptyBody_Untyped()
    {
      // Arrange
      Request request = Session.Bind(AnyEchoTemplate);

      // Act
      using (Response response = request.Accept("text/plain").ContentType("application/x-www-url-formencoded").Put())
      {
        // Assert
        Assert.IsNull(response.Body);
      }
    }
  }
}
