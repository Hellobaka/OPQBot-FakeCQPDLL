# OPQBot-FakeCQPDLL
用于 [OPQBot-Native](https://github.com/Hellobaka/OPQBot-Native) 的伪造CQP.dll，使用了[Native.FrameWork](https://github.com/Jie2GG/Native.Framework)的部分CQ码处理函数
## 接口实现情况
- [x] CQ_sendPrivateMsg(只做了文字、at和图片的CQ码的解析)
- [x] CQ_sendGroupMsg(只做了文字、at和图片的CQ码的解析)
- [ ] CQ_sendDiscussMsg(讨论组已经不存在了)
- [x] CQ_deleteMsg
- [x] CQ_sendLikeV2
- [x] CQ_getCookiesV2
- [ ] CQ_getRecordV2(待实现)
- [ ] CQ_getCsrfToken(未找到此接口)
- [x] CQ_getAppDirectory
- [x] CQ_getLoginQQ
- [x] CQ_getLoginNick
- [ ] CQ_setGroupKick(待实现)
- [x] CQ_setGroupBan
- [ ] CQ_setGroupAdmin(未找到此接口)
- [ ] CQ_setGroupSpecialTitle(未找到此接口)
- [x] CQ_setGroupWholeBan
- [ ] CQ_setGroupAnonymousBan(未找到此接口)
- [ ] CQ_setGroupAnonymous(未找到此接口)
- [x] CQ_setGroupCard
- [ ] CQ_setGroupLeave(待实现)
- [ ] CQ_setDiscussLeave(讨论组已经不存在了)
- [ ] CQ_setFriendAddRequest(待实现)
- [ ] CQ_setGroupAddRequestV2(待实现)
- [x] CQ_addLog
- [ ] CQ_setFatal(待实现)
- [x] CQ_getGroupMemberInfoV2
- [x] CQ_getGroupMemberList
- [x] CQ_getGroupList
- [ ] CQ_getStrangerInfo(未找到此接口)
- [x] CQ_canSendImage
- [x] CQ_canSendRecord
- [x] CQ_getImage
- [x] CQ_getGroupInfo
- [x] CQ_getFriendList
## 实现原理
重写各个函数，使用[DllExport](https://github.com/3F/DllExport)用stdcall形式导出函数，使用[Fody](https://github.com/Fody/Costura)打包为单个文件
