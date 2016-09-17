# acellentWeb
An OWIN based windows service created by Acellent IT Studio (優捷科技工作室 Tom Liao)

Welcome to use acellentWeb Windows Service program!

This OWIN Self-hosted Web windows service program is developed in C# 6.0. If you have the need for applying your web app on a simple web server that is not complicate as IIS, and hoping it could be pack with your the simple web server with your web app in a single .msi installation file, then this is the tool for you. 

This windows service program also supports .Net WebApi functionality. As long as you make the right declaration of your webapi dll seting in the webconfig.json file, then this windows service should be execute your webapi.


Tom Liao

Acellent IT Studio

Taipei, Taiwan. 2015/9/3


Documentation

Requirements:

   Microsoft .Net framework 4.5.2 or above.
   
   Microsoft.AspNet.WebApi.Client version=5.2.3
   
   Microsoft.AspNet.WebApi.Core version=5.2.3
   
   Microsoft.AspNet.WebApi.Owin version=5.2.3
   
   Microsoft.Owin version=3.0.1
   
   Microsoft.Owin.Diagnostics version=3.0.1
   
   Microsoft.Owin.FileSystems version=3.0.1
   
   Microsoft.Owin.Host.HttpListener version=3.0.1
   
   Microsoft.Owin.Hosting version=3.0.1
   
   Microsoft.Owin.StaticFiles version=3.0.1
   
   Newtonsoft.Json version=8.0.3
   
   Owin version=1.0
   

Installation:

1. Default installation folder：C:\Program Files\acellent

2. No matter you install this program under the default folder or not, the installation folder contains the following sub-folders:

   2.1 Root director：
   
     The main program(acellentWeb.exe).
     
     All dll files that are necessary for running this program.
     
   2.2 Web folders：Your web app should be placed in the following folders. By default, there are stored under the default installation folder, but you can change that by editing the webconfig.json file.
   
     webroot: The root directory of your web app (required).
     
     webroot\css: The folder for storing css files(optional)
     
     webroot\files: The folder for storing read only files without folder listing permission. Meaning, users must know exactly the path and file name of the file they are looking for. (optional)
     
     webroot\js: The folder for storing JavaScript files(optional).
     
     webroot\public: The folder that provides file listing functionality.(optional)
     

Settings of webconfig.json file:

1. There are four main setions in the webconfig.json file:

   UrlSetting: 
   
     -- Domain: Default is +, which allows all IP addresses, including localhost, 127.0.0.1, 192.160.x.x and any other IP that tight to this computer。
     
     -- Port: Default is port 80。
     
   FolderSetting: Three types of folder are supported.
   
     -- root:(Required) This is the root directory of your web app, it is required. By default it is the webroot directory under your installation folder.
     
     -- allow_list:(Optional) The public folder in the webroot directory.
     
     -- file_only:(Optional) The files folder in the webroot directory. Difference between files and public is that public allows file listing while files not! 
     
   WebApiSetting: Please refer to the webconfig.json file for edit the WebApi settings.
   
   AngularJS: This function is designed to execute AngularJS 1.x(Angular 2 has not been tested yet). If you need your Angular SPA program to be SEO friendly, please make sure the html5Mode setting is set to Y.
   
   
2. Default settings in webconfig.json: 

   https://github.com/acellent-studio/acellentWeb/blob/master/acellentWeb/webconfig.json 
   
   
