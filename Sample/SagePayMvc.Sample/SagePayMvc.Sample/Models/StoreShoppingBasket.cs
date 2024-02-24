using System.Text.Json;

namespace SagePayMvc.Sample.Models {
    using System;
    using System.Collections.Generic;
    using System.Web;
    using System.Linq;

    public class ShoppingBasketItem {
        public Product Product { get; set; }
        public int Id { get; set; }
        public int Quantity { get; set; }

        public decimal Price
        {
            get { return Product.Price * Quantity; }
        }
    }

    public class StoreShoppingBasket : IShoppingBasket {
        // This is not thread safe!
        // In reality, this would be generated by the database or your ORM tool.
        private static int _idCounter = 0;

        public ShoppingBasketItem[] GetItemsInBasket(HttpContext httpContext) {
            return GetBasketContents(httpContext).ToArray();
        }

        public void AddProduct(Product product, HttpContext httpContext) {
            var contents = GetBasketContents(httpContext);
            var currentItemForProduct = contents.SingleOrDefault(x => x.Product.Id == product.Id);

            if (currentItemForProduct != null) {
                currentItemForProduct.Quantity++;
            }
            else {
                // ID count is not thread safe. This is only here for demo purposes.
                // In reality, this would be generated by the database or your ORM tool.
                contents.Add(new ShoppingBasketItem { Id = ++_idCounter, Quantity = 1, Product = product });
            }

            httpContext.Session.Set("_basket", contents);
        }

        public void RemoveItem(int id, HttpContext httpContext) {
            var contents = GetBasketContents(httpContext);
            var item = contents.Single(x => x.Id == id);
            item.Quantity--;
            if (item.Quantity <= 0) {
                contents.Remove(item);
            }

            httpContext.Session.Set("_basket", contents);
        }

        private IList<ShoppingBasketItem> GetBasketContents(HttpContext httpContext) {
            // Note: We're using the Session here directly.
            // In a real app, the basket contents would probably be stored in the database

            var contents = httpContext.Session.Get<IList<ShoppingBasketItem>>("_basket"); // as IList<ShoppingBasketItem>;
            if (contents == null || contents.Count == 0) {
                contents = new List<ShoppingBasketItem>();
                httpContext.Session.Set("_basket", contents);
            }

            return contents;
        }
    }

    public interface IShoppingBasket {
        ShoppingBasketItem[] GetItemsInBasket(HttpContext httpContext);
        void AddProduct(Product product, HttpContext httpContext);
        void RemoveItem(int id, HttpContext httpContext);
    }
}

public static class SessionExtensions {
    public static void Set<T>(this ISession session, string key, T value) {
        session.SetString(key, JsonSerializer.Serialize(value));
    }

    public static T? Get<T>(this ISession session, string key) {
        var value = session.GetString(key);
        return value == null ? default : JsonSerializer.Deserialize<T>(value);
    }
}