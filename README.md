# C# SDK 使用指南

## 功能说明

WcsLib 是较为原始的封装，没有引入 JSON 库，所有正确操作返回的结果都是 JSON 字符串，您需要自己选择一个 JSON 库，并按照文档去正确解读。您可能觉得不够便利，但这样做也有好处：您可以自行选择 JSON 库。如果我们内部也使用，则存在和您选择的不同的可能性。

我们会提供更多基于 WcsLib 的范例代码，做更高层封装，来解决便利性问题。

## 版本说明

### 1.0.1.0

首次发布，还不支持 UWP，主要是 HttpManager 类还不支持。

## 初始化

在使用 SDK 之前，您需要获得一对有效的 AccessKey 和 SecretKey 签名授权。

可以通过如下方法获得：

1. 开通网宿云存储账号
2. 登录网宿 SI 平台，在安全管理-秘钥管理查看 AccessKey 和 SecretKey
3. 登录网宿 SI 平台，在安全管理-域名管理查看上传域名（UploadHost）和管理域名(ManageHost)。

获取上面配置之后，调用如下代码进行初始化：

```
Mac mac = new Mac("<AccessKey>", "<SecretKey>");
//Config config = new Config("<UploadHost>", "<ManageHost>", false);	// use HTTP
Config config = new Config("<UploadHost>", "<ManageHost>", true);	// use HTTPS
```
