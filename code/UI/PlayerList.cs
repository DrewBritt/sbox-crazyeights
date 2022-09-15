using System.Collections.Generic;
using System.Linq;
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

namespace CrazyEights;

[UseTemplate]
public partial class PlayerList : Panel
{
    Dictionary<Client, PlayerListEntry> Entries = new();

    public override void Tick()
    {
        base.Tick();

        // Add newly joined clients
        foreach(var client in Client.All.Except(Entries.Keys))
        {
            var entry = AddClient(client);
            Entries[client] = entry;
        }

        //Remove disconnected clients
        foreach(var client in Entries.Keys.Except(Client.All))
        {
            if(Entries.TryGetValue(client, out var row))
            {
                row?.Delete();
                Entries.Remove(client);
            }
        }
    }

    protected virtual PlayerListEntry AddClient(Client cl)
    {
        var entry = new PlayerListEntry(cl);
        AddChild(entry);
        return entry;
    }
}

public partial class PlayerListEntry: Panel
{
    private Client client;
    public Image Avatar { get; set; }
    public Label Name { get; set; }

    public PlayerListEntry(Client cl)
    {
        client = cl;
        BindClass("currentplayer", () => client == Game.Current.CurrentPlayer?.Client);

        Avatar = Add.Image($"avatar:{cl.PlayerId}", "avatar");
        Name = Add.Label(cl.Name.Truncate(12, "..."), "name");
    }
}
