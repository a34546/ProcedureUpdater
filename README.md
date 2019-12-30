# ProcedureUpdater
通过程序更新存储过程，第一次执行会在数据库中创建procedurerecord表，用于存放每次执行的存储过程记录。
目前支持Mysql


## 详细介绍

**ProcScripts文件夹**
- 该文件用于存放存储过程，所有脚本文件需要更改属性，复制到输出目录为:始终复制
- 存储过程文件名称分为两部分，由“_”分割，

    例如：TestProc_1.sql文件
    
    第一部分为名称：TestProc
    
    第二部分为版本号：1
    
- 新增存储过程    
    
    在ProcScripts文件夹下新增存储过程脚本，文件名按上面规则命名

    例如：TestProc_1.sql文件

- 更新存储过程    
    
    1.在ProcScripts文件夹下找到需要更新存储过程脚本

    2.修改存储过程脚本
    
    3.修改文件名中的版本号，比原来的版本号大即可

    例如：原文件TestProc_1.sql，修改后为TestProc_2.sql

