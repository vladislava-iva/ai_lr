using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _32_1_Ivanova_project1.NeuroNet
{
    class Network
    {
        // все слои сети
        private InputLayer input_layer = null;
        private HiddenLayer hidden_layer1 = new HiddenLayer(70, 15, NeuronType.Hidden, nameof(hidden_layer1));
        private HiddenLayer hidden_layer2 = new HiddenLayer(38, 70, NeuronType.Hidden, nameof(hidden_layer2));
        private OutputLayer output_layer = new OutputLayer(10, 38, NeuronType.Output, nameof(output_layer));

        private double[] fact = new double[10];  // массив фактического выхода
        private double[] e_error_avr;            // среднее значение энергии ошибки эпохи обучения

        // свойства
        public double[] Fact { get => fact; }    // массив фактического выхода сети

        // среднее значение энергии ошибки эпохи обучения
        public double[] E_Error_Avr 
        {
            get => e_error_avr;
            set=> e_error_avr=value;
        }

        public Network() { }

        public void ForwardPass(double[] netInput)
        {
            hidden_layer1.Data=netInput;
            hidden_layer1.Recognize(null,hidden_layer2);
            hidden_layer2.Recognize(null,output_layer);

            output_layer.Recognize(this, null);

        }
    }
}
