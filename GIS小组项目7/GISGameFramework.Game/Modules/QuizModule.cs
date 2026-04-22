using System;
using System.Collections.Generic;
using GISGameFramework.Core;
using GISGameFramework.Core.Contracts;

namespace GISGameFramework.Game.Modules
{
    public class QuizModule : IQuizService
    {
        private readonly Dictionary<string, QuizQuestion> _questions = new Dictionary<string, QuizQuestion>();
        private readonly Dictionary<string, QuizSession> _sessions = new Dictionary<string, QuizSession>();

        public ResponseResult<bool> AddQuestion(QuizQuestion question)
        {
            if (question == null || string.IsNullOrWhiteSpace(question.QuestionId))
            {
                return ResponseFactory.Fail<bool>(ErrorCodes.InvalidArgument, "Question argument is invalid.");
            }

            _questions[question.QuestionId] = question;
            return ResponseFactory.Ok(true, "Question added to bank.");
        }

        public ResponseResult<QuizSession> EndSession(string sessionId)
        {
            QuizSession session;
            if (!_sessions.TryGetValue(sessionId, out session))
            {
                return ResponseFactory.Fail<QuizSession>(ErrorCodes.NotFound, "Quiz session not found.");
            }

            if (!session.IsCompleted)
            {
                return ResponseFactory.Fail<QuizSession>(ErrorCodes.InvalidState, "Quiz session is not completed yet.");
            }

            session.Status = QuizSessionStatus.Completed;
            if (!session.CompletedAt.HasValue)
            {
                session.CompletedAt = DateTime.UtcNow;
            }

            if (session.SessionType == QuizSessionType.PlayerVsPlayer)
            {
                if (session.InitiatorScore > session.OpponentScore)
                {
                    session.WinnerPlayerId = session.InitiatorPlayerId;
                }
                else if (session.OpponentScore > session.InitiatorScore)
                {
                    session.WinnerPlayerId = session.OpponentPlayerId;
                }
                else
                {
                    session.WinnerPlayerId = string.Empty;
                }
            }

            return ResponseFactory.Ok(session, "Quiz session completed.");
        }

        public ResponseResult<QuizQuestion> GetQuestion(string questionId)
        {
            QuizQuestion question;
            if (_questions.TryGetValue(questionId, out question))
            {
                return ResponseFactory.Ok(question, "Question loaded.");
            }

            return ResponseFactory.Fail<QuizQuestion>(ErrorCodes.NotFound, "Question not found.");
        }

        public ResponseResult<IList<QuizQuestion>> GetQuestionsForSession(string sessionId)
        {
            QuizSession session;
            if (!_sessions.TryGetValue(sessionId, out session))
            {
                return ResponseFactory.Fail<IList<QuizQuestion>>(ErrorCodes.NotFound, "Quiz session not found.");
            }

            var questions = new List<QuizQuestion>();
            foreach (var questionId in session.QuestionIds)
            {
                QuizQuestion question;
                if (_questions.TryGetValue(questionId, out question))
                {
                    questions.Add(question);
                }
            }

            return ResponseFactory.Ok((IList<QuizQuestion>)questions, "Session questions loaded.");
        }

        public ResponseResult<QuizSession> GetSession(string sessionId)
        {
            QuizSession session;
            if (_sessions.TryGetValue(sessionId, out session))
            {
                return ResponseFactory.Ok(session, "Quiz session loaded.");
            }

            return ResponseFactory.Fail<QuizSession>(ErrorCodes.NotFound, "Quiz session not found.");
        }

        public ResponseResult<bool> Initialize()
        {
            LoadBuiltInQuestions();
            return ResponseFactory.Ok(true, "Quiz module initialized.");
        }

