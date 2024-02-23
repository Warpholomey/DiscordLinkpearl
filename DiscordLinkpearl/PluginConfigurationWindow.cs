using Dalamud.Interface;
using Dalamud.Interface.Internal;

using Discord;

using DiscordModule;

using ImGuiNET;

using System;
using System.Diagnostics;
using System.IO;
using System.Numerics;
using System.Reflection;

namespace DiscordLinkpearl;

public sealed class PluginConfigurationWindow
{
	private bool _isVisible = false;

	private readonly DiscordModuleManager _discordModuleManager;
	private readonly Configuration _configuration;
	private readonly UiBuilder _uiBuilder;

	private string _discordKey;
	private bool _isEnabled;

	private readonly IDalamudTextureWrap? _helpImage01TextureWrap;
	private readonly IDalamudTextureWrap? _helpImage02TextureWrap;
	private readonly IDalamudTextureWrap? _helpImage03TextureWrap;
	private readonly IDalamudTextureWrap? _helpImage04TextureWrap;
	private readonly IDalamudTextureWrap? _helpImage05TextureWrap;
	private readonly IDalamudTextureWrap? _helpImage06TextureWrap;

	public PluginConfigurationWindow(
		DiscordModuleManager discordModuleManager,
		Configuration configuration,
		UiBuilder uiBuilder)
	{
		_discordModuleManager = discordModuleManager;
		_configuration = configuration;
		_uiBuilder = uiBuilder;

		_discordKey = configuration.DiscordKey;
		_isEnabled = configuration.IsEnabled;

		var assembly = Assembly.GetExecutingAssembly();

		_helpImage01TextureWrap = TryLoadImageFromResources(assembly, "01.png");
		_helpImage02TextureWrap = TryLoadImageFromResources(assembly, "02.png");
		_helpImage03TextureWrap = TryLoadImageFromResources(assembly, "03.png");
		_helpImage04TextureWrap = TryLoadImageFromResources(assembly, "04.png");
		_helpImage05TextureWrap = TryLoadImageFromResources(assembly, "05.png");
		_helpImage06TextureWrap = TryLoadImageFromResources(assembly, "06.png");
	}

	private IDalamudTextureWrap? TryLoadImageFromResources(Assembly assembly, string img)
	{
		using var s = assembly.GetManifestResourceStream($"DiscordLinkpearl.Resources.{img}");

		if (s == null)
		{
			return null;
		}

		using var ms = new MemoryStream();

		s.CopyTo(ms);

		return _uiBuilder.LoadImage(ms.ToArray());
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

		if (ImGui.BeginTabItem("Help"))
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

			TryDrawCenteredImage(_helpImage01TextureWrap);
			ImGui.Text("You can specify any application name — only you will see it.");
			ImGui.EndTabItem();
		}

		if (ImGui.BeginTabItem("Step 2"))
		{
			ImGui.Text("Open the «bot» section of newly created application:");
			TryDrawCenteredImage(_helpImage02TextureWrap);
			ImGui.Text("Generate a new token for your bot:");
			TryDrawCenteredImage(_helpImage03TextureWrap);
			ImGui.Text("Then copy it into the «Discord Key» field below.");
			ImGui.EndTabItem();
		}

		if (ImGui.BeginTabItem("Step 3"))
		{
			ImGui.Text("On the same page below enable «Message Content Intent»:");
			TryDrawCenteredImage(_helpImage04TextureWrap);
			ImGui.Text("Now you can save bot settings and close browser window.");
			ImGui.EndTabItem();
		}

		if (ImGui.BeginTabItem("Step 4"))
		{
			ImGui.Text("Press «Apply» button below and wait until status indicator turns green saying «Connected»:");
			TryDrawCenteredImage(_helpImage05TextureWrap);
			ImGui.Text("Now you can press «Authorize» button to add bot to your server:");
			TryDrawCenteredImage(_helpImage06TextureWrap);

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

		ImGui.Checkbox("Enabled?", ref _isEnabled);

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

	private static void TryDrawCenteredImage(IDalamudTextureWrap? dalamudTextureWrap)
	{
		if (dalamudTextureWrap == null)
		{
			return;
		}

		var windowDimensions = ImGui.GetWindowSize();
		ImGui.SetCursorPosX((windowDimensions.X - dalamudTextureWrap.Width) * 0.5f);
		ImGui.Image(
			dalamudTextureWrap.ImGuiHandle,
			new Vector2(
				dalamudTextureWrap.Width,
				dalamudTextureWrap.Height));
	}
}
