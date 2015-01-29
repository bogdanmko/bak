<?


//----------------------------------------------------------------
//Emailer (c) Колисниченко Д.Н.
// Издательство: "Наука и техника"
//----------------------------------------------------------------
class CEmailer {
var $parts;
var $to;
var $from;
var $headers;
var $subject;
var $body;

// создаем класс
function CEmailer() 
{
 $this->parts = array();
 $this->to =  "";
 $this->from =  "";
 $this->subject =  "";
 $this->body =  "";
 $this->body2 = "";
 $this->headers =  "";
}

// как раз сама функция добавления файлов в мыло
function add_attachment($message, $name = "", $ctype = "application/octet-stream") 
{
 $this->parts [] = array (
  "ctype" => $ctype,
  "message" => $message,
  "encode" => $encode,
  "name" => $name
 );
}

// Построение сообщения (multipart)
function build_message($part) 
{
 $message = $part["message"];
 $message = chunk_split(base64_encode($message));
 $encoding = "base64";
 return "Content-Type: ".$part["ctype"].($part["name"]? "; name=\"".$part["name"]."\"" : "")."\nContent-Transfer-Encoding: $encoding\n\n$message\n";
}

function build_multipart(&$headers, &$body) 
{
 $boundary = "b".md5(uniqid(time()));
 $headers = "MIME-Version: 1.0\nContent-Type: multipart/mixed; boundary=\"$boundary\"\n";
 $body ="This is a MIME encoded message.\n--$boundary";
 for($i = sizeof($this->parts)-1; $i>=0; $i--) $body .= "\n".$this->build_message($this->parts[$i]). "--$boundary";

 $body .= "--\n";
 
}

// Посылка сообщения, последняя вызываемая функция класса
function send() 
{
 $mime = "";
 if (!empty($this->from)) $mime .= "From: ".$this->from. "\n";
 if (!empty($this->headers)) $mime .= $this->headers. "\n";
 if (!empty($this->body)) $this->add_attachment($this->body, "", "text/plain");  
 $headers;
 $body;
 $this->build_multipart( $headers, $body );
 $mime .= $headers;
//  print "<pre>".$body."</pre>";
 return mail($this->to, $this->subject, $body, $mime);
 
}
};
//----------------------------------------------------------------
// CEmail end
//----------------------------------------------------------------

