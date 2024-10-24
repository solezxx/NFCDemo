
// The following ifdef block is the standard way of creating macros which make exporting 
// from a DLL simpler. All files within this DLL are compiled with the CS_EXPORTS
// symbol defined on the command line. this symbol should not be defined on any project
// that uses this DLL. This way any other project whose source files include this file see 
// CS_API functions as being imported from a DLL, wheras this DLL sees symbols
// defined with this macro as being exported.
#ifdef ALL_EXPORTS
#define ALL_API __declspec(dllexport)
#else
#define ALL_API __declspec(dllimport)
#endif

// This class is exported from the cs.dll
//class CS_API CCs {
//public:
//	CCs(void);
	// TODO: add your methods here.
//};

//extern CS_API int nCs;

//CS_API int fnCs(void);
//***************Desfire card command*******************************
extern  "C"   int ALL_API  __stdcall API_PiccActivateIdle(HANDLE commHandle,int DeviceAddress,unsigned char *buffer);
extern  "C"   int ALL_API  __stdcall API_PiccGetAts(HANDLE commHandle,int DeviceAddress,unsigned char *buffer);
extern  "C"   int ALL_API  __stdcall API_DESfireAuthenticate(HANDLE commHandle,int DeviceAddress,unsigned char KeyNum,unsigned char *buffer);
extern  "C"   int ALL_API  __stdcall API_DESfireChangeKeySettings(HANDLE commHandle,int DeviceAddress,unsigned char KeyAttribute,unsigned char *buffer);
extern  "C"   int ALL_API  __stdcall API_DESfireGetKeySettings(HANDLE commHandle,int DeviceAddress,unsigned char *buffer);
extern  "C"   int ALL_API  __stdcall API_DESfireChangeKey(HANDLE commHandle,int DeviceAddress,unsigned char KeyNum,unsigned char *buffer);
extern  "C"   int ALL_API  __stdcall API_DESfireGetKeyVersion(HANDLE commHandle,int DeviceAddress,unsigned char KeyNum,unsigned char *buffer);

extern  "C"   int ALL_API  __stdcall API_DESfireSelectApplication(HANDLE commHandle,int DeviceAddress,unsigned char *buffer);
extern  "C"   int ALL_API  __stdcall API_DESfireFormatCard(HANDLE commHandle,int DeviceAddress,unsigned char *buffer);
extern  "C"   int ALL_API  __stdcall API_DESfireCreateApplication(HANDLE commHandle,int DeviceAddress,unsigned char KeyAttribute,unsigned char MaxKeyNum,unsigned char *buffer);
extern  "C"   int ALL_API  __stdcall API_DESfireDeleteApplication(HANDLE commHandle,int DeviceAddress,unsigned char *buffer);
extern  "C"   int ALL_API  __stdcall API_DESfireGetApplicationsID(HANDLE commHandle,int DeviceAddress,unsigned char *buffer);
extern  "C"   int ALL_API  __stdcall API_DESfireGetVersions(HANDLE commHandle,int DeviceAddress,unsigned char *buffer);

extern  "C"   int ALL_API  __stdcall API_DESfireGetFileIDs(HANDLE commHandle,int DeviceAddress,unsigned char *buffer);
extern  "C"   int ALL_API  __stdcall API_DESfireGetFileSettings(HANDLE commHandle,int DeviceAddress,unsigned char fileNO,unsigned char *buffer);
extern  "C"   int ALL_API  __stdcall API_DESfireChangeFileSettings(HANDLE commHandle,int DeviceAddress,unsigned char fileNO,unsigned char mode,unsigned char *buffer);
extern  "C"   int ALL_API  __stdcall API_DESfireCreateStdDataFile(HANDLE commHandle,int DeviceAddress,unsigned char fileNO,unsigned char mode,unsigned char * AccessRights,unsigned char *buffer);
extern  "C"   int ALL_API  __stdcall API_DESfireCreateBackupDataFile(HANDLE commHandle,int DeviceAddress,unsigned char fileNO,unsigned char mode,unsigned char * AccessRights,unsigned char *buffer);
extern  "C"   int ALL_API  __stdcall API_DESfireCreateValueFile(HANDLE commHandle,int DeviceAddress,unsigned char fileNO,unsigned char mode,unsigned char * AccessRights,unsigned char * LowerLimit,unsigned char * UpperLimit,unsigned char * Value,unsigned char LimitCreditEnable,unsigned char *buffer);
extern  "C"   int ALL_API  __stdcall API_DESfireCreateLinearRecordFile(HANDLE commHandle,int DeviceAddress,unsigned char fileNO,unsigned char mode,unsigned char * AccessRights,unsigned char * RecordSize,unsigned char * MaxRecordNum,unsigned char *buffer);
extern  "C"   int ALL_API  __stdcall API_DESfireCreateCyclicRecordFile(HANDLE commHandle,int DeviceAddress,unsigned char fileNO,unsigned char mode,unsigned char * AccessRights,unsigned char * RecordSize,unsigned char * MaxRecordNum,unsigned char *buffer);
extern  "C"   int ALL_API  __stdcall API_DESfireDeleteFile(HANDLE commHandle,int DeviceAddress,unsigned char fileNO,unsigned char *buffer);


