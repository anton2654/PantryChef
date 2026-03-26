using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PantryChef.Data.Migrations
{
    /// <inheritdoc />
    public partial class SeedIngredientsData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Видаляємо старі тестові англійські дані (якщо вони там ще є)
            migrationBuilder.Sql(@"
                DELETE FROM user_ingredient;
                DELETE FROM recipe_ingredient;
                DELETE FROM ingredient;
            ");

            // 2. Записуємо нашу нову красиву базу продуктів
            migrationBuilder.Sql(@"
                INSERT INTO ingredient (""Name"", ""Category"", ""Calories"", ""Proteins"", ""Fats"", ""Carbohydrates"", ""Photo"") VALUES
                ('Банан', 'Фрукти', 96, 1.1, 0.3, 22.8, 'https://src.zakaz.atbmarket.com/cache/photos/18797/catalog_product_main_18797.jpg'),
                ('Апельсин', 'Фрукти', 43, 0.9, 0.2, 8.3, 'https://univela-morocco.com/wp-content/uploads/2018/02/closeup-view-fresh-navel-orange-isolated_572148031-1600x1171.jpg'),
                ('Груша', 'Фрукти', 57, 0.4, 0.3, 15.5, 'https://market.rukavychka.ua/image/catalog/products/1101010135/1101010135.png'),
                ('Виноград', 'Фрукти', 67, 0.6, 0.2, 17.2, 'https://upload.wikimedia.org/wikipedia/commons/thumb/b/bb/Table_grapes_on_white.jpg/1200px-Table_grapes_on_white.jpg'),
                ('Ананас', 'Фрукти', 50, 0.5, 0.2, 13.1, 'https://nebanan.com.ua/wp-content/uploads/2017/07/ananas-gold-e1602788345313.jpeg'),
                ('Ківі', 'Фрукти', 61, 1.1, 0.5, 14.7, 'https://nebanan.com.ua/wp-content/uploads/2019/11/gold-kivi-fidani.jpg'),
                ('Персик', 'Фрукти', 46, 0.9, 0.1, 9.5, 'https://fruit-time.ua/images/cache/products/3a/persik-tureccina-500x500.jpeg'),
                ('Чорниця', 'Фрукти', 57, 1.0, 0.3, 14.5, 'https://images.prom.ua/204892268_w600_h600_chornitsya--korisni.jpg'),
                ('Полуниця', 'Фрукти', 41, 0.8, 0.4, 7.7, 'https://safaritrade.com.ua/wp-content/uploads/2020/09/000000188.jpg'),
                ('Малина', 'Фрукти', 52, 1.2, 0.7, 12.0, 'https://fruit-time.ua/images/cache/products/2e/malina-500x500.jpeg'),
                ('Слива', 'Фрукти', 49, 0.8, 0.3, 9.6, 'https://greenshop.com.ua/image/cache/catalog/dopphoto/sliva-500x500.jpg'),
                ('Гранат', 'Фрукти', 72, 0.9, 1.2, 18.7, 'https://foodplus.in.ua/food_pictures/pomegranate.jpg'),
                ('Хурма', 'Фрукти', 67, 0.6, 0.3, 18.6, 'https://fruit-time.ua/images/cache/products/ce/xurma-saron-imp-500x500.jpeg'),
                ('Яблуко', 'Фрукти', 52, 0.3, 0.4, 14.0, 'https://fruit-time.ua/images/cache/products/5a/yabluko-zelene-grenni-smit-imp__126-500x500.jpeg'),
                ('Манго', 'Фрукти', 60, 0.8, 0.4, 15.0, 'https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcRJFDwdkpRgybZ4Izxm25H9KhgjsNICFr9mFg&s'),
                ('Авокадо', 'Фрукти', 160, 2.0, 15.0, 9.0, 'https://media-cdn.oriflame.com/contentImage?externalMediaId=625b18a9-7005-4bd1-aa2e-826c02194cbf&name=avocado&inputFormat=png'),
                ('Лимон', 'Фрукти', 29, 1.1, 0.3, 9.3, 'https://src.zakaz.atbmarket.com/cache/photos/25/catalog_product_main_25.png'),
                ('Абрикос', 'Фрукти', 48, 1.4, 0.1, 11.0, 'https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcSluMdm5eECTEyfd50f0SOzuan7j1QznHnrbQ&s'),
                ('Мандарин', 'Фрукти', 53, 0.8, 0.3, 13.3, 'https://fruit-time.ua/images/cache/products/57/mandarin-aixan-imp-500x500.jpeg'),
                ('Черешня', 'Фрукти', 63, 1.1, 0.4, 16.0, 'https://otvalentiny.od.ua/upload/image/store/products/product-132/chereshnya-1-kg-283.jpg'),
                ('Кавун', 'Фрукти', 30, 0.6, 0.2, 7.6, 'https://fruit-time.ua/images/products/5a/kavun-imp.jpeg'),
                ('Диня', 'Фрукти', 33, 0.8, 0.2, 8.0, 'https://fruit-time.ua/images/cache/products/c5/dinya-imp__547-500x500.jpeg'),
                ('Папайя', 'Фрукти', 43, 0.5, 0.3, 11.0, 'https://nebanan.com.ua/wp-content/uploads/2019/11/fcb7ad251a.jpg'),
                ('Лічі', 'Фрукти', 66, 0.8, 0.4, 16.5, 'https://nebanan.com.ua/wp-content/uploads/2019/11/de24cacd4bb58c1c0e1ad2a943bc009b.jpg'),
                ('Гуава', 'Фрукти', 68, 2.6, 0.9, 14.9, 'https://fruit-time.ua/images/cache/products/32/guayava-imp-500x500.jpeg'),
                ('Кокос', 'Фрукти', 354, 3.3, 33.5, 15.2, 'https://img.fozzyshop.com.ua/odesa/151549-thickbox_default/kokos.jpg'),
                ('Маракуйя', 'Фрукти', 97, 2.2, 0.4, 23.4, 'https://nebanan.com.ua/wp-content/uploads/2019/11/100026650852b0.jpg'),
                ('Оливки', 'Овочі', 115, 0.8, 10.7, 6.3, 'https://calorizator.ru/sites/default/files/imagecache/product_512/product/olive-1.jpg'),
                ('Картопля', 'Овочі', 77, 2.0, 0.4, 17.0, 'https://img.fozzyshop.com.ua/252916-thickbox_default/kartoshka-belaya.jpg'),
                ('Морква', 'Овочі', 41, 1.3, 0.1, 9.6, 'https://agrarii-razom.com.ua/sites/default/files/byr/morkva_zvichayna.jpg'),
                ('Буряк', 'Овочі', 43, 1.5, 0.1, 9.7, 'https://img.fozzyshop.com.ua/rivne/210844-thickbox_default/svekla.jpg'),
                ('Огірок', 'Овочі', 15, 0.8, 0.1, 3.6, 'https://soncesad.com/assets/images/products/1945/.jpeg'),
                ('Помідор', 'Овочі', 19, 0.9, 0.2, 3.9, 'https://fruit-time.ua/images/cache/products/3a/pomidor__274-500x500.jpeg'),
                ('Цибуля', 'Овочі', 40, 1.1, 0.1, 9.3, 'https://img.fozzyshop.com.ua/210845-thickbox_default/luk-repchatyj-zheltyj.jpg'),
                ('Часник', 'Овочі', 149, 6.4, 0.5, 33.1, 'https://images.silpo.ua/products/1600x1600/19d61480-bfdd-4564-8083-a1b31fbc6de1.png'),
                ('Капуста білокачанна', 'Овочі', 28, 1.3, 0.1, 5.8, 'https://fruit-time.ua/images/cache/products/36/kapusta__139-500x500.jpeg'),
                ('Кабачок', 'Овочі', 24, 1.2, 0.3, 4.6, 'https://greenshop.com.ua/image/cache/catalog/dopphoto/kabachok-500x500.jpg'),
                ('Броколі', 'Овочі', 34, 2.8, 0.4, 6.6, 'https://maminaferma.com.ua/image/cache/catalog/eda72ee100ddda8d241c14720adac53b_obj-500x500.jpeg'),
                ('Перець болгарський', 'Овочі', 27, 1.0, 0.3, 6.0, 'https://img.fozzyshop.com.ua/kharkiv/19228-large_default/perec-bolgarskij.jpg'),
                ('Редиска', 'Овочі', 20, 1.2, 0.1, 3.4, 'https://www.fruit-market.com.ua/wp-content/uploads/2020/04/%D0%A1%D0%BD%D0%B8%D0%BC%D0%BE%D0%BA-2.jpg'),
                ('Шпинат', 'Овочі', 23, 2.9, 0.4, 3.6, 'https://images.silpo.ua/products/1600x1600/9c946c10-b2bd-4ad3-a165-aeb94a02293f.png'),
                ('Баклажан', 'Овочі', 24, 1.2, 0.1, 5.7, 'https://www.povarenok.ru/data/cache/2013sep/08/03/504758_85719.jpg'),
                ('Цвітна капуста', 'Овочі', 25, 2.5, 0.3, 5.2, 'https://img.fozzyshop.com.ua/211060-thickbox_default/kapusta-cvetnaya.jpg'),
                ('Спаржа', 'Овочі', 20, 2.2, 0.1, 3.9, 'https://goodfruits.com.ua/wp-content/uploads/2024/02/sparzha-zelena-1-scaled-1.jpg'),
                ('Салат (листя)', 'Овочі', 15, 1.2, 0.2, 2.3, 'https://img.postershop.me/4667/21ecaf1f-f983-43f3-96d4-70419e5f3327_image.jpg'),
                ('Гриби', 'Овочі', 22, 3.1, 0.3, 3.3, 'https://nov-rada.gov.ua/wp-content/uploads/2021/08/photo.jpg'),
                ('Селера', 'Овочі', 16, 0.9, 0.2, 3.4, 'https://advice.telegazeta.com.ua/wp-content/uploads/2024/05/selera-koryst-i-shkoda-vlastyvosti-ta-pravyla-vzhyvannya.jpg'),
                ('Горошок зелений', 'Овочі', 81, 5.4, 0.4, 14.5, 'https://fruit-time.ua/images/cache/products/c3/gorosok-zelenii-molodii-500x500.jpeg'),
                ('Квасоля стручкова', 'Овочі', 31, 1.8, 0.2, 7.0, 'https://bauer-foods.pl/wp-content/uploads/2021/08/fasola_zielona_szparagowa_cieta_2-1-1170x780.jpg'),
                ('Кукурудза', 'Овочі', 96, 3.5, 1.5, 21.0, 'https://vip.shuvar.com/media/catalog/product/cache/628b1a33a4779cd89563027f2a2c1a58/8/a/8a3984fc-0306-4ba0-8ec2-d5123e54a988.jpeg');
                                
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Якщо міграцію скасують, видаляємо ці продукти
            migrationBuilder.Sql(@"
                DELETE FROM ingredient WHERE ""Category"" IN ('Фрукти', 'Овочі');
            ");
        }
    }
}