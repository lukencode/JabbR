﻿using JabbR.Models;

namespace JabbR.Services
{
    public interface IChatService
    {
        // Users
        ChatUser AddUser(string userName, string clientId, string password);
        void AddClient(ChatUser user, string clientId);
        void AuthenticateUser(string userName, string password);
        void ChangeUserName(ChatUser user, string newUserName);
        void ChangeUserPassword(ChatUser user, string oldPassword, string newPassword);
        void SetUserPassword(ChatUser user, string password);
        void UpdateActivity(ChatUser user, string clientId);
        ChatUser DisconnectClient(string clientId);

        // Rooms
        ChatRoom AddRoom(ChatUser user, string roomName);
        void JoinRoom(ChatUser user, ChatRoom room, string inviteCode);
        void LeaveRoom(ChatUser user, ChatRoom room);
        void SetInviteCode(ChatUser user, ChatRoom room, string inviteCode);

        // Messages
        ChatMessage AddMessage(ChatUser user, ChatRoom room, string content);

        // Owner commands
        void AddOwner(ChatUser user, ChatUser targetUser, ChatRoom targetRoom);
        void RemoveOwner(ChatUser user, ChatUser targetUser, ChatRoom targetRoom);
        void KickUser(ChatUser user, ChatUser targetUser, ChatRoom targetRoom);
        void AllowUser(ChatUser user, ChatUser targetUser, ChatRoom targetRoom);
        void UnallowUser(ChatUser user, ChatUser targetUser, ChatRoom targetRoom);
        void LockRoom(ChatUser user, ChatRoom targetRoom);
        void CloseRoom(ChatUser user, ChatRoom targetRoom);
    }
}
