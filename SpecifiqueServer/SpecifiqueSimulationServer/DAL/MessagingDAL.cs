using System;
using System.Collections.Generic;
using System.Linq;
using Model;

namespace DAL
{
    /// <summary>
    ///     Contains messaging dal
    /// </summary>
    public class MessagingDal
    {
        private readonly DBlinqDataContext _db = new DBlinqDataContext();

        /// <summary>
        ///     Gets all teams
        /// </summary>
        /// <returns>Teams</returns>
        public List<TeamModel> GetTeams()
        {
            List<TeamModel> teams = (from t in _db.teams
                                     select new TeamModel
                                         {
                                             Id = t.Id,
                                             Name = t.teamName
                                         }).ToList();
            return teams;
        }

        /// <summary>
        ///     Gets team
        /// </summary>
        /// <param name="id">Team id</param>
        /// <returns>Team</returns>
        public TeamModel GetTeamById(int id)
        {
            TeamModel team = (from t in _db.teams
                              where t.Id == id
                              select new TeamModel
                                  {
                                      Id = t.Id,
                                      Name = t.teamName
                                  }).ToList().Last();
            return team;
        }

        /// <summary>
        ///     Adds new message to DB
        /// </summary>
        /// <param name="m">Message</param>
        /// <returns>Message</returns>
        public MessageModel NewMessage(MessageModel m)
        {
            var newMessage = new msg
                {
                    teamFromId = m.FromId,
                    teamFromName = m.FromName,
                    teamToId = m.ToId,
                    teamToName = m.ToName,
                    parentId = m.ParentId,
                    msgSubject = m.Subject,
                    msgText = m.Text,
                    msgTime = m.Time
                };

            _db.msgs.InsertOnSubmit(newMessage);
            int parentId = m.ParentId;

            try
            {
                _db.SubmitChanges();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            if (m.ParentId == 0)
            {
                // Query the database for the row to be updated.
                IQueryable<msg> query =
                    from msgs in _db.msgs
                    where msgs.parentId == 0
                    select msgs;

                // Execute the query, and change the column values
                // you want to change.
                foreach (msg message in query)
                {
                    message.parentId = message.Id;
                    parentId = message.parentId;
                    // Insert any additional changes to column values.
                }

                // Submit the changes to the database.
                try
                {
                    _db.SubmitChanges();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    // Provide for exceptions.
                }
            }


            MessageModel outgoing = GetMessageByParentId(parentId);
            return outgoing;
        }

        /// <summary>
        ///     Gets message
        /// </summary>
        /// <param name="parent">Parent id</param>
        /// <returns>Message</returns>
        public MessageModel GetMessageByParentId(int parent)
        {
            MessageModel msg = (from msgs in _db.msgs
                                where msgs.parentId == parent
                                select new MessageModel
                                    {
                                        Id = msgs.Id,
                                        ToId = msgs.teamToId,
                                        ToName = msgs.teamToName,
                                        FromId = msgs.teamFromId,
                                        FromName = msgs.teamFromName,
                                        ParentId = msgs.parentId,
                                        Subject = msgs.msgSubject,
                                        Text = msgs.msgText,
                                        Time = (DateTime) msgs.msgTime
                                    }).ToList().Last();

            return msg;
        }

        /// <summary>
        ///     Gets all messages for team
        /// </summary>
        /// <param name="teamId">To or from id</param>
        /// <returns>Messages</returns>
        public List<MessageModel> GetMessages(int teamId)
        {
            List<MessageModel> messages = (from msgs in _db.msgs
                                           where msgs.teamFromId == teamId
                                                 || msgs.teamToId == teamId
                                           orderby msgs.Id
                                           select new MessageModel
                                               {
                                                   Id = msgs.Id,
                                                   ToId = msgs.teamToId,
                                                   ToName = msgs.teamToName,
                                                   FromId = msgs.teamFromId,
                                                   FromName = msgs.teamFromName,
                                                   ParentId = msgs.parentId,
                                                   Subject = msgs.msgSubject,
                                                   Text = msgs.msgText,
                                                   Time = (DateTime) msgs.msgTime
                                               }).ToList();

            return messages;
        }
    }
}