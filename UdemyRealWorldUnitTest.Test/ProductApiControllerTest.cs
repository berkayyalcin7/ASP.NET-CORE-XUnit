using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UdemyRealWorldUnitTest.Web.Controllers;
using UdemyRealWorldUnitTest.Web.Helpers;
using UdemyRealWorldUnitTest.Web.Models;
using UdemyRealWorldUnitTest.Web.Repository;
using Xunit;

namespace UdemyRealWorldUnitTest.Test
{
    public class ProductApiControllerTest
    {

        private readonly Mock<IRepository<Product>> _mockRepo;

        private readonly ProductsApiController _apiController;

        private List<Product> products;

        private readonly Helper _helper;


        public ProductApiControllerTest()
        {
            _mockRepo = new Mock<IRepository<Product>>();
            _apiController = new ProductsApiController(_mockRepo.Object);
            products = new List<Product>() { new Product { ProductId = 3, Name = "Monitör", Price = 750, Stock = 15, Color = "Siyah" } };
            _helper = new Helper();
        }

        [Theory]
        [InlineData(4,5,9)]
        public void Add_PlusVariable_ReturnOkTotal(int a , int b,int total)
        {
            var result = _helper.add(a, b);

            Assert.Equal(total, result);
        }
       
        [Fact]
        public async void GetProduct_ActionExecutes_ReturnOkResultWithProduct()
        {
            _mockRepo.Setup(x => x.GetAll()).ReturnsAsync(products);

            var result = await _apiController.GetProduct();

            var redirect = Assert.IsType<OkObjectResult>(result);
            //Türetilmiş sınıflarda en uygun olanı AssignableFrom'u kullanmak .
            //Value üzerinden datayı alıyoruz .
            var returnProducts = Assert.IsAssignableFrom<IEnumerable<Product>>(redirect.Value);
            //1 değer var ise doğru
            Assert.Single(returnProducts.ToList());
        }

        [Theory]
        [InlineData(10)]
        public async void GetProduct_ProductNull_ReturnNotFound(int productId)
        {

            _mockRepo.Setup(x => x.GetById(productId)).ReturnsAsync((Product)null);

            var result = await _apiController.GetProduct(productId);

            Assert.IsType<NotFoundResult>(result);

        }

        [Theory]
        [InlineData(3)]
        public async void GetProduct_ProductValid_ReturnOkResult(int productId)
        {
            _mockRepo.Setup(x => x.GetById(productId)).ReturnsAsync(products.First(x => x.ProductId == productId));

            var result = await _apiController.GetProduct(productId);

            var redirect = Assert.IsType<OkObjectResult>(result);

            var returnProduct = Assert.IsType<Product>(redirect.Value);

            Assert.Equal(productId, returnProduct.ProductId);
        }

        [Theory]
        [InlineData(3)]
        public void PutProduct_IdIsNotEqualProductId_BadRequest(int productId)
        {
            var product = products.First(x => x.ProductId == productId);

            var result = _apiController.PutProduct(2, product);

            Assert.IsType<BadRequestResult>(result);
            
        }

        [Theory]
        [InlineData(3)]
        public void PutProduct_ActionExecutes_ReturnNoContent(int productId)
        {

            _mockRepo.Setup(x => x.Update(products.First(x => x.ProductId == productId)));

            var result = _apiController.PutProduct(productId,products.First(x => x.ProductId == productId));

            _mockRepo.Verify(x => x.Update(It.IsAny<Product>()), Times.Once);

            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async void PostProduct_ActionExecute_ReturnCreatedAtAction()
        {
            var product = products.First();
            _mockRepo.Setup(x => x.Create(product)).Returns(Task.CompletedTask);

            var result = await _apiController.PostProduct(product);

            var createdAtAction = Assert.IsType<CreatedAtActionResult>(result);
            //Herhangi bir Product Nesnesi ile 1 kez çalışırsa işlem doğru
            _mockRepo.Verify(x => x.Create(It.IsAny<Product>()), Times.Once);

            Assert.Equal("GetProduct", createdAtAction.ActionName);

        }

        [Theory]
        [InlineData(2)]
        public async void DeleteProduct_ProductNull_ReturnNotFound(int productId)
        {
            _mockRepo.Setup(x => x.GetById(productId)).ReturnsAsync((Product)null);

            var result = await _apiController.DeleteProduct(productId);

            var redirect = Assert.IsType<NotFoundResult>(result);

        }

        [Theory]
        [InlineData(3)]
        public async void DeleteProduct_ActionExecutes_ReturnNoContent(int productId)
        {
            //GetById ile mock işlemi uygulamaz isek Product Null dönecektir
            _mockRepo.Setup(x => x.GetById(productId)).ReturnsAsync(products.First(x=>x.ProductId==productId));

            _mockRepo.Setup(x => x.Delete(products.First(x => x.ProductId == productId)));

            var result = await _apiController.DeleteProduct(productId);

            var redirect = Assert.IsType<NoContentResult>(result);

            _mockRepo.Verify(x => x.Delete(It.IsAny<Product>()), Times.Once);
        }

        




    }
}
