# AliceMQTTHandler
Прослойка для соединения проекта VVip-68 и "Домовенка Кузи"
# Оглавление
1. Установка
    * Windows
    * Linux (Легкий пакет)
    * Linux (Полный пакет)
2. Настройка программы
3. Настройка "Кузи"
4. TODO лист


# Установка
Проект выходит для 2 самых популярных платформ -  Windows и Linux (Debian based).
Скачать архив с программой можно [тут](https://github.com/Krypt0nC0R3/AliceMQTTHandler/releases)

## Windows
Программа тестировалась на Windows 10 21H2.
1. Для работы потребуется [.NET 5.0 ](https://dotnet.microsoft.com/en-us/download/dotnet/5.0);
2. Скачайте и распакуйте архив `AliceMQTTHandler-win-x64.zip` в удобном для Вас месте;
3. Запустите `AliceMQTTHandler.exe`. Она сообщит об отсутствии файла конфигурации;
4. [Настройте](#Настройка) программу;


## Linux (Легкий пакет)
1. Скачайте архив `AliceMQTTHandler-linux-x64-lite.zip`;
2. Установите [.NET 5.0 ](https://dotnet.microsoft.com/en-us/download/dotnet/5.0) **С ПРАВАМИ АДМИНИСТРАТОРА** (например `sudo`);
3. Распакуйте архив в удобном для Вас месте (`unzip -a AliceMQTTHandler-linux-x64-lite.zip`);
4. Запустите программу **от имени администратора** (`sudo`). Она сообщит об отсутствии файла конфигурации;
5. [Настройте](#Настройка) программу;


## Linux (Полный пакет)
1. Скачайте архив `AliceMQTTHandler-linux-x64-full.zip`;
2. Распакуйте архив в удобном для Вас месте (`unzip -a AliceMQTTHandler-linux-x64-full.zip`);
3. Запустите программу **от имени администратора** (`sudo`). Она сообщит об отсутствии файла конфигурации;
4. [Настройте](#Настройка) программу;


# Настройка
Файл настройки представляет собой обычный json-документ, как пример:
```json
{
  "Do_Output": true,
  "Web_Port": 88,
  "Web_Path_Prefix": "/smarthome/lamp1",
  "MQTT_address": "92.68.145.12",
  "MQTT_Port": 1883,
  "MQTT_Path": "/my/path/to/lamp/or/matrix",
  "MQTT_Username": "Krypt0nC0R3",
  "MQTT_Password": "TheM0stStr0ngP@sSw0rd",
  "Secret_Phrase": "Random_Secret_phrase"
}
```
Разберем более подробно, для чего служит каждый параметр.
____
#### Do_Output

Определяет, будет ли программа писать в станадартный вывод общую информацию. `true` если вывод необходим, `false` если вывод не нужен. **Информация об ошибках и предупреждениях выводится в любом случае**.
____
#### Web_Port

Порт, который будет слушать программа и который должен быть доступен извне. На одном ПК можно запустить несколько копий программы на разных портах. Они не будут мешать друг другу. В качестве значения принимает число, отличное от нуля. **Желательно не использовать стандартные значиея, типа 21, 22, 88, 443 и т.д.**.
____
#### Web_Path_Prefix

Адрес, по которому программа будет принимать запросы на управление. Полный адрес для управления выглядит следующим образом `http://{Ваш IP}:{Web_Port}{Web_Path_Prefix}`. Принимает в качестве значения строку, начинающуюся с `/`.
____
#### MQTT_address

Адрес MQTT сервера, к которому подключена Ваша лампа/матрица. Если вы запускаете программу на том же ПК, где стоит MQTT-сервер можно вписать `localhost`. Принимает в качестве значения IP-адрес сервера или его WS:// адрес.
____
#### MQTT_Port

Порт MQTT сервера. Зачастую можно оставить по умолчанию. В качестве значения принимает ненулевое число
____
#### MQTT_Path

Путь (префикс) вашей лампы на MQTT-сервере. Должен совпадать с префиксом MQTT из настроек матрицы. 
____
#### MQTT_Username

Имя пользователя для аутентификации на MQTT-сервере. По умолчанию `null`. При таком значении авторизация по логину-паролю отключена. Значение - строка либо `null`.
____
#### MQTT_Password

Пароль для аутентификации на MQTT-сервере. По умолчанию `null`. При таком значении авторизация по логину-паролю отключена. Значение - строка либо `null`.
____
#### Secret_Phrase

Секретная комбинация, без которой программа не будет воспринимать команды от "Кузи" и через `GET` запросы. Можно оставить значение по умолчанию (значение у каждого свое, генерируется при первом запуске) либо придумать свое. **Запрещается использовать символы, отличные от букв анлийского алфавита и цифр**.

# Настройка "Кузи"

Для начала нам понадобится [Ваш IP-адрес](https://2ip.ru/) или адрес Вашего домена, если он есть.
Вам необходим адрес, к которому мы будем добавлять различные суфффиксы. Адрес получается вида `http://{IP}:{Web_Port}/{Web_Path_Prefix}/`. Используя данные из примера выше получится следующее: `http://142.15.224.163:88/smarthome/lamp1/`. Назовем получивщуюся конструкицю **`Базовым адресом`**.
Для управления лампой/матрицей нам потребуется создать 8 HTTP правил и 1 RGB-лампу в приложении.

![image](https://user-images.githubusercontent.com/46594859/155583200-daad0655-793c-4837-a890-6978ee22ba47.png)
## Правила
____
### Правило запроса состояния
Отдает "Кузе" информацию о том, ключена лампа или нет.

В `URL управления устройством, доступный из интернета` вписываем базовый адрес и к нему сзади приписываем `getstate/?secret={Secret_Phrase}`, где `{Secret_Phrase}` нужно заменить на вашу секретную фразу.
**Обязательно** ставим галочку `Ждать ответ от сервера`

![image](https://user-images.githubusercontent.com/46594859/155583588-bc159a90-1056-45d3-8ae4-dc1e6bd4c011.png)

**На всех правилах по запросу информации обязательно ставить галочку `Ждать ответ от сервера`. Без нее Ваша лампа будет отключаться спустя 5-10 секунд, т.к. "Кузя" не дождался ответа.**
____
### Правило установки состояния
Принимает от "Кузи" информацию о том, что нужно сделать с лампой - включить или выключить.

`URL управления устройством, доступный из интернета`: Базовый адрес + `setstate/{value}?secret={Secret_Phrase}`, заменяем `{Secret_Phrase}` на секретную фразу. `{value}` ни в коем случае не трогаем, оставляем как есть
В поле `Поиск значений в фразе` выбираем значение `Цифры, вкл/выкл`.
____
### Правило запроса цвета
Отдает информацию о том, какой последний цвет использовался для рисования.
> На версии прошивки 1.12 и ниже может не работать из-за [бага](https://github.com/vvip-68/GyverPanelWiFi/issues/217)

По умолчанию - Черный цвет.

`URL управления устройством, доступный из интернета`: Базовый адрес + `getcolor/?secret={Secret_Phrase}`, заменяем `{Secret_Phrase}` на секретную фразу.
____
### Правило установки цвета
Принимает информацию от "Кузи" о том, каким цветом залить лампу/матрицу.

`URL управления устройством, доступный из интернета`: Базовый адрес + `setcolor/{r}.{g}.{b}?secret={Secret_Phrase}`, заменяем `{Secret_Phrase}` на секретную фразу.
____
### Правило запроса яркости
Отдает информацю "Кузе" о текущей яркости гирлянды.

`URL управления устройством, доступный из интернета`: Базовый адрес + `getbrightness/?secret={Secret_Phrase}`, заменяем `{Secret_Phrase}` на секретную фразу.
____
### Правило установки яркости
Принимает информацю у "Кузи" о текущей яркости гирлянды.

`URL управления устройством, доступный из интернета`: Базовый адрес + `setbrightness/{value}?secret={Secret_Phrase}`, заменяем `{Secret_Phrase}` на секретную фразу.
____
### Правило запроса текущего эффекта
Отдает информацю "Кузе" о текущем эффекте.

`URL управления устройством, доступный из интернета`: Базовый адрес + `geteffect/?secret={Secret_Phrase}`, заменяем `{Secret_Phrase}` на секретную фразу.
____
### Правило установки текущего эффекта
Принимает информацю у "Кузи" о текущем эфекте.

`URL управления устройством, доступный из интернета`: Базовый адрес + `seteffect/{value}?secret={Secret_Phrase}`, заменяем `{Secret_Phrase}` на секретную фразу.
____
## Настройка устройства
Создаем RGB-лампу. С помощью цветовой температуры мы будем управлять эффектами.
Выставляем в настройках правила следующим образом:

![image](https://user-images.githubusercontent.com/46594859/155660457-cdda7cad-616a-4bd2-8dca-96c34c30e2f9.png)


Если попросить Алису установить цвет, то вся лампа/матрица будет залита нужным цветом. Для возврата в демо режим нужно попросить ее сделать цвет потеплее или похолоднее. В таком случае будет включен следующий режим.
Включение/выключение и управление яркостью работают так, как ожидается, тут сюрпризов нет.


# TODO-лист
- [X] Do_Output имеет влияние на лог;
- [X] ~~Запрос цвета работает корректно~~ Оказалось это глюк "Кузи", на следующий день баг пропал сам собой;
- [X] Переключение эффектов работает корректно всегда;
- [ ] Управление из одного приложения несколькими лампами (долгострой);
