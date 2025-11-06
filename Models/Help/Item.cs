namespace TP2.Models.Help
{
    public class Item
    {
        public int quantite { get; set; }
        private int _ProduitId;
        public CarListing _carListing = null;
        public CarListing CarListing
        {
            get { return _carListing; }
            set { _carListing = value; }
        }
        public string Description
        {
            get { return _carListing.Name; }
        }
        public decimal UnitPrice
        {
            get { return _carListing.Price; }
        }
        public int? categoryId
        {
            get { return _carListing.CategoryId; }
        }
        public Category? category
        {
            get { return _carListing.Category; }
        }
        public decimal TotalPrice
        {
            get { return _carListing.Price * quantite; }
        }
        public Item(CarListing c) {
            this.CarListing = c;
        }

        public bool Equals(Item item)
        {
            return item.CarListing.ProductId == this.CarListing.ProductId;
        }
    }
}
