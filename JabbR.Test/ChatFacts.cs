﻿using System.Collections.Specialized;
using System.Security.Principal;
using JabbR.ContentProviders.Core;
using JabbR.Models;
using JabbR.Services;
using Moq;
using Newtonsoft.Json;
using SignalR;
using SignalR.Abstractions;
using SignalR.Hubs;
using Xunit;

namespace JabbR.Test
{
    public class ChatFacts
    {
        public class Join
        {
            [Fact]
            public void CanJoinChat()
            {
                var clientState = new TrackingDictionary();
                string clientId = "1";
                var user = new ChatUser
                {
                    Id = "1234",
                    Name = "John"
                };

                TestableChat chat = GetTestableChat(clientId, clientState, user);
                chat.Caller.id = "1234";

                bool result = chat.Join();

                Assert.Equal("1234", clientState["id"]);
                Assert.Equal("John", clientState["name"]);
                Assert.True(result);
                // TODO: find out why these don't work
                //Assert.Equal(1, user.ConnectedClients.Count);
                //Assert.Equal("1", user.ConnectedClients.First().Id);

                chat.MockedConnection.Verify(m => m.Broadcast("Chat." + clientId, It.IsAny<object>()), Times.Once());
                chat.MockedChatService.Verify(c => c.UpdateActivity(user, clientId), Times.Once());
            }

            [Fact]
            public void MissingUsernameReturnsFalse()
            {
                var clientState = new TrackingDictionary();
                string clientId = "1";
                var user = new ChatUser();

                TestableChat chat = GetTestableChat(clientId, clientState, user);

                bool result = chat.Join();

                Assert.False(result);
            }

            [Fact]
            public void CanDeserializeClientState()
            {
                var clientState = new TrackingDictionary();
                string clientId = "1";
                var user = new ChatUser
                {
                    Id = "1234",
                    Name = "John"
                };

                var cookies = new NameValueCollection();
                cookies["jabbr.state"] = JsonConvert.SerializeObject(new ClientState { UserId = user.Id });


                TestableChat chat = GetTestableChat(clientId, clientState, user, cookies);

                bool result = chat.Join();

                Assert.Equal("1234", clientState["id"]);
                Assert.Equal("John", clientState["name"]);
                Assert.True(result);

                chat.MockedConnection.Verify(m => m.Broadcast("Chat." + clientId, It.IsAny<object>()), Times.Once());
                chat.MockedChatService.Verify(c => c.UpdateActivity(user, clientId), Times.Once());
            }
        }

        public static TestableChat GetTestableChat(string clientId, TrackingDictionary clientState, ChatUser user)
        {
            return GetTestableChat(clientId, clientState, user, new NameValueCollection());
        }

        public static TestableChat GetTestableChat(string clientId, TrackingDictionary clientState, ChatUser user, NameValueCollection cookies)
        {
            // setup things needed for chat
            var repository = new InMemoryRepository();
            var resourceProcessor = new Mock<IResourceProcessor>();
            var chatService = new Mock<IChatService>();
            var connection = new Mock<IConnection>();

            // add user to repository
            repository.Add(user);

            // create testable chat
            var chat = new TestableChat(resourceProcessor, chatService, repository, connection);
            var mockedConnectionObject = chat.MockedConnection.Object;

            // setup client agent
            chat.Agent = new ClientAgent(mockedConnectionObject, "Chat");

            var request = new Mock<IRequest>();
            request.Setup(m => m.Cookies).Returns(cookies);

            // setup signal agent
            var prinicipal = new Mock<IPrincipal>();
            chat.Caller = new SignalAgent(mockedConnectionObject, clientId, "Chat", clientState);

            // setup context
            chat.Context = new HubContext(new HostContext(request.Object, null, prinicipal.Object), clientId);

            return chat;
        }

        public class TestableChat : Chat
        {
            public Mock<IResourceProcessor> MockedResourceProcessor { get; private set; }
            public Mock<IChatService> MockedChatService { get; private set; }
            public IJabbrRepository Repository { get; private set; }
            public Mock<IConnection> MockedConnection { get; private set; }

            public TestableChat(Mock<IResourceProcessor> mockedResourceProcessor, Mock<IChatService> mockedChatService, IJabbrRepository repository, Mock<IConnection> connection)
                : base(mockedResourceProcessor.Object, mockedChatService.Object, repository)
            {
                MockedResourceProcessor = mockedResourceProcessor;
                MockedChatService = mockedChatService;
                Repository = repository;
                MockedConnection = connection;
            }
        }
    }
}
