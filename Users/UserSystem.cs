using ComputerUtils.Encryption;
using ComputerUtils.Logging;
using ComputerUtils.RandomExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ModUploadSite.Users
{
    public class UserSystem
    {
        public static EmailClient emailClient = null;
        public static Config config { get
            {
                return MUSEnvironment.config;
            } }
        public static void Initialize()
        {
            emailClient = new EmailClient(config.emailSender, config.emailPassword, config.emailServer);
        }

        public static GenericRequestResponse Login(string user)
        {
            try
            {
                User auth = MongoDBInteractor.GetUser(JsonSerializer.Deserialize<User>(user));
                if (auth == null)
                {
                    return new GenericRequestResponse(400, "Wrong password and/or username. Upper and lower case is taken into account for both password and username");
                }
                else
                {
                    string token = RandomExtension.CreateToken();
                    auth.currentToken = Hasher.GetSHA256OfString(token);
                    MongoDBInteractor.UpdateUser(auth);
                    return new GenericRequestResponse(200, auth.currentToken);
                }
            }
            catch (Exception e)
            {
                Logger.Log("Exception while trying to logging in:\n" + e, LoggingType.Error);
                return new GenericRequestResponse(500, "An unknown error occurred: " + e.Message);
            }
        }

        public static GenericRequestResponse GetLoggedInUser(string token)
        {
            User auth = MongoDBInteractor.GetUserByToken(token);
            if (auth == null) return new GenericRequestResponse(200, "false");
            auth.password = "";
            auth.currentPasswordResetToken = "";
            auth.currentToken = "";
            return new GenericRequestResponse(200, "true");
        }

        public static GenericRequestResponse GetUserByToken(string token)
        {
            User auth = MongoDBInteractor.GetUserByToken(token);
            if (auth == null) return new GenericRequestResponse(200, "false");
            auth.password = "";
            auth.currentPasswordResetToken = "";
            auth.currentToken = "";
            return new GenericRequestResponse(200, JsonSerializer.Serialize(auth));
        }

        public static GenericRequestResponse RequestPasswordReset(string user)
        {
            User auth = null;
            try
            {
                auth = JsonSerializer.Deserialize<User>(user);
            }
            catch (Exception e)
            {
                return new GenericRequestResponse(400, "Expected json");
            }
            User found = MongoDBInteractor.GetUserByUsernameAndEmail(auth);
            if (found == null) return new GenericRequestResponse(404, "This user does not exist. Upper and lower case is taken into account for usernames");

            string token = RandomExtension.CreateToken();
            found.currentPasswordResetToken = Hasher.GetSHA256OfString(token);
            found.passwordResetRequestTime = DateTime.UtcNow;
            MongoDBInteractor.UpdateUser(found);
            if (!emailClient.SendEmail(found.email, "Reset password", "You requested a password reset on " + MUSEnvironment.config.publicAddress + " . Click this link to reset your password (Due to security this link will expire in 15 minutes): " + MUSEnvironment.config.publicAddress + "confirmpasswordreset?token=" + token + "&username=" + found.username + "&email=" + found.email + "\n\nDidn't request a password reset? Safely ignore this email."))
            {
                return new GenericRequestResponse(500, "We were unable to send an email to you. Please try again later");
            }
            return new GenericRequestResponse(200, "Reset email sent. Check your inbox and spam for an email by " + MUSEnvironment.config.emailSender);
        }

        public static GenericRequestResponse ResetPasswordConfirmed(string user)
        {
            User auth = null;
            try
            {
                auth = JsonSerializer.Deserialize<User>(user);
            }
            catch (Exception e)
            {
                return new GenericRequestResponse(400, "Expected JSON");
            }
            User found = MongoDBInteractor.GetUserByUsernameAndEmail(auth);
            if (found == null) return new GenericRequestResponse(404, "This user does not exist. Upper and lower case is taken into account for usernames");

            if (found.currentPasswordResetToken != Hasher.GetSHA256OfString(auth.currentPasswordResetToken) || found.currentPasswordResetToken == "")
            {
                return new GenericRequestResponse(400, "You did not request a password reset. How did you even end up here???");
            }
            if ((DateTime.UtcNow - found.passwordResetRequestTime).TotalMinutes >= 15)
            {
                return new GenericRequestResponse(400, "The link expired. Request a new one");
            }
            if (!emailClient.SendEmail(found.email, "Your password has been changed", "Your password for " + MUSEnvironment.config.publicAddress + " has been changed. If you did not change it request a password reset now! To reset your password go here " + config.publicAddress + "requestpasswordreset"))
            {
                return new GenericRequestResponse(500, "We were unable to send an email to you. Please try again later");
            }
            found.password = Hasher.GetSHA256OfString(auth.password);
            found.currentPasswordResetToken = "";
            found.currentToken = "";
            MongoDBInteractor.UpdateUser(found);
            return new GenericRequestResponse(200, "Your password has been successfully changed");
        }

        public static GenericRequestResponse Register(string user)
        {
            try
            {
                User newUser = JsonSerializer.Deserialize<User>(user);
                if (newUser.email == "" || newUser.username == "" || newUser.password == "")
                {
                    return new GenericRequestResponse(400, "Fill out all fields!");
                }
                if (MongoDBInteractor.GetUserByUsername(newUser.username) != null)
                {
                    return new GenericRequestResponse(400, "A user with this username already exists");
                }
                newUser.password = Hasher.GetSHA256OfString(newUser.password);
                MongoDBInteractor.AddUser(newUser);
                return new GenericRequestResponse(200, "Created User");
            }
            catch (Exception e)
            {
                Logger.Log("Exception while trying to create user:\n" + e, LoggingType.Error);
                return new GenericRequestResponse(500, "There was an error creating the user: " + e.Message);
            }
        }
    }
}