extern  "C"   int ALL_API  __stdcall API_DESfireReadData(HANDLE commHandle,int DeviceAddress,unsigned char fileNO,unsigned char mode,unsigned char * Offset,unsigned char * Length,unsigned char *buffer);
extern  "C"   int ALL_API  __stdcall API_DESfireWriteData(HANDLE commHandle,int DeviceAddress,unsigned char fileNO,unsigned char mode,unsigned char * Offset,unsigned char * Length,unsigned char *buffer);
extern  "C"   int ALL_API  __stdcall API_DESfireGetValue(HANDLE commHandle,int DeviceAddress,unsigned char fileNO,unsigned char mode,unsigned char *buffer);
extern  "C"   int ALL_API  __stdcall API_DESfireCredit(HANDLE commHandle,int DeviceAddress,unsigned char fileNO,unsigned char mode,unsigned char *buffer);
extern  "C"   int ALL_API  __stdcall API_DESfireDebit(HANDLE commHandle,int DeviceAddress,unsigned char fileNO,unsigned char mode,unsigned char *buffer);
extern  "C"   int ALL_API  __stdcall API_DESfireLimitedCredit(HANDLE commHandle,int DeviceAddress,unsigned char fileNO,unsigned char mode,unsigned char *buffer);
extern  "C"   int ALL_API  __stdcall API_DESfireWriteRecord(HANDLE commHandle,int DeviceAddress,unsigned char fileNO,unsigned char mode,unsigned char * Offset,unsigned char * Length,unsigned char *buffer);
extern  "C"   int ALL_API  __stdcall API_DESfireReadRecord(HANDLE commHandle,int DeviceAddress,unsigned char fileNO,unsigned char mode,unsigned char * Offset,unsigned char * Length,unsigned char *buffer);
extern  "C"   int ALL_API  __stdcall API_DESfireClearRecordFile(HANDLE commHandle,int DeviceAddress,unsigned char fileNO,unsigned char *buffer);
extern  "C"   int ALL_API  __stdcall API_DESfireCommitTransaction(HANDLE commHandle,int DeviceAddress,unsigned char *buffer);
extern  "C"   int ALL_API  __stdcall API_DESfireAbortTransaction(HANDLE commHandle,int DeviceAddress,unsigned char *buffer);

