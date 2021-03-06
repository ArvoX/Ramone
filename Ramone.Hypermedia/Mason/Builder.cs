﻿using Newtonsoft.Json.Linq;
using Ramone.Hypermedia.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;


namespace Ramone.Hypermedia.Mason
{
  public class Builder
  {
    private NamespaceManager Namespaces = new NamespaceManager();


    public Resource Build(JObject json, Type targetType)
    {
      if (json == null)
        return null;

      if (targetType != null && !typeof(Resource).IsAssignableFrom(targetType))
        throw new InvalidOperationException(string.Format("Cannot decode type {0} since it does not inherit fra Resource.", targetType));

      JObject namespaceJson = json[MasonProperties.Namespaces] as JObject;
      if (namespaceJson != null)
        BuildNamespaces(namespaceJson);

      Resource result = (targetType == null ? new MasonResource() : Activator.CreateInstance(targetType) as Resource);
      MasonResource mason = result as MasonResource;

      foreach (var pair in json)
      {
        if (pair.Key == MasonProperties.Namespaces && pair.Value is JObject)
        {
          // Ignore - already handled
        }
        else if (pair.Key == MasonProperties.Links && pair.Value is JObject)
        {
          foreach (JProperty linkJson in pair.Value.Children().OfType<JProperty>())
            result.Controls.Add(BuildLink(linkJson));
        }
        else if (pair.Key == MasonProperties.LinkTemplates && pair.Value is JObject)
        {
          foreach (JProperty linkTemplateJson in pair.Value.Children().OfType<JProperty>())
            result.Controls.Add(BuildLinkTemplate(linkTemplateJson));
        }
        else if (pair.Key == MasonProperties.Actions && pair.Value is JObject)
        {
          foreach (JProperty actionJson in pair.Value.Children().OfType<JProperty>())
            result.Controls.Add(BuildAction(actionJson));
        }
        else if (pair.Key == MasonProperties.Meta && pair.Value is JObject)
        {
          //MetaJsonValue = pair.Value;
          //Description = GetValue<string>(pair.Value, MasonProperties.MetaProperties.Description);
          //JToken metaLinksProperty = pair.Value[MasonProperties.Links];
          //if (metaLinksProperty is JObject)
          //{
          //  MetaLinksJsonValue = metaLinksProperty;
          //  MetaLinks = new ObservableCollection<LinkViewModel>(
          //    metaLinksProperty.Children().OfType<JProperty>().Select(l => new LinkViewModel(this, l, context)));
          //}
        }
        else if (pair.Key == MasonProperties.Error && pair.Value is JObject)
        {
          //ResourcePropertyViewModel error = new ResourcePropertyViewModel(this, pair.Value, pair.Key, new ResourceViewModel(this, (JObject)pair.Value, context));
          //error.IsError = true;
          //Properties.Add(error);
        }
        else
        {
          PropertyInfo pi = (targetType != null ? targetType.GetProperty(pair.Key) : null);
          MethodInfo mi = (pi != null ? pi.GetSetMethod() : null);

          // If the JSON property name matches a property name on the target class then deserialize into that .NET property
          if (pi != null && pi.CanWrite && mi != null && mi.IsPublic)
            pi.SetValue(result, CreatePropertyRecursively(pair.Value, pi.PropertyType), new object[0]);
          else
            result.RegisterPropertyValue(pair.Key, CreatePropertyRecursively(pair.Value, null));
        }
      }

      return result;
    }


    private IControl BuildLink(JProperty linkJson)
    {
      JObject linkObject = linkJson.Value as JObject;
      if (linkObject == null)
        return null;

      string prefix;
      string reference;
      string nsname;
      string name = Namespaces.Expand(linkJson.Name, out prefix, out reference, out nsname);

      string href = GetValue<string>(linkObject, "href");

      Link link = new Link(name, href);
      return link;
    }

    
    private IControl BuildLinkTemplate(JProperty linkTemplateJson)
    {
      JObject linkTemplateObject = linkTemplateJson.Value as JObject;
      if (linkTemplateObject == null)
        return null;

      string prefix;
      string reference;
      string nsname;
      string name = Namespaces.Expand(linkTemplateJson.Name, out prefix, out reference, out nsname);

      string template = GetValue<string>(linkTemplateObject, "template");

      LinkTemplate linkTemplate = new LinkTemplate(name, template);
      return linkTemplate;
    }


