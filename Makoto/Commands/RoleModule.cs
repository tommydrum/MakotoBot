using Discord.Commands;
using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;

namespace Makoto.Commands
{
    [Name("Role Commands")]
    public class RoleModule : ModuleBase
    {
        private readonly IConfigurationRoot _config;
        private string prefix;

        //Constructor
        public RoleModule(IConfigurationRoot config)
        {
            _config = config;
            prefix = _config["discord:prefix"];
        }

        #region notify
        [Command("notify"), Summary("Add's or remove's mentioning user to a notify role")]
        public async Task Notify([Remainder, Summary("Role name (minus the prefix)")] string RoleName)
        {
            //Test if PM, ignore if it is.
            if (Context.Guild == null)
            {
                await ReplyAsync("Please use this in the server you wish to change your role in");
                return;
            }
            string[] splitargs = RoleName.Split(' ');
            //search for requested role, null if not found
            var role = Context.Guild.Roles.Where(e => e.Name == prefix + splitargs[0]).DefaultIfEmpty(null).First();
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
        #endregion

        #region list
        [Command("list"), Summary("list all notify roles")]
        public async Task List()
        {
            //Test if PM, ignore if it is.
            if (Context.Guild == null)
            {
                await ReplyAsync("Please use this in the server you wish to change your role in");
                return;
            }
            var roles = Context.Guild.Roles.Where(e => e.Name.StartsWith(prefix));
            string output = "Here's a list of all notify roles: ```\n";
            foreach (var role in roles)
            {
                output += $"{role.Name}\n";
            }
            output += "```";
            await ReplyAsync(output);
        }
        #endregion

        #region new
        [Command("new"), Summary("Create's a new role. Admin only")]
        [RequireUserPermission(ChannelPermission.ManageRoles)]
        public async Task NewRole([Remainder, Summary("Role name (minus the prefix)")] string RoleName)
        {
            //Test if PM, ignore if it is.
            if (Context.Guild == null)
            {
                await ReplyAsync("Please use this in the server you wish to change your role in");
                return;
            }
            string[] splitargs = RoleName.Split(' ');
            var role = Context.Guild.Roles.Where(e => e.Name == prefix + splitargs[0]).DefaultIfEmpty(null).First();
            if (role == null)
            {
                //Create the role
                IRole newRole = await Context.Guild.CreateRoleAsync(prefix + splitargs[0], GuildPermissions.None, new Color(255, 0, 0), false);
                await newRole.ModifyAsync(e => e.Mentionable = true);
                await ReplyAsync("Created @" + newRole.Name);
            }
            else
            {
                await ReplyAsync("Cannot add a role that already exists");
            }
        }
        #endregion

        #region remove
        [Command("remove"), Summary("Remove role. Admin only")]
        [RequireUserPermission(ChannelPermission.ManageRoles)]
        public async Task RemoveRole([Remainder, Summary("Role name (minus the prefix)")] string RoleName)
        {
            //Test if PM, ignore if it is.
            if (Context.Guild == null)
            {
                await ReplyAsync("Please use this in the server you wish to change your role in");
                return;
            }
            string[] splitargs = RoleName.Split(' ');

            var role = Context.Guild.Roles.Where(e => e.Name == prefix + splitargs[0]).DefaultIfEmpty(null).First();
            if (role == null)
            {
                await ReplyAsync("Cannot find role... sorry..");
            }
            else
            {
                await role.DeleteAsync();
                await ReplyAsync("Deleted role.");
            }
        }
        #endregion

    }
}
