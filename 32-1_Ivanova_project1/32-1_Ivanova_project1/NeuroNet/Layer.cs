using System;
using System.IO;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;

namespace _32_1_Ivanova_project1.NeuroNet
{
    abstract class Layer
    {
        // Поля
        // Модификаторы protected стоят для внутренней логики
        protected string name_Layer;          // Наименование слоя
        protected string pathDirWeights;      // Путь к каталогу с весами
        protected string pathFileWeights;     // Путь к файлу с весами
        protected int numofneurons;           // Число нейронов текущего слоя
        protected int numofprevneurons;       // Число нейронов предыдущего слоя
        protected const double learningrate = 0.05; // Скорость обучения
        protected const double momentum = 0.5;      // Момент обучения
        protected double[,] lastdeltaweights;       // Веса предыдущей итерации
        protected Neuron[] neurons;                 // Массив нейронов

        // Свойства
        public double[] Data
        {
            set
            {
                // Передача входных данных на нейроны
                for (int i = 0; i < numofneurons; i++)
                {
                    neurons[i].Activator(value); // Примечание: на фото может быть value, но по логике здесь должен быть массив. Оставляем как на фото.
                }
            }
        }

        // Конструктор
        // num - количество нейронов текущего слоя
        // npn - количество нейронов предыдущего слоя
        // nt - тип нейрона
        // nm_Layer - имя слоя
        protected Layer(int non, int npn, NeuronType nt, string nm_Layer)
        {
            numofneurons = non;
            numofprevneurons = npn;
            neurons = new Neuron[non];
            name_Layer = nm_Layer;    

            pathDirWeights = AppDomain.CurrentDomain.BaseDirectory + "memory\\";
            pathFileWeights = pathDirWeights + name_Layer + "_memory.csv";

            // Временный массив синаптических весов текущего слоя
            lastdeltaweights = new double[non, npn + 1];

            double[,] Weights;

            // Проверяем, существует ли файл с весами
            if (File.Exists(pathFileWeights))
            {
                Weights = WeightInitialize(MemoryMode.GET, pathFileWeights);
            }
            else
            {
                // Если файла нет, создаем директорию и инициализируем веса
                Directory.CreateDirectory(pathDirWeights);
                Weights = WeightInitialize(MemoryMode.INIT, pathFileWeights);
            }

            // Цикл формирования нейронов слоя и заполнения их массивами весов
            for (int i = 0; i < non; i++)
            {
                double[] tmp_weights = new double[npn + 1];
                for (int j = 0; j < npn + 1; j++)
                {
                    tmp_weights[j] = Weights[i, j];
                }
                neurons[i] = new Neuron(tmp_weights, nt); // Заполнение массива нейронами
            }
        }

