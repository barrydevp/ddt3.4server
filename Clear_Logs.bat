@echo off

DEL /S /Q /F "%CD%\Center.Service\bin\Debug\logs\*.*"
DEL /S /Q /F "%CD%\Fighting.Service\bin\Debug\logs\*.*"
DEL /S /Q /F "%CD%\Road.Service\bin\Debug\logs\*.*"
DEL /S /Q /F "%CD%\Request\logs\*.*"