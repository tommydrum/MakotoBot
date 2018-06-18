using Discord.Commands;
using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.WebSocket;

namespace Makoto.Commands
{
    public class RoleManager : ModuleBase
    {
        #region notify
        [Command("notify"), Summary("Add's or remove's mentioning user to a notify role")]
        public async Task Notify([Remainder, Summary("The role name to add/remove yourself into (minus the prefix)")] string args)
        {
            //Test if PM, ignore if it is.
            if (Context.Guild == null)
            {
                await ReplyAsync("Please use this in the server you wish to change your role in");
            }
            string[] splitargs = args.Split(' ');
            if (splitargs.Length < 1)
            {
                await ReplyAsync("Need's role name, use format: @me notify role");
            }
            else
            {
                //search for requested role, null if not found
                var role = Context.Guild.Roles.Where(e => e.Name == "ns_" + splitargs[0]).DefaultIfEmpty(null).First();
                if (role == null)
                {
                    await ReplyAsync("Did not find that role. If you want this to become a role, ask your server admin to create it.");
                    return;
                }

                // See if user has the role. If yes, remove the role, of not, add the role.
                if ((Context.User as IGuildUser).RoleIds.Contains(role.Id))
                {
                    var user = await Context.Guild.GetUserAsync(Context.User.Id);
                    await user.RemoveRoleAsync(role);
                    await ReplyAsync("You are no longer apart of " + role.Name);
                }
                else
                {
                    var user = await Context.Guild.GetUserAsync(Context.User.Id);
                    await user.AddRoleAsync(role);
                    await ReplyAsync("You are now apart of " + role.Name);
                }
            }
        }
        #endregion

        #region new
        [Command("new"), Summary("Create's a new role. Admin only")]
        [RequireUserPermission(ChannelPermission.ManageRoles)]
        public async Task NewRole([Remainder, Summary("The role name to create (minus the prefix)")] string args)
        {
            //Test if PM, ignore if it is.
            if (Context.Guild == null)
            {
                await ReplyAsync("Please use this in the server you wish to change your role in");
            }
            string[] splitargs = args.Split(' ');
            if (splitargs.Length < 1)
            {
                await ReplyAsync("Need's role name, use format: @me notify role");
            }
            else
            {
                var role = Context.Guild.Roles.Where(e => e.Name == "ns_" + splitargs[0]).DefaultIfEmpty(null).First();
                if (role == null)
                {
                    //Create the role
                    IRole newRole = await Context.Guild.CreateRoleAsync("ns_" + splitargs[0], GuildPermissions.None, new Color(255, 0, 0), false);
                    await newRole.ModifyAsync(e => e.Mentionable = true);
                    await ReplyAsync("Created " + newRole.Name);
                }
                else
                {
                    await ReplyAsync("Cannot add a role that already exists");
                    return;
                }
            }
        }
        #endregion
    }
}
