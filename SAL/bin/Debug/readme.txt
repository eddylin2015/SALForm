發布需知Deloyment:
/////////////////////////
//安裝執行環境
//
////////////////////////

(一).安裝DotNetFramework2.0 和 Office.Net程式設計支援工具

		dotnetfx2.exe     Dot Net Framework 2.0

		OfficeVSTOWindowsInstaller.exe

(二).Windows.ReportView 組件
		
		ReportViewer.exe 

(三).開啟office2003 .net程式設計支援　選項.
	控制台>新增移除程式>
	Office2003 變更
	選擇應用程式的進階自訂  Excel .NET 程式設計支援
					OFFICE工具DOT NET FRAME2.0 程式設計支援

(四).DotNetFramework2.0代碼區域安全性設定
	C:\windows\Microsoft.Net\Framework\v2.0.50727\
					caspol -machine -chggroup LocalIntranet_Zone FullTrust

參考資科Reference:
1.caspol uassage: DotNetFramework2.0代碼區域安全性設定
http://cht.gotdotnet.com/quickstart/howto/doc/security/secscripting.aspx


