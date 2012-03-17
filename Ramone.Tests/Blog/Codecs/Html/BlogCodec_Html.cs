﻿using System.Collections.Generic;
using System.Linq;
using HtmlAgilityPack;
using Ramone.MediaTypes.Html;
using Ramone.HyperMedia;


namespace Ramone.Tests.Blog.Codecs.Html
{
  public class BlogCodec_Html : BaseCodec_Html<Resources.Blog>
  {
    public override Resources.Blog ReadFromHtml(HtmlDocument html, ReaderContext context)
    {
      HtmlNode doc = html.DocumentNode;

      List<Resources.Blog.Post> posts = new List<Resources.Blog.Post>();

      foreach (HtmlNode postNode in doc.SelectNodes(@"//div[@class=""post""]"))
      {
        HtmlNode title = postNode.SelectNodes(@".//*[@class=""post-title""]").First();
        HtmlNode content = postNode.SelectNodes(@".//*[@class=""post-content""]").First();
        List<Anchor> links = new List<Anchor>(postNode.Anchors());

        posts.Add(new Resources.Blog.Post
        {
          Title = title.InnerText,
          Text = content.InnerText,
          Links = links
        });
      }

      List<ILink> blogLinks = new List<ILink>(doc.Anchors().Cast<ILink>().Union(doc.Links()));

      Resources.Blog blog = new Resources.Blog()
      {
        Title = doc.SelectNodes(@".//*[@class=""blog-title""]").First().InnerText,
        Posts = posts,
        Links = blogLinks
      };

      return blog;
    }
  }
}