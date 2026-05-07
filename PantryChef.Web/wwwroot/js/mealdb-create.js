(function () {
  document.addEventListener('DOMContentLoaded', function () {
    const panel = document.querySelector('.mealdb-panel');
    const areaSelect = document.getElementById('mealdb-area');
    const mealSelect = document.getElementById('mealdb-meal');
    const status = document.getElementById('mealdb-status');
    const nameInput = document.getElementById('Name');
    const descriptionInput = document.getElementById('Description');

    if (!panel || !areaSelect || !mealSelect || !status || !nameInput || !descriptionInput) {
      return;
    }

    const urls = {
      areas: panel.dataset.areasUrl,
      meals: panel.dataset.mealsUrl,
      instructions: panel.dataset.instructionsUrl
    };

    const setStatus = function (text) {
      status.textContent = text;
    };

    const clearMealOptions = function (placeholder) {
      mealSelect.innerHTML = '';
      const option = document.createElement('option');
      option.value = '';
      option.textContent = placeholder;
      mealSelect.appendChild(option);
    };

    const loadAreas = async function () {
      try {
        setStatus('Завантаження списку країн...');
        const response = await fetch(urls.areas);
        if (!response.ok) {
          throw new Error('Areas request failed');
        }

        const areas = await response.json();
        for (const item of areas) {
          const option = document.createElement('option');
          option.value = item.strArea;
          option.textContent = item.strCountry ? `${item.strCountry} (${item.strArea})` : item.strArea;
          areaSelect.appendChild(option);
        }

        setStatus('Оберіть країну для підбору страв.');
      } catch (error) {
        setStatus('Не вдалося завантажити країни з TheMealDB.');
      }
    };

    areaSelect.addEventListener('change', async function () {
      const area = areaSelect.value;
      if (!area) {
        clearMealOptions('Спочатку оберіть країну');
        mealSelect.disabled = true;
        setStatus('Оберіть країну для підбору страв.');
        return;
      }

      try {
        mealSelect.disabled = true;
        clearMealOptions('Завантаження страв...');
        setStatus('Отримуємо список страв...');

        const response = await fetch(`${urls.meals}?area=${encodeURIComponent(area)}`);
        if (!response.ok) {
          throw new Error('Meals request failed');
        }

        const meals = await response.json();
        clearMealOptions('Оберіть страву');

        for (const meal of meals) {
          const option = document.createElement('option');
          option.value = meal.idMeal;
          option.textContent = meal.strMeal;
          option.dataset.mealName = meal.strMeal;
          mealSelect.appendChild(option);
        }

        mealSelect.disabled = false;
        setStatus('Оберіть назву страви, щоб заповнити форму.');
      } catch (error) {
        clearMealOptions('Не вдалося завантажити страви');
        mealSelect.disabled = true;
        setStatus('Не вдалося отримати страви для обраної країни.');
      }
    });

    mealSelect.addEventListener('change', async function () {
      const selectedOption = mealSelect.options[mealSelect.selectedIndex];
      const mealId = selectedOption ? selectedOption.value : '';
      const mealName = selectedOption ? selectedOption.dataset.mealName : '';

      if (!mealId || !mealName) {
        return;
      }

      nameInput.value = mealName;
      nameInput.dispatchEvent(new Event('input', { bubbles: true }));

      try {
        setStatus('Завантажуємо опис страви...');
        const response = await fetch(`${urls.instructions}?mealId=${encodeURIComponent(mealId)}`);
        if (!response.ok) {
          throw new Error('Instructions request failed');
        }

        const data = await response.json();
        descriptionInput.value = data.instructions || '';
        descriptionInput.dispatchEvent(new Event('input', { bubbles: true }));
        setStatus('Поля "Назва" та "Опис" заповнено з TheMealDB.');
      } catch (error) {
        setStatus('Назву заповнено, але опис отримати не вдалося.');
      }
    });

    loadAreas();
  });
})();
