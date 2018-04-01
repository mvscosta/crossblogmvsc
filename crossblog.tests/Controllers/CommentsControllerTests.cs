using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using crossblog.Controllers;
using crossblog.Domain;
using crossblog.Model;
using crossblog.Repositories;
using FizzWare.NBuilder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace crossblog.tests.Controllers
{
    public class CommentsControllerTests
    {
        private CommentsController _commentsController;

        private Mock<IArticleRepository> _articleRepositoryMock = new Mock<IArticleRepository>();
        private Mock<ICommentRepository> _commentRepositoryMock = new Mock<ICommentRepository>();

        public CommentsControllerTests()
        {
            _commentsController = new CommentsController(_articleRepositoryMock.Object, _commentRepositoryMock.Object);
        }

        [Fact]
        public async Task Get_Comment_NotFound()
        {
            // Arrange
            _articleRepositoryMock.Setup(m => m.GetAsync(1)).Returns(Task.FromResult<Article>(null));
            _commentRepositoryMock.Setup(m => m.GetAsync(1)).Returns(Task.FromResult<Comment>(null));

            // Act
            var result = await _commentsController.Get(1);

            // Assert
            Assert.NotNull(result);

            var objectResult = result as NotFoundResult;
            Assert.NotNull(objectResult);
        }

        [Fact]
        public async Task Get_Comment_ByArticleId_ReturnsItem()
        {
            // Arrange
            _articleRepositoryMock.Setup(m => m.GetAsync(2)).Returns(Task.FromResult<Article>(Builder<Article>.CreateNew().Build()));
            var commentDbSetMock = Builder<Comment>.CreateListOfSize(3).Build().ToAsyncDbSetMock();
            _commentRepositoryMock.Setup(m => m.Query()).Returns(commentDbSetMock.Object);

            // Act
            var result = await _commentsController.Get(2);

            // Assert
            Assert.NotNull(result);

            var objectResult = result as OkObjectResult;
            Assert.NotNull(objectResult);

            var content = objectResult.Value as CommentListModel;
            Assert.NotNull(content);

            Assert.Single(content.Comments);
        }
        
        [Fact]
        public async Task Get_Comment_ByArticleIdCommentId_ReturnsItem()
        {
            // Arrange
            _articleRepositoryMock.Setup(m => m.GetAsync(1)).Returns(Task.FromResult<Article>(Builder<Article>.CreateNew().Build()));
            var commentDbSetMock = Builder<Comment>.CreateListOfSize(2).Build().ToAsyncDbSetMock();
            _commentRepositoryMock.Setup(m => m.Query()).Returns(commentDbSetMock.Object);

            // Act
            var result = await _commentsController.Get(1, 1);

            // Assert
            Assert.NotNull(result);

            var objectResult = result as OkObjectResult;
            Assert.NotNull(objectResult);

            var content = objectResult.Value as CommentModel;
            Assert.NotNull(content);

            Assert.Equal("Title1", content.Title);
        }

        [Fact]
        public async Task Post_Comment_New()
        {
            // Arrange
            _articleRepositoryMock.Setup(m => m.GetAsync(1)).Returns(Task.FromResult<Article>(Builder<Article>.CreateNew().Build()));
            var commentDbSetMock = Builder<Comment>.CreateNew().Build();
            _commentRepositoryMock.Setup(m => m.InsertAsync(commentDbSetMock));
            var commentModel = new CommentModel
            {
                Id = 1,
                Email = "EmailNewer",
                Title = "TitleNewer",
                Content = "ContentNewer",
                Published = commentDbSetMock.Published
            };

            // Act
            var result = await _commentsController.Post(1, commentModel);

            // Assert
            Assert.NotNull(result);

            var objectResult = result as CreatedResult;
            Assert.NotNull(objectResult);

            var content = objectResult.Value as CommentModel;
            Assert.NotNull(content);

            Assert.Equal(commentModel.Email, content.Email);
            Assert.Equal(commentModel.Title, content.Title);
            Assert.Equal(commentModel.Content, content.Content);
            Assert.Equal(DateTime.UtcNow.ToLocalTime().Date, content.Date.Date);
            Assert.Equal(commentModel.Published, content.Published);
        }
    }
}
