using TP2.Models;
using TP2.Models.Help;

namespace TP2.Models.Help
{
    public class ListeCart
    {
        public List<Item> Items { get; private set; }
        public static readonly ListeCart Instance;

        static ListeCart()
        {
            Instance = new ListeCart();
            Instance.Items = new List<Item>();
        }

        protected ListeCart() { }

        public void AddItem(CarListing carListing)
        {
            Boolean iswhat = false;
            // Create a new item to add to the cart
            foreach (Item a in Items)
            {
                if (a.CarListing.ProductId == carListing.ProductId)
                {
                    a.quantite++;
                    iswhat = true;
                    return;
                }
            }

            if (iswhat == false)
            {
                Item newItem = new Item(carListing);
                newItem.quantite = 1;
                Items.Add(newItem);
            }
        }

        public void setToNUll()
        { }

        public void SetLessOneItem(CarListing carListing)
        {
            foreach (Item a in Items)
            {
                if (a.CarListing.ProductId == carListing.ProductId)
                {
                    if (a.quantite <= 0)
                    {
                        RemoveItem(a.CarListing);
                        return;
                    }
                    else
                    {
                        a.quantite--;
                        return;
                    }
                }
            }
        }

        public void SetItemQuantity(CarListing carListing, int quantity)
        {
            if (quantity == 0)
            {
                RemoveItem(carListing);
                return;
            }

            foreach (Item a in Items)
            {
                if (a.CarListing.ProductId == carListing.ProductId)
                {
                    a.quantite = quantity;
                    return;
                }
            }
        }

        public void RemoveItem(CarListing carListing)
        {
            Item t = null;
            foreach (Item a in Items)
            {
                if (a.CarListing.ProductId == carListing.ProductId)
                {
                    t = a;
                }
            }
            if (t != null)
            {
                Items.Remove(t);
            }
        }

        public float GetSubTotal()
        {
            float subTotal = 0;
            foreach (Item i in Items)
                subTotal += (float)i.TotalPrice;
            return subTotal;
        }
    }
}