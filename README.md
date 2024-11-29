

## Основные компоненты кода

### 1. Подключение библиотек
```csharp
using MPI;
using System.Diagnostics;
```
Здесь подключаются необходимые библиотеки: `MPI` для работы с параллельными вычислениями и `System.Diagnostics` для измерения времени выполнения.

### 2. Основной класс и метод
```csharp
class Program
{
    static void Main(string[] args)
    {
        var sw = new Stopwatch();
        ...
    }
}
```
Класс `Program` содержит метод `Main`, который является точкой входа в приложение. Создается объект `Stopwatch` для измерения времени выполнения.

### 3. Инициализация MPI
```csharp
MPI.Environment.Run(ref args, communicator =>
{
    int rank = communicator.Rank;
    int size = communicator.Size;
    ...
});
```
Метод `Run` инициализирует среду MPI. Внутри него определяется `rank` (номер процесса) и `size` (общее количество процессов).

### 4. Определение размеров матриц
```csharp
int rowsA = 100; // Количество строк в первой матрице
int colsA = 100; // Количество столбцов в первой матрице (и строк во второй)
int colsB = 100; // Количество столбцов во второй матрице
```
Здесь задаются размеры двух матриц: `A` и `B`, которые будут перемножаться.

### 5. Инициализация массивов для матриц
```csharp
int[] matrixA = new int[rowsA * colsA];
int[] matrixB = new int[colsA * colsB];
int[] matrixC = new int[rowsA * colsB];
```
Создаются одномерные массивы для хранения элементов матриц `A`, `B` и результирующей матрицы `C`.

### 6. Заполнение матриц (только для процесса с рангом 0)
```csharp
if (rank == 0)
{
    sw.Start();
    ...
}
```
Только процесс с рангом 0 заполняет матрицы значениями. В данном случае используются простые последовательные значения для заполнения.

### 7. Отправка данных другим процессам
```csharp
for (int i = 1; i < size; i++)
{
    foreach (int item in matrixA)
    {
        communicator.Send(item, i, 0);
    }
    foreach (int item in matrixB)
    {
        communicator.Send(item, i, 1);
    }
}
```
Процесс с рангом 0 отправляет данные из матриц `A` и `B` всем остальным процессам.

### 8. Получение данных другими процессами
```csharp
else
{
    for (int i = 0; i < rowsA; i++)
    {
        for (int j = 0; j < colsA; j++)
        {
            matrixA[i * colsA + j] = communicator.Receive<int>(0, 0);
        }
    }
    ...
}
```
Другие процессы получают данные из матриц `A` и `B`.

### 9. Умножение матриц
```csharp
int rowsPerProcess = rowsA / size;
int startRow = rank * rowsPerProcess;
int endRow = (rank + 1) * rowsPerProcess;

for (int i = startRow; i < endRow; i++)
{
    for (int j = 0; j < colsB; j++)
    {
        matrixC[i * colsB + j] = 0;
        for (int k = 0; k < colsA; k++)
        {
            matrixC[i * colsB + j] += matrixA[i * colsA + k] * matrixB[k * colsB + j];
        }
    }
}
```
Каждый процесс вычисляет часть результирующей матрицы `C`, обрабатывая только свои строки.

### 10. Сбор результатов на процессе с рангом 0
```csharp
if (rank == 0)
{
    for (int i = 1; i < size; i++)
    {
        for (int row = i * rowsPerProcess; row < (i + 1) * rowsPerProcess; row++)
        {
            for (int col = 0; col < colsB; col++)
            {
                matrixC[row * colsB + col] = communicator.Receive<int>(i, 2);
            }
        }
    }
}
else
{
    for (int i = startRow; i < endRow; i++)
    {
        for (int j = 0; j < colsB; j++)
        {
            communicator.Send(matrixC[i * colsB + j], 0, 2);
        }
    }
}
```
Процесс с рангом 0 собирает результаты от всех других процессов, а остальные процессы отправляют свои результаты.

### 11. Вывод результата и время выполнения
```csharp
if (rank == 0)
{
    Console.WriteLine("Результирующая матрица:");
    ...
    Console.WriteLine($"Elapsed {sw.Elapsed}.");
}
```
Наконец, процесс с рангом 0 выводит результирующую матрицу и время, затраченное на выполнение.

## Заключение

Этот код демонстрирует основные принципы работы с MPI для распараллеливания задачи умножения матриц. Он показывает, как распределить данные между процессами, выполнить вычисления параллельно и собрать результаты обратно в один процесс.
## Примечания
### Когда нужно было просто объяснить код, но ты чилловый парень, который любит пошутить.
![](https://github.com/f0lp1x/mpiLab/blob/master/maxresdefault.jpg)

