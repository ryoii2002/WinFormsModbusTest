using NModbus;
using System.Net.Sockets;

namespace WinFormsModbusTest
{
    public partial class Form1 : Form
    {
        private IModbusMaster master;
        private TcpClient client;
        public Form1()
        {
            InitializeComponent();
            InitializeModbus();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            timer1.Start();
        }

        private void InitializeModbus()
        {
            System.Threading.Thread.Sleep(2000);
            MessageBox.Show("Sleep�� ����");
            // TCP Ŭ���̾�Ʈ ����
            client = new TcpClient();
            client.Connect("127.0.0.1", 502); // �����̺� ��ġ�� IP �ּҿ� ��Ʈ ��ȣ

            // Modbus ���丮 ����, ĸ��ȭ,������,���뼺,����������.
            IModbusFactory factory = new ModbusFactory();

            // Modbus TCP ������ ����
            master = factory.CreateMaster(client);
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            await UpdateHoldingRegister();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (client != null && client.Connected)
            {
                client.Close();
            }
        }

        private async void button2_Click(object sender, EventArgs e)
        {
            //�߰��Ұ� : ��ȿ�� �˻�,����ó��,Ÿ�Ӿƿ�����.
            try
            {
                short signedValue;
                if (!short.TryParse(textBox1.Text, out signedValue))
                {
                    MessageBox.Show("�߸��� �Է��Դϴ�. ������ �Է����ּ���.");
                    return;
                }

                ushort registerValue = (ushort)signedValue;

                ushort registerAddress = 0; // �ּ� 0����
                //ushort[] values = new ushort[] { Convert.ToUInt16(textBox1.Text) }; // TextBox���� �Էµ� ���� ushort�� ��ȯ
                ushort[] values = new ushort[] { registerValue };
                byte slaveId = 1; // �����̺� ID
                await master.WriteMultipleRegistersAsync(slaveId, registerAddress, values); // Modbus�� �������Ϳ� �� ����

                MessageBox.Show("�����Ͱ� ���������� ������Ʈ �Ǿ����ϴ�.");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"������Ʈ ����: {ex.Message}");
            }
        }

        private async void button3_Click(object sender, EventArgs e)
        {
            try
            {
                byte slaveId = 1; // �����̺� ID
                ushort coilAddress = 0; // ������ ���� �ּ�
                ushort numCoils = 1; // ���� ������ ��

                // ���� �б�
                bool[] coilStatus = await master.ReadCoilsAsync(slaveId, coilAddress, numCoils);

                // ����� TextBox2�� ǥ��
                textBox2.Text = coilStatus[0] ? "ON" : "OFF";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"���� : {ex.Message}", "����", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void button4_Click(object sender, EventArgs e)
        {
            try
            {
                await Task.Delay(1000);
                byte slaveId = 1; // �����̺� ID
                ushort coilAddress = 0; // ������ �ּ�
                bool writeValue = textBox2.Text.Trim().ToUpper() == "ON"; // TextBox���� �Էµ� ���� bool�� ��ȯ

                // ���� ����
                await master.WriteSingleCoilAsync(slaveId, coilAddress, writeValue);

                MessageBox.Show("���� ���°� ���������� ������Ʈ �Ǿ����ϴ�.");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"������Ʈ ����: {ex.Message}", "����", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            UpdateHoldingRegister();
        }

        private async Task UpdateHoldingRegister()
        {
            //�߰���� ����: 1.����ó��,2.�Ҽ���ǥ��,3.�����ð��̿Ϸ�� �˸�
            try
            {
                byte slaveId = 1;  // �����̺� ID
                ushort startAddress = 0;  // ���� �ּ�
                ushort numRegisters = 1;  // ���� ���������� ��

                // Ȧ�� �������� �б�
                ushort[] registers = await master.ReadHoldingRegistersAsync(slaveId, startAddress, numRegisters);

                short[] signedValues = Array.ConvertAll(registers, r => (short)r);

                // ����� TextBox�� ǥ��
                textBox1.Text = signedValues[0].ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"���� : {ex.Message}");
            }
        }

    }
}