/**********DESfire card end***********************************/
/////////////////////////////////////////
//***************LCD control******************
extern  "C"   int ALL_API  __stdcall API_GetData(HANDLE commHandle,int DeviceAddress,int daly,unsigned char *buffer);
extern  "C"   int ALL_API  __stdcall API_LCDSetTime(HANDLE commHandle,int DeviceAddress,unsigned char secound,unsigned char minute,unsigned char hour,unsigned char day,unsigned char month, unsigned char week,unsigned char year,unsigned char *buffer);
extern  "C"   int ALL_API  __stdcall API_LCDSetPassivity(HANDLE commHandle,int DeviceAddress,unsigned char *buffer);
extern  "C"   int ALL_API  __stdcall API_LCDSetInitiative(HANDLE commHandle,int DeviceAddress,unsigned char *buffer);
extern  "C"   int ALL_API  __stdcall API_LCDClrLCD(HANDLE commHandle,int DeviceAddress,unsigned char *buffer);
extern  "C"   int ALL_API  __stdcall API_LCDShowAscii(HANDLE commHandle,int DeviceAddress,unsigned char row,unsigned char col,unsigned char Number,unsigned char * data,unsigned char *buffer);
extern  "C"   int ALL_API  __stdcall API_LCDShowTime(HANDLE commHandle,int DeviceAddress,unsigned char row,unsigned char col,unsigned char *buffer);
extern  "C"   int ALL_API  __stdcall API_LCDShowDate(HANDLE commHandle,int DeviceAddress,unsigned char row,unsigned char col,unsigned char *buffer);
extern  "C"   int ALL_API  __stdcall API_LCDShowWeek(HANDLE commHandle,int DeviceAddress,unsigned char row,unsigned char col,unsigned char *buffer);
extern  "C"   int ALL_API  __stdcall API_LCDSetLCDFlag(HANDLE commHandle,int DeviceAddress,unsigned char flagtype,unsigned char value,unsigned char *buffer);
extern  "C"   int ALL_API  __stdcall API_LCDSetTimeFlag(HANDLE commHandle,int DeviceAddress,unsigned char flagtype,unsigned char row,unsigned char col,unsigned char *buffer);
extern  "C"   int ALL_API  __stdcall API_LCDSetLCDFlash(HANDLE commHandle,int DeviceAddress,unsigned char row,unsigned char *data,unsigned char *buffer);
extern  "C"   int ALL_API  __stdcall API_SetIDDATA(HANDLE commHandle,int DeviceAddress,unsigned char row,unsigned char col,unsigned char value,unsigned char *buffer);
extern  "C"   int ALL_API  __stdcall API_refresh_cycle(HANDLE commHandle,int DeviceAddress,unsigned char time,unsigned char *buffer);
extern  "C"   int ALL_API  __stdcall API_DispalySwitch(HANDLE commHandle,int DeviceAddress,unsigned char value,unsigned char *buffer);


///////////////////////////////////////////////////
////////////ÌØÊâ¶Á¿¨Æ÷ÃüÁî/////////////////////
/////¿ÉÉèÖÃ´®ÐÐ½Ó¿Ú½Ó¿Ú¶Á¿¨Æ÷º¯Êý
extern  "C"   int ALL_API  __stdcall API_ReadDataR232(HANDLE commHandle,int DeviceAddress,unsigned char *buffer);
extern  "C"   int ALL_API  __stdcall API_SetDataOutputR232(HANDLE commHandle,int DeviceAddress,unsigned char ControlMode,unsigned char ReadInfo,unsigned char StartAddress,unsigned char *Key,unsigned char *buffer);
extern  "C"   int ALL_API  __stdcall API_SetDataOutputPS2(HANDLE commHandle,int DeviceAddress,unsigned char ControlMode,unsigned char ReadInfo,unsigned char StartAddress,unsigned char *Key,unsigned char *buffer);
extern  "C"   int ALL_API  __stdcall API_GetSysComm(unsigned char *buffer);
//////////////////////
//ultra light ¿¨²Ù×÷º¯Êý

///¶Á¿¨ÃüÁî
extern  "C"   int ALL_API  __stdcall API_UL_ReadPage(HANDLE commHandle, int DeviceAddress,unsigned char mode,  unsigned char blk_add, unsigned char num_blk,unsigned char *buffer);
///Ð´¿¨ÃüÁî
extern  "C"   int ALL_API  __stdcall API_UL_WritePage(HANDLE commHandle, int DeviceAddress,unsigned char mode, unsigned char blk_add, unsigned char num_blk,unsigned char *buffer);
///Ñ°¿¨ÃüÁî
extern  "C"   int ALL_API  __stdcall API_UL_Request(HANDLE commHandle, int DeviceAddress,unsigned char inf_mode, unsigned char *Buffer);