    private IControl BuildAction(JProperty actionJson)
    {
      JObject actionObject = actionJson.Value as JObject;
      if (actionObject == null)
        return null;

      string prefix;
      string reference;
      string nsname;
      string name = Namespaces.Expand(actionJson.Name, out prefix, out reference, out nsname);

      string type = GetValue<string>(actionObject, "type");
      string method = GetValue<string>(actionObject, "method", "POST");
      string href = GetValue<string>(actionObject, "href");

      if (type == MasonProperties.ActionTypes.Void)
      {
        return new VoidAction(name, href, method);
      }
      else if (type == MasonProperties.ActionTypes.JSON)
      {
        return new JsonAction(name, href, method);
      }
      else if (type == MasonProperties.ActionTypes.JSONFiles)
      {
        string jsonFile = GetValue<string>(actionObject, "jsonFile");
        return new JsonFilesAction(name, href, method, jsonFile);
      }
      
      throw new NotImplementedException("Unknown action type: " + type);
    }


    private object CreatePropertyRecursively(JToken json, Type expectedType)
    {
      if (json == null)
      {
        return null;
      }
      else if (json is JArray)
      {
        if (expectedType != null)
        {
          // Figure out expected item type (assuming expectedType is an array, list or other collection)
          Type itemType = GetItemTypeFromSequenceType(expectedType);

          // Create a "list of itemtype"
          Type genericListType = typeof(List<>);
          Type listType = genericListType.MakeGenericType(itemType);

          // Get "Add" method for adding items to the list
          MethodInfo listAddMethod = listType.GetMethod("Add");

          // Create the list
          object list = Activator.CreateInstance(listType);

          // Build the sequence of items in the JSON array
          IEnumerable<object> items = ((JArray)json).Select(item => CreatePropertyRecursively(item, itemType));

          // Move the JSON items into typed list
          foreach (object i in items)
            listAddMethod.Invoke(list, new object[] { i });

          // Construct expected type with typed list as constructor argument - hoping that such one exists
          var result = Activator.CreateInstance(expectedType, list);

          return result;
        }
        else
        {
          IEnumerable<MasonResource> items = ((JArray)json).Select(item => CreatePropertyRecursively(item, null)).Cast<MasonResource>();
          return new List<MasonResource>(items);
        }
      }
      else if (json is JObject)
      {
        return Build((JObject)json, expectedType);
      }
      else if (json is JValue)
      {
        return ((JValue)json).Value;
      }
      else
        throw new NotImplementedException(string.Format("Cannot build property value from JSON type '{0}'", json.GetType()));
    }

    
    
    private Type GetItemTypeFromSequenceType(Type expectedType)
    {
      // Crazy guess ...
      if (expectedType.IsGenericType && expectedType.GetGenericArguments().Length == 1)
        return expectedType.GetGenericArguments()[0];
      if (expectedType.IsArray)
        return expectedType.GetElementType();
      return null;
    }


    private void BuildNamespaces(JObject namespaces)
    {
      foreach (JProperty ns in namespaces.Properties())
      {
        JObject nsDef = ns.Value as JObject;
        if (nsDef != null)
        {
          JToken jsonName = nsDef[MasonProperties.NamespaceProperties.Name];
          string ns_name = (jsonName != null && jsonName.Type == JTokenType.String ? jsonName.Value<string>() : null);
          string ns_prefix = ns.Name;
          if (!string.IsNullOrWhiteSpace(ns_prefix) && !string.IsNullOrWhiteSpace(ns_name))
            Namespaces.Namespace(ns_prefix, ns_name);
        }
      }
    }


    protected T GetValue<T>(JToken t, string name, T defaultValue = null)
      where T : class
    {
      JToken value = t[name];
      if (value != null)
        return value.Value<T>();
      return defaultValue;
    }
  }
}
