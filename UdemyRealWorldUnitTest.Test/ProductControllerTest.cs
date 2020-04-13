using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UdemyRealWorldUnitTest.Web.Controllers;
using UdemyRealWorldUnitTest.Web.Models;
using UdemyRealWorldUnitTest.Web.Repository;
using Xunit;

namespace UdemyRealWorldUnitTest.Test
{
    public class ProductControllerTest
    {
        //Bi kere nesne örneği oluşturduktan sonra bir şey değiştirmeyeceğiz . Taklit Edeceğimiz Yer Mock kısmı
        private readonly Mock<IRepository<Product>> _mockRepo;

        // Asıl olarak ProductController
        private readonly ProductsController _controller;
        
        private List<Product> product;

        public ProductControllerTest()
        {
            _mockRepo = new Mock<IRepository<Product>>();
            _controller = new ProductsController(_mockRepo.Object);
            //Örnek olarak Product Nesnemizi oluşturduk .
            product = new List<Product>() { new Product { ProductId = 3, Name = "Monitör", Price = 750, Stock = 15, Color = "Siyah" } };
        }


        [Fact]
        public async void Index_ActionExecutes_View()
        {

            //Mocklamayı Setup yapmadığımız için Product Değerleri Boş gelecek ...
            //ViewResult durumu
            var result = await _controller.Index();
            //Gelen değerinin tipinin ViewResult olması gerekiyor bunu Test Ediyoruz .. 
            Assert.IsType<ViewResult>(result);

        }

        [Fact]
        public async void Index_ActionExecutes_ReturnProductList()
        {
            //Yukarıdaki product Örneğimizi dönelim 
            //Mock ile VT'ye bağlanmak yerine sanal ortamda deneyerek daha hızlı çalıştırılmasını sağladık ...
             _mockRepo.Setup(x => x.GetAll()).ReturnsAsync(product);

            //Hem tip Kontrolü hem Değerleri döndük
            var result = await _controller.Index();
            var viewResult = Assert.IsType<ViewResult>(result);
            //IsAssignableFrom ile Hem Obje tiplerinin türetilmiş tip olup olmadığını anlıyoruz 
            var productList = Assert.IsAssignableFrom<IEnumerable<Product>>(viewResult.Model);
            //Ürün Sayımız 1 'idi Single ile Tek değer gelirse TEst başarılı geçecektir .
            Assert.Single(productList);
        }

        [Fact]
        public async void Detail_IdIsNull_ReturnRedirectToIndex()
        {
            //Detay sayfasını çağırıyoruz ve null gönderiyoruz
            var result = await _controller.Details(null);
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            //redirect'in Dönüş değeri Index ile karşılaştırıyoruz ...
            Assert.Equal("Index", redirect.ActionName);
        }

        [Fact]
        public async void Detail_ProductIsNull_ReturnNotFound()
        {
            //null dönecek ama Product'tipinde -> Id 0 olur ise
            _mockRepo.Setup(x => x.GetById(0)).ReturnsAsync((Product)null);
            var result = await _controller.Details(0);
            var redirect = Assert.IsType<NotFoundResult>(result);
            // Http 404 hatası dönmesi gerekiyor
            Assert.Equal<int>(404, redirect.StatusCode);
        }

        [Theory]
        [InlineData(3)]
        public async void Detail_ValidId_ReturnProduct(int productId)
        {
            Product prdc = product.First(x => x.ProductId == productId);
            _mockRepo.Setup(x => x.GetById(productId)).ReturnsAsync(prdc);

            var result = await _controller.Details(productId);

            var viewResult = Assert.IsType<ViewResult>(result);

            var resultProduct = Assert.IsAssignableFrom<Product>(viewResult.Model);

            //Eşitlik kontrolleri
            Assert.Equal(prdc.ProductId, resultProduct.ProductId);
            Assert.Equal(prdc.Name, resultProduct.Name);
        }

