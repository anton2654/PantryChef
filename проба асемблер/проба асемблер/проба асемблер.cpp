#include <iostream>
using namespace std;

// Формат рядка для printf
char FORMAT[] = "%s %s! %s \n";
// Рядки, які будуть виведені на екран
char HELLO[] = "Hello";
char WORLD[] = "World";
char COPYRIGHT[] = "Made by Kril Roman (2024)";

int main() {
    __asm {
        // Підготовка аргументів для printf
        mov eax, offset COPYRIGHT // Завантажуємо адресу рядка "Made by Kril Roman (2024)" в eax
        push eax // Ложимо адресу рядка "Made by Kril Roman (2024)" в стек
        mov eax, offset WORLD // Завантажуємо адресу рядка "World" в eax
        push eax // Ложимо адресу рядка "World" в стек
        mov eax, offset HELLO // Завантажуємо адресу рядка "Hello" в eax
        push eax // Ложимо адресу рядка "Hello" в стек
        mov eax, offset FORMAT // Завантажуємо адресу рядка формату в eax
        push eax // Ложимо адресу рядка формату в стек
        mov edi, printf // Завантажуємо адресу функції printf в регістр edi
        call edi // Викликаємо функцію printf з аргументами, що знаходяться в стеці

        // Чистимо аргументи зі стеку
        pop ebx
        pop ebx
        pop ebx
        pop ebx
    }
    return 0;
}