/////////////
/////////////////T5557//////////////////
extern  "C"   int ALL_API  __stdcall API_T5557_Read(HANDLE commHandle, int DeviceAddress,unsigned char num_blk, unsigned char * PassWords,unsigned char *buffer);
extern  "C"   int ALL_API  __stdcall API_T5557_Write(HANDLE commHandle, int DeviceAddress,unsigned char num_blk, unsigned char Lock,unsigned char * PassWords,unsigned char *buffer);
//////////////EM4469////////////////////
extern  "C"   int ALL_API  __stdcall API_EM4469_Read(HANDLE commHandle, int DeviceAddress,unsigned char number,unsigned char add_blk,unsigned char *buffer);
extern  "C"   int ALL_API  __stdcall API_EM4469_Write(HANDLE commHandle, int DeviceAddress,unsigned char number,unsigned char add_blk,unsigned char * data,unsigned char *buffer);
extern  "C"   int ALL_API  __stdcall API_EM4469_Disable(HANDLE commHandle, int DeviceAddress,unsigned char *buffer);
extern  "C"   int ALL_API  __stdcall API_EM4469_Login(HANDLE commHandle, int DeviceAddress,unsigned char * data,unsigned char *buffer);

////////////////////
///////////////////////////////////
//
extern "C" __declspec(dllexport) HANDLE __stdcall API_OpenComm(int nCom, int nBaudrate); 

extern "C" __declspec(dllexport) BOOL __stdcall API_CloseComm(HANDLE commHandle);

extern  "C"   __declspec(dllexport) int __stdcall API_HANDSHAKE(HANDLE commHandle,int DeviceAddress);
extern "C" __declspec(dllexport) int __stdcall API_SetDeviceAddress(
                                                   HANDLE commHandle, int DeviceAddress,
                                                   unsigned char newAddr, unsigned char *buffer);

extern "C" __declspec(dllexport) int __stdcall API_SetBaudrate(
                                                   HANDLE commHandle, int DeviceAddress,
                                                   unsigned char newBaud,unsigned char *buffer);

extern "C" __declspec(dllexport) int __stdcall API_SetSerNum(
                                                   HANDLE commHandle, int DeviceAddress,
                                                   unsigned char *newValue, unsigned char *buffer);

extern "C" __declspec(dllexport) int __stdcall API_GetSerNum(
                                                   HANDLE commHandle, int DeviceAddress,unsigned char *buffer);

extern "C" __declspec(dllexport)   int __stdcall API_WriteUserInfo(
                                                    HANDLE commHandle, int DeviceAddress,
                                                    int num_blk, int num_length, char *user_info);

//xx Get user information
extern "C" __declspec(dllexport) int __stdcall API_ReadUserInfo(
                                                    HANDLE commHandle, int DeviceAddress,
                                                    int num_blk, int num_length,char *user_info);

extern "C" __declspec(dllexport) int __stdcall API_GetVersionNum(
	                                             HANDLE commHandle, int DeviceAddress, char *VersionNum);

extern "C" __declspec(dllexport) int __stdcall API_ControlLED(
                                                   HANDLE commHandle, int DeviceAddress,
                                                   unsigned char freq,unsigned char duration, unsigned char *buffer);

//extern "C" __declspec(dllexport) int __stdcall API_ControlMultiLED(HANDLE commHandle, int DeviceAddress,unsigned char lednum, unsigned char freq,unsigned char duration, unsigned char *buffer);

extern "C" __declspec(dllexport) int __stdcall API_ControlBuzzer(
                                                   HANDLE commHandle, int DeviceAddress,
                                                   unsigned char freq, unsigned char duration,
                                                   unsigned char *buffer);

//add by feng
extern "C" __declspec(dllexport) int __stdcall API_SetCardType(
                                                   HANDLE commHandle, int DeviceAddress,
                                                   unsigned char cardtype, unsigned char *buffer);



///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//ISO14443 Functions.
// ISO14443 Type-A Functions 
extern "C" __declspec(dllexport) int __stdcall API_MF_Request(
                                                   HANDLE commHandle, int DeviceAddress, 
                                                   unsigned char inf_mode, unsigned char *Buffer);

extern "C" __declspec(dllexport) int __stdcall API_MF_Anticoll(
	                                            HANDLE commHandle, int DeviceAddress,
	                                            unsigned char *flag,unsigned char *uid);

extern "C" __declspec(dllexport) int __stdcall API_MF_Select(
	                                            HANDLE commHandle, int DeviceAddress,
	                                            unsigned char *uid, unsigned char *buffer);

