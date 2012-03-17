﻿using System.Collections.Generic;
using NUnit.Framework;
using Ramone.HyperMedia;
using Ramone.Utility;
using System.Text;
using System.Web;


namespace Ramone.Tests.Utility
{
  [TestFixture]
  public class LinkHeaderTests : TestHelper
  {

    [Test]
    public void CanReadMultipleWebLinks()
    {
      // Arrange
      string header = @"<http://example.com/TheBook/chapter2>; rel=""previous""; title=""Previous chapter"", 
  <http://example.com/TheBook/chapter4>; rel=""next""; title=""Next chapter""";

      // Act
      IList<IParameterizedLink> links = WebLinkParser.ParseLinks(header);

      // Assert
      Assert.IsNotNull(links);
      Assert.AreEqual(2, links.Count);

      ILink l1 = links[0];
      Assert.AreEqual("http://example.com/TheBook/chapter2", l1.HRef);
      Assert.AreEqual("previous", l1.RelationshipType);
      Assert.AreEqual("Previous chapter", l1.Title);

      ILink l2 = links[1];
      Assert.AreEqual("http://example.com/TheBook/chapter4", l2.HRef);
      Assert.AreEqual("next", l2.RelationshipType);
      Assert.AreEqual("Next chapter", l2.Title);
    }


    [Test]
    public void CanReadSingleWebLink()
    {
      // Arrange
      string header = @"<http://example.com/TheBook/chapter3>; rel=""previous""; title=""Previous chapter""";

      // Act
      IList<IParameterizedLink> links = WebLinkParser.ParseLinks(header);

      // Assert
      Assert.IsNotNull(links);
      Assert.AreEqual(1, links.Count);

      ILink l1 = links[0];
      Assert.AreEqual("http://example.com/TheBook/chapter3", l1.HRef);
      Assert.AreEqual("previous", l1.RelationshipType);
      Assert.AreEqual("Previous chapter", l1.Title);
    }


    [Test]
    public void CanReadSingleUrl()
    {
      // Arrange
      string header = @"<http://example.com/TheBook/chapter5>";

      // Act
      IList<IParameterizedLink> links = WebLinkParser.ParseLinks(header);

      // Assert
      Assert.IsNotNull(links);
      Assert.AreEqual(1, links.Count);

      ILink l1 = links[0];
      Assert.AreEqual("http://example.com/TheBook/chapter5", l1.HRef);
      Assert.IsNull(l1.RelationshipType);
      Assert.IsNull(l1.Title);
    }


    [Test]
    public void CanReadInternationalTitles()
    {
      // Arrange
      string header = @"<http://example.com/TheBook/chapter6>; rel=""previous""; title*=""UTF-8'de'N%c3%a4chstes%20Kapitel""";

      // Act
      IList<IParameterizedLink> links = WebLinkParser.ParseLinks(header);

      // Assert
      Assert.IsNotNull(links);
      Assert.AreEqual(1, links.Count);

      ILink l1 = links[0];
      Assert.AreEqual("http://example.com/TheBook/chapter6", l1.HRef);
      Assert.AreEqual("Nächstes Kapitel", l1.Title);
    }
  }
}
