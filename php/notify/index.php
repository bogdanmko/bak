<?

$b = "";


include_once $b."../lib/template.php";
include_once $b."../lib/utils.php";
include_once $b."../lib/email.php";
include_once $b."../common/comdefines.php";

$subject = $_POST['subject'];
$test = $_POST['test'];
$sender = $_POST['sender'];
$text = $_POST['text'];

if( preg_match( '/(\w+)/s', $sender, $Matches ) )
  $sender = $Matches[1];
else
  $sender = "notify";


$to = "bogdan@pad2pad.com;jimlewis@emachineshop.com";
if( $test == '1' )
  $to = "bodiic@mail.ru";


if( $subject == '' )
{
?>
<html><body><form action='index.php' method='POST'  enctype='multipart/form-data'>
Subject: <input type='text' name='subject'><br>
Desc:<br>
<textarea name='text' rows='5' maxlength='1024'></textarea><br>
<input type='hidden' name='test' value='1'>
<input type='hidden' name='who' value='test'>
<input type='submit' value='Submit'>
</body></html>
<?
exit;

}

  $from = $sender."_noreply@pad2pad.com";

  $mail = new CEmailer();
  $mail->from = $from;
  $mail->headers = "Errors-To: ". $from."\n".
                   "Reply-To: ".$from;
  $mail->to = $to;
  $mail->subject = $subject;


  $mail->body = $text;


  if( $mail->send() )
     print "===OK===";
  else
     print "===ERROR===";



?>
