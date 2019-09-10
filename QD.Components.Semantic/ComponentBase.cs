﻿using Microsoft.AspNetCore.Components.Rendering;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;

namespace QD.Components.Semantic
{
	public abstract class ComponentBase : Microsoft.AspNetCore.Components.ComponentBase
	{
		[Parameter]
		public string RenderAs { get; set; }

		[Parameter]
		public Type RenderAsComponent { get; set; }

		[Parameter]
		public RenderFragment ChildContent { get; set; }

		[Parameter(CaptureUnmatchedValues = true)]
		public IDictionary<string, object> InputAttributes { get; set; }

		protected string ElementTag { get; set; }
		protected string ElementClass { get; set; }
		protected IDictionary<string, object> ElementAttributes { get; }

		private RenderFragment _componentRenderer;

		protected ComponentBase()
		{
			ElementTag = "div";
			ElementClass = "";
			ElementAttributes = new Dictionary<string, object>();
		}

		protected override void OnInitialized()
		{
			ConfigureComponent();
			_componentRenderer = builder =>
			{
				if (RenderAsComponent != null)
				{
					if (!ElementAttributes.ContainsKey(nameof(ChildContent)))
					{
						ElementAttributes.Add(nameof(ChildContent), ChildContent);
					}
					builder.OpenComponent(0, RenderAsComponent);
					builder.AddMultipleAttributes(1, ElementAttributes);
					builder.AddMultipleAttributes(2, InputAttributes);
					builder.CloseComponent();
				}
				else
				{
					if (!ElementAttributes.ContainsKey("Class"))
					{
						ElementAttributes.Add("Class", ElementClass);
					}
					builder.OpenElement(0, string.IsNullOrWhiteSpace(RenderAs) ? ElementTag : RenderAs);
					builder.AddMultipleAttributes(1, ElementAttributes);
					builder.AddContent(2, ChildContent);
					builder.CloseElement();
				}
			};
		}


		public override async Task SetParametersAsync(ParameterView parameters)
		{
			await base.SetParametersAsync(parameters);
			if (RenderAsComponent == null && InputAttributes != null)
			{
				foreach (KeyValuePair<string, object> attribute in InputAttributes)
				{
					Console.WriteLine(attribute.Key);
				}
				foreach (KeyValuePair<string, object> attribute in InputAttributes)
				{
					ThrowForUnknownIncomingParameterName(attribute.Key);
				}
			}
		}

		protected override void BuildRenderTree(RenderTreeBuilder builder)
		{
			builder.AddContent(0, _componentRenderer);
		}

		private void ThrowForUnknownIncomingParameterName(string parameterName)
		{
			Type componentType = GetType();
			PropertyInfo property = componentType.GetProperty(parameterName, BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			if (property != null)
			{
				if (!property.IsDefined(typeof(ParameterAttribute)) && !property.IsDefined(typeof(CascadingParameterAttribute)))
					throw new InvalidOperationException("Object of type '" + componentType.FullName + "' has a property matching the name '" + parameterName + "', but it does not have [ParameterAttribute] or [CascadingParameterAttribute] applied.");
				throw new InvalidOperationException("No writer was cached for the property '" + property.Name + "' on type '" + componentType.FullName + "'.");
			}
			throw new InvalidOperationException("Object of type '" + componentType.FullName + "' does not have a property matching the name '" + parameterName + "'.");
		}

		protected abstract void ConfigureComponent();
	}
}