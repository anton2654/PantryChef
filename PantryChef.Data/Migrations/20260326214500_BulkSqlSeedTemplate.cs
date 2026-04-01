using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using PantryChef.Data.Context;

#nullable disable

namespace PantryChef.Data.Migrations
{
    [DbContext(typeof(PantryChefDbContext))]
    [Migration("20260326214500_BulkSqlSeedTemplate")]
    public class BulkSqlSeedTemplate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                @"INSERT INTO ingredient (""Name"", ""Category"", ""Calories"", ""Proteins"", ""Fats"", ""Carbohydrates"", ""Photo"")
SELECT t.""Name"", t.""Category"", t.""Calories"", t.""Proteins"", t.""Fats"", t.""Carbohydrates"", t.""Photo""
FROM (
    VALUES
        ('Банан', 'Фрукти', 96::double precision, 1.1::double precision, 0.3::double precision, 22.8::double precision, 'https://src.zakaz.atbmarket.com/cache/photos/18797/catalog_product_main_18797.jpg'),
        ('Апельсин', 'Фрукти', 43::double precision, 0.9::double precision, 0.2::double precision, 8.3::double precision, 'https://univela-morocco.com/wp-content/uploads/2018/02/closeup-view-fresh-navel-orange-isolated_572148031-1600x1171.jpg'),
        ('Груша', 'Фрукти', 57::double precision, 0.4::double precision, 0.3::double precision, 15.5::double precision, 'https://market.rukavychka.ua/image/catalog/products/1101010135/1101010135.png'),
        ('Виноград', 'Фрукти', 67::double precision, 0.6::double precision, 0.2::double precision, 17.2::double precision, 'https://upload.wikimedia.org/wikipedia/commons/thumb/b/bb/Table_grapes_on_white.jpg/1200px-Table_grapes_on_white.jpg'),
        ('Ананас', 'Фрукти', 50::double precision, 0.5::double precision, 0.2::double precision, 13.1::double precision, 'https://nebanan.com.ua/wp-content/uploads/2017/07/ananas-gold-e1602788345313.jpeg'),
        ('Ківі', 'Фрукти', 61::double precision, 1.1::double precision, 0.5::double precision, 14.7::double precision, 'https://nebanan.com.ua/wp-content/uploads/2019/11/gold-kivi-fidani.jpg'),
        ('Персик', 'Фрукти', 46::double precision, 0.9::double precision, 0.1::double precision, 9.5::double precision, 'https://fruit-time.ua/images/cache/products/3a/persik-tureccina-500x500.jpeg'),
        ('Чорниця', 'Фрукти', 57::double precision, 1.0::double precision, 0.3::double precision, 14.5::double precision, 'https://images.prom.ua/204892268_w600_h600_chornitsya--korisni.jpg'),
        ('Полуниця', 'Фрукти', 41::double precision, 0.8::double precision, 0.4::double precision, 7.7::double precision, 'https://safaritrade.com.ua/wp-content/uploads/2020/09/000000188.jpg'),
        ('Малина', 'Фрукти', 52::double precision, 1.2::double precision, 0.7::double precision, 12.0::double precision, 'https://fruit-time.ua/images/cache/products/2e/malina-500x500.jpeg'),
        ('Слива', 'Фрукти', 49::double precision, 0.8::double precision, 0.3::double precision, 9.6::double precision, 'https://greenshop.com.ua/image/cache/catalog/dopphoto/sliva-500x500.jpg'),
        ('Гранат', 'Фрукти', 72::double precision, 0.9::double precision, 1.2::double precision, 18.7::double precision, 'https://foodplus.in.ua/food_pictures/pomegranate.jpg'),
        ('Хурма', 'Фрукти', 67::double precision, 0.6::double precision, 0.3::double precision, 18.6::double precision, 'https://fruit-time.ua/images/cache/products/ce/xurma-saron-imp-500x500.jpeg'),
        ('Яблуко', 'Фрукти', 52::double precision, 0.3::double precision, 0.4::double precision, 14.0::double precision, 'https://fruit-time.ua/images/cache/products/5a/yabluko-zelene-grenni-smit-imp__126-500x500.jpeg'),
        ('Манго', 'Фрукти', 60::double precision, 0.8::double precision, 0.4::double precision, 15.0::double precision, 'https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcRJFDwdkpRgybZ4Izxm25H9KhgjsNICFr9mFg&s'),
        ('Авокадо', 'Фрукти', 160::double precision, 2.0::double precision, 15.0::double precision, 9.0::double precision, 'https://media-cdn.oriflame.com/contentImage?externalMediaId=625b18a9-7005-4bd1-aa2e-826c02194cbf&name=avocado&inputFormat=png'),
        ('Лимон', 'Фрукти', 29::double precision, 1.1::double precision, 0.3::double precision, 9.3::double precision, 'https://src.zakaz.atbmarket.com/cache/photos/25/catalog_product_main_25.png'),
        ('Абрикос', 'Фрукти', 48::double precision, 1.4::double precision, 0.1::double precision, 11.0::double precision, 'https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcSluMdm5eECTEyfd50f0SOzuan7j1QznHnrbQ&s'),
        ('Мандарин', 'Фрукти', 53::double precision, 0.8::double precision, 0.3::double precision, 13.3::double precision, 'https://fruit-time.ua/images/cache/products/57/mandarin-aixan-imp-500x500.jpeg'),
        ('Черешня', 'Фрукти', 63::double precision, 1.1::double precision, 0.4::double precision, 16.0::double precision, 'https://otvalentiny.od.ua/upload/image/store/products/product-132/chereshnya-1-kg-283.jpg'),
        ('Кавун', 'Фрукти', 30::double precision, 0.6::double precision, 0.2::double precision, 7.6::double precision, 'https://fruit-time.ua/images/products/5a/kavun-imp.jpeg'),
        ('Диня', 'Фрукти', 33::double precision, 0.8::double precision, 0.2::double precision, 8.0::double precision, 'https://fruit-time.ua/images/cache/products/c5/dinya-imp__547-500x500.jpeg'),
        ('Папайя', 'Фрукти', 43::double precision, 0.5::double precision, 0.3::double precision, 11.0::double precision, 'https://nebanan.com.ua/wp-content/uploads/2019/11/fcb7ad251a.jpg'),
        ('Лічі', 'Фрукти', 66::double precision, 0.8::double precision, 0.4::double precision, 16.5::double precision, 'https://nebanan.com.ua/wp-content/uploads/2019/11/de24cacd4bb58c1c0e1ad2a943bc009b.jpg'),
        ('Гуава', 'Фрукти', 68::double precision, 2.6::double precision, 0.9::double precision, 14.9::double precision, 'https://fruit-time.ua/images/cache/products/32/guayava-imp-500x500.jpeg'),
        ('Кокос', 'Фрукти', 354::double precision, 3.3::double precision, 33.5::double precision, 15.2::double precision, 'https://img.fozzyshop.com.ua/odesa/151549-thickbox_default/kokos.jpg'),
        ('Маракуйя', 'Фрукти', 97::double precision, 2.2::double precision, 0.4::double precision, 23.4::double precision, 'https://nebanan.com.ua/wp-content/uploads/2019/11/100026650852b0.jpg'),
        ('Оливки', 'Овочі', 115::double precision, 0.8::double precision, 10.7::double precision, 6.3::double precision, 'https://calorizator.ru/sites/default/files/imagecache/product_512/product/olive-1.jpg'),
        ('Картопля', 'Овочі', 77::double precision, 2.0::double precision, 0.4::double precision, 17.0::double precision, 'https://img.fozzyshop.com.ua/252916-thickbox_default/kartoshka-belaya.jpg'),
        ('Морква', 'Овочі', 41::double precision, 1.3::double precision, 0.1::double precision, 9.6::double precision, 'https://agrarii-razom.com.ua/sites/default/files/byr/morkva_zvichayna.jpg'),
        ('Буряк', 'Овочі', 43::double precision, 1.5::double precision, 0.1::double precision, 9.7::double precision, 'https://img.fozzyshop.com.ua/rivne/210844-thickbox_default/svekla.jpg'),
        ('Огірок', 'Овочі', 15::double precision, 0.8::double precision, 0.1::double precision, 3.6::double precision, 'https://soncesad.com/assets/images/products/1945/.jpeg'),
        ('Помідор', 'Овочі', 19::double precision, 0.9::double precision, 0.2::double precision, 3.9::double precision, 'https://fruit-time.ua/images/cache/products/3a/pomidor__274-500x500.jpeg'),
        ('Цибуля', 'Овочі', 40::double precision, 1.1::double precision, 0.1::double precision, 9.3::double precision, 'https://img.fozzyshop.com.ua/210845-thickbox_default/luk-repchatyj-zheltyj.jpg'),
        ('Часник', 'Овочі', 149::double precision, 6.4::double precision, 0.5::double precision, 33.1::double precision, 'https://images.silpo.ua/products/1600x1600/19d61480-bfdd-4564-8083-a1b31fbc6de1.png'),
        ('Капуста білокачанна', 'Овочі', 28::double precision, 1.3::double precision, 0.1::double precision, 5.8::double precision, 'https://fruit-time.ua/images/cache/products/36/kapusta__139-500x500.jpeg'),
        ('Кабачок', 'Овочі', 24::double precision, 1.2::double precision, 0.3::double precision, 4.6::double precision, 'https://greenshop.com.ua/image/cache/catalog/dopphoto/kabachok-500x500.jpg'),
        ('Броколі', 'Овочі', 34::double precision, 2.8::double precision, 0.4::double precision, 6.6::double precision, 'https://maminaferma.com.ua/image/cache/catalog/eda72ee100ddda8d241c14720adac53b_obj-500x500.jpeg'),
        ('Перець болгарський', 'Овочі', 27::double precision, 1.0::double precision, 0.3::double precision, 6.0::double precision, 'https://img.fozzyshop.com.ua/kharkiv/19228-large_default/perec-bolgarskij.jpg'),
        ('Редиска', 'Овочі', 20::double precision, 1.2::double precision, 0.1::double precision, 3.4::double precision, 'https://www.fruit-market.com.ua/wp-content/uploads/2020/04/%D0%A1%D0%BD%D0%B8%D0%BC%D0%BE%D0%BA-2.jpg'),
        ('Шпинат', 'Овочі', 23::double precision, 2.9::double precision, 0.4::double precision, 3.6::double precision, 'https://images.silpo.ua/products/1600x1600/9c946c10-b2bd-4ad3-a165-aeb94a02293f.png'),
        ('Баклажан', 'Овочі', 24::double precision, 1.2::double precision, 0.1::double precision, 5.7::double precision, 'https://www.povarenok.ru/data/cache/2013sep/08/03/504758_85719.jpg'),
        ('Цвітна капуста', 'Овочі', 25::double precision, 2.5::double precision, 0.3::double precision, 5.2::double precision, 'https://img.fozzyshop.com.ua/211060-thickbox_default/kapusta-cvetnaya.jpg'),
        ('Спаржа', 'Овочі', 20::double precision, 2.2::double precision, 0.1::double precision, 3.9::double precision, 'https://goodfruits.com.ua/wp-content/uploads/2024/02/sparzha-zelena-1-scaled-1.jpg'),
        ('Салат (листя)', 'Овочі', 15::double precision, 1.2::double precision, 0.2::double precision, 2.3::double precision, 'https://img.postershop.me/4667/21ecaf1f-f983-43f3-96d4-70419e5f3327_image.jpg'),
        ('Гриби', 'Овочі', 22::double precision, 3.1::double precision, 0.3::double precision, 3.3::double precision, 'https://nov-rada.gov.ua/wp-content/uploads/2021/08/photo.jpg'),
        ('Селера', 'Овочі', 16::double precision, 0.9::double precision, 0.2::double precision, 3.4::double precision, 'https://advice.telegazeta.com.ua/wp-content/uploads/2024/05/selera-koryst-i-shkoda-vlastyvosti-ta-pravyla-vzhyvannya.jpg'),
        ('Горошок зелений', 'Овочі', 81::double precision, 5.4::double precision, 0.4::double precision, 14.5::double precision, 'https://fruit-time.ua/images/cache/products/c3/gorosok-zelenii-molodii-500x500.jpeg'),
        ('Квасоля стручкова', 'Овочі', 31::double precision, 1.8::double precision, 0.2::double precision, 7.0::double precision, 'https://bauer-foods.pl/wp-content/uploads/2021/08/fasola_zielona_szparagowa_cieta_2-1-1170x780.jpg'),
        ('Кукурудза', 'Овочі', 96::double precision, 3.5::double precision, 1.5::double precision, 21.0::double precision, 'https://vip.shuvar.com/media/catalog/product/cache/628b1a33a4779cd89563027f2a2c1a58/8/a/8a3984fc-0306-4ba0-8ec2-d5123e54a988.jpeg')
) AS t(""Name"", ""Category"", ""Calories"", ""Proteins"", ""Fats"", ""Carbohydrates"", ""Photo"")
WHERE NOT EXISTS (
    SELECT 1
    FROM ingredient i
    WHERE i.""Name"" = t.""Name""
);");