extern "C" __declspec(dllexport) int __stdcall API_MF_Halt(
	                                            HANDLE commHandle, int DeviceAddress);


//Mifare application functions. for Type A

extern "C" __declspec(dllexport) int __stdcall API_MF_Read(
                                                   HANDLE commHandle, int DeviceAddress,
                                                   unsigned char mode,  unsigned char blk_add, unsigned char num_blk,
							  unsigned char *snr, unsigned char *buffer);

extern "C" __declspec(dllexport) int __stdcall API_MF_Write(
	                                            HANDLE commHandle, int DeviceAddress,
	                                            unsigned char mode, unsigned char blk_add, unsigned char num_blk,
							  unsigned char *key, unsigned char *text, unsigned char *buffer);

extern "C" __declspec(dllexport) int __stdcall API_MF_InitVal(
	                                            HANDLE commHandle, int DeviceAddress,
	                                            unsigned char mode, unsigned char sec_num,
							  unsigned char *key, int  value, unsigned char *buffer);

extern "C" __declspec(dllexport) int __stdcall  API_MF_Dec(
	                                            HANDLE commHandle, int DeviceAddress,
	                                            unsigned char mode,unsigned char sec_num,
                                                   unsigned char *key, int value,int *result, unsigned char *buffer);

extern "C" __declspec(dllexport) int __stdcall API_MF_Inc(
	                                            HANDLE commHandle, int DeviceAddress,
	                                            unsigned char mode,unsigned char sec_num,
                                                   unsigned char *key, int value,int *result, unsigned char *buffer);

extern "C" __declspec(dllexport) int __stdcall API_MF_GET_SNR(
	                                            HANDLE  commHandle,int DeviceAddress, 
	                                            unsigned char mode,unsigned char cmd,unsigned char *flag, 
	                                            unsigned char *buffer);

extern "C" __declspec(dllexport) int __stdcall API_MF_Value(
	                                            HANDLE  commHandle,int DeviceAddress, 
	                                            unsigned char mode,unsigned char add_blk,int value);

extern "C" __declspec(dllexport) int __stdcall API_MF_PowerOn(
	                                            HANDLE  commHandle,int DeviceAddress, 
	                                            unsigned char mode,unsigned char cmd, unsigned char *buffer);

extern "C" __declspec(dllexport) int __stdcall API_MF_TransferCMD(
	                                            HANDLE   commHandle,int DeviceAddress, 
	                                            unsigned char mode, unsigned char cmdlength, 
	                                            unsigned char *cmd, unsigned char *returnlen, unsigned char *buffer);

extern "C" __declspec(dllexport) int __stdcall API_MF_RST_Antenna(
	                                           HANDLE  commHandle,int DeviceAddress,unsigned char *buffer);

extern "C" __declspec(dllexport) int __stdcall API_MF_Copy_Block(
	                                           HANDLE  commHandle,int DeviceAddress,
	                                           unsigned char add_blk, unsigned char key, 
	                                           unsigned char *keyvalue,unsigned char *buffer);

extern "C" __declspec(dllexport) int __stdcall API_MF_Auth(
	                                           HANDLE  commHandle,int DeviceAddress,
	                                           unsigned char mode, unsigned char add_blk, unsigned char *keyvalue);

//ISO14443 Type-B Functions
extern "C" __declspec(dllexport) int __stdcall API_Request_B(
                                                  HANDLE commHandle,int DeviceAddress, 
                                                  unsigned char * returnlen,unsigned char *buffer);

extern "C" __declspec(dllexport) int __stdcall API_Anticoll_B(
	                                           HANDLE commHandle,int DeviceAddress, 
	                                           unsigned char *returnlen, unsigned char *buffer);

extern "C" __declspec(dllexport) int __stdcall API_Attrib_B(
	                                           HANDLE  commHandle,int DeviceAddress,
	                                           unsigned char *SerialNum,unsigned char *buffer);

extern "C" __declspec(dllexport) int __stdcall API_RESET_B(
	                                           HANDLE commHandle,int DeviceAddress, unsigned char *buffer);