        private void LoadBuiltInQuestions()
        {
            // ========== 主题1：同济历史（101-120）==========
            AddQ(101, "同济大学的建校时间是哪一年？", 0, new[] { "1907年", "1912年", "1919年", "1927年" }, "2", 1);
            AddQ(102, "同济大学最初的校名是？", 0, new[] { "同济德文医学堂", "上海医科大学", "同济大学堂", "华东同济学堂" }, "1", 1);
            AddQ(103, "同济大学的校训是？", 0, new[] { "严谨求实", "宁静致远", "同舟共济", "团结创新" }, "3", 1);
            AddQ(104, "抗战时期，同济大学曾进行过多次迁校，以下哪个城市不是其迁校目的地？", 0, new[] { "金华", "宜宾", "桂林", "成都" }, "4", 2);
            AddQ(105, "在哪一年教育部正式批准将同济大学列入创建世界一流大学（即\"985工程\"）名单？", 0, new[] { "1985年", "1996年", "2001年", "2010年" }, "3", 5);
            AddQ(106, "2000年，同济大学合并了以下哪所高校，进一步扩大了学科覆盖面？", 0, new[] { "上海交通大学医学院", "上海铁道大学", "华东师范大学", "上海财经大学" }, "2", 5);
            AddQ(107, "同济大学的校徽中，不包含以下哪种元素？", 0, new[] { "龙舟", "小人", "齿轮", "船桨" }, "3", 1);
            AddQ(108, "以下哪个学院没有搬迁至同济大学嘉定校区？", 0, new[] { "汽车", "测绘", "机械", "电信" }, "2", 1);
            AddQ(109, "在1952年的调整中，下列哪些大学的土木、建筑、测量各系、科、组被集中于同济大学？（多选）", 1, new[] { "交通大学", "震旦大学", "上海市工业专科学校", "中华工商学校" }, "1,2,3,4", 5);
            AddQ(110, "同济大学的校庆日是每年的哪一天？", 0, new[] { "5月20日", "6月1日", "9月10日", "10月15日" }, "1", 2);
            AddQ(111, "以下关于同济大学的历史沿革，说法正确的有（多选）？", 1, new[] { "最初由德国医生埃里希·宝隆创办", "属于教育部直属高校", "\"同济\"二字来自德语", "是\"985工程\"重点建设高校" }, "1,2,3,4", 2);
            AddQ(112, "抗战时期，同济大学迁校过程中，坚持办学，培养了大批人才，以下哪些学科在当时已具备较强实力（多选）？", 1, new[] { "土木工程", "医学", "测绘工程", "艺术" }, "1,2,3", 2);
            AddQ(113, "同济大学成为国家\"双一流\"建设高校后，重点建设的学科领域包括（多选）？", 1, new[] { "土木工程", "测绘科学与技术", "环境科学与工程", "建筑与城市规划" }, "1,2,3,4", 2);
            AddQ(114, "以下哪些建筑是同济大学历史建筑，至今仍在使用（多选）？", 1, new[] { "一·二九礼堂", "大礼堂", "西南一楼", "大学生活动中心" }, "1,2,3", 2);
            AddQ(115, "同济大学在发展过程中，形成了多个校区，以下哪些是同济大学的正式校区（多选）？", 1, new[] { "四平路校区", "嘉定校区", "闵行校区", "沪西校区" }, "1,2,4", 2);
            AddQ(116, "以下关于同济大学医学学科的说法，正确的有（多选）？", 1, new[] { "源自建校初期的德文医学堂", "曾独立为上海医科大学", "现设有医学院和口腔医学院", "拥有多家附属医院" }, "1,3,4", 2);
            AddQ(117, "同济大学的校歌中，包含以下哪些关键词（多选）？", 1, new[] { "齐心协力", "自强不息", "严谨求实", "振兴中华" }, "1,3", 5);
            AddQ(118, "建国后，同济大学在哪些领域为国家建设做出了重要贡献（多选）？", 1, new[] { "城市规划", "桥梁建设", "航天工程", "水利工程" }, "1,2,3,4", 2);
            AddQ(119, "以下哪些是同济大学的特色文化活动（多选）？", 1, new[] { "社团\"百团大战\"", "\"一·二九\"爱国主题宣讲", "高雅艺术进校园", "新生杯体育比赛" }, "1,2,3,4", 2);
            AddQ(120, "以下关于同济大学与国外高校合作的说法，正确的有（多选）？", 1, new[] { "与德国多所高校有长期合作", "设有中外合作办学项目", "学生可申请交换生项目", "仅与欧洲高校合作" }, "1,2,3", 2);

            // ========== 主题2：同济测绘（201-210）==========
            AddQ(201, "同济大学测绘与地理信息学院成立于哪一年？", 0, new[] { "1956年", "1985年", "2000年", "2012年" }, "4", 2);
            AddQ(202, "测绘专业最常用的基础测量仪器是？", 0, new[] { "全站仪", "显微镜", "示波器", "天平" }, "1", 1);
            AddQ(203, "以下哪种技术属于同济大学测绘学院的优势研究方向？", 0, new[] { "卫星导航定位", "分子生物学", "电动力学", "古典文学" }, "1", 2);
            AddQ(204, "测绘专业学生进行野外实习时，以下哪种物品不需要携带？", 0, new[] { "听诊器", "全站仪", "三脚架", "棱镜" }, "1", 1);
            AddQ(205, "同济大学测绘学院拥有的重要科研平台是？", 0, new[] { "自然资源部现代工程测量重点实验室", "城市污染控制国家工程研究中心", "海洋地质全国重点实验室", "土木工程防灾减灾全国重点实验室" }, "1", 2);
            AddQ(206, "以下关于GIS（地理信息系统）的说法，正确的有（多选）？", 1, new[] { "是测绘专业的核心课程之一", "可用于城市规划", "能实现空间数据可视化", "是用于输入、存储、分析和显示地理数据的计算机系统" }, "1,2,3,4", 2);
            AddQ(207, "3S系统指的是（多选）？", 1, new[] { "全自动绘图系统(ADS)", "遥感(RS)", "地理信息系统(GIS)", "全球导航卫星系统(GNSS/GPS)" }, "2,3,4", 2);
            AddQ(208, "以下哪些是测绘工程专业的核心课程（多选）？", 1, new[] { "测量学", "量子力学", "地理信息系统", "误差理论与测量平差" }, "1,3,4", 2);
            AddQ(209, "同济大学测绘学院在以下哪些领域有突出成果（多选）？", 1, new[] { "卫星导航定位技术", "遥感图像处理", "精密工程测量", "建筑设计" }, "1,2,3", 2);
            AddQ(210, "以下哪些是测量实习的内容（多选）？", 1, new[] { "水准测量", "导线测量", "地形测量", "地形成图" }, "1,2,3,4", 2);

            // ========== 主题3：同济景观（301-330）==========
            AddQ(301, "同济大学的主校门位于？", 0, new[] { "赤峰路50号", "赤峰路200号", "四平路1239号", "国康路99号" }, "3", 1);
            AddQ(302, "衷和楼是同济百年校庆的标志性建筑之一，下列关于衷和楼的说法错误的是？", 0, new[] { "原名综合楼", "中庭等公共空间外包混凝土幕墙", "除会议室、办公室外还设有阶梯教室", "地上有21层，象征21世纪" }, "2", 2);
            AddQ(303, "每年四月初吸引许多师生和公众前来游览的樱花大道位于哪条路上？", 0, new[] { "南大道", "景行路", "东大道", "爱校路" }, "4", 2);
            AddQ(304, "同济大学校史馆位于哪栋楼内？", 0, new[] { "同文楼", "文远楼", "同创楼", "汇文楼" }, "3", 5);
            AddQ(305, "下列有关同济大学四平路校区内的毛主席塑像的说法中，错误的是", 0, new[] { "该塑像于1967年7月1日落成", "毛主席背手呈\"闲庭信步\"的形象", "为上海高校中第一尊毛泽东塑像", "毛主席像 7.1 米高" }, "2", 2);
            AddQ(306, "同济高等讲堂经常在哪栋楼里举行？", 0, new[] { "教学南楼", "一·二九大楼", "行政楼", "逸夫楼" }, "4", 2);
            AddQ(307, "同济大学大礼堂被誉为\"远东第一跨\"，下列说法正确的有（多选）？", 1, new[] { "1962年建成时为亚洲最大的无柱中空大礼堂", "最初的建造需求为饭厅兼礼堂", "2007年完成外立面改造和更新扩建", "采用自然通风和机械通风相结合" }, "1,2,3,4", 2);
            AddQ(308, "同济大学四平路校区内的室内羽毛球场地分布在（多选）？", 1, new[] { "攀岩馆", "一·二九训练馆", "拳操房", "体育馆" }, "1,2,4", 5);
            AddQ(309, "以下哪些建筑是同济大学四平路校区的历史保护建筑？", 1, new[] { "中法中心", "旭日楼", "文远楼", "图书馆" }, "2,3,4", 5);
            AddQ(310, "同济校园内现存最早的建筑是？", 0, new[] { "衷和楼", "经纬楼", "一·二九大楼", "图书馆" }, "3", 2);
            AddQ(311, "下列学院中哪些的本部位于四平路校区（多选）？", 1, new[] { "马克思主义学院", "物理科学与工程学院", "测绘与地理信息学院", "材料科学与工程学院" }, "1,2,3", 5);
            AddQ(312, "以下关于一·二九学生运动纪念园的说法，正确的有（多选）？", 1, new[] { "1987年纪念园落成", "采用青石、汉白玉为基色", "园中石壁上铭刻着同济烈士的英名", "已被确定为杨浦区爱国主义教育基地" }, "1,2,3,4", 2);
            AddQ(313, "以下属于女生宿舍的楼宇有（多选）？", 1, new[] { "学三楼", "西南八楼", "西南九楼", "西南二楼" }, "1,3,4", 2);
            AddQ(314, "以下属于男生宿舍的楼宇有（多选）？", 1, new[] { "西南九楼", "西南八楼", "西北五楼", "西南七楼" }, "2,3,4", 2);
            AddQ(315, "同济大学四平路校区主区内有几个食堂（肯德基和面包房不算）？", 0, new[] { "3", "4", "5", "6" }, "3", 2);
            AddQ(316, "同济大学建筑与城市规划学院拥有悠久的历史和雄厚的学科基础。城规学院在四平路校区有几栋教学楼？", 0, new[] { "3", "4", "5", "6" }, "2", 2);
            AddQ(317, "以下建筑内有自习区域的有？（多选）", 1, new[] { "旭日楼", "瑞安楼", "图书馆", "南北楼" }, "2,3,4", 2);
            AddQ(318, "下列属于环境科学与工程学院的办公教学楼宇是（多选）？", 1, new[] { "致远楼", "生态楼", "宁静楼", "明净楼" }, "2,4", 5);
            AddQ(319, "以下属于同济大学四平路校区内的工程实验室的是？（多选）", 1, new[] { "机械馆", "结构试验馆", "声学馆", "汽车工程实验中心" }, "1,2,3", 2);
            AddQ(320, "四平路校区下列建筑在最近一年内重新装修的有（多选）？", 1, new[] { "西苑饮食广场", "海洋楼", "西南七楼", "运筹楼" }, "1,3", 2);
            AddQ(321, "\"三好坞\"春有流水潺湲，夏有榴花明媚，秋有笛声清越，冬有细雪纷飞。三好坞的两座小石桥的名字是（多选）？", 1, new[] { "秀隐", "隐秀", "枕流", "瀑虹" }, "2,3", 5);
            AddQ(322, "下列哪些区域有供人休憩的草坪（多选）？", 1, new[] { "一·二九运动场", "西南一楼楼前", "南楼西侧", "南北楼之间" }, "2,3,4", 2);
            AddQ(323, "同济大学校医院离哪个校门最接近？", 0, new[] { "国康路99号", "赤峰路200号", "四平路1239号", "赤峰路50号" }, "4", 5);
            AddQ(324, "下列位于西苑饮食广场周围的建筑有（多选）？", 1, new[] { "济阳楼", "西南七楼", "西南九楼", "西南一楼" }, "1,3,4", 2);
            AddQ(325, "下列位于西大道两侧的建筑有（多选）？", 1, new[] { "光学馆", "济阳楼", "解放楼", "实验动物中心" }, "1,2,4", 2);
            AddQ(326, "下列选项中的两个建筑在正射影像上形状相仿的有？", 1, new[] { "西北四楼和西北五楼", "青年楼和解放楼", "西北一楼和西北二楼", "南楼和北楼" }, "1,4", 5);
            AddQ(327, "国康路99号校门附近的建筑有（多选）？", 1, new[] { "学四楼", "西北三楼", "学五楼", "西北二楼" }, "1,3,4", 2);
            AddQ(328, "以下哪种食物无法在北苑饮食广场享用到？", 0, new[] { "烤盘饭", "面包", "烤串", "石锅拌饭" }, "3", 2);
            AddQ(329, "以下哪个食堂提供清真风味餐食？", 0, new[] { "北苑饮食广场", "三好坞饮食广场", "西苑饮食广场", "学苑饮食广场" }, "2", 2);
            AddQ(330, "下面是同济学生小王的一天，哪些说法是正确的？（多选）", 1, new[] { "早上来到爱校路面包房购买早餐", "上午在西北教超二楼购买高数习题册", "下午5点半来到一·二九运动场刷锻", "晚上去大学生活动中心后的开水房打水" }, "1,3", 5);
        }
        private void AddQ(int id, string content, int qType, string[] options, string answerKey, int score)
        {
            var q = new QuizQuestion
            {
                QuestionId = id.ToString(),
                Content = content,
                Mode = qType == 0 ? QuizMode.SingleChoice : QuizMode.MultipleChoice,
                AnswerKey = answerKey,
                ScoreValue = score
            };
            foreach (var opt in options) q.Options.Add(opt);
            AddQuestion(q);
        }

