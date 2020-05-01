using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace Patrol_service
{
    public class CrewClass
    {
        //Номер экипажа, номер маршрута
        public int Bort_num, Track_ID;
        //ФИО сотрудников
        public string Sotr1_FIO, Sotr1_rank, Sotr1_pidpfilename;
        public string Sotr2_FIO, Sotr2_rank, Sotr2_pidpfilename;
        public string Sotr3_FIO, Sotr3_rank, Sotr3_pidpfilename;

        //Функция задержания сотрудниками преступника
        private void BustedTick(Object source, System.Timers.ElapsedEventArgs e) {
            busted_tick++;
        }

        //Функция определения статьи нарушения по id
        private float GetArticle(int val) {
            switch (val) {
                case 1:  return 115.1f;
                case 2:  return 178f;
                case 3:  return 122.2f;
                case 4:  return 122.1f;
                case 5:  return 127.1f;
                case 6:  return 121.5f;
                case 7:  return 115f;
                case 8:  return 186f;
                case 9:  return 185f;
                case 10: return 352f;
                case 11: return 307.1f;
                case 12: return 146f;
                case 13: return 126f;
                default: return 0;
            }
        }

        private void Detention() {
            //Определяем нарушителей на контрольной точке
            //выбираем случайное число в диапазоне от 1 до 99
            Random rnd = new Random();
            int val1 = rnd.Next(1, 99);
            //13 раз случайным образом выбираем нарушение и ожидаем его появления
            for (int i = 0; i < 13; i++)
            {
                int val2 = rnd.Next(1, 13);
                if (val1 <= localtrack.control_points[this.GetTekCPid()].probabilities[val2])
                {
                    //начинаем процедуру задержания
                    //инициализируем и запускаем таймер принималова
                    busted = true;

                    this.bustedtimer = new System.Timers.Timer();
                    this.bustedtimer.Interval = 1000;
                    this.bustedtimer.Elapsed += BustedTick;
                    this.bustedtimer.Start();

                    busted_tick = 0;
                    //ждем 5 сек
                    while (true){
                        Thread.Sleep(0);
                        if (busted_tick >= 5){
                            bustedtimer.Stop();
                            break;
                        }
                    }

                    //случайным образом выбираем кто из сотрудников экипажа будет составлять протокол
                    int diapsotr;
                    if (Sotr3_FIO != "") diapsotr = 3; else diapsotr = 2;
                    int val3 = rnd.Next(1, diapsotr);

                    //случайным образом выбираем преступника и свидетеля
                    int intruder_id = rnd.Next(0, intruders.Count - 1);
                    int with1_id = rnd.Next(0, witnesses.Count - 1);
                    int with2_id;
                    while (true) {
                        with2_id = rnd.Next(0, witnesses.Count - 1);
                        Thread.Sleep(0);
                        if (with1_id != with2_id) break;
                    }
                    
                    //заполняем протокол
                    protolocal.Add(new ProtocolClass());
                    protolocal[protolocal.Count - 1].Article = GetArticle(val2);

                    DateTime dtr = DateTime.Now;
                    dtr.AddDays(rnd.Next(2, 20));
                    dtr.AddMonths(rnd.Next(0, 2));
                    protolocal[protolocal.Count - 1].Date = dtr;

                    switch (val3) {
                        case 1:
                            protolocal[protolocal.Count - 1].Sotr_FIO = this.Sotr1_FIO;
                            protolocal[protolocal.Count - 1].Sotr_rank = this.Sotr1_rank;
                            protolocal[protolocal.Count - 1].SOTR_pidpfilepath = this.Sotr1_pidpfilename;
                            break;
                        case 2:
                            protolocal[protolocal.Count - 1].Sotr_FIO = this.Sotr2_FIO;
                            protolocal[protolocal.Count - 1].Sotr_rank = this.Sotr2_rank;
                            protolocal[protolocal.Count - 1].SOTR_pidpfilepath = this.Sotr2_pidpfilename;
                            break;
                        case 3:
                            protolocal[protolocal.Count - 1].Sotr_FIO = this.Sotr3_FIO;
                            protolocal[protolocal.Count - 1].Sotr_rank = this.Sotr3_rank;
                            protolocal[protolocal.Count - 1].SOTR_pidpfilepath = this.Sotr3_pidpfilename;
                            break;
                    }
                    protolocal[protolocal.Count - 1].Intruder_FIO = intruders[intruder_id].FIO;
                    protolocal[protolocal.Count - 1].Intruder_pidpfilepath = intruders[intruder_id].pidpfilename;
                    protolocal[protolocal.Count - 1].Witnesses1_FIO = witnesses[with1_id].FIO;
                    protolocal[protolocal.Count - 1].Witnesses2_FIO = witnesses[with2_id].FIO;
                    protolocal[protolocal.Count - 1].Witness1_pidpfilepath = witnesses[with1_id].pidpfilename;
                    protolocal[protolocal.Count - 1].Witness2_pidpfilepath = witnesses[with2_id].pidpfilename;

                    //+1 протокол составлен
                    report.Proto_count++;

                    break;
                }
            }
        }

        //Функция патрулирования (работа с координатами, логика движения, задержания и выписывания протокола)
        private void Tick(Object source, System.Timers.ElapsedEventArgs e)
        {
            //Если маршрут закончился, прибавить круг, начать новый
            if (this.GetTekCPid() == localtrack.control_points.Count - 1)
            {
                this.SetTekCPid(localtrack.point_back_id);
                laps--;
                if (laps == 0) this.SetTekCPid(0);
            }
            //Если все круги пройдены и экипаж вернулся в гараж
            if (laps == 0 && this.GetTekCPid() == localtrack.point_back_id)
            {
                this.SetEciDone();
                timer.Stop();
                report.Finish = DateTime.Now;
                MessageBox.Show("Экипаж №" + this.Bort_num + " завершил патрулирование", "Отслеживание патрулирования", MessageBoxButtons.OK, MessageBoxIcon.Information);  
            }
            else
            {
                int x1, x2, y1, y2;
                //Если все круги пройдены, нужно вернуться в гараж
                if (laps == 0)
                {
                    x2 = localbacktrack.control_points[this.GetTekCPid() + 1].x;
                    y2 = localbacktrack.control_points[this.GetTekCPid() + 1].y;
                    x1 = localbacktrack.control_points[this.GetTekCPid()].x;
                    y1 = localbacktrack.control_points[this.GetTekCPid()].y;
                }
                else
                {
                    x2 = localtrack.control_points[this.GetTekCPid() + 1].x;
                    y2 = localtrack.control_points[this.GetTekCPid() + 1].y;
                    x1 = localtrack.control_points[this.GetTekCPid()].x;
                    y1 = localtrack.control_points[this.GetTekCPid()].y;
                }

                this.x = x1 + (x2 - x1) * timer_takts_count / steps;
                this.y = y1 + (y2 - y1) * timer_takts_count / steps;

                //Достижение контрольной точки
                if (this.x == x2 && this.y == y2)
                {
                    this.SetTekCPid(this.GetTekCPid() + 1);
                    timer_takts_count = 1;

                    //Если патруль не едет в гараж (или везем преступника в гараж)
                    if (this.laps != 0) {
                        //останавливаем таймер патрулирования
                        timer.Stop();
                        //Проверяем преступления на контрольной точке
                        Detention();
                        //Продолжаем патрулирование
                        busted = false;
                        timer.Start();
                    }
                }
            }
            timer_takts_count++;
        }

        //Функция инициализации значений экипажа для начала патрулирования. 
        //Параметры: координаты x, y, маршрут патрулирования, маршрут возврата в гараж, интервал таймера, количество кругов,
        //используемые нарушители, используемые свидетели
        public void CrewToPatrolInit(int x, int y, TracksClass t1, TracksClass t2, int interval, int laps, List<IntrudersClass> intrds, List<WitnessesClass> wtns) {
            //координаты
            this.x = x;
            this.y = y;
            //текущая начальная точка = начальная
            this.tek_control_point_id = 0;
            //Загружаем текстуру экипажа на карте
            this.btm = Image.FromFile("data/police_crew.png");
            this.localtrack = t1;
            this.localbacktrack = t2;
            //Инициализация таймера и его значений
            this.timer_takts_count = 1;
            this.steps = 10;
            this.timer = new System.Timers.Timer();
            this.timer.Interval = interval;
            this.timer.Elapsed += Tick;
            this.laps = laps;
            //Нарушители
            this.intruders = intrds;
            //Свидетели
            this.witnesses = wtns;
            //патруль не завершил патрулирование
            done = false;
            //добавляем начальные данные в отчет о патрулировании
            report.Sotr1_FIO = this.Sotr1_FIO;
            report.Sotr2_FIO = this.Sotr2_FIO;
            report.Sotr3_FIO = this.Sotr3_FIO;
            report.Sotr1_rank = this.Sotr1_rank;
            report.Sotr2_rank = this.Sotr2_rank;
            report.Sotr3_rank = this.Sotr3_rank;
            report.Track_ID = this.Track_ID;
            report.Crew_Bort_num = this.Bort_num;
            report.Proto_count = 0;
        }

        //Функция запуска патрулирования
        public void StartMoving() {
            //записываем дату/время начала патрулирования
            report.Start = DateTime.Now;
            //запускаем экипаж в работу
            this.timer.Start();
        }

        public ReportClass GetReport() {
            return report;
        }
        public bool BustedStatus() {
            return busted;
        }
        public int GetTekCPid() {
            return tek_control_point_id;
        }
        public void SetTekCPid(int val) {
            this.tek_control_point_id = val;
        }
        public int GetX() {
            return x;
        }
        public int GetY() {
            return y;
        }
        public Image GetBTM() {
            return btm;
        }
        public void SetEciDone() {
            done = true;
        }
        public bool ifECiDone() {
            return done;
        }
        public int GetProtoCout() {
            return protolocal.Count;
        }
        public ProtocolClass GetProto(int index) {
            return protolocal[index];
        }

        //Координаты, текущая контрольная точка, количество кругов, тактов таймера патрулирования прошло, 
        //количество промежуточных звен между контрольными точками, тактов таймера задержания прошло
        private int x, y, tek_control_point_id, laps, timer_takts_count, steps;
        static private int busted_tick;
        //Текстура экипажа
        private Image btm;
        //Патрулирование завершено
        private bool done = false;
        //Патруль занят задержанием преступника
        private bool busted = false;
        //Таймер патрулирования, таймер задержания
        private System.Timers.Timer timer;
        private System.Timers.Timer bustedtimer;
        //Маршут патрулирования, маршрут возврата в гараж
        private TracksClass localtrack, localbacktrack;
        //Составленные протоколы экипажем в процессе патрулирования
        private List<ProtocolClass> protolocal = new List<ProtocolClass>();
        //Преступники, которые могут учавствовать в преступлениях на маршруте патрулирования
        //(в данной версии все они одинаковы, но возможно использование различных. Загружаются в инициализации экипажа)
        private List<IntrudersClass> intruders = new List<IntrudersClass>();
        //Свидетели, которые могут учавствовать в преступленияхна маршруте патрулирования
        //(в данной версии все они одинаковы, но возможно использование различных. Загружаются в инициализации экипажа)
        private List<WitnessesClass> witnesses = new List<WitnessesClass>();
        //Отчет о патрулировании
        private ReportClass report = new ReportClass();
    }
}