            migrationBuilder.Sql(
                @"INSERT INTO recipe (""Name"", ""Description"", ""Category"", ""Calories"", ""Proteins"", ""Fats"", ""Carbohydrates"", ""Photo"")
SELECT r.""Name"", r.""Description"", r.""Category"", r.""Calories"", r.""Proteins"", r.""Fats"", r.""Carbohydrates"", r.""Photo""
FROM (
    VALUES
        ('Смузі Банан-Полуниця', 'Легкий фруктовий смузі для сніданку.', 'Сніданки', 180::double precision, 3.0::double precision, 1.0::double precision, 41.0::double precision, 'https://images.unsplash.com/photo-1553530666-ba11a7da3888?q=80&w=1200&auto=format&fit=crop'),
        ('Овочевий Салат', 'Свіжий салат з огірком, помідором та перцем.', 'Салати', 120::double precision, 3.0::double precision, 4.0::double precision, 18.0::double precision, 'https://images.unsplash.com/photo-1512621776951-a57141f2eefd?q=80&w=1200&auto=format&fit=crop'),
        ('Фруктова Тарілка', 'Мікс сезонних фруктів для перекусу.', 'Снеки', 210::double precision, 2.0::double precision, 1.0::double precision, 52.0::double precision, 'https://images.unsplash.com/photo-1619566636858-adf3ef46400b?q=80&w=1200&auto=format&fit=crop'),
        ('Тушковані Овочі', 'Овочі, тушковані на повільному вогні.', 'Вечері', 240::double precision, 6.0::double precision, 7.0::double precision, 39.0::double precision, 'https://images.unsplash.com/photo-1547592166-23ac45744acd?q=80&w=1200&auto=format&fit=crop'),
        ('Фруктовий Боул', 'Боул із фруктів та ягід для швидкого перекусу.', 'Десерти', 230::double precision, 3.0::double precision, 2.0::double precision, 54.0::double precision, 'https://images.unsplash.com/photo-1490474418585-ba9bad8fd0ea?q=80&w=1200&auto=format&fit=crop')
) AS r(""Name"", ""Description"", ""Category"", ""Calories"", ""Proteins"", ""Fats"", ""Carbohydrates"", ""Photo"")
WHERE NOT EXISTS (
    SELECT 1
    FROM recipe x
    WHERE x.""Name"" = r.""Name""
);

INSERT INTO recipe_ingredient (""RecipeId"", ""IngredientId"", ""Id"", ""Quantity"")
SELECT rec.""Id"", ing.""Id"", 0, l.""Quantity""
FROM (
    VALUES
        ('Смузі Банан-Полуниця', 'Банан', 180::double precision),
        ('Смузі Банан-Полуниця', 'Полуниця', 120::double precision),
        ('Овочевий Салат', 'Огірок', 120::double precision),
        ('Овочевий Салат', 'Помідор', 150::double precision),
        ('Овочевий Салат', 'Перець болгарський', 100::double precision),
        ('Овочевий Салат', 'Оливки', 40::double precision),
        ('Фруктова Тарілка', 'Яблуко', 150::double precision),
        ('Фруктова Тарілка', 'Апельсин', 150::double precision),
        ('Фруктова Тарілка', 'Виноград', 120::double precision),
        ('Тушковані Овочі', 'Кабачок', 200::double precision),
        ('Тушковані Овочі', 'Броколі', 160::double precision),
        ('Тушковані Овочі', 'Морква', 120::double precision),
        ('Тушковані Овочі', 'Цибуля', 80::double precision),
        ('Фруктовий Боул', 'Манго', 120::double precision),
        ('Фруктовий Боул', 'Ківі', 100::double precision),
        ('Фруктовий Боул', 'Чорниця', 80::double precision),
        ('Фруктовий Боул', 'Малина', 80::double precision)
) AS l(""RecipeName"", ""IngredientName"", ""Quantity"")
JOIN recipe rec ON rec.""Name"" = l.""RecipeName""
JOIN ingredient ing ON ing.""Name"" = l.""IngredientName""
WHERE NOT EXISTS (
    SELECT 1
    FROM recipe_ingredient ri
    WHERE ri.""RecipeId"" = rec.""Id""
      AND ri.""IngredientId"" = ing.""Id""
);");

            migrationBuilder.Sql(
                 @"DELETE FROM user_ingredient WHERE ""UserId"" = 1;

INSERT INTO user_ingredient (""UserId"", ""IngredientId"", ""Quantity"")
SELECT 1, i.""Id"", v.""Quantity""
FROM (
    VALUES
        ('Апельсин', 1000::double precision),
        ('Яблуко', 500::double precision),
        ('Морква', 400::double precision),
        ('Огірок', 300::double precision),
        ('Ківі', 600::double precision),
        ('Авокадо', 200::double precision),
        ('Перець болгарський', 300::double precision),
        ('Броколі', 450::double precision),
        ('Полуниця', 300::double precision),
        ('Банан', 800::double precision),
        ('Ананас', 1::double precision),
        ('Виноград', 450::double precision),
        ('Персик', 600::double precision),
        ('Черешня', 500::double precision),
        ('Кавун', 1::double precision),
        ('Помідор', 750::double precision),
        ('Часник', 100::double precision),
        ('Шпинат', 150::double precision),
        ('Кукурудза', 350::double precision),
        ('Гриби', 400::double precision)
) AS v(""Name"", ""Quantity"")
JOIN ingredient i ON i.""Name"" = v.""Name"";");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Optional rollback SQL for records inserted in Up.
            // Example:
            // migrationBuilder.Sql(@"DELETE FROM ingredient WHERE \"Name\" IN ('Банан');");
        }
    }
}