extern "C" __declspec(dllexport) int __stdcall API_TransferCMD_B(
	                                           HANDLE commHandle, int DeviceAddress,unsigned char cmdSize, 
	                                           unsigned char *cmd, unsigned char *returnlen, unsigned char *buffer);
/////ST  SRI serier card command
extern "C" __declspec(dllexport) int __stdcall API_SRI_READ(HANDLE commHandle, int deviceAddress,
                  unsigned char block_frist,  unsigned char block_number,  unsigned char *pBuffer);
extern "C" __declspec(dllexport) int __stdcall API_SRI_WRITE(HANDLE commHandle, int deviceAddress,
                  unsigned char block_frist,  unsigned char block_number,const unsigned char * data, unsigned char *pBuffer);
extern "C" __declspec(dllexport) int __stdcall API_SRI_LOCK(HANDLE commHandle, int deviceAddress,
                  unsigned char block_frist, unsigned char *pBuffer);
extern "C" __declspec(dllexport) int __stdcall API_SRI_DEC(HANDLE commHandle, int deviceAddress,
                  unsigned char block_frist, const unsigned char * value, unsigned char *pBuffer);
extern "C" __declspec(dllexport) int __stdcall API_SRI_GETSN(HANDLE commHandle, int deviceAddress,
                  unsigned char *pBuffer);
extern "C" __declspec(dllexport) int __stdcall API_SRI_GETSTATU(HANDLE commHandle, int deviceAddress,
                  unsigned char *pBuffer);
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//ISO15693 Functions.
extern "C" __declspec(dllexport) int __stdcall API_ISO15693_Inventory(
                                                  HANDLE commHandle, int deviceAddress,
                                                  unsigned char flag, unsigned char afi,unsigned char datalen,
                                                  const unsigned char *pData,unsigned char *nrOfCard,
                                                  unsigned char *pBuffer);

extern "C" __declspec(dllexport) int __stdcall API_ISO15693_Read(
	                                            HANDLE commHandle,int DeviceAddress,unsigned char flags,
                                                   unsigned char blk_add,unsigned char num_blk,
                                                   unsigned char *uid, unsigned char *returnlen, unsigned char *buffer);

extern "C" __declspec(dllexport) int __stdcall API_ISO15693_Write(
	                                            HANDLE commHandle,int DeviceAddress,unsigned char flags,
                                                   unsigned char blk_add,unsigned char num_blk,
                                                   unsigned char *uid, unsigned char *data);


extern "C" __declspec(dllexport) int __stdcall API_ISO15693_Lock(
	                                            HANDLE commHandle,int DeviceAddress,unsigned char flags,
                                                   unsigned char num_blk,
                                                   unsigned char *uid,unsigned char  *buffer);

extern "C" __declspec(dllexport) int __stdcall API_ISO15693_StayQuiet(
	                                            HANDLE commHandle,int DeviceAddress,unsigned char flags,
                                                   unsigned char *uid,  unsigned char  *buffer );

extern "C" __declspec(dllexport) int __stdcall API_ISO15693_Select(
	                                           HANDLE commHandle,int DeviceAddress,unsigned char flags,
                                                  unsigned char *uid,  unsigned char  *buffer );

extern "C" __declspec(dllexport) int __stdcall API_ISO15693_ResetToReady(
	                                           HANDLE commHandle,int DeviceAddress,unsigned char flags,
                                                  unsigned char *uid,  unsigned char  *buffer );

extern "C" __declspec(dllexport) int __stdcall API_ISO15693_WriteAFI(
	                                           HANDLE commHandle,int DeviceAddress,unsigned char flags,
	                                           unsigned char  afi,unsigned char *uid,  unsigned char  *buffer );

extern "C" __declspec(dllexport) int __stdcall API_ISO15693_LockAFI(
	                                          HANDLE commHandle,int DeviceAddress,unsigned char flags,
                                                 unsigned char *uid,  unsigned char  *buffer );

extern "C" __declspec(dllexport) int __stdcall API_ISO15693_WriteDSFID(
	                                          HANDLE commHandle,int DeviceAddress,unsigned char flags,
	                                          unsigned char  DSFID,unsigned char *uid,  unsigned char  *buffer );

extern "C" __declspec(dllexport) int __stdcall API_ISO15693_LockDSFID(
	                                          HANDLE commHandle,int DeviceAddress,unsigned char flags,
                                                 unsigned char *uid,  unsigned char  *buffer );

