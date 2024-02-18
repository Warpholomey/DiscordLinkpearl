using Dalamud.Configuration;

using System.Collections.Generic;

namespace DiscordLinkpearl;

public sealed partial class Configuration : IPluginConfiguration
{
	private readonly List<ConfigurationChanged> _onConfigurationChangedHandlers = [];

	public event ConfigurationChanged OnConfigurationChanged
	{
		add
		{
			OnConfigurationChangedCore += value;
			_onConfigurationChangedHandlers.Add(value);
		}

		remove
		{
			OnConfigurationChangedCore -= value;
			_onConfigurationChangedHandlers.Remove(value);
		}
	}

	private event ConfigurationChanged? OnConfigurationChangedCore;

	public void Save()
	{
		OnConfigurationChangedCore?.Invoke();
	}

	public void ClearHandlers()
	{
		foreach (var handler in _onConfigurationChangedHandlers.ToArray())
		{
			OnConfigurationChanged -= handler;
		}
	}

	public delegate void ConfigurationChanged();
}
