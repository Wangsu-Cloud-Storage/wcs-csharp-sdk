# C# SDK User Guide

## Language
- [简体中文](README.md)
- [English](README.en.md)

## Feature Description

WcsLib is a relatively raw encapsulation. It does not introduce a JSON library. All results returned by correct operations are JSON strings. You need to choose a JSON library yourself and interpret it correctly according to the documentation. You may find it inconvenient, but there are also benefits to doing this: you can choose the JSON library yourself. If we also use it internally, there is a possibility that it is different from the one you choose.

We will provide more sample code based on WcsLib to do higher-level encapsulation to solve the convenience problem.

## Version Description

### 1.0.1.0

First release, UWP is not supported yet, mainly because the HttpManager class is not supported yet.

## Initialization

Before using the SDK, you need to obtain a pair of valid AccessKey and SecretKey signature authorizations.

You can obtain it through the following methods:

1. Activate Wangsu Cloud Storage account
2. Log in to Wangsu SI platform, view AccessKey and SecretKey in Security Management - Key Management
3. Log in to Wangsu SI platform, view upload domain name (UploadHost) and management domain name (ManageHost) in Security Management - Domain Management.

After obtaining the above configuration, call the following code for initialization:

```
Mac mac = new Mac("<AccessKey>", "<SecretKey>");
//Config config = new Config("<UploadHost>", "<ManageHost>", false); // use HTTP
Config config = new Config("<UploadHost>", "<ManageHost>", true); // use HTTPS
```

## Usage Instructions
Please refer to the demo in the test directory