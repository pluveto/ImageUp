# ImageUp

一个 Windows 下的图片快捷上传工具。

## 特性

[x] 小体积、低占用。（再也不用为了上传个图片就跑起整个浏览器内核）
[x] 支持命令行调用，兼容 Typora。
[ ] 界面美观，采用和 OS 一致的 WinUI 风格。

### 使用方法

**目前暂未实现通过 GUI 配置。功能开发中。**

`app_config.yaml` 配置文件（或 `json` 格式，结构和命名一致即可）：

```yaml
chainUploaders:
  # 上传使用的插件，可以是多个，这样会全部上传，以作备份
  - GithubUploader

  # - SmmsUploader

# 命名模板（并非所有图床都支持）
namingTemplate: "{year}/{month}/{fileNameNoExt}_{ts}{ext}"

# 是否启用代理上传
enableProxy: true

# 代理服务器地址
proxy: 'http://127.0.0.1:7890'
```

在 `plugins` 目录下对插件进行配置。配置文件名为 `插件主类名.配置文件扩展名`，例如 `GithubUploader.yaml`:

```yaml
# Github 的访问令牌
accessToken: '*******************'

# 仓库
repository: pluveto/0images

# 分支
branch: master

# 路径前缀，必须以 / 结尾
pathPrefix: /

# 是否启用 CDN，启用后产生的 URL 会改用此前缀
useCDN: false

# CDN 前缀
CDN: 'https://cdn.jsdelivr.net/gh/pluveto/0images@master/'

# 超时时间 (ms)
Timeout: 10000
```

### 接入 Typora 使用

首先请正确填写配置文件。然后：

在 Typora 的 *偏好设置/上传服务设定* 命令处填写 `"程序 exe 路径"` 即可。

![image-20210909154242166](https://i.loli.net/2021/09/09/jo1kexdGpJucX3t.png)

## 模块说明


### ImageUpWpf.Core
提供核心功能

### ImageUpWpf.Uploader
提供基础的上传插件

### ImageUpWpf
提供用户界面封装
<<<<<<< HEAD

## 插件开发

请遵照 Core 中的几个接口进行开发。下面是一个例子。

建立 .NET Core 类库项目。一个项目中可以实现多个插件。

你的插件类需要实现 IPlugin, IUploader 接口。

### 插件信息

你的插件需要带有一个 `PluginInfo` 属性：

```cs
        public PluginInfo PluginInfo { get; set; } = new PluginInfo()
        {
            Name = "SM.MS Uploader",
            Type = PluginType.Uploader,
            Icon = Resources.SmmsLogo,
            Author = "Pluveto",
            Version = "0.0.1",
        };
```

| 成员    | 说明                                                       |
|---------|------------------------------------------------------------|
| Name    | 插件的展示名称                                             |
| Type    | 插件的类型                                                 |
| Icon    | 插件图标的 Base64 值，大小为 16x16                         |
| Author  | 作者姓名                                                   |
| Repo    | Git 仓库地址，包含协议头                                   |
| Version | 版本，必须和仓库 Release 的 Tag 名称一致，以便以后进行更新 |

### 配置文件

如果插件需要配置使用，则请遵照下面的代码：

```cs
        // 此处必须返回一个插件具体的实例，以便 PluginManager 自动根据类的特征进行反序列化，将用户的配置加载到你的插件里。
        private SmmsConfig config = new SmmsConfig();
        IPluginConfig IPlugin.Config { get => config; set => config = (SmmsConfig)value; }
```

如果插件需要一些高级功能，则可以定义 `public IuAppContext Context { get; set; }` 属性，这样 PluginManager 会向你的插件注入一个 IuAppContext 实例。通过这个上下文实例可以操纵应用，实现更复杂的功能。同时，此实例提供帮助函数，比如 `Context.Helper.GetProxyHandler()` 可以获取一个 ProxyHandler给你的插件中的 HttpClient 使用。

### 日志

你可以通过如下方式获取日志记录器：

```c
private Logger logger = NLog.LogManager.GetCurrentClassLogger();
```

### 上传核心代码

你需要实现 `Upload` 方法，从而实现上传功能。如果成功，请返回字符串，失败，则请抛出 `Core.Upload.UploadException` 异常，或者返回 `null`.

### 并行上传

默认采用的是并行上传。

> 考虑到有的接口限制并发、访问频率等特殊情况，你可以仿照 GithubUploader 的代码，通过信号量使之同步上传。

定义简单信号量：
```cs
private readonly static SemaphoreSlim mutex = new SemaphoreSlim(1);
```

等待进入临界区的权限：

```
mutex.Wait();
```

归还权限：

```
mutex.Release();
```

**如果你使用信号量等机制同步，必须妥善处理锁的释放，以免锁死程序！**

**请勿使用 Mutex，Lock 语句等进行加锁，它们与 async/await 不相容。**

### 参考代码

```cs
    public class SmmsUploader : IPlugin, IUploader
    {
        private SmmsConfig config = new SmmsConfig();
        IPluginConfig IPlugin.Config { get => config; set => config = (SmmsConfig)value; }

        private Logger logger = NLog.LogManager.GetCurrentClassLogger();
        public IuAppContext Context { get; set; }

        public PluginInfo PluginInfo { get; set; } = new PluginInfo()
        {
            Name = "SM.MS Uploader",
            Type = PluginType.Uploader,
            Icon = Resources.SmmsLogo,
            Author = "Pluveto",
            Version = "0.0.1",
        };

        public async Task<string> Upload(Stream sr, string name)
        {
            var fileName = Path.GetFileName(name);
            checkConfig();
            HttpResponseMessage resp;
            using (var client = new HttpClient(handler: Context.Helper.GetProxyHandler()))
            using (sr)
            {
                client.Timeout = TimeSpan.FromMilliseconds(this.config.Timeout);
                try
                {
                    logger.Info("Start uploading " + fileName);
                    // request 自己创建
                    resp = await client.SendAsync(request);
                    logger.Info("Finish uploading " + fileName);
                }
                catch (HttpRequestException e)
                {
                    logger.Error(e.Message);
                    throw e;
                }
                catch (Exception e)
                {
                    throw e;
                }
                finally
                {
                }
                var code = (int)resp.StatusCode;
                logger.Info($"Status Code: {resp.StatusCode}({code})");
                var respStr = await resp.Content.ReadAsStringAsync();
                logger.Info("Response Body: " + respStr);
                if (300 > code && code >= 200)
                {
                    var ret = JsonConvert.DeserializeObject<SmmsRespModel.SmmsResp>(respStr);
                    if (ret.Success)
                    {
                        return ret.Data.Url.AbsoluteUri;
                    }
                    else
                    {
                        throw new Core.Upload.UploadException(ret.Message);
                    }
                }
                else
                {
                    throw new Core.Upload.UploadException(respStr);
                }
            }
            return null;
        }
    }

    public class SmmsConfig : IPluginConfig
    {
        internal string BaseUrl { get; set; } = "https://sm.ms/api/v2";
        public string Token { get; set; }
        public double Timeout { get; set; } = 60*1000;

        public IDictionary<string, ConfigItemMeta> ConfigFormMeta => new Dictionary<string, ConfigItemMeta>() {
            {"Token",       new ConfigItemMeta{ Type = ConfigItemType.String,      DisplayName="Token" , Description = "Get it from https://sm.ms/home/"} },
            {"Timeout",     new ConfigItemMeta{ Type = ConfigItemType.Integer,     DisplayName="Timeout in ms" , DefaultValue = "10000" } },
        };

    }
}
```