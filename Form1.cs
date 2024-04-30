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
            MessageBox.Show("Sleep후 시작");
            // TCP 클라이언트 생성
            client = new TcpClient();
            client.Connect("127.0.0.1", 502); // 슬레이브 장치의 IP 주소와 포트 번호

            // Modbus 팩토리 생성, 캡슐화,안정성,재사용성,의존성관리.
            IModbusFactory factory = new ModbusFactory();

            // Modbus TCP 마스터 생성
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
            //추가할것 : 유효성 검사,예외처리,타임아웃설정.
            try
            {
                short signedValue;
                if (!short.TryParse(textBox1.Text, out signedValue))
                {
                    MessageBox.Show("잘못된 입력입니다. 정수를 입력해주세요.");
                    return;
                }

                ushort registerValue = (ushort)signedValue;

                ushort registerAddress = 0; // 주소 0번지
                //ushort[] values = new ushort[] { Convert.ToUInt16(textBox1.Text) }; // TextBox에서 입력된 값을 ushort로 변환
                ushort[] values = new ushort[] { registerValue };
                byte slaveId = 1; // 슬레이브 ID
                await master.WriteMultipleRegistersAsync(slaveId, registerAddress, values); // Modbus로 레지스터에 값 쓰기

                MessageBox.Show("데이터가 성공적으로 업데이트 되었습니다.");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"업데이트 실패: {ex.Message}");
            }
        }

        private async void button3_Click(object sender, EventArgs e)
        {
            try
            {
                byte slaveId = 1; // 슬레이브 ID
                ushort coilAddress = 0; // 코일의 시작 주소
                ushort numCoils = 1; // 읽을 코일의 수

                // 코일 읽기
                bool[] coilStatus = await master.ReadCoilsAsync(slaveId, coilAddress, numCoils);

                // 결과를 TextBox2에 표시
                textBox2.Text = coilStatus[0] ? "ON" : "OFF";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"에러 : {ex.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void button4_Click(object sender, EventArgs e)
        {
            try
            {
                await Task.Delay(1000);
                byte slaveId = 1; // 슬레이브 ID
                ushort coilAddress = 0; // 코일의 주소
                bool writeValue = textBox2.Text.Trim().ToUpper() == "ON"; // TextBox에서 입력된 값을 bool로 변환

                // 코일 쓰기
                await master.WriteSingleCoilAsync(slaveId, coilAddress, writeValue);

                MessageBox.Show("코일 상태가 성공적으로 업데이트 되었습니다.");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"업데이트 실패: {ex.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            UpdateHoldingRegister();
        }

        private async Task UpdateHoldingRegister()
        {
            //추가기능 예정: 1.예외처리,2.소수점표현,3.일정시간미완료시 알림
            try
            {
                byte slaveId = 1;  // 슬레이브 ID
                ushort startAddress = 0;  // 시작 주소
                ushort numRegisters = 1;  // 읽을 레지스터의 수

                // 홀딩 레지스터 읽기
                ushort[] registers = await master.ReadHoldingRegistersAsync(slaveId, startAddress, numRegisters);

                short[] signedValues = Array.ConvertAll(registers, r => (short)r);

                // 결과를 TextBox에 표시
                textBox1.Text = signedValues[0].ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"에러 : {ex.Message}");
            }
        }

    }
}
