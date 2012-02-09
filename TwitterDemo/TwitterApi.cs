﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TwitterDemo
{
  public class TwitterApi
  {
    public const string UserTimeLinePath = "statuses/user_timeline.json?screen_name={screen_name}&count={count}";
    public const string UpdateProfilePath = "account/update_profile.json?name={name}";

    public static UriTemplate UserTimeLineTemplate = new UriTemplate(UserTimeLinePath);
    public static UriTemplate UpdateProfileTemplate = new UriTemplate(UpdateProfilePath);
  }
}