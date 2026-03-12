#include <iostream>
#include <cmath>
#include <string>

using namespace std;

int main()
{
	int number;
	cin >> number;

	string txt = to_string(number);

	int sum = 0;
	for (int i = 0; i < txt.length(); i++) sum += (int)(number / pow(10, i)) % 10;

	cout << sum;

	return 0;
}