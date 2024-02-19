using Discord;

using DiscordModule;

using ImGuiNET;

using System;
using System.Diagnostics;
using System.Numerics;

namespace DiscordLinkpearl;

public sealed class PluginConfigurationWindow(DiscordModuleManager discordModuleManager, Configuration configuration)
{
	private bool _isVisible = false;

	private readonly DiscordModuleManager _discordModuleManager = discordModuleManager;
	private readonly Configuration _configuration = configuration;

	private string _discordKey = configuration.DiscordKey;
	private bool _isEnabled = configuration.IsEnabled;

	public void Show()
	{
		_isVisible = true;
	}

	public void Draw()
	{
		if (!_isVisible)
		{
			return;
		}

		ImGui.Begin("Discord Linkpearl Settings", ref _isVisible, ImGuiWindowFlags.AlwaysAutoResize);

		ImGui.PushItemWidth(500);
		ImGui.InputText("Discord Key", ref _discordKey, 100, ImGuiInputTextFlags.Password);
		ImGui.PopItemWidth();

		ImGui.Checkbox("Enabled?", ref _isEnabled);

		ImGui.SameLine();

		if (ImGui.Button("Close"))
		{
			_discordKey = _configuration.DiscordKey;
			_isEnabled = _configuration.IsEnabled;
			_isVisible = false;
		}

		ImGui.SameLine();

		if (ImGui.Button("Apply"))
		{
			_configuration.DiscordKey = _discordKey;
			_configuration.IsEnabled = _isEnabled;
			_configuration.Save(true);
		}

		ImGui.SameLine();

		if (_discordModuleManager.ConnectionState == ConnectionState.Connected && ImGui.Button("Authorize"))
		{
			Process.Start(
				new ProcessStartInfo
				{
					FileName = $"https://discordapp.com/oauth2/authorize?client_id={_discordModuleManager.DiscordId}&scope=bot&permissions={DiscordModuleManager.RequiredPermissionsCode}",
					UseShellExecute = true,
				});
		}

		ImGui.SameLine();

		var info = _discordModuleManager.ConnectionState == ConnectionState.Connected && _discordModuleManager.GuildName != null
			? $"Authorized ({_discordModuleManager.GuildName})"
			: _discordModuleManager.ConnectionState.ToString();

		ImGui.TextColored(GetStatusColor(_discordModuleManager.ConnectionState), $"Status: {info}");

		ImGui.End();
	}

	private static Vector4 GetStatusColor(ConnectionState connectionState) => connectionState switch
	{
		ConnectionState.Connected => new Vector4(0f, 1f, 0f, 1f),
		ConnectionState.Connecting => new Vector4(1f, 1f, 0f, 1f),
		ConnectionState.Disconnected => new Vector4(1f, 0f, 0f, 1f),
		ConnectionState.Disconnecting => new Vector4(1f, 1f, 0f, 1f),
		_ => throw new NotImplementedException(),
	};
}
