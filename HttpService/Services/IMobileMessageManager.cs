/**
 * 매개변수 설명
 * //(필요없음)proc = _SendMobileMSG
 * 
 * gubun = send_sms (사용안함 -> send_public_sms)
 * sessionID = {사용자 인증 후 발급받은 번호}
 * pole_num = {기기구분값-로그기록에서 사용}
 * 
 * receiver_phone = {받을 사람 전화번호 -형식 :010-1234-5678}
 * sender_phone = {보내는 사람 전화번호 -형식 : 010-1234-5678}
 * 
 * msg_type = {MMS|SMS|URL}
 * 
 * msg = {
 *      mms 발송 내용 - MMS일 경우 - 한글 1000자 기준, 
 *      SMS일 경우 - 한글 40자/영문 80자, 
 *      URL일 경우 - “http:// wap.test.co.kr 제목타이틀”
 * }
 * 
 * (아래 이하는 msg_type = MMS일 경우)
 * mms_subject = {mms 발송 제목 - 한글 20자 기준}
 * 
 * attachment_key = {로컬 파일 사용시 mms_file_name1~5, mms_file_url1~5 대신 사용, 파일 1개 기준}
 * 
 * mms_file_cnt = {1~5사이 값, attachment_key값이 있을 경우 무시}
 * mms_file_type1~5 = IMG (자세한 사항 아래 참조, attachment_key값을 사용할 경우 mms_file_type1만 사용)
 * 
 * mms_file_name1~5 = {발송할 이미지 파일명 -예: xxx.jpg, attachment_key값이 있을 경우 무시}
 * mms_file_url1~5 = {발송할 이미지를 가져올 url -예: http://mediapole2.cafe24.com/.../savePic/, attachment_key값이 있을 경우 무시}
 */
using HttpService.Lib;
using HttpService.Models;
using System.Threading.Tasks;

namespace HttpService.Services
{
    public interface IMobileMessageManager
    {
        Task<ResponseModel> Send(RequestModel model);
    }
}
