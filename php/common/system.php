<?
//Site specific utilities
include_once $b."../common/comdefines.php";
include_once $b."../lib/utils.php";


function GetNormalURI( $FileOnServer )
{
  if( IsSSL() )
      return GetURI( ROOT_URL, $FileOnServer, "",  SSL_URL_PREFIX );
    else
      return GetURI( ROOT_URL, $FileOnServer );
}

function GetSSLURI( $FileOnServer )
{
  if( IsSSL() )
    return GetURI( SSL_ROOT_URL, $FileOnServer );
  else
    return GetURI( SSL_ROOT_URL, $FileOnServer, SSL_URL_PREFIX );
}



?>