﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace Andl.Server {
  public class RouteConfig {
    public static void RegisterRoutes(RouteCollection routes) {
      routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

      // Mapping for the index page
      routes.MapRoute(
          name: "Default",
          url: "",
          defaults: new { controller = "Home", action = "Index" }
      );

      routes.MapRoute(
          name: "Other",
          url: "{action}",
          defaults: new { controller = "Home", action = "{action}" }
      );
    }
  }
}
