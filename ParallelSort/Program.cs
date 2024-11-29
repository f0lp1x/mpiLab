    using MPI;
    using System.Diagnostics;
    class Program
    {
        static void Main(string[] args)
        {
            var sw = new Stopwatch();
            MPI.Environment.Run(ref args, communicator =>
            {
                int rank = communicator.Rank;
                int size = communicator.Size;
                int rowsA = 100; // Количество строк в первой матрице
                int colsA = 100; // Количество столбцов в первой матрице (и строк во второй)
                int colsB = 100; // Количество столбцов во второй матрице
                int[] matrixA = new int[rowsA * colsA];
                int[] matrixB = new int[colsA * colsB];
                int[] matrixC = new int[rowsA * colsB];

                if (rank == 0)
                {
                    sw.Start();
                    int countA = 10;
                    int countB = 1;
                    for (int i = 0; i < rowsA; i++)
                    {
                        for (int j = 0; j < colsA; j++)
                        {
                            matrixA[i * colsA + j] = countA++; // Пример заполнения
                        }
                    }

                    for (int i = 0; i < colsA; i++)
                    {
                        for (int j = 0; j < colsB; j++)
                        {
                            matrixB[i * colsB + j] = countB++; // Пример заполнения
                        }
                    }

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
                }
                else
                {
                    for (int i = 0; i < rowsA; i++)
                    {
                        for (int j = 0; j < colsA; j++)
                        {
                            matrixA[i * colsA + j] = communicator.Receive<int>(0, 0);
                        }
                    }
                    for (int i = 0; i < colsA; i++)
                    {
                        for (int j = 0; j < colsB; j++)
                        {
                            matrixB[i * colsB + j] = communicator.Receive<int>(0, 1);
                        }
                    }
                }

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

                if (rank == 0)
                {
                    Console.WriteLine("Результирующая матрица:");
                    for (int i = 0; i < rowsA; i++)
                    {
                        for (int j = 0; j < colsB; j++)
                        {
                            Console.Write(matrixC[i * colsB + j] + "\t");
                        }
                        Console.WriteLine();
                    }
                    Console.WriteLine($"Elapsed {sw.Elapsed}.");
                }
            
            });
        }
    }