        public ResponseResult<bool> Shutdown()
        {
            return ResponseFactory.Ok(true, "Quiz module closed.");
        }

        public ResponseResult<QuizSession> StartPkQuiz(string playerId, string opponentId)
        {
            if (string.IsNullOrWhiteSpace(playerId) || string.IsNullOrWhiteSpace(opponentId))
            {
                return ResponseFactory.Fail<QuizSession>(ErrorCodes.InvalidArgument, "Player ids are required.");
            }

            var questionIds = GetAllQuestionIds();
            if (questionIds.Count == 0)
            {
                return ResponseFactory.Fail<QuizSession>(ErrorCodes.InvalidState, "Question bank is empty.");
            }

            var session = new QuizSession
            {
                SessionId = "pk_" + playerId + "_" + opponentId + "_" + DateTime.UtcNow.Ticks,
                SessionType = QuizSessionType.PlayerVsPlayer,
                Status = QuizSessionStatus.InProgress,
                InitiatorPlayerId = playerId,
                OpponentPlayerId = opponentId
            };
            foreach (var questionId in questionIds)
            {
                session.QuestionIds.Add(questionId);
            }

            _sessions[session.SessionId] = session;
            return ResponseFactory.Ok(session, "PK session created.");
        }

        public ResponseResult<QuizSession> StartSoloQuiz(string playerId, string pointId)
        {
            if (string.IsNullOrWhiteSpace(playerId))
            {
                return ResponseFactory.Fail<QuizSession>(ErrorCodes.InvalidArgument, "Player id is required.");
            }

            var questionIds = GetAllQuestionIds();
            if (questionIds.Count == 0)
            {
                return ResponseFactory.Fail<QuizSession>(ErrorCodes.InvalidState, "Question bank is empty.");
            }

            var session = new QuizSession
            {
                SessionId = "solo_" + playerId + "_" + pointId + "_" + DateTime.UtcNow.Ticks,
                SessionType = QuizSessionType.Solo,
                Status = QuizSessionStatus.InProgress,
                InitiatorPlayerId = playerId
            };
            foreach (var questionId in questionIds)
            {
                session.QuestionIds.Add(questionId);
            }

            _sessions[session.SessionId] = session;
            return ResponseFactory.Ok(session, "Solo quiz session created.");
        }

