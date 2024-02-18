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

		ImGui.InputText("Discord Key", ref _discordKey, 100, ImGuiInputTextFlags.Password);

		if (_discordModuleManager.GuildName != null)
		{
			ImGui.Text($"Guild: {_discordModuleManager.GuildName}");
		}

		if (ImGui.Button("Close"))
		{
			_discordKey = _configuration.DiscordKey;
			_isVisible = false;
		}

		ImGui.SameLine();

		if (ImGui.Button("Apply"))
		{
			_configuration.DiscordKey = _discordKey;
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

		ImGui.TextColored(GetStatusColor(_discordModuleManager.ConnectionState), $"Status: {_discordModuleManager.ConnectionState}");

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
