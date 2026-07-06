using System;
using System.Collections.Generic;
using System.Linq; // Para .Contains() y .Select()

namespace LivriaBackend.commerce.Domain.Model.Aggregates
{
    /// <summary>
    /// Representa la entidad agregada 'Libro' en el dominio de comercio.
    /// Un <see cref="Book"/> es un objeto con una identidad global.
    /// </summary>
    public class Book
    {
        /// <summary>
        /// Obtiene el identificador único del libro.
        /// </summary>
        public int Id { get; private set; }

        /// <summary>
        /// Obtiene el título del libro.
        /// </summary>
        public string Title { get; private set; }

        /// <summary>
        /// Obtiene la descripción detallada del libro.
        /// </summary>
        public string Description { get; private set; }

        /// <summary>
        /// Obtiene el autor del libro.
        /// </summary>
        public string Author { get; private set; }

        /// <summary>
        /// Obtiene el precio de venta del libro (165% del precio de compra).
        /// </summary>
        public decimal SalePrice { get; private set; }

        /// <summary>
        /// Obtiene el precio de compra del libro (generado aleatoriamente por género).
        /// </summary>
        public decimal PurchasePrice { get; private set; }

        /// <summary>
        /// Obtiene o establece la cantidad de stock disponible del libro.
        /// Este valor es mutable a través de métodos de comportamiento.
        /// </summary>
        public int Stock { get; private set; }

        /// <summary>
        /// Obtiene la URL o ruta de la imagen de la portada del libro.
        /// </summary>
        public string Cover { get; private set; }

        /// <summary>
        /// Obtiene el género al que pertenece el libro.
        /// </summary>
        public string Genre { get; private set; }

        /// <summary>
        /// Obtiene el idioma en el que está escrito el libro.
        /// </summary>
        public string Language { get; private set; }
        
        /// <summary>
        /// Define el estado de activo o inactivo de un libro (soft delete)
        /// Mutable a través de los métodos de creación y eliminación
        /// </summary>
        public bool IsActive { get; private set; }
        
        private static readonly Random _random = new Random();
        
        public static readonly HashSet<string> AllowedGenres = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "literature", "non_fiction", "fiction", "mangas_comics", "juvenile", "children", "ebooks_audiobooks"
        };
        