        public ResponseResult<QuizSubmissionResult> SubmitAnswer(QuizSubmission submission)
        {
            if (submission == null || string.IsNullOrWhiteSpace(submission.SessionId) || string.IsNullOrWhiteSpace(submission.QuestionId))
            {
                return ResponseFactory.Fail<QuizSubmissionResult>(ErrorCodes.InvalidArgument, "Submission is invalid.");
            }

            QuizSession session;
            if (!_sessions.TryGetValue(submission.SessionId, out session))
            {
                return ResponseFactory.Fail<QuizSubmissionResult>(ErrorCodes.NotFound, "Quiz session not found.");
            }

            if (session.IsCompleted)
            {
                return ResponseFactory.Fail<QuizSubmissionResult>(ErrorCodes.InvalidState, "Quiz session is already completed.");
            }

            if (!session.QuestionIds.Contains(submission.QuestionId))
            {
                return ResponseFactory.Fail<QuizSubmissionResult>(ErrorCodes.InvalidArgument, "Question is not part of the session.");
            }

            if (HasAnswer(session, submission.PlayerId, submission.QuestionId))
            {
                return ResponseFactory.Fail<QuizSubmissionResult>(ErrorCodes.AlreadyExists, "Player already answered this question.");
            }

            QuizQuestion question;
            if (!_questions.TryGetValue(submission.QuestionId, out question))
            {
                return ResponseFactory.Fail<QuizSubmissionResult>(ErrorCodes.NotFound, "Question not found.");
            }

            var isCorrect = string.Equals(question.AnswerKey, submission.Answer, StringComparison.OrdinalIgnoreCase);
            var scoreAwarded = isCorrect ? question.ScoreValue : 0;

            var record = new QuizAnswerRecord
            {
                PlayerId = submission.PlayerId,
                QuestionId = submission.QuestionId,
                SubmittedAnswer = submission.Answer,
                IsCorrect = isCorrect,
                ScoreAwarded = scoreAwarded
            };

            session.Answers.Add(record);

            if (submission.PlayerId == session.InitiatorPlayerId)
            {
                session.InitiatorScore += scoreAwarded;
            }
            else if (submission.PlayerId == session.OpponentPlayerId)
            {
                session.OpponentScore += scoreAwarded;
            }

            session.CurrentQuestionIndex = CalculateCurrentQuestionIndex(session);
            session.IsCompleted = IsSessionCompleted(session);
            if (session.IsCompleted)
            {
                session.Status = QuizSessionStatus.Completed;
                session.CompletedAt = DateTime.UtcNow;
                if (session.SessionType == QuizSessionType.PlayerVsPlayer)
                {
                    if (session.InitiatorScore > session.OpponentScore)
                    {
                        session.WinnerPlayerId = session.InitiatorPlayerId;
                    }
                    else if (session.OpponentScore > session.InitiatorScore)
                    {
                        session.WinnerPlayerId = session.OpponentPlayerId;
                    }
                    else
                    {
                        session.WinnerPlayerId = string.Empty;
                    }
                }
            }

            return ResponseFactory.Ok(new QuizSubmissionResult
            {
                SessionId = session.SessionId,
                QuestionId = submission.QuestionId,
                PlayerId = submission.PlayerId,
                IsCorrect = isCorrect,
                ScoreAwarded = scoreAwarded,
                IsSessionCompleted = session.IsCompleted,
                Session = session
            }, isCorrect ? "Answer is correct." : "Answer is incorrect.");
        }

        private List<string> GetAllQuestionIds()
        {
            var questionIds = new List<string>();
            foreach (var question in _questions.Values)
            {
                questionIds.Add(question.QuestionId);
            }
            return questionIds;
        }

        private static bool HasAnswer(QuizSession session, string playerId, string questionId)
        {
            foreach (var answer in session.Answers)
            {
                if (answer.PlayerId == playerId && answer.QuestionId == questionId)
                {
                    return true;
                }
            }
            return false;
        }

        private static int CalculateCurrentQuestionIndex(QuizSession session)
        {
            var answeredCount = 0;
            foreach (var questionId in session.QuestionIds)
            {
                if (HasAnswer(session, session.InitiatorPlayerId, questionId))
                {
                    answeredCount++;
                }
            }

            return answeredCount;
        }

        private static bool IsSessionCompleted(QuizSession session)
        {
            if (session.SessionType == QuizSessionType.Solo)
            {
                return session.Answers.Count >= session.QuestionIds.Count;
            }

            foreach (var questionId in session.QuestionIds)
            {
                if (!HasAnswer(session, session.InitiatorPlayerId, questionId))
                {
                    return false;
                }
                if (!HasAnswer(session, session.OpponentPlayerId, questionId))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
