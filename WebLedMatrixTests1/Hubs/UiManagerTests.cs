﻿using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using NSubstitute;
using WebLedMatrix;
using WebLedMatrix.Hubs;
using WebLedMatrix.Logic.Authentication.Abstract;
using WebLedMatrix.Logic.Authentication.Models;
using Xunit;

namespace WebLedMatrixTests1.Hubs
{
    public class UiManagerTests : BaseTest
    {
        private readonly ILoginStatusChecker _loginStatusChecker = new LoginStatusChecker();


        static IRequest getIdentityRequest(bool isAuthenticated, bool isAdministrator)
        {
            var request = Substitute.For<IRequest>();

            request.User.When(x=>x.IsInRole("Administrators")).DoNotCallBase();
            request.User.IsInRole("Administrators").Returns(isAdministrator);
            request.User.Identity.IsAuthenticated.Returns(isAuthenticated);

            return request;
        }

        public delegate void showSectionsDelegate(bool matrixesSection, bool sendingSection, bool administrationSection);

        public void CoreAccountTest(State expectedState, IRequest identityRequest)
        {

            var matrixManager = Substitute.For<MatrixManager>();
            matrixManager.When(x=>x.UpdateMatrices()).DoNotCallBase();

            UiManagerHub managerHub = Substitute.For<UiManagerHub>(_loginStatusChecker, matrixManager);
            managerHub.Context = new HubCallerContext(identityRequest,"1");
            managerHub.Clients = Substitute.For<IHubCallerConnectionContext<IUiManagerHub>>();

            string result = "";
            managerHub.Clients.When(x => { var r = x.Caller; }).DoNotCallBase();
            managerHub.Clients.Caller.WhenForAnyArgs(x=>x.loginStatus("")).Do(x=> { result = x[0].ToString(); });

            managerHub.LoginStatus();
            Assert.Equal(expectedState.ToString(),result);
        }

        [Fact()]
        public void NotLoggedCaseTest()
        {
            CoreAccountTest(State.NotLogged, getIdentityRequest(false, false));
        }

        [Fact()]
        public void LoggedCaseTest()
        {
            CoreAccountTest(State.Logged, getIdentityRequest(true, false));
        }

        [Fact()]
        public void AdminCaseTest()
        {
            CoreAccountTest(State.Admin, getIdentityRequest(true, true));
        }
    }
}