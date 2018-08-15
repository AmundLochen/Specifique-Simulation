using System.Collections.Generic;
using DAL;
using Model;

namespace BLL
{
    /// <summary>
    ///     Contains messaging logic
    /// </summary>
    public static class MessagingLogic
    {
        private static readonly MessagingDal MessageDal = new MessagingDal();

        /// <summary>
        ///     Gets all teams
        /// </summary>
        /// <returns>Teams</returns>
        public static List<TeamModel> GetTeams()
        {
            return MessageDal.GetTeams();
        }

        /// <summary>
        ///     Gets team
        /// </summary>
        /// <param name="id">Team id</param>
        /// <returns>Team</returns>
        public static TeamModel GetTeamById(int id)
        {
            return MessageDal.GetTeamById(id);
        }

        /// <summary>
        ///     Adds message to DB
        /// </summary>
        /// <param name="m">Message</param>
        /// <returns>Message</returns>
        public static MessageModel ForwardMessage(MessageModel m)
        {
            return MessageDal.NewMessage(m);
        }

        /// <summary>
        ///     Gets message
        /// </summary>
        /// <param name="parent">Parent id</param>
        /// <returns>Message</returns>
        public static MessageModel GetMessageByParentId(int parent)
        {
            return MessageDal.GetMessageByParentId(parent);
        }

        /// <summary>
        ///     Gets messages
        /// </summary>
        /// <param name="teamId">To or from id</param>
        /// <returns>Messages</returns>
        public static List<MessageModel> GetMessages(int teamId)
        {
            return MessageDal.GetMessages(teamId);
        }
    }
}