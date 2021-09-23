# ICLibrary
RL8000 ICLibrary

已实现HTTP WEB API调用

已打包单文件：ICLibrary.exe

可执行文件路径：ICLibrary/bin/ICLibrary.exe

默认测试页面：http://localhost:33448

调用读卡API：http://localhost:33448/tryread

调用提示音API：http://localhost:33448/beep

# 简单调用

```csharp
ICLibrary.ICCard.ICHelper.Depends().ListenA();

// 测试访问：http://localhost:33448

// or 调用api自己编写逻辑

ICLibrary.ICCard.ICHelper.Depends();//释放必要资源
ICLibrary.ICCard.ICHelper.LoadDriver();//加载驱动
ICLibrary.ICCard.ICHelper.OpenDev();//打开设备
ICLibrary.ICCard.ICHelper.TryRead();//读取卡号
ICLibrary.ICCard.ICHelper.Beep(); //提示音
ICLibrary.ICCard.ICHelper.Close(); //关闭读卡器，关闭句柄
```

# Dependencies

+ Ceen.Httpd.0.9.9

+ DotNetZip.1.15.0

+ Zepto v1.2.0 [non-essential] html演示用