        [Fact]
        public void Create_ActionExecutes_View()
        {
            var result = _controller.Create();
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public async void CreatePOST_InValidModelState_ReturnView()
        {
            _controller.ModelState.AddModelError("Name", "İsim Alanı Gereklidir");
            //Hataya düşeceğinden dolayı çalışmayacak Test Başarılı
            var result = await _controller.Create(product.First());

            var viewResult = Assert.IsType<ViewResult>(result);
            //viewResult Modelinin Product Olup olmadığı kontrolü
            Assert.IsType<Product>(viewResult.Model);
        }

        [Fact]
        public async void CreatePOST_ValidModelState_ReturnIndex()
        {
            //Burada Veri dolduğu zaman Index'e dönüp dönmediğini test ediceğiz Create Metodunun çalışıp çalışmadığına bakmayacağız 
            //Gereksiz yere Create metodunu mocklamıyoruz
            //İlk Kaydı veriyoruz - Kayıt işlemi önemli değil
            var result = await _controller.Create(product.First());
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            //Action Name ile kontrolü sağladık
            Assert.Equal("Index", redirect.ActionName);
        }

        [Fact]
        public async void CreatePOST_ValidModelState_CreateMethodExecute()
        {
            Product createProduct = null;
            //It.IsAny ile herhangi bir gelen product nesnesini callback yardımıyla createproduct'a aktarıyoruz
            _mockRepo.Setup(x => x.Create(It.IsAny<Product>())).Callback<Product>(x => createProduct = x);

            //Yukarıdaki x Buradaki product.First()'den gelecek olan değer
            var result = await _controller.Create(product.First());
            //Metodun çalışop çalışmadığını kontrol ediyoruz . 1 kez çalıştıise
            _mockRepo.Verify(x => x.Create(It.IsAny<Product>()), Times.Once);
            //Id'ler eşit ise doğru sonucunu alıyoruz .
            Assert.Equal(product.First().ProductId, createProduct.ProductId);
        }

        [Fact]
        public async void CreatePOST_IsValidModelState_NeverCreateMethodExecute()
        {
            _controller.ModelState.AddModelError("", "Hata Tespit Edildi");

            var result = await _controller.Create(product.First());
            //Asla çalışmaması gerekiyor Hata yakalandığında bunu kontrol ediyoruz
            _mockRepo.Verify(x => x.Create(It.IsAny<Product>()), Times.Never);

        }


        [Fact]
        public async void Edit_IsNull_RedirectActionIndex()
        {
            //id olarak null
            var result = await _controller.Edit(null);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            //Index sayfasına dönüyor mu
            Assert.Equal("Index", redirect.ActionName);

        }

        [Theory]
        [InlineData(5)]
        public async void Edit_ProductNull_ReturnNotFound(int productId)
        {
            Product prdc = null;
            _mockRepo.Setup(x => x.GetById(productId)).ReturnsAsync(prdc);

            var result = await _controller.Edit(productId);

            var redirect = Assert.IsType<NotFoundResult>(result);

            Assert.Equal(404, redirect.StatusCode);
        }

        [Theory]
        [InlineData(3)]
        public async void Edit_ProductSuccess_ReturnProduct(int productId)
        {
            var _product = product.First(x => x.ProductId == productId);

            _mockRepo.Setup(x => x.GetById(productId)).ReturnsAsync(_product);

            var result = await _controller.Edit(productId);

            var viewResult = Assert.IsType<ViewResult>(result);

            var resultProduct = Assert.IsAssignableFrom<Product>(viewResult.Model);

            Assert.Equal(_product.ProductId, resultProduct.ProductId);

            Assert.Equal(_product.Name, resultProduct.Name);


        }


        [Theory]
        [InlineData(3)]
        public void EditPost_IdIsNotEqualProduct_ReturnNotFound(int productId)
        {
            var result = _controller.Edit(1, product.First(x => x.ProductId == productId));

            var redirect = Assert.IsType<NotFoundResult>(result);      

        }

        [Theory]
        [InlineData(3)]
        public void EditPost_InValidModelState_ReturnView(int productId)
        {
            _controller.ModelState.AddModelError("Name", "İsim Alanı Hatalı");
            //Burda id ksıımları doğru olacak
            var result = _controller.Edit(3, product.First(x => x.ProductId == productId));

            var redirect = Assert.IsType<ViewResult>(result);
        }

        [Theory]
        [InlineData(3)]
        public void EditPost_ValidModelState_ReturnRedirectToIndex(int productId)
        {
            var result = _controller.Edit(productId, product.First(x => x.ProductId == productId));

            var redirect = Assert.IsType<RedirectToActionResult>(result);

            Assert.Equal("Index", redirect.ActionName);
        }

        [Theory]
        [InlineData(3)]
        public void EditPost_ValidModelState_UpdateProduct(int productId)
        {
            var productResult = product.First(x => x.ProductId == productId);
            _mockRepo.Setup(x => x.Update(productResult));

            _controller.Edit(productId, productResult);
            //Update işlemi olduğunda 1 kere çalışacaktır.
            _mockRepo.Verify(x => x.Update(It.IsAny<Product>()), Times.Once);

        }

        [Fact]
        public async void Delete_IdIsNull_ReturnNotFound()
        {
            var result = await _controller.Delete(null);
            var redirect = Assert.IsType<NotFoundResult>(result);
        }

        [Theory]
        [InlineData(1)]
        public async void Delete_ProductNull_ReturnNotFound(int productId)
        {
            _mockRepo.Setup(x => x.GetById(productId)).ReturnsAsync((Product)null);

            var result = await _controller.Delete(productId);

            var redirect = Assert.IsType<NotFoundResult>(result);
        }

        [Theory]
        [InlineData(3)]
        public async void Delete_ActionExecute_ReturnView(int productId)
        {

            _mockRepo.Setup(x => x.GetById(productId)).ReturnsAsync(product.First(x => x.ProductId == productId));

            var result = await _controller.Delete(productId);

            var viewResult = Assert.IsType<ViewResult>(result);

            var deger = Assert.IsAssignableFrom<Product>(viewResult.Model);

            Assert.Equal(productId, deger.ProductId);


        }
        [Theory]
        [InlineData(5)]
        public async void DeleteConfirmed_ActionExecutes_ReturnIndex(int productId)
        {   
            var result = await _controller.DeleteConfirmed(productId);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
        }

        [Theory]
        [InlineData(3)]
        public async void DeleteConfirmed_ActionExecutes_ProductDelete(int productId)
        {
            _mockRepo.Setup(x => x.Delete(product.First(x => x.ProductId == productId)));

            await _controller.DeleteConfirmed(productId);
            // herhangi bir product ve 1 kez çalışabilir ...
            _mockRepo.Verify(x => x.Delete(It.IsAny<Product>()), Times.Once);


        }






    }
}