extern "C" __declspec(dllexport) int __stdcall API_ISO15693_GetSysInfo(
	                                          HANDLE commHandle, int deviceAddress,
                                                 unsigned char flag, unsigned char *uid, unsigned char *buffer);

extern "C" __declspec(dllexport) int __stdcall API_ISO15693_GetMulSecurity(
	                                          HANDLE commHandle, int deviceAddress,
                                                 unsigned char flag, unsigned char blkAddr, unsigned char blkNum,
                                                 const unsigned char *uid, unsigned char *flags, unsigned char *returnlen, 
                                                 unsigned char *pBuffer);
 
extern "C" __declspec(dllexport) int __stdcall API_ISO15693_TransCmd(
	                                          HANDLE commHandle, int DeviceAddress, int cmdSize, 
	                                          unsigned char *cmd, unsigned char *returnlen, unsigned char *pbuffer);

//iso15693 NXP SL@ ICS53 secret command  by level

extern "C" __declspec(dllexport) int __stdcall API_ISO15693_SetPWS(HANDLE commHandle, int deviceAddress,
                                unsigned char flag, const unsigned char *uid, 
								unsigned char PWS_Identifier, unsigned char *PassWordS, 
								unsigned char *pBuffer);
extern "C" __declspec(dllexport) int __stdcall API_ISO15693_WritePWS(HANDLE commHandle, int deviceAddress,
                                unsigned char flag, const unsigned char *uid, 
								unsigned char PWS_Identifier, unsigned char *PassWordS, 
								unsigned char pws_flag,unsigned char *pBuffer);
extern "C" __declspec(dllexport) int __stdcall API_ISO15693_ProtectPage(HANDLE commHandle, int deviceAddress,
                                unsigned char flag, const unsigned char *uid, 
								unsigned char Page_number, unsigned char Protect_Mode,
								unsigned char Mode_State, unsigned char *pBuffer);
extern "C" __declspec(dllexport) int __stdcall API_ISO15693_PWS64Protect(HANDLE commHandle, int deviceAddress,
                                unsigned char flag, const unsigned char *uid, unsigned char *pBuffer);
extern "C" __declspec(dllexport) int __stdcall API_ISO15693_ProtectStatus(HANDLE commHandle, int deviceAddress,
                                unsigned char flag, const unsigned char *uid, 
								unsigned char first_number, unsigned char number_block,
								 unsigned char *pBuffer);
extern "C" __declspec(dllexport)  int __stdcall API_ISO15693_DestroySLI(HANDLE commHandle, int deviceAddress,unsigned char flag, 
						const unsigned char *uid, unsigned char *pBuffer);
extern "C" __declspec(dllexport) int __stdcall API_ISO15693_EnablePrivacy(HANDLE commHandle, int deviceAddress,
                                unsigned char flag, const unsigned char *uid, unsigned char *pBuffer);
extern "C" __declspec(dllexport) int __stdcall API_ISO15693_PageRead(HANDLE commHandle, int deviceAddress,
                                unsigned char flag, const unsigned char afi, 
								unsigned char first_number, unsigned char number_block,
								 unsigned char *pBuffer);
extern "C" __declspec(dllexport) int __stdcall API_ISO15693_OperateEAS(HANDLE commHandle, int deviceAddress,
                                unsigned char flag, const unsigned char *uid, unsigned char mode,unsigned char *pBuffer);
extern "C" __declspec(dllexport) int __stdcall API_ISO15693_AlarmEAS(HANDLE commHandle, int deviceAddress,
                  unsigned char flag, const unsigned char *uid, unsigned char mask_length,
				  const unsigned char *mask_value,unsigned char *pBuffer);
extern "C" __declspec(dllexport) int __stdcall API_WriteEASID(HANDLE commHandle, int deviceAddress,
                  unsigned char flag, const unsigned char *uid,
				  const unsigned char *mask_value,unsigned char *pBuffer);
extern "C" __declspec(dllexport) int __stdcall API_ReadEPC(HANDLE commHandle, int deviceAddress,
                  unsigned char flag, const unsigned char *uid, unsigned char *pBuffer);























