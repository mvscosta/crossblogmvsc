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
    public class ArticlesControllerTests
    {
        private ArticlesController _articlesController;

        private Mock<IArticleRepository> _articleRepositoryMock = new Mock<IArticleRepository>();

        public ArticlesControllerTests()
        {
            _articlesController = new ArticlesController(_articleRepositoryMock.Object);
        }

        [Fact]
        public async Task Search_Article_ReturnsEmptyList()
        {
            // Arrange
            var articleDbSetMock = Builder<Article>.CreateListOfSize(3).Build().ToAsyncDbSetMock();
            _articleRepositoryMock.Setup(m => m.Query()).Returns(articleDbSetMock.Object);

            // Act
            var result = await _articlesController.Search("Invalid");

            // Assert
            Assert.NotNull(result);

            var objectResult = result as OkObjectResult;
            Assert.NotNull(objectResult);

            var content = objectResult.Value as ArticleListModel;
            Assert.NotNull(content);

            Assert.Empty(content.Articles);
        }

        [Fact]
        public async Task Search_Articles_ReturnsList()
        {
            // Arrange
            var articleDbSetMock = Builder<Article>.CreateListOfSize(3).Build().ToAsyncDbSetMock();
            _articleRepositoryMock.Setup(m => m.Query()).Returns(articleDbSetMock.Object);

            // Act
            var result = await _articlesController.Search("Title");

            // Assert
            Assert.NotNull(result);

            var objectResult = result as OkObjectResult;
            Assert.NotNull(objectResult);

            var content = objectResult.Value as ArticleListModel;
            Assert.NotNull(content);

            Assert.Equal(3, content.Articles.Count());
        }

        [Fact]
        public async Task Get_Article_NotFound()
        {
            // Arrange
            _articleRepositoryMock.Setup(m => m.GetAsync(1)).Returns(Task.FromResult<Article>(null));

            // Act
            var result = await _articlesController.Get(1);

            // Assert
            Assert.NotNull(result);

            var objectResult = result as NotFoundResult;
            Assert.NotNull(objectResult);
        }

        [Fact]
        public async Task Get_Article_ReturnsItem()
        {
            // Arrange
            _articleRepositoryMock.Setup(m => m.GetAsync(1)).Returns(Task.FromResult<Article>(Builder<Article>.CreateNew().Build()));

            // Act
            var result = await _articlesController.Get(1);

            // Assert
            Assert.NotNull(result);

            var objectResult = result as OkObjectResult;
            Assert.NotNull(objectResult);

            var content = objectResult.Value as ArticleModel;
            Assert.NotNull(content);

            Assert.Equal("Title1", content.Title);
        }

        [Fact]
        public async Task Post_Article_New()
        {
            // Arrange
            var articleDbSetMock = Builder<Article>.CreateNew().Build();
            _articleRepositoryMock.Setup(m => m.InsertAsync(articleDbSetMock));
            var articleModel = new ArticleModel
            {
                Title = articleDbSetMock.Title,
                Content = articleDbSetMock.Content,
                Published = articleDbSetMock.Published
            };

            // Act
            var result = await _articlesController.Post(articleModel);

            // Assert
            Assert.NotNull(result);

            var objectResult = result as CreatedResult;
            Assert.NotNull(objectResult);

            var content = objectResult.Value as ArticleModel;
            Assert.NotNull(content);

            Assert.Equal(articleModel.Title, content.Title);
            Assert.Equal(articleModel.Content, content.Content);
            Assert.Equal(DateTime.UtcNow.ToLocalTime().Date, content.Date.Date);
            Assert.Equal(articleModel.Published, content.Published);
        }
        
        [Fact]
        public async Task Put_Article_Edit()
        {
            // Arrange
            _articleRepositoryMock.Setup(m => m.GetAsync(1)).Returns(Task.FromResult<Article>(Builder<Article>.CreateNew().Build()));
            var articleDbSetMock = Builder<Article>.CreateNew().Build();
            _articleRepositoryMock.Setup(m => m.UpdateAsync(articleDbSetMock));
            var articleModel = new ArticleModel
            {
                Title = "TitleEdited",
                Content = "ContentEdited",
                Published = articleDbSetMock.Published
            };

            // Act
            var result = await _articlesController.Put(1, articleModel);

            // Assert
            Assert.NotNull(result);

            var objectResult = result as OkObjectResult;
            Assert.NotNull(objectResult);

            var content = objectResult.Value as ArticleModel;
            Assert.NotNull(content);

            Assert.Equal(articleModel.Title, content.Title);
            Assert.Equal(articleModel.Content, content.Content);
            Assert.Equal(DateTime.UtcNow.ToLocalTime().Date, content.Date.Date);
            Assert.Equal(articleModel.Published, content.Published);
        }

        [Fact]
        public async Task Delete_Article_Delete()
        {
            // Arrange
            _articleRepositoryMock.Setup(m => m.GetAsync(5)).Returns(Task.FromResult<Article>(Builder<Article>.CreateNew().Build()));

            var articleDbSetMock = Builder<Article>.CreateNew().Build();
            articleDbSetMock.Id = 5;

            _articleRepositoryMock.Setup(m => m.DeleteAsync(articleDbSetMock));
            
            // Act
            var result = await _articlesController.Delete(5);

            // Assert
            Assert.NotNull(result);

            var okResult = result as OkResult;
            Assert.NotNull(okResult);

            Assert.Equal(200, okResult.StatusCode);
        }
    }
}