        public static readonly HashSet<string> AllowedLanguages = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "english", "español"
        };


        /// <summary>
        /// Constructor protegido para uso de frameworks ORM (como Entity Framework Core).
        /// No debe ser utilizado directamente para la creación de instancias de <see cref="Book"/>.
        /// </summary>
        protected Book() { }

        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="Book"/> con los detalles proporcionados.
        /// El PurchasePrice se genera aleatoriamente según el género y el SalePrice es el 165% del PurchasePrice.
        /// </summary>
        /// <param name="title">El título del libro.</param>
        /// <param name="description">La descripción del libro.</param>
        /// <param name="author">El autor del libro.</param>
        /// <param name="stock">La cantidad inicial de stock disponible.</param>
        /// <param name="cover">La URL o ruta de la imagen de la portada.</param>
        /// <param name="genre">El género del libro.</param>
        /// <param name="language">El idioma del libro.</param>
        /// <exception cref="ArgumentException">Se lanza si el idioma no es 'english' o 'español' o el género no es válido.</exception>
        public Book(string title, string description, string author, int stock, string cover, string genre, string language)
        {
            if (string.IsNullOrEmpty(language) || !AllowedLanguages.Contains(language))
            {
                throw new ArgumentException("El idioma del libro debe ser 'english' o 'español'.", nameof(language));
            }

            if (string.IsNullOrEmpty(genre) || !AllowedGenres.Contains(genre))
            {
                throw new ArgumentException($"El género del libro debe ser uno de los siguientes: {string.Join(", ", AllowedGenres)}.", nameof(genre));
            }
            if (stock < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(stock), "El stock inicial no puede ser negativo.");
            }

            Title = title;
            Description = description;
            Author = author;
            Stock = stock;
            Cover = cover;
            Genre = genre;
            Language = language;
            
            PurchasePrice = GenerateRandomPurchasePrice(genre);
            
            SalePrice = Math.Round(PurchasePrice * 1.65m, 1, MidpointRounding.AwayFromZero);

            IsActive = true;
        }

        /// <summary>
        /// Genera un precio de compra aleatorio basado en el género del libro.
        /// </summary>
        /// <param name="genre">El género del libro.</param>
        /// <returns>El precio de compra aleatorio.</returns>
        private decimal GenerateRandomPurchasePrice(string genre)
        {
            decimal minPrice;
            decimal maxPrice;

            switch (genre.ToLower())
            {
                case "literature":
                    minPrice = 25m;
                    maxPrice = 35m;
                    break;
                case "non_fiction":
                case "fiction":
                    minPrice = 20m;
                    maxPrice = 30m;
                    break;
                case "mangas_comics":
                    minPrice = 15m;
                    maxPrice = 35m;
                    break;
                case "juvenile":
                    minPrice = 20m;
                    maxPrice = 25m;
                    break;
                case "children":
                    minPrice = 15m;
                    maxPrice = 20m;
                    break;
                case "ebooks_audiobooks":
                    minPrice = 20m;
                    maxPrice = 30m;
                    break;
                default:
                    minPrice = 10m; 
                    maxPrice = 20m;
                    break;
            }
            
            int randomInt = _random.Next(Convert.ToInt32(minPrice * 100), Convert.ToInt32(maxPrice * 100 + 1));
            var price = (decimal)randomInt / 100m;
            return Math.Round(price, 1, MidpointRounding.AwayFromZero);
        }

        /// <summary>
        /// Añade una cantidad específica al stock actual del libro.
        /// </summary>
        /// <param name="quantity">La cantidad a añadir. Debe ser no negativa.</param>
        /// <exception cref="ArgumentOutOfRangeException">Se lanza si la cantidad es negativa.</exception>
        public void AddStock(int quantity)
        {
            if (quantity < 0) 
            {
                throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity to add to stock cannot be negative.");
            }
            Stock += quantity;
        }

        /// <summary>
        /// Disminuye la cantidad de stock del libro por la cantidad especificada.
        /// </summary>
        /// <param name="quantity">La cantidad por la cual disminuir el stock.</param>
        /// <exception cref="ArgumentOutOfRangeException">Se lanza si la cantidad es negativa.</exception>
        /// <exception cref="InvalidOperationException">Se lanza si la cantidad a disminuir es mayor que el stock actual.</exception>
        public void DecreaseStock(int quantity)
        {
            if (quantity < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity to decrease cannot be negative.");
            }
            if (Stock < quantity)
            {
                throw new InvalidOperationException($"Insufficient stock. Available: {Stock}, Requested: {quantity}.");
            }
            Stock -= quantity;
        }

        /// <summary>
        /// Establece la cantidad de stock del libro a un nuevo valor.
        /// </summary>
        /// <param name="newStock">La nueva cantidad de stock. No puede ser negativa.</param>
        /// <exception cref="ArgumentOutOfRangeException">Se lanza si el nuevo stock es negativo.</exception>
        public void SetStock(int newStock)
        {
            if (newStock < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(newStock), "El stock no puede ser negativo.");
            }
            Stock = newStock;
        }

        /// <summary>
        /// Actualiza todos los detalles mutables del libro.
        /// El SalePrice se recalcula como el 165% del PurchasePrice proporcionado.
        /// </summary>
        /// <param name="title">El nuevo título del libro.</param>
        /// <param name="description">La nueva descripción del libro.</param>
        /// <param name="author">El nuevo autor del libro.</param>
        /// <param name="purchasePrice">El nuevo precio de compra del libro.</param>
        /// <param name="cover">La nueva URL o ruta de la imagen de la portada.</param>
        /// <param name="genre">El nuevo género del libro.</param>
        /// <param name="language">El nuevo idioma del libro.</param>
        /// <exception cref="ArgumentException">Se lanza si el idioma no es 'english' o 'español' o el género no es válido.</exception>
        public void Update(string title, string description, string author, decimal purchasePrice, string cover, string genre, string language)
        {
            if (string.IsNullOrEmpty(language) || !AllowedLanguages.Contains(language))
            {
                throw new ArgumentException("El idioma del libro debe ser 'english' o 'español'.", nameof(language));
            }

            if (string.IsNullOrEmpty(genre) || !AllowedGenres.Contains(genre))
            {
                throw new ArgumentException($"El género del libro debe ser uno de los siguientes: {string.Join(", ", AllowedGenres)}.", nameof(genre));
            }

            Title = title;
            Description = description;
            Author = author;
            PurchasePrice = purchasePrice;
            SalePrice = Math.Round(PurchasePrice * 1.65m, 1, MidpointRounding.AwayFromZero);
            Cover = cover;
            Genre = genre;
            Language = language;
        }

        public void Deactivate()
        {
            IsActive = false;
        }
        
        public void Reactivate()
        {
            IsActive = true;
        }
    }
}