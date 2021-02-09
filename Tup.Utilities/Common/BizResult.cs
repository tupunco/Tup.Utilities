namespace Tup.Utilities
{
    /// <summary>
    /// 业务逻辑 返回结果
    /// </summary>
    /// <remarks>
    /// FROM:https://gist.github.com/tupunco/5c69a6c0fc5b27af1b3c
    /// 建议以本类型为返回对象的方法， 不要返回 null 对象.
    ///
    /// 提供三种方法创建本实例:
    /// 直接实例:
    ///     new BizResult<Dish>(...);
    ///
    /// 默认静态实例(使用有局限):
    ///     BizResult.SucceedResult/FailResult;
    ///
    /// 静态创建方法:
    ///     BizResult.Create(...)
    ///     BizResult.Create<TSome>(...)
    ///
    /// </remarks>
    public class BizResult
    {
        /// <summary>
        /// Message 成功
        /// </summary>
        protected static readonly string Default_Succeed_Message = "成功";

        /// <summary>
        /// Message 失败
        /// </summary>
        protected static readonly string Default_Fail_Message = "失败";

        /// <summary>
        /// 默认 0:成功结果
        /// </summary>
        public static readonly BizResult SucceedResult = new BizResult(true, 0, Default_Succeed_Message/*成功*/);

        /// <summary>
        /// 默认 -1:失败结果
        /// </summary>
        public static readonly BizResult FailResult = new BizResult(false, -1, Default_Fail_Message/*失败*/);

        /// <summary>
        /// 成功与否
        /// </summary>
        public bool Succeed { get; private set; }

        /// <summary>
        /// 返回结果码(0/1:成功, -1:失败)
        /// </summary>
        public int ResultCode { get; private set; }

        /// <summary>
        /// 返回结果消息
        /// </summary>
        public string ResultMessage { get; private set; }

        /// <summary>
        ///
        /// </summary>
        public BizResult()
            : this(false, -1, Default_Fail_Message/*失败*/)
        {
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="succeed">成功与否</param>
        public BizResult(bool succeed)
            : this(succeed, succeed ? 0 : -1, succeed ? Default_Succeed_Message/*成功*/ : Default_Fail_Message/*失败*/)
        {
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="succeed">成功与否</param>
        /// <param name="resultCode">返回结果码(0/1:成功, -1:失败)</param>
        /// <param name="resultMessage">返回结果消息</param>
        public BizResult(bool succeed, int resultCode, string resultMessage)
        {
            this.Succeed = succeed;
            this.ResultCode = resultCode;
            this.ResultMessage = resultMessage;
        }

        #region Create

        /// <summary>
        /// 创建 返回结果
        /// </summary>
        /// <param name="succeed">成功与否</param>
        /// <param name="resultCode">返回结果码(0/1:成功, -1:失败)</param>
        /// <param name="resultMessage">返回结果消息</param>
        /// <returns></returns>
        public static BizResult Create(bool succeed, int resultCode, string resultMessage)
        {
            return new BizResult(succeed, resultCode, resultMessage);
        }

        /// <summary>
        /// 创建 带 "泛型结果数据对象" 的返回结果
        /// </summary>
        /// <typeparam name="TResult">结果数据对象 类型</typeparam>
        /// <param name="succeed">成功与否</param>
        /// <param name="resultCode">返回结果码(0/1:成功,1:失败)</param>
        /// <param name="resultMessage">返回结果消息</param>
        /// <returns></returns>
        public static BizResult<TResult> Create<TResult>(bool succeed, int resultCode, string resultMessage)
        {
            return new BizResult<TResult>(succeed, resultCode, resultMessage);
        }

        /// <summary>
        /// 创建 带 "泛型结果数据对象" 的返回结果
        /// </summary>
        /// <typeparam name="TResult">结果数据对象 类型</typeparam>
        /// <param name="succeed">成功与否</param>
        /// <param name="resultObject">结果数据对象</param>
        /// <returns></returns>
        public static BizResult<TResult> Create<TResult>(bool succeed, TResult resultObject)
        {
            return new BizResult<TResult>(succeed, resultObject);
        }

        /// <summary>
        /// 创建 带 "泛型结果数据对象" 的返回结果
        /// </summary>
        /// <typeparam name="TResult">结果数据对象 类型</typeparam>
        /// <param name="succeed">成功与否</param>
        /// <returns></returns>
        public static BizResult<TResult> Create<TResult>(bool succeed)
        {
            return new BizResult<TResult>(succeed, default(TResult));
        }

        /// <summary>
        /// 创建 带 "泛型结果数据对象" 的返回结果
        /// </summary>
        /// <typeparam name="TResult">结果数据对象 类型</typeparam>
        /// <param name="succeed">成功与否</param>
        /// <param name="resultCode">返回结果码(0/1:成功,1:失败)</param>
        /// <param name="resultMessage">返回结果消息</param>
        /// <param name="resultObject">结果数据对象</param>
        /// <returns></returns>
        public static BizResult<TResult> Create<TResult>(bool succeed, int resultCode, string resultMessage, TResult resultObject)
        {
            return new BizResult<TResult>(succeed, resultCode, resultMessage, resultObject);
        }

        #endregion

        /// <summary>
        /// ToString
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("[BizResult Succeed:{0}, ResultCode:{1}, ResultMessage:{2}]",
                                        this.Succeed, this.ResultCode, this.ResultMessage);
        }
    }

    /// <summary>
    /// 业务逻辑 带 "泛型结果数据对象" 的返回结果
    /// </summary>
    /// <typeparam name="TResult">结果数据对象 类型</typeparam>
    public class BizResult<TResult> : BizResult
    {
        /// <summary>
        /// 结果数据对象
        /// </summary>
        public TResult ResultObject { get; set; }

        /// <summary>
        ///
        /// </summary>
        public BizResult() { }

        /// <summary>
        ///
        /// </summary>
        /// <param name="succeed">成功与否</param>
        /// <param name="resultCode">返回结果码(0:成功,1:失败)</param>
        /// <param name="resultMessage">返回结果消息</param>
        public BizResult(bool succeed, int resultCode, string resultMessage)
            : base(succeed, resultCode, resultMessage)
        {
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="succeed">成功与否</param>
        /// <param name="resultObject">结果数据对象</param>
        public BizResult(bool succeed, TResult resultObject)
            : this(succeed, succeed ? 0 : -1, succeed ? Default_Succeed_Message/*成功*/ : Default_Fail_Message/*失败*/, resultObject)
        { }

        /// <summary>
        ///
        /// </summary>
        /// <param name="succeed">成功与否</param>
        /// <param name="resultCode">返回结果码(0/1:成功,1:失败)</param>
        /// <param name="resultMessage">返回结果消息</param>
        /// <param name="resultObject">结果数据对象</param>
        public BizResult(bool succeed, int resultCode, string resultMessage, TResult resultObject)
            : base(succeed, resultCode, resultMessage)
        {
            this.ResultObject = resultObject;
        }

        /// <summary>
        /// ToString
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("[BizResult<{0}> Succeed:{1}, ResultCode:{2}, ResultMessage:{3}, ResultObject:{4}]",
                                     typeof(TResult), this.Succeed, this.ResultCode, this.ResultMessage, this.ResultObject);
        }
    }
}