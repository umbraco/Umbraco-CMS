using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Web.Services;
using umbraco.cms.businesslogic.member;

namespace umbraco.webservices.members
{
    [WebService(Namespace = "http://umbraco.org/webservices/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [ToolboxItem(false)]
    public class memberService : BaseWebService
    {

        override public Services Service
        {
            get
            {
                return Services.MemberService;
            }
        }


        /// <summary>
        /// Reads the user with the specified memberId
        /// </summary>
        /// <returns></returns>
        [WebMethod]
        public memberCarrier readByLogin(string memberLoginName, string memberPassword, string username, string password)
        {
            Authenticate(username, password);

            umbraco.cms.businesslogic.member.Member foundMember = umbraco.cms.businesslogic.member.Member.GetMemberFromLoginNameAndPassword(memberLoginName, memberPassword);
            if (foundMember == null)
                return null;

            return CreateMemberCarrier(foundMember);
        }

        /// <summary>
        /// Reads the user with the specified memberId
        /// </summary>
        /// <returns></returns>
        [WebMethod]
        public memberCarrier readById(int memberId, string username, string password)
        {
            Authenticate(username, password);

            umbraco.cms.businesslogic.member.Member foundMember = new umbraco.cms.businesslogic.member.Member(memberId);
            if (foundMember == null)
                return null;

            return CreateMemberCarrier(foundMember);
        }

        /// <summary>
        /// Reads the full list of members
        /// </summary>
        /// <returns></returns>
        [WebMethod]
        public List<memberCarrier> readList(string username, string password)
        {
            Authenticate(username, password);

            Member[] members = Member.GetAll;
            List<memberCarrier> Members = new List<memberCarrier>();

            foreach (umbraco.cms.businesslogic.member.Member member in members)
            {
                Members.Add(CreateMemberCarrier(member));
            }

            return Members;
        }


        [WebMethod]
        public void update(memberCarrier carrier, string username, string password)
        {
            Authenticate(username, password);

            // Some validation
            if (carrier.Id == 0) throw new Exception("ID must be specifed when updating");
            if (carrier == null) throw new Exception("No carrier specified");

            // Get the user
            umbraco.BusinessLogic.User user = GetUser(username, password);

            // We load the member
            Member member = new Member(carrier.Id);

            // We assign the new values:
            member.LoginName = carrier.LoginName;
            member.Text = carrier.DisplayedName;
            member.Email = carrier.Email;
            member.Password = carrier.Password;

            // We iterate the properties in the carrier
            if (carrier.MemberProperties != null)
            {
                foreach (memberProperty updatedproperty in carrier.MemberProperties)
                {
                    umbraco.cms.businesslogic.property.Property property = member.getProperty(updatedproperty.Key);
                    if (property != null)
                    {
                        property.Value = updatedproperty.PropertyValue;
                    }
                }
            }
        }


        [WebMethod]
        public int create(memberCarrier carrier, int memberTypeId, string username, string password)
        {
            Authenticate(username, password);

            // Some validation
            if (carrier == null) throw new Exception("No carrier specified");
            if (carrier.Id != 0) throw new Exception("ID cannot be specifed when creating. Must be 0");
            if (string.IsNullOrEmpty(carrier.DisplayedName)) carrier.DisplayedName = "unnamed";

            // we fetch the membertype
            umbraco.cms.businesslogic.member.MemberType mtype = new umbraco.cms.businesslogic.member.MemberType(memberTypeId);

            // Check if the membertype exists
            if (mtype == null) throw new Exception("Membertype " + memberTypeId + " not found");

            // Get the user that creates
            umbraco.BusinessLogic.User user = GetUser(username, password);

            // Create the new member
            umbraco.cms.businesslogic.member.Member newMember = umbraco.cms.businesslogic.member.Member.MakeNew(carrier.DisplayedName, mtype, user);

            // We iterate the properties in the carrier
            if (carrier.MemberProperties != null)
            {
                foreach (memberProperty updatedproperty in carrier.MemberProperties)
                {
                    umbraco.cms.businesslogic.property.Property property = newMember.getProperty(updatedproperty.Key);
                    if (property != null)
                    {
                        property.Value = updatedproperty.PropertyValue;
                    }
                }
            }
            return newMember.Id;
        }




        /// <summary>
        /// Deletes the document with the specified ID
        /// </summary>
        /// <param name="id"></param>
        /// <param name="cred"></param>
        /// <returns></returns>
        [WebMethod]
        public void delete(int id, string username, string password)
        {

            Authenticate(username, password);

            // We load the member
            umbraco.cms.businesslogic.member.Member deleteMember;
            try
            {
                deleteMember = new umbraco.cms.businesslogic.member.Member(id);
            }
            catch (Exception ex)
            {
                throw new Exception("Member not found" + ex.Message);
            }

            // We delete him (cruel world)
            try
            {
                deleteMember.delete();
            }
            catch (Exception ex)
            {
                throw new Exception("Member could not be deleted" + ex.Message);
            }
        }



        /// <summary>
        /// Creates container-object for member
        /// </summary>
        /// <param name="member"></param>
        /// <returns></returns>
        private memberCarrier CreateMemberCarrier(umbraco.cms.businesslogic.member.Member member)
        {
            memberCarrier carrier = new memberCarrier();

            carrier.Id = member.Id;

            carrier.LoginName = member.LoginName;
            carrier.DisplayedName = member.Text;
            carrier.Email = member.Email;
            carrier.Password = member.Password;

            carrier.MembertypeId = member.ContentType.Id;
            carrier.MembertypeName = member.ContentType.Text;

            // Adding groups to member-carrier
            IDictionaryEnumerator Enumerator;
            Enumerator = member.Groups.GetEnumerator();
            while (Enumerator.MoveNext())
            {
                memberGroup group = new memberGroup(Convert.ToInt32(Enumerator.Key), ((umbraco.cms.businesslogic.member.MemberGroup)Enumerator.Value).Text);
                carrier.Groups.Add(group);
            }

            // Loading properties to carrier
            var props = member.getProperties;
            foreach (umbraco.cms.businesslogic.property.Property prop in props)
            {
                memberProperty carrierprop = new memberProperty();

                if (prop.Value == System.DBNull.Value)
                {
                    carrierprop.PropertyValue = null;
                }
                else
                {
                    carrierprop.PropertyValue = prop.Value;
                }

                carrierprop.Key = prop.PropertyType.Alias;
                carrier.MemberProperties.Add(carrierprop);
            }
            return carrier;
        }

    }
}