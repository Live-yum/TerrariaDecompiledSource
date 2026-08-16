using System;

namespace Terraria.Testing.ChatCommands;

[Flags]
public enum CommandRequirement
{
	SinglePlayer = 1,
	MultiplayerClient = 2,
	MultiplayerRPC = 4,
	LocalServer = 8,
	ClientAuthority = 5,
	AnyAuthority = 0xD,
	Client = 3,
	All = 0xF
}
