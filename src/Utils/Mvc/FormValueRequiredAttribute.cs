﻿using System;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.AspNetCore.Routing;

namespace Utils.Mvc
{
    public sealed class FormValueRequiredAttribute : ActionMethodSelectorAttribute
    {
        private readonly string name;

        public FormValueRequiredAttribute(string name)
        {
            this.name = name;
        }

        public override bool IsValidForRequest(RouteContext context, ActionDescriptor action)
        {
            if (string.Equals(context.HttpContext.Request.Method, "GET", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(context.HttpContext.Request.Method, "HEAD", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(context.HttpContext.Request.Method, "DELETE", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(context.HttpContext.Request.Method, "TRACE", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            if (string.IsNullOrEmpty(context.HttpContext.Request.ContentType))
            {
                return false;
            }

            if (!context.HttpContext.Request.ContentType.StartsWith("application/x-www-form-urlencoded", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            return !string.IsNullOrEmpty(context.HttpContext.Request.Form[name]);
        }
    }
}
