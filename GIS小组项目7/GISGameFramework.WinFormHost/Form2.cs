using GISGameFramework.Core;
using GISGameFramework.Game;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace QA
{
    public partial class Form2 : Form
    {
        // 数据结构
        private class Question
        {
            public int Qid { get; set; }           // 题目ID
            public string Content { get; set; }     // 题面内容
            public int QType { get; set; }          // 0:单选, 1:多选
            public int OptionCount { get; set; }    // 选项数量
            public List<string> Options { get; set; } // 选项列表
            public List<int> AnswerKey { get; set; }   // 正确答案索引
            public int Score { get; set; }          // 分值
            public int Theme { get; set; }       // 主题：1-历史，2-测绘，3-景观
        }

        // 题库数据
        private List<Question> allQuestions = new List<Question>();

        // 当前抽取的5道题目
        private List<Question> currentQuestions = new List<Question>();

        private int currentIndex = 0;           // 当前题目索引（从0开始）
        private int totalScore = 0;              // 累计得分
        private List<string> userAnswers = new List<string>();     // 用户每题的答案
        private HashSet<int> scoredQuestions = new HashSet<int>(); // 已得分题目
        private bool isQuizCompleted = false;  // 是否已完成所有题目
        private GameCoreManager _game;
        private string _sessionId;
        private string _playerId = "user_1";
        public event EventHandler QuizCompleted;
        public Action<string> OnLog;

        public Form2(GameCoreManager mainGame, string sessionId = null, Action<string> onLog = null)
        {
            InitializeComponent();
            _game = mainGame;
            _sessionId = sessionId;
            OnLog = onLog;
            InitializeCustomControls();
            LoadBuiltInQuestions();  // 加载内置题库
            StartNewQuiz();          // 开始新的一轮答题
        }

        private void InitializeCustomControls()
        {
            // 设置窗体属性
            this.Text = "同济历史与校园知识答题系统";
            this.StartPosition = FormStartPosition.CenterScreen;

            // 初始化单选按钮组（初始隐藏）
            for (int i = 1; i <= 4; i++)
            {
                Control[] ctrls = this.Controls.Find("rbOption" + i, true);
                RadioButton rb = ctrls.Length > 0 ? ctrls[0] as RadioButton : null;
                if (rb != null) rb.Visible = false;
            }

            // 初始化复选框组（初始隐藏）
            for (int i = 1; i <= 4; i++)
            {
                Control[] ctrls = this.Controls.Find("cbOption" + i, true);
                CheckBox cb = ctrls.Length > 0 ? ctrls[0] as CheckBox : null;
                if (cb != null) cb.Visible = false;
            }

            // 设置进度条
            progressBar1.Minimum = 0;
            progressBar1.Maximum = 100;
            progressBar1.Value = 0;

        }

        // ==================== 内置题库数据 ====================
        private void LoadBuiltInQuestions()
        {
            // ========== 主题1：同济历史（101-120）==========
            AddQuestion(101, "同济大学的建校时间是哪一年？", 0, 4,
                new List<string> { "1907年", "1912年", "1919年", "1927年" },
                new List<int> { 1 }, 1, 1);

            AddQuestion(102, "同济大学最初的校名是？", 0, 4,
                new List<string> { "同济德文医学堂", "上海医科大学", "同济大学堂", "华东同济学堂" },
                new List<int> { 1 }, 1, 1);

            AddQuestion(103, "同济大学的校训是？", 0, 4,
                new List<string> { "严谨求实", "宁静致远", "同舟共济", "团结创新" },
                new List<int> { 3 }, 1, 1);

            AddQuestion(104, "抗战时期，同济大学曾进行过多次迁校，以下哪个城市不是其迁校目的地？", 0, 4,
                new List<string> { "金华", "宜宾", "桂林", "成都" },
                new List<int> { 4 }, 2, 1);

            AddQuestion(105, "在哪一年教育部正式批准将同济大学列入创建世界一流大学（即\"985工程\"）名单？", 0, 4,
                new List<string> { "1985年", "1996年", "2001年", "2010年" },
                new List<int> { 3 }, 5, 1);

            AddQuestion(106, "2000年，同济大学合并了以下哪所高校，进一步扩大了学科覆盖面？", 0, 4,
                new List<string> { "上海交通大学医学院", "上海铁道大学", "华东师范大学", "上海财经大学" },
                new List<int> { 2 }, 5, 1);

            AddQuestion(107, "同济大学的校徽中，不包含以下哪种元素？", 0, 4,
                new List<string> { "龙舟", "小人", "齿轮", "船桨" },
                new List<int> { 3 }, 1, 1);

            AddQuestion(108, "以下哪个学院没有搬迁至同济大学嘉定校区？", 0, 4,
                new List<string> { "汽车", "测绘", "机械", "电信" },
                new List<int> { 2 }, 1, 1);

            AddQuestion(109, "在1952年的调整中，下列哪些大学的土木、建筑、测量各系、科、组被集中于同济大学？（多选）", 1, 4,
                new List<string> { "交通大学", "震旦大学", "上海市工业专科学校", "中华工商学校" },
                new List<int> { 1, 2, 3, 4 }, 5, 1);

            AddQuestion(110, "同济大学的校庆日是每年的哪一天？", 0, 4,
                new List<string> { "5月20日", "6月1日", "9月10日", "10月15日" },
                new List<int> { 1 }, 2, 1);

            AddQuestion(111, "以下关于同济大学的历史沿革，说法正确的有（多选）？", 1, 4,
                new List<string> { "最初由德国医生埃里希·宝隆创办", "属于教育部直属高校", "\"同济\"二字来自德语", "是\"985工程\"重点建设高校" },
                new List<int> { 1, 2, 3, 4 }, 2, 1);

            AddQuestion(112, "抗战时期，同济大学迁校过程中，坚持办学，培养了大批人才，以下哪些学科在当时已具备较强实力（多选）？", 1, 4,
                new List<string> { "土木工程", "医学", "测绘工程", "艺术" },
                new List<int> { 1, 2, 3 }, 2, 1);

            AddQuestion(113, "同济大学成为国家\"双一流\"建设高校后，重点建设的学科领域包括（多选）？", 1, 4,
                new List<string> { "土木工程", "测绘科学与技术", "环境科学与工程", "建筑与城市规划" },
                new List<int> { 1, 2, 3, 4 }, 2, 1);

            AddQuestion(114, "以下哪些建筑是同济大学历史建筑，至今仍在使用（多选）？", 1, 4,
                new List<string> { "一·二九礼堂", "大礼堂", "西南一楼", "大学生活动中心" },
                new List<int> { 1, 2, 3 }, 2, 1);

            AddQuestion(115, "同济大学在发展过程中，形成了多个校区，以下哪些是同济大学的正式校区（多选）？", 1, 4,
                new List<string> { "四平路校区", "嘉定校区", "闵行校区", "沪西校区" },
                new List<int> { 1, 2, 4 }, 2, 1);

            AddQuestion(116, "以下关于同济大学医学学科的说法，正确的有（多选）？", 1, 4,
                new List<string> { "源自建校初期的德文医学堂", "曾独立为上海医科大学", "现设有医学院和口腔医学院", "拥有多家附属医院" },
                new List<int> { 1, 3, 4 }, 2, 1);

            AddQuestion(117, "同济大学的校歌中，包含以下哪些关键词（多选）？", 1, 4,
                new List<string> { "齐心协力", "自强不息", "严谨求实", "振兴中华" },
                new List<int> { 1, 3 }, 5, 1);

            AddQuestion(118, "建国后，同济大学在哪些领域为国家建设做出了重要贡献（多选）？", 1, 4,
                new List<string> { "城市规划", "桥梁建设", "航天工程", "水利工程" },
                new List<int> { 1, 2, 3, 4 }, 2, 1);

            AddQuestion(119, "以下哪些是同济大学的特色文化活动（多选）？", 1, 4,
                new List<string> { "社团\"百团大战\"", "\"一·二九\"爱国主题宣讲", "高雅艺术进校园", "新生杯体育比赛" },
                new List<int> { 1, 2, 3, 4 }, 2, 1);

            AddQuestion(120, "以下关于同济大学与国外高校合作的说法，正确的有（多选）？", 1, 4,
                new List<string> { "与德国多所高校有长期合作", "设有中外合作办学项目", "学生可申请交换生项目", "仅与欧洲高校合作" },
                new List<int> { 1, 2, 3 }, 2, 1);

            // ========== 主题2：同济测绘（201-210）==========
            AddQuestion(201, "同济大学测绘与地理信息学院成立于哪一年？", 0, 4,
                new List<string> { "1956年", "1985年", "2000年", "2012年" },
                new List<int> { 4 }, 2, 2);

            AddQuestion(202, "测绘专业最常用的基础测量仪器是？", 0, 4,
                new List<string> { "全站仪", "显微镜", "示波器", "天平" },
                new List<int> { 1 }, 1, 2);

            AddQuestion(203, "以下哪种技术属于同济大学测绘学院的优势研究方向？", 0, 4,
                new List<string> { "卫星导航定位", "分子生物学", "电动力学", "古典文学" },
                new List<int> { 1 }, 2, 2);

            AddQuestion(204, "测绘专业学生进行野外实习时，以下哪种物品不需要携带？", 0, 4,
                new List<string> { "听诊器", "全站仪", "三脚架", "棱镜" },
                new List<int> { 1 }, 1, 2);

            AddQuestion(205, "同济大学测绘学院拥有的重要科研平台是？", 0, 4,
                new List<string> { "自然资源部现代工程测量重点实验室", "城市污染控制国家工程研究中心", "海洋地质全国重点实验室", "土木工程防灾减灾全国重点实验室" },
                new List<int> { 1 }, 2, 2);

            AddQuestion(206, "以下关于GIS（地理信息系统）的说法，正确的有（多选）？", 1, 4,
                new List<string> { "是测绘专业的核心课程之一", "可用于城市规划", "能实现空间数据可视化", "是用于输入、存储、分析和显示地理数据的计算机系统" },
                new List<int> { 1, 2, 3, 4 }, 2, 2);

            AddQuestion(207, "3S系统指的是（多选）？", 1, 4,
                new List<string> { "全自动绘图系统(ADS)", "遥感(RS)", "地理信息系统(GIS)", "全球导航卫星系统(GNSS/GPS)" },
                new List<int> { 2, 3, 4 }, 2, 2);

            AddQuestion(208, "以下哪些是测绘工程专业的核心课程（多选）？", 1, 4,
                new List<string> { "测量学", "量子力学", "地理信息系统", "误差理论与测量平差" },
                new List<int> { 1, 3, 4 }, 2, 2);

            AddQuestion(209, "同济大学测绘学院在以下哪些领域有突出成果（多选）？", 1, 4,
                new List<string> { "卫星导航定位技术", "遥感图像处理", "精密工程测量", "建筑设计" },
                new List<int> { 1, 2, 3 }, 2, 2);

            AddQuestion(210, "以下哪些是测量实习的内容（多选）？", 1, 4,
                new List<string> { "水准测量", "导线测量", "地形测量", "地形成图" },
                new List<int> { 1, 2, 3, 4 }, 2, 2);

            // ========== 主题3：同济景观（301-330）==========
            AddQuestion(301, "同济大学的主校门位于？", 0, 4,
                new List<string> { "赤峰路50号", "赤峰路200号", "四平路1239号", "国康路99号" },
                new List<int> { 3 }, 1, 3);

            AddQuestion(302, "衷和楼是同济百年校庆的标志性建筑之一，下列关于衷和楼的说法错误的是？", 0, 4,
                new List<string> { "原名综合楼", "中庭等公共空间外包混凝土幕墙", "除会议室、办公室外还设有阶梯教室", "地上有21层，象征21世纪" },
                new List<int> { 2 }, 2, 3);

            AddQuestion(303, "每年四月初吸引许多师生和公众前来游览的樱花大道位于哪条路上？", 0, 4,
                new List<string> { "南大道", "景行路", "东大道", "爱校路" },
                new List<int> { 4 }, 2, 3);

            AddQuestion(304, "同济大学校史馆位于哪栋楼内？", 0, 4,
                new List<string> { "同文楼", "文远楼", "同创楼", "汇文楼" },
                new List<int> { 3 }, 5, 3);

            AddQuestion(305, "下列有关同济大学四平路校区内的毛主席塑像的说法中，错误的是", 0, 4,
                new List<string> { "该塑像于1967年7月1日落成", "毛主席背手呈\"闲庭信步\"的形象", "为上海高校中第一尊毛泽东塑像", "毛主席像 7.1 米高" },
                new List<int> { 2 }, 2, 3);

            AddQuestion(306, "同济高等讲堂经常在哪栋楼里举行？", 0, 4,
                new List<string> { "教学南楼", "一·二九大楼", "行政楼", "逸夫楼" },
                new List<int> { 4 }, 2, 3);

            AddQuestion(307, "同济大学大礼堂被誉为\"远东第一跨\"，下列说法正确的有（多选）？", 1, 4,
                new List<string> { "1962年建成时为亚洲最大的无柱中空大礼堂", "最初的建造需求为饭厅兼礼堂", "2007年完成外立面改造和更新扩建", "采用自然通风和机械通风相结合" },
                new List<int> { 1, 2, 3, 4 }, 2, 3);

            AddQuestion(308, "同济大学四平路校区内的室内羽毛球场地分布在（多选）？", 1, 4,
                new List<string> { "攀岩馆", "一·二九训练馆", "拳操房", "体育馆" },
                new List<int> { 1, 2, 4 }, 5, 3);

            AddQuestion(309, "以下哪些建筑是同济大学四平路校区的历史保护建筑？", 1, 4,
                new List<string> { "中法中心", "旭日楼", "文远楼", "图书馆" },
                new List<int> { 2, 3, 4 }, 5, 3);

            AddQuestion(310, "同济校园内现存最早的建筑是？", 0, 4,
                new List<string> { "衷和楼", "经纬楼", "一·二九大楼", "图书馆" },
                new List<int> { 3 }, 2, 3);

            AddQuestion(311, "下列学院中哪些的本部位于四平路校区（多选）？", 1, 4,
                new List<string> { "马克思主义学院", "物理科学与工程学院", "测绘与地理信息学院", "材料科学与工程学院" },
                new List<int> { 1, 2, 3 }, 5, 3);

            AddQuestion(312, "以下关于一·二九学生运动纪念园的说法，正确的有（多选）？", 1, 4,
                new List<string> { "1987年纪念园落成", "采用青石、汉白玉为基色", "园中石壁上铭刻着同济烈士的英名", "已被确定为杨浦区爱国主义教育基地" },
                new List<int> { 1, 2, 3, 4 }, 2, 3);

            AddQuestion(313, "以下属于女生宿舍的楼宇有（多选）？", 1, 4,
                new List<string> { "学三楼", "西南八楼", "西南九楼", "西南二楼" },
                new List<int> { 1, 3, 4 }, 2, 3);

            AddQuestion(314, "以下属于男生宿舍的楼宇有（多选）？", 1, 4,
                new List<string> { "西南九楼", "西南八楼", "西北五楼", "西南七楼" },
                new List<int> { 2, 3, 4 }, 2, 3);

            AddQuestion(315, "同济大学四平路校区主区内有几个食堂（肯德基和面包房不算）？", 0, 4,
                new List<string> { "3", "4", "5", "6" },
                new List<int> { 3 }, 2, 3);

            AddQuestion(316, "同济大学建筑与城市规划学院拥有悠久的历史和雄厚的学科基础。城规学院在四平路校区有几栋教学楼？", 0, 4,
                new List<string> { "3", "4", "5", "6" },
                new List<int> { 2 }, 2, 3);

            AddQuestion(317, "以下建筑内有自习区域的有？（多选）", 1, 4,
                new List<string> { "旭日楼", "瑞安楼", "图书馆", "南北楼" },
                new List<int> { 2, 3, 4 }, 2, 3);

            AddQuestion(318, "下列属于环境科学与工程学院的办公教学楼宇是（多选）？", 1, 4,
                new List<string> { "致远楼", "生态楼", "宁静楼", "明净楼" },
                new List<int> { 2, 4 }, 5, 3);

            AddQuestion(319, "以下属于同济大学四平路校区内的工程实验室的是？（多选）", 1, 4,
                new List<string> { "机械馆", "结构试验馆", "声学馆", "汽车工程实验中心" },
                new List<int> { 1, 2, 3 }, 2, 3);

            AddQuestion(320, "四平路校区下列建筑在最近一年内重新装修的有（多选）？", 1, 4,
                new List<string> { "西苑饮食广场", "海洋楼", "西南七楼", "运筹楼" },
                new List<int> { 1, 3 }, 2, 3);

            AddQuestion(321, "\"三好坞\"春有流水潺湲，夏有榴花明媚，秋有笛声清越，冬有细雪纷飞。三好坞的两座小石桥的名字是（多选）？", 1, 4,
                new List<string> { "秀隐", "隐秀", "枕流", "瀑虹" },
                new List<int> { 2, 3 }, 5, 3);

            AddQuestion(322, "下列哪些区域有供人休憩的草坪（多选）？", 1, 4,
                new List<string> { "一·二九运动场", "西南一楼楼前", "南楼西侧", "南北楼之间" },
                new List<int> { 2, 3, 4 }, 2, 3);

            AddQuestion(323, "同济大学校医院离哪个校门最接近？", 0, 4,
                new List<string> { "国康路99号", "赤峰路200号", "四平路1239号", "赤峰路50号" },
                new List<int> { 4 }, 5, 3);

            AddQuestion(324, "下列位于西苑饮食广场周围的建筑有（多选）？", 1, 4,
                new List<string> { "济阳楼", "西南七楼", "西南九楼", "西南一楼" },
                new List<int> { 1, 3, 4 }, 2, 3);

            AddQuestion(325, "下列位于西大道两侧的建筑有（多选）？", 1, 4,
                new List<string> { "光学馆", "济阳楼", "解放楼", "实验动物中心" },
                new List<int> { 1, 2, 4 }, 2, 3);

            AddQuestion(326, "下列选项中的两个建筑在正射影像上形状相仿的有？", 1, 4,
                new List<string> { "西北四楼和西北五楼", "青年楼和解放楼", "西北一楼和西北二楼", "南楼和北楼" },
                new List<int> { 1, 4 }, 5, 3);

            AddQuestion(327, "国康路99号校门附近的建筑有（多选）？", 1, 4,
                new List<string> { "学四楼", "西北三楼", "学五楼", "西北二楼" },
                new List<int> { 1, 3, 4 }, 2, 3);

            AddQuestion(328, "以下哪种食物无法在北苑饮食广场享用到？", 0, 4,
                new List<string> { "烤盘饭", "面包", "烤串", "石锅拌饭" },
                new List<int> { 3 }, 2, 3);

            AddQuestion(329, "以下哪个食堂提供清真风味餐食？", 0, 4,
                new List<string> { "北苑饮食广场", "三好坞饮食广场", "西苑饮食广场", "学苑饮食广场" },
                new List<int> { 2 }, 2, 3);

            AddQuestion(330, "下面是同济学生小王的一天，哪些说法是正确的？（多选）", 1, 4,
                new List<string> { "早上来到爱校路面包房购买早餐", "上午在西北教超二楼购买高数习题册", "下午5点半来到一·二九运动场刷锻", "晚上去大学生活动中心后的开水房打水" },
                new List<int> { 1, 3 }, 5, 3);
        }

        // 添加题目的辅助方法
        private void AddQuestion(int qid, string content, int qType, int optionCount,
            List<string> options, List<int> answerKey, int score, int theme)
        {
            Question q = new Question();
            q.Qid = qid;
            q.Content = content;
            q.QType = qType;
            q.OptionCount = optionCount;
            q.Options = options;
            q.AnswerKey = answerKey;
            q.Score = score;
            q.Theme = theme;
            var mq = new QuizQuestion
            {
                QuestionId = qid.ToString(),
                Content = content,
                Mode = qType == 0 ? QuizMode.SingleChoice : QuizMode.MultipleChoice,
                AnswerKey = string.Join(",",answerKey),
                ScoreValue = score
            };
            foreach (var opt in options) mq.Options.Add(opt);
            this._game.QuizService.AddQuestion(mq);
            allQuestions.Add(q);
        }

        // ==================== 随机抽题逻辑 ====================
        private void StartNewQuiz()
        {
            Random rand = new Random();
            currentQuestions.Clear();
            userAnswers.Clear();
            scoredQuestions.Clear();
            totalScore = 0;
            currentIndex = 0;

            // 按主题分组
            List<Question> theme1 = allQuestions.Where(q => q.Theme == 1).ToList(); // 历史
            List<Question> theme2 = allQuestions.Where(q => q.Theme == 2).ToList(); // 测绘
            List<Question> theme3 = allQuestions.Where(q => q.Theme == 3).ToList(); // 景观

            // 从每个主题中随机抽取题目
            // 分配：主题1抽2题，主题2抽1题，主题3抽2题（共5题）
            currentQuestions.AddRange(GetRandomQuestions(theme1, 2, rand));
            currentQuestions.AddRange(GetRandomQuestions(theme2, 1, rand));
            currentQuestions.AddRange(GetRandomQuestions(theme3, 2, rand));

            // 打乱题目顺序（可选）
            currentQuestions = currentQuestions.OrderBy(x => rand.Next()).ToList();

            // 初始化用户答案（-1表示未作答）
            for (int i = 0; i < currentQuestions.Count; i++)
            {
                userAnswers.Add("-1");
            }

            // 更新UI显示
            UpdateScoreDisplay();
            LoadQuestion(currentIndex);
            SetUIState(true);
            UpdateProgressBar();

            // 重置完成状态和按钮文本
            isQuizCompleted = false;
            btnNext.Text = "下一题";
        }

        // 从题目列表中随机抽取指定数量的题目
        private List<Question> GetRandomQuestions(List<Question> source, int count, Random rand)
        {
            if (source.Count <= count)
            {
                return new List<Question>(source);
            }

            // 随机抽取不重复的题目
            List<Question> shuffled = new List<Question>(source);
            List<Question> result = new List<Question>();

            for (int i = 0; i < count; i++)
            {
                int index = rand.Next(shuffled.Count);
                result.Add(shuffled[index]);
                shuffled.RemoveAt(index);
            }

            return result;
        }

        private void SetUIState(bool enabled)
        {
            groupOptions.Enabled = enabled;
            btnSubmit.Enabled = enabled;
            btnNext.Enabled = enabled;
            btnPrev.Enabled = enabled;
        }

        private string Truncate(string str, int maxLen)
        {
            if (str.Length <= maxLen) return str;
            return str.Substring(0, maxLen) + "...";
        }

        private void LoadQuestion(int index)
        {
            if (index < 0 || index >= currentQuestions.Count) return;

            Question q = currentQuestions[index];

            // 更新标题
            lblQuestionTitle.Text = string.Format("第 {0} / {1} 题", index + 1, currentQuestions.Count);
            lblQuestionContent.Text = q.Content;
            lblQuestionType.Text = q.QType == 0 ? "【单选题】" : "【多选题】";

            // 显示选项
            DisplayOptions(q);

            // 显示已保存的答案（如果有）
            if (userAnswers[index] != "-1")
            {
                RestoreUserAnswer(q, userAnswers[index]);

                //恢复之前的结果显示
                RestoreResultDisplay(q, userAnswers[index]);
            }
            else
            {
                ClearOptions();
                // 清空结果显示
                lblCorrectAnswer.Text = "";
                lblUserAnswer.Text = "";
            }

            // 更新按钮状态
            btnPrev.Enabled = index > 0;
            btnNext.Enabled = index < currentQuestions.Count - 1;

            // 更新按钮文本（最后一题显示"结束"）
            if (index == currentQuestions.Count - 1)
            {
                btnNext.Text = "结束";
            }
            else
            {
                btnNext.Text = "下一题";
            }

            // 更新进度条
            UpdateProgressBar();
        }

        // 恢复之前的结果显示
        private void RestoreResultDisplay(Question q, string userAnswer)
        {
            if (userAnswer == "-1") return;

            // 格式化正确答案
            string correctStr = "";
            for (int i = 0; i < q.AnswerKey.Count; i++)
            {
                if (i > 0) correctStr += ",";
                correctStr += Convert.ToChar(64 + q.AnswerKey[i]).ToString();
            }

            // 格式化用户答案
            string userStr = FormatUserAnswer(q, userAnswer);

            lblCorrectAnswer.Text = string.Format("正确答案：{0}", correctStr);
            lblUserAnswer.Text = string.Format("您的答案：{0}", userStr);
        }

        private void DisplayOptions(Question q)
        {
            // 隐藏所有选项控件
            for (int i = 1; i <= 4; i++)
            {
                Control[] rbCtrls = this.Controls.Find("rbOption" + i, true);
                RadioButton rb = rbCtrls.Length > 0 ? rbCtrls[0] as RadioButton : null;
                if (rb != null) rb.Visible = false;

                Control[] cbCtrls = this.Controls.Find("cbOption" + i, true);
                CheckBox cb = cbCtrls.Length > 0 ? cbCtrls[0] as CheckBox : null;
                if (cb != null) cb.Visible = false;
            }

            if (q.QType == 0) // 单选题
            {
                for (int i = 0; i < q.OptionCount && i < 4; i++)
                {
                    Control[] ctrls = this.Controls.Find("rbOption" + (i + 1), true);
                    RadioButton rb = ctrls.Length > 0 ? ctrls[0] as RadioButton : null;
                    if (rb != null)
                    {
                        rb.Text = string.Format("{0}. {1}", Convert.ToChar(65 + i), q.Options[i]);
                        rb.Visible = true;
                        rb.Checked = false;
                    }
                }
            }
            else // 多选题
            {
                for (int i = 0; i < q.OptionCount && i < 4; i++)
                {
                    Control[] ctrls = this.Controls.Find("cbOption" + (i + 1), true);
                    CheckBox cb = ctrls.Length > 0 ? ctrls[0] as CheckBox : null;
                    if (cb != null)
                    {
                        cb.Text = string.Format("{0}. {1}", Convert.ToChar(65 + i), q.Options[i]);
                        cb.Visible = true;
                        cb.Checked = false;
                    }
                }
            }
        }


        private void ClearOptions()
        {
            for (int i = 1; i <= 4; i++)
            {
                Control[] rbCtrls = this.Controls.Find("rbOption" + i, true);
                RadioButton rb = rbCtrls.Length > 0 ? rbCtrls[0] as RadioButton : null;
                if (rb != null) rb.Checked = false;

                Control[] cbCtrls = this.Controls.Find("cbOption" + i, true);
                CheckBox cb = cbCtrls.Length > 0 ? cbCtrls[0] as CheckBox : null;
                if (cb != null) cb.Checked = false;
            }
        }

        private string GetCurrentUserAnswer(Question q)
        {
            if (q.QType == 0) // 单选
            {
                for (int i = 0; i < q.OptionCount && i < 4; i++)
                {
                    Control[] ctrls = this.Controls.Find("rbOption" + (i + 1), true);
                    RadioButton rb = ctrls.Length > 0 ? ctrls[0] as RadioButton : null;
                    if (rb != null && rb.Checked)
                    {
                        return (i + 1).ToString();  // 返回 "1", "2" 等
                    }
                }
                return "-1";
            }
            else // 多选
            {
                List<int> selected = new List<int>();
                for (int i = 0; i < q.OptionCount && i < 4; i++)
                {
                    Control[] ctrls = this.Controls.Find("cbOption" + (i + 1), true);
                    CheckBox cb = ctrls.Length > 0 ? ctrls[0] as CheckBox : null;
                    if (cb != null && cb.Checked)
                    {
                        selected.Add(i + 1);
                    }
                }
                if (selected.Count == 0) return "-1";

                return string.Join(",", selected);  // 返回 "2,4" 格式
            }
        }

        private void RestoreUserAnswer(Question q, string answerValue)
        {
            ClearOptions();
            if (answerValue == "-1") return;

            if (q.QType == 0) // 单选
            {
                Control[] ctrls = this.Controls.Find("rbOption" + answerValue, true);
                RadioButton rb = ctrls.Length > 0 ? ctrls[0] as RadioButton : null;
                if (rb != null) rb.Checked = true;
            }
            else // 多选
            {
                string[] selected = answerValue.ToString().Split(',');
                foreach (string s in selected)
                {
                    int idx;
                    if (int.TryParse(s, out idx))
                    {
                        Control[] ctrls = this.Controls.Find("cbOption" + idx, true);
                        CheckBox cb = ctrls.Length > 0 ? ctrls[0] as CheckBox : null;
                        if (cb != null) cb.Checked = true;
                    }
                }
            }
        }

        private void btnSubmit_Click(object sender, EventArgs e)
        {
            if (currentIndex >= currentQuestions.Count) return;

            Question q = currentQuestions[currentIndex];
            string userAnswer = GetCurrentUserAnswer(q);

            if (userAnswer == "-1")
            {
                MessageBox.Show("请先作答！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // 保存答案（字符串格式，如 "2" 或 "2,4"）
            userAnswers[currentIndex] = userAnswer;

            // 计算是否正确
            bool isCorrect = IsAnswerCorrect(q, userAnswer);

            // 格式化正确答案和用户答案
            string correctStr = "";
            for (int i = 0; i < q.AnswerKey.Count; i++)
            {
                if (i > 0) correctStr += ",";
                correctStr += Convert.ToChar(64 + q.AnswerKey[i]).ToString();
            }

            string userStr = FormatUserAnswer(q, userAnswer);

            lblCorrectAnswer.Text = string.Format("正确答案：{0}", correctStr);
            lblUserAnswer.Text = string.Format("您的答案：{0}", userStr);

            // 判断是否已得分
            bool alreadyScored = scoredQuestions.Contains(currentIndex);

            if (isCorrect && !alreadyScored)
            {
                totalScore += q.Score;
                UpdateScoreDisplay();
                scoredQuestions.Add(currentIndex);

                // 回传主框架，触发积分/成就联动
                if (_game != null && _sessionId != null)
                {
                    var r = _game.ProcessQuizSubmissionFlow(new QuizSubmission
                    {
                        SessionId = _sessionId,
                        PlayerId = _playerId,
                        QuestionId = q.Qid.ToString(),
                        Answer = userAnswer
                    });
                    if (OnLog != null)
                    {
                        var player = _game.PlayerService.GetPlayer(_playerId);
                        string totalScore1 = player.Success ? player.Data.TotalScore.ToString() : "?";
                        OnLog(string.Format("答题得分 +{0}，当前总分：{1}", q.Score, totalScore1));
                    }
                }

                MessageBox.Show(string.Format("回答正确！ +{0}分", q.Score), "恭喜",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else if (isCorrect && alreadyScored)
            {
                MessageBox.Show("回答正确！（本题已得分）", "提示",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show(string.Format("回答错误！\n正确答案是：{0}", correctStr), "遗憾",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            UpdateProgressBar();
        }

        private bool CheckIfQuestionScored(int questionIndex)
        {
            return scoredQuestions.Contains(questionIndex);
        }

        private bool IsAnswerCorrect(Question q, string userAnswer)
        {
            if (q.QType == 0) // 单选
            {
                int userAns;
                if (int.TryParse(userAnswer, out userAns))
                {
                    return q.AnswerKey.Count == 1 && q.AnswerKey[0] == userAns;
                }
                return false;
            }
            else // 多选
            {
                string[] userSelected = userAnswer.Split(',');
                List<int> userSet = new List<int>();
                foreach (string s in userSelected)
                {
                    int num;
                    if (int.TryParse(s, out num)) userSet.Add(num);
                }
                userSet = userSet.OrderBy(x => x).ToList();
                List<int> correctSet = q.AnswerKey.OrderBy(x => x).ToList();

                // 对比两个列表是否完全一致
                if (userSet.Count != correctSet.Count) return false;
                for (int i = 0; i < userSet.Count; i++)
                {
                    if (userSet[i] != correctSet[i]) return false;
                }
                return true;
            }
        }

        private string FormatUserAnswer(Question q, string userAnswer)
        {
            if (userAnswer == "-1") return "未作答";

            if (q.QType == 0)
            {
                int ans;
                if (int.TryParse(userAnswer, out ans))
                {
                    return Convert.ToChar(64 + ans).ToString();
                }
                return userAnswer;
            }
            else
            {
                string[] parts = userAnswer.Split(',');
                List<string> charParts = new List<string>();
                foreach (string p in parts)
                {
                    int num;
                    if (int.TryParse(p, out num))
                    {
                        charParts.Add(Convert.ToChar(64 + num).ToString());
                    }
                }
                return string.Join(",", charParts.ToArray());
            }
        }

        private void UpdateScoreDisplay()
        {
            lblScoreValue.Text = totalScore.ToString();
        }

        private void UpdateProgressBar()
        {
            if (currentQuestions.Count == 0) return;
            int answeredCount = 0;
            for (int i = 0; i < userAnswers.Count; i++)
            {
                if (userAnswers[i] != "-1") answeredCount++;
            }
            progressBar1.Value = answeredCount * 100 / currentQuestions.Count;
        }

        private void btnNext_Click(object sender, EventArgs e)
        {
            if (currentIndex < currentQuestions.Count - 1)
            {
                currentIndex++;
                LoadQuestion(currentIndex);

                // 到了最后一题，按钮改为"结束"
                if (currentIndex == currentQuestions.Count - 1)
                {
                    btnNext.Text = "结束";
                    btnNext.Enabled = true;
                }
            }
            else
            {
                // 最后一题点击"结束"
                DialogResult result = MessageBox.Show(
                    string.Format("恭喜你完成了所有题目！\n\n最终得分：{0}分\n\n是否结束答题？", totalScore),
                    "完成",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    MessageBox.Show(
                        string.Format("答题结束！\n最终得分：{0}分", totalScore),
                        "感谢参与",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                    if (QuizCompleted != null) QuizCompleted(this, EventArgs.Empty);
                    this.Close();
                }
            }
        }

        private void btnPrev_Click(object sender, EventArgs e)
        {
            if (currentIndex > 0)
            {
                currentIndex--;
                LoadQuestion(currentIndex);
            }
        }
    }
}