//Attend command    add by feng 

extern "C" __declspec(dllexport) int __stdcall API_SysClockSet(
                                                 HANDLE commHandle, int DeviceAddress,unsigned char *datetime, 
                                                 unsigned char *buffer);

extern "C" __declspec(dllexport) int __stdcall API_SysClockGet(
	                                          HANDLE commHandle, int DeviceAddress, unsigned char *buffer);

extern "C" __declspec(dllexport) int __stdcall API_DisplayInf(
	                                          HANDLE commHandle, int DeviceAddress, unsigned char brightness, 
	                                          unsigned char row_adr, unsigned char col_adr, unsigned char strlen, 
	                                          unsigned char *str, unsigned char *buffer);

extern "C" __declspec(dllexport) int __stdcall API_GetKeyValue(
	                                          HANDLE commHandle, int DeviceAddress, unsigned char *len, 
	                                          unsigned char *buffer);

extern "C" __declspec(dllexport) int __stdcall API_SetKeyOnOff(
	                                          HANDLE commHandle, int DeviceAddress, unsigned char onoff, 
	                                          unsigned char *buffer);

extern "C" __declspec(dllexport) int __stdcall API_GetHistoryInfo(
	                                         HANDLE commHandle, int DeviceAddress, 
	                                         unsigned short *unreadcount, unsigned char *len, 
	                                         unsigned char *types, unsigned char *buffer);

extern "C" __declspec(dllexport) int __stdcall API_DelHistoryInfo(
	                                         HANDLE commHandle, int DeviceAddress, unsigned char *buffer);

extern "C" __declspec(dllexport) int __stdcall API_RDDesCode(
	                                         HANDLE commHandle, int DeviceAddress, unsigned char mode, 
	                                         unsigned short keylen, unsigned char *key, unsigned short *datalen,  
	                                         unsigned char *buffer);

extern "C" __declspec(dllexport) int __stdcall API_RDSamReset(
	                                         HANDLE commHandle, int DeviceAddress, unsigned char samno, 
	                                         unsigned char baudrate, unsigned short *strlen, unsigned char *buffer);

extern "C" __declspec(dllexport) int __stdcall API_RDSamGen(
	                                         HANDLE commHandle, int DeviceAddress, unsigned char nad, 
	                                         unsigned char pcb, unsigned char cmdlen, unsigned char *cmd, 
	                                         unsigned char *ret_nad, unsigned char *ret_pcb, 
	                                         unsigned char *ret_cmdlen, unsigned char *buffer);

extern "C" __declspec(dllexport) int __stdcall API_RDSamOff(
	                                         HANDLE commHandle, int DeviceAddress, unsigned char samno, 
	                                         unsigned char *buffer);

extern "C" __declspec(dllexport) int __stdcall API_RDCpuReset(
	                                         HANDLE commHandle, int DeviceAddress, unsigned char types, 
	                                         unsigned char baudrate, unsigned char *strlen, unsigned char *buffer);

extern "C" __declspec(dllexport) int __stdcall API_RDCpuGen(
	                                         HANDLE commHandle, int DeviceAddress, unsigned char nad, 
	                                         unsigned char pcb, unsigned char cmdlen, unsigned char *cmd, 
	                                         unsigned char *ret_nad, unsigned char *ret_pcb, 
	                                         unsigned char *res_cmdlen, unsigned char *buffer);

extern "C" __declspec(dllexport) int __stdcall API_RDRfcpuReset(
	                                         HANDLE commHandle, int DeviceAddress, 
	                                         unsigned char types, unsigned char *strlen, unsigned char *buffer);

extern "C" __declspec(dllexport) int __stdcall API_RDRfcpuGen(
	                                         HANDLE commHandle, int DeviceAddress, unsigned char nad, 
	                                         unsigned char cid, unsigned char pcb, unsigned char cmdlen, 
	                                         unsigned char *cmd, unsigned char *ret_nad, unsigned char *ret_cid, 
	                                         unsigned char *ret_pcb, unsigned char *ret_cmdlen, unsigned char *buffer);

extern "C" __declspec(dllexport) int __stdcall API_RDRfcpuHalt(
	                                         HANDLE commHandle, int DeviceAddress, unsigned char *buffer);


