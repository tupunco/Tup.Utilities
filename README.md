# 搜集的一组 C# 工具助手类 (C# tools a set of helper classes)

## .NET 版本

- .NET Framework 4.6.2

## 目前包括助手类 (Currently includes):

- RequestHelper: [HttpRequest](<http://msdn.microsoft.com/en-us/library/system.net.httpwebrequest(v=vs.80).aspx>)助手(HttpWebRequest helper)

  - 支持 GET/POST 下载 HTML 或者 Stream 数据

- AsyncHelper `异步处理` 工具类
- ConfigHelper `Config 配置操作` 工具类
- CollectionHelper `集合处理` 工具类
- DateTimeHelper `时间操作` 工具类
- BatchHelper `批量执行动作` 工具类
- FieldHelper `DataTable/IDataReader 数据字段` 工具类
- RetryHelper `重试` 工具类
- JsonHelper `Newtonsoft.Json JSON 操作封装` 工具类
- StringHelper `字符串操作` 工具类
- ThrowHelper `异常处理` 工具类
- ProcessHelper `进程处理` 工具类
- Reflection/ActivatorHelper `类型 Activator 实例化` 工具类
- Reflection/PropertyHelper `快速属性反射` 工具类
- Reflection/ReflectionHelper `反射` 工具类
- Security/CryptHelper `加密 (3DES/MD5/HMAC/SHA256/SecurityStamp)` 工具类
- Security/Base32/Base64 `Base32/Base64` 编码封装

  - https://github.com/aspnet/AspNetCore/blob/master/src/Identity/src/Core/Base32.cs
  - https://raw.githubusercontent.com/aspnet/AspNetCore/master/src/Identity/src/Core/Base32.cs
  - https://github.com/aspnet/AspNetKatana/tree/master/src/Microsoft.Owin.Security/DataHandler/Encoder

- Security/Totp/\* `TOTP (Time-based One-Time Password)` 编码封装

  - https://github.com/aspnet/AspNetIdentity/blob/master/src/Microsoft.AspNet.Identity.Core/TotpSecurityStampBasedTokenProvider.cs
  - https://github.com/aspnet/Identity/blob/master/src/Core/AuthenticatorTokenProvider.cs
  - https://github.com/google/google-authenticator/wiki/Key-Uri-Format
  - https://github.com/aspnet/Identity/blob/master/src/UI/Areas/Identity/Pages/V4/Account/Manage/EnableAuthenticator.cshtml.cs#L92

- Security/Fnv/\* `HNV Hash 算法` 封装

  - https://gist.github.com/RobThree/25d764ea6d4849fdd0c79d15cda27d61

- Common/BizResult `业务逻辑 返回结果`
- Common/SerializableDictionary `支持 XML 序列化的泛型 Dictionary 类`
- Common/ConcurrentConsumerQueue/ConcurrentConsumerBufferQueue `并行多生产者单消费者队列`
- Wcf/WcfServiceProxyFactory
- Wcf/WcfServiceHost
- Wcf/GlobalExceptionHandler
- Msmq/MsmqUtils
- Msmq/MsmqBindUtils
- Msmq/MsmqServiceProxyFactory
- MimeTypeMap `MIME 表 操作处理`
- DbWrapper/\* `System.Data.Common.DBXXXX 包装` _不稳定_
- Dapper/\* `Dapper` 操作扩展类
  - https://github.com/tangxuehua/ecommon/blob/master/src/ECommon/ThirdParty/Dapper/SqlMapperExtensions.cs
  - https://github.com/StackExchange/Dapper/blob/master/Dapper.Contrib/SqlMapperExtensions.cs
  - https://github.com/phnx47/MicroOrm.Dapper.Repositories/blob/master/src/MicroOrm.Dapper.Repositories/SqlGenerator/SqlGenerator.cs
- Data/BaseDao `Dapper.BaseDao` DAO 抽象类
- XmlSerializeHelper: .NET 序列化反序列化助手(.NET XML Serialization Deserialization helper);
- LogHelper: [Log4Net](http://logging.apache.org/log4net/index.html) 日志助手(Log4Net log helper)
- WebProxyHelper: HTTP 代理操作助手(HTTP proxy operations helper)
  - 自动下载代理 IP
  - 代理 IP 健康检查

## 其他 (Other):

- 第三方组件 `Log4Net/Dapper/Newtonsoft.Json`
- 本类库使用到 [DLR](http://dlr.codeplex.com/license) 部分类.
