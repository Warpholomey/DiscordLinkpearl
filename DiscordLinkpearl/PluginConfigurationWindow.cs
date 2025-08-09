using Discord;

using DiscordModule;

using Dalamud.Bindings.ImGui;

using System;
using System.Diagnostics;
using System.IO;
using System.Numerics;

using ThreadLoadImageHandler = ECommons.ImGuiMethods.ThreadLoadImageHandler;

namespace DiscordLinkpearl;

public sealed class PluginConfigurationWindow
{
	private bool _isVisible = false;

	private readonly DiscordModuleManager _discordModuleManager;
	private readonly Configuration _configuration;
	private readonly string _assemblyDirectory;

	private string _discordKey;
	private bool _isEnabled;

	public PluginConfigurationWindow(
		DiscordModuleManager discordModuleManager,
		Configuration configuration,
		string assemblyDirectory)
	{
		_discordModuleManager = discordModuleManager;
		_configuration = configuration;
		_assemblyDirectory = assemblyDirectory;

		_discordKey = configuration.DiscordKey;
		_isEnabled = configuration.IsEnabled;
	}

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

		ImGui.BeginTabBar("#docs");

		if (ImGui.BeginTabItem("Main"))
		{
			ImGui.Text("Follow the instructions on tabs «Step 1-4» above to set up plugin.");
			ImGui.EndTabItem();
		}

		if (ImGui.BeginTabItem("Step 1"))
		{
			ImGui.Text("First you need to create an application:");

			if (ImGui.Button("Open «Discord Developer Portal»"))
			{
				Process.Start(
					new ProcessStartInfo
					{
						FileName = "https://discord.com/developers/applications",
						UseShellExecute = true,
					});
			}

			TryDrawCenteredImage("Help/01.png");
			ImGui.Text("You can specify any application name — only you will see it.");
			ImGui.EndTabItem();
		}

		if (ImGui.BeginTabItem("Step 2"))
		{
			ImGui.Text("Open the «bot» section of newly created application:");
			TryDrawCenteredImage("Help/02.png");
			ImGui.Text("Generate a new token for your bot:");
			TryDrawCenteredImage("Help/03.png");
			ImGui.Text("Then copy it into the «Discord Key» field below.");
			ImGui.EndTabItem();
		}

		if (ImGui.BeginTabItem("Step 3"))
		{
			ImGui.Text("On the same page below enable «Message Content Intent»:");
			TryDrawCenteredImage("Help/04.png");
			ImGui.Text("Now you can save bot settings and close browser window.");
			ImGui.EndTabItem();
		}

		if (ImGui.BeginTabItem("Step 4"))
		{
			ImGui.Text("Press «Apply Settings» button below and wait until status indicator turns green saying «Connected»:");
			TryDrawCenteredImage("Help/05.png");
			ImGui.Text("Now you can press «Authorize» button to add bot to your server:");
			TryDrawCenteredImage("Help/06.png");

			ImGui.TextColored(
				new Vector4(1f, 0f, 0f, 1f),
				"You must create a new empty server! Do not add a bot to a server with other people!");

			ImGui.EndTabItem();
		}

		ImGui.EndTabBar();
		ImGui.Separator();
		ImGui.PushItemWidth(500);
		ImGui.InputText("Discord Key", ref _discordKey, 100, ImGuiInputTextFlags.Password);
		ImGui.PopItemWidth();

		ImGui.Checkbox("Enable Messages Forwarding", ref _isEnabled);

		ImGui.SameLine();

		if (ImGui.Button("Apply Settings"))
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

	public void ReloadConfigurationValues()
	{
		_discordKey = _configuration.DiscordKey;
		_isEnabled = _configuration.IsEnabled;
	}

	private static Vector4 GetStatusColor(ConnectionState connectionState) => connectionState switch
	{
		ConnectionState.Connected => new Vector4(0f, 1f, 0f, 1f),
		ConnectionState.Connecting => new Vector4(1f, 1f, 0f, 1f),
		ConnectionState.Disconnected => new Vector4(1f, 0f, 0f, 1f),
		ConnectionState.Disconnecting => new Vector4(1f, 1f, 0f, 1f),
		_ => throw new NotImplementedException(),
	};

	private void TryDrawCenteredImage(string img)
	{
		var url = Path.Combine(_assemblyDirectory, img);

		if (ThreadLoadImageHandler.TryGetTextureWrap(url, out var dalamudTextureWrap))
		{
			var windowDimensions = ImGui.GetWindowSize();
			ImGui.SetCursorPosX((windowDimensions.X - dalamudTextureWrap.Width) * 0.5f);
			ImGui.Image(
				dalamudTextureWrap.Handle,
				new Vector2(
					dalamudTextureWrap.Width,
					dalamudTextureWrap.Height));
		}
	}
}
