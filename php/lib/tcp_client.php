<?

define( "TCPERROR_SIGN", '***' );


class CTCPClient
{

var $request;
var $answer;
var $request_len;
var $error;

var $server_ip;
var $server_port;

function CTCPClient( $server_ip, $server_port)
{
  $this->server_ip = $server_ip;
  $this->server_port = $server_port;
}


function Send( $request )
{
  $this->request = $request;
  
  
  $RequestLen = 0;
  
  $this->error = "";
  $this->answer = "NO_ANSWER";

  //Create client socket
  $socket = @fsockopen( $this->server_ip, $this->server_port, $errno, $errstr, 30 );
 
  if(  $socket )
  {
    $this->request_len = strlen( $this->request );
    //Write request to socket
	
//	$DelaySec =  $this->request_len / 300000;
//	if( $DelaySec < 1 )
//	  $DelaySec = 1;

//	sleep( $DelaySec ); //This delay is needed. Otherwise server receives incomplete data!!!!!!!!!!
	
    fwrite( $socket, $request, $this->request_len );
	
	fflush( $socket );
	

	$this->answer = "";
	
	
	
	//Read answer from socket
    while (!feof($socket))
	  $this->answer .= fgets( $socket, 1024 );
	
	fclose( $socket );
  }
  else
   $this->error = "$errstr ($errno)";

  

  if( $this->error != "" )
    $this->answer = TCPERROR_SIGN."Unable to process the request: $this->error. Try again later or contact us.";
  
  
}

function GetAnswer()
{
  return $this->answer;
}

function IsTimeOutError()
{
  if( preg_match( '/timed\s+out/i', $this->answer ) )
    return 1;
  else
    return 0;
}


}




?>