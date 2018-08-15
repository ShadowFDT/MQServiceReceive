using System;
using Xunit;

namespace XUnitTestReceive
{
    public class ReceiveUnitTest
    {
        [Fact]
        public void ValidateMessage_Delimited_NameEquals()
        {
            //Arrange
            var rmq = new Receive.RMQService();
            //Act
            string result = rmq.ValidateMessage("Hello my name is, Luke");
            //Assert
            Assert.Equal("Luke", result);
        }

        [Fact]
        public void ValidateMessage_NotDelimited_NameNotEquals()
        {
            //Arrange
            var rmq = new Receive.RMQService();
            //Act
            string result = rmq.ValidateMessage("Hello my name is Luke");
            //Assert
            Assert.NotEqual("Luke", result);
        }

        [Fact]
        public void ValidateMessage_NotDelimited_NameEmpty()
        {
            //Arrange
            var rmq = new Receive.RMQService();
            //Act
            string result = rmq.ValidateMessage("");
            //Assert
            Assert.Equal("", result);
        }

        [Fact]
        public void ValidateMessage_NotDelimited_NameSpace()
        {
            //Arrange
            var rmq = new Receive.RMQService();
            //Act
            string result = rmq.ValidateMessage(" ");
            //Assert
            Assert.Equal("", result);
        }
    }
}