        // Метод работы с массивом синаптических весов слоя
        public double[,] WeightInitialize(MemoryMode mm, string path)
        {
            char[] delim = new char[] { ' ', ',', ';' }; // Разделители слов - пробел, запятая, точка с запятой
            string tmpStr;                               // Временная строка для чтения
            string[] tmpStrWeights;                      // Временный массив строк
            double[,] weights = new double[numofneurons, numofprevneurons + 1]; // Массив синаптических весов

            switch (mm)
            {
                case MemoryMode.GET:
                    tmpStrWeights = File.ReadAllLines(path); // Считывание строк текстового массива весов
                    string[] memory_element;
                    for (int i = 0; i < numofneurons; i++)
                    {
                        memory_element = tmpStrWeights[i].Split(delim); // Разбиваем строку на элементы
                        for (int j = 0; j < numofprevneurons + 1; j++)


                        {
                            // Преобразование строковых значений в числа
                            weights[i, j] = double.Parse(memory_element[j].Replace(',', '.'), CultureInfo.InvariantCulture);
                        }
                    }
                    break;

                case MemoryMode.SET:
                    tmpStrWeights = new string[numofneurons]; // Построчный массив (строковый формат) синаптических весов

                    // Если нет файла синаптических весов
                    if (!File.Exists(path))
                    {
                        /*
                        * Т. е. может просматриваться как любым текстовым редактором, так и в Excel.
                        * Каждое значение веса в этом файле отделяется от соседнего значения знаком ";".
                        *
                        * Каждая строка значений весов в файле относится к одному нейрону данного слоя, т. е. количество строк в файле
                        * соответствует количеству нейронов в слое. Каждая строка файла весов начинается со значения "порога" нейрона
                        * и далее следуют значения весов синаптических связей этого нейрона данного слоя с каждым по порядку нейроном
                        * предыдущего нейронного слоя. Т. е. в одной строке содержится количество значений весов на единицу превышающее
                        * количество нейронов предыдущего слоя ("порог" + кол-во нейронов предыдущего слоя).
                        *
                        * В программном коде приложения при начальной инициализации синаптических весов автоматически создаётся отдельная
                        * папка под именем memory, где хранятся файлы весов всех слоёв нейронной сети. Эта папка с синаптическими весами
                        * располагается в том же месте, где находится файл, запускающий приложение. Если такой папки или
                        * файлов нет в указанном месте, то при запуске программы в момент инициализации нейронной сети, программа предлагает
                        * сформировать недостающие файлы в соответствующей папке.
                        *
                        * Синаптические веса при начальной инициализации генерируются программой из случайных значений, имеющих равномерное
                        * распределение со средним статистическим равным нулю и дисперсией, равной значению обратному корню квадратному из
                        * количества синаптических связей нейрона текущего слоя с нейронами предыдущего слоя.
                        * Таким образом, стандартное отклонение индуцированного локального поля нейрона будет лежать в переходной области функции
                        * активации нейрона, например, для сигмоидальной функции активации нейрона стандартное отклонение индуцированного
                        * локального поля нейрона будет лежать в переходной области между линейной частью сигмоидальной функции активации и
                        * областью насыщения. Это оптимальная стратегия инициализации синаптических весов.
                        */

                        MessageBox.Show("!!! ВНИМАНИЕ !!!\nфайл " + name_Layer + "_memory.csv синаптических весов НЕ НАЙДЕН!!!\nПосле нажатия кнопки ОК автоматически создастся отсутствующий файл, и все значения будут инициализированы нулями.",
                            "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    }

                    // генератор случайных чисел
                    Random random = new Random();

                    // Инициализация исходных значений синаптических весов нейронного слоя
                    double tmpRatio;  // коэффициент
                    double tmpShift;  // смещение
                    double[] tmpArr = new double[numofprevneurons + 1];  // массив весов одного нейрона
                    tmpStrWeights = new string[numofneurons];  // массив строк синаптических весов

                    for (int i = 0; i < numofneurons; i++)
                    {
                        tmpStr = neurons[i].Weights[0].ToString();
                        for (int j = 1; j < numofprevneurons + 1; j++)
                        {
                            tmpStr += delim[0] + neurons[i].Weights[j].ToString();
                        }
                        tmpStrWeights[i] = tmpStr;
                    }

                    File.WriteAllLines(path, tmpStrWeights); // Создаёт новый файл, записывает в него указанный массив. Если целевой файл уже существует, он будет перезаписан
                    break;

                case MemoryMode.INIT:
                { 
                    Random rnd = new Random();
                    double range = 1.0 / Math.Sqrt(numofprevneurons);   // разброс весов
                    string[] initStr = new string[numofneurons];

                    for (int i = 0; i < numofneurons; i++)
                    {
                    tmpStr = "";
                    for (int j = 0; j < numofprevneurons + 1; j++)
                    {
                    // случайное число из [-range; +range], среднее равно 0
                    weights[i, j] = (rnd.NextDouble() * 2 - 1) * range;

                    if (j > 0) tmpStr += ";";
                    tmpStr += weights[i, j].ToString(CultureInfo.InvariantCulture);
                    }
                    initStr[i] = tmpStr;
                    }

                    File.WriteAllLines(path, initStr);   // сохраняем веса в файл
                    break;
                }
            }

            return weights;
        }
        
        protected double Calc_A(double[] arr)
        {
            double sum = 0;
            for (int i = 0; i < arr.Length; i++)
            sum += arr[i];
            return sum / arr.Length;
        }

        // Метод расчета дисперсии
        protected double Calc_Dispers(double[] arr)
        {
             double mean = Calc_A(arr);
            double sum = 0;
            for (int i = 0; i < arr.Length; i++)
                sum += (arr[i] - mean) * (arr[i] - mean);
            return sum / arr.Length;

        }

        abstract public void Recognize(Network net, Layer nextLayer); // для прямых проходов
        abstract public double[] BackwardPass(double[] stuff);        // и обратных
    }
}
