<?
//Common utilities

$Keys = array( 'AKDGAN18293', '5461agsjhags', '1227384haafgb', '2738909aganfhg' );

class CObject
{
  var $b;
  
  function CObject()
  {
    global $b;
	$this->b = $b;
  }
};



function IsEmpty( $String )
{
  return  preg_match( '/^\s*$/', $String )? 1 : 0;
}

function IsNewJersey( $State )
{
  return preg_match( '/^(NJ|N.J.|New Jersey|NewJersey|New Jersy|New Jersi)$/i', $State )? 1 : 0;
}

function IsUSA( $Country )
{
  return preg_match( '/(USA|U\.S\.A\.|U\.S\.A|United\s+States|US|United\s+States\s+of\s+America)$/i', $Country )? 1 : 0;
}

function MCrypt( $Data, $Key, $Delim = ',' )
{
   $KeyLen = strlen( $Key );
   $arrKey = str_split( $Key );
   $arrData = str_split( $Data );
   
   $KeyIdx = 0;
   
   $Data = "";
   
   foreach( $arrData as $DataElement )
   {
     if( $Data != "" )
	   $Data .= $Delim;
	   
     $Data .= sprintf( "%d", ord($DataElement) ^ ord($arrKey[$KeyIdx]) );
 
	 $KeyIdx++;
	 if( $KeyIdx >= $KeyLen )
	   $KeyIdx = 0;
   }
   
   return $Data;
   
}

function MDecrypt( $Data, $Key )
{
  $KeyLen = strlen( $Key );
  $arrKey = str_split( $Key );
  $arrCharCodes = preg_split(  '/\D/', $Data  );
  $Result = '';
  $KeyIdx = 0;
  foreach(  $arrCharCodes as $CharCode )
  {


    $Result .= chr( $CharCode ^ ord($arrKey[$KeyIdx]) );

    $KeyIdx++;
  if( $KeyIdx >= $KeyLen )
    $KeyIdx = 0;


  }

  return $Result;
}


function IsSSL()
{
  return $_SERVER['HTTPS'] != '';
}

function GetScriptDirectory($AddURLPrefix="", $RemoveURLPrefix="")
{
  $ScriptURL = $_SERVER['SCRIPT_NAME'];
  $Matches = array();
  $Result = "";
  if( preg_match( '/^(.+\/)[^\/]+?$/', $ScriptURL, $Matches ) )
    $Result = $Matches[1];
	
  if( $AddURLPrefix != "" )
  {
    $Result = $AddURLPrefix.$Result;
  }
  else if( $RemoveURLPrefix != "" )
  {
   $RemoveURLPrefix = preg_quote( $RemoveURLPrefix, '/' );
    if( preg_match( '/^$RemoveURLPrefix(.+)$/', $Matches ) )
	  $Result = $Matches[1];
  }
	
  return $Result;
}

function GetURI( $HTTPRoot, $FileOnServer, $AddURLPrefix="", $RemoveURLPrefix="" )
{
  
  if( preg_match( '/^\//', $FileOnServer ) ) //Absolute path
    return $HTTPRoot.$FileOnServer;
  else //Relative path
    return $HTTPRoot.GetScriptDirectory($AddURLPrefix, $RemoveURLPrefix).$FileOnServer;
}

function CheckForExpiration( $Month, $Year )
{
 $Date = getdate();  
 $Expired = 0;
 if( $Date['year'] > $Year )
    $Expired = 1;
  else if( $Date['year'] == $Year )
  {
    if( $Date['mon'] >= $Month )
	  $Expired = 1;
  }
  return $Expired;
}

function CCNumberIsOK( $CCNumber )
{
  $OK = 0;
  $Len = strlen( $CCNumber );
  
  if( preg_match( '/^4111111111111111$/i', $CCNumber ) )
    return  0;
  
  
  if( ($Len != 16) && ($Len != 13) && ($Len != 15 ) )
    return 0;
	
  //	Check digits
  
    $sum = 0; 
    $mul = 1; 
	$Digits = str_split( $CCNumber );
    for ($i = $Len-1; $i >= 0; $i--)
    {
        
	    $CurDigit = $Digits[$i];
        $tproduct = $CurDigit * $mul;
	
        if ($tproduct >= 10)
	{
	  $sum += ($tproduct % 10) + 1;
	}
        else
	{
	  $sum += $tproduct;
	}
        if ($mul == 1)  $mul++; else $mul--;
    }
	
    return ($sum % 10) == 0;

}

function GetYearsFromCurrent( $Number )
{
 $Date = getdate();  
 $CCYears = array();
 $Year = $Date['year'];
 for( $i=0; $i<$Number; $i++ )
 {
   array_push( $CCYears, $Year );
   $Year++;
 }

 return $CCYears;

}

function RmChars( $Value, $Chars, $Replacers = ' ' )
{
  $CharsArr = str_split( $Chars );
  $ValueChars = str_split( $Value );
  $ReplacerChars = str_split( $Replacers );
  $Result = '';

  foreach( $ValueChars as $ValueChar )
  {
    $bReject = 0;
    $nIndex = 0;
    foreach( $CharsArr as $Char )
    {
      if( ord($Char) == ord($ValueChar) )
      {
        $bReject = 1;
        break;
      }
      $nIndex++;
    }

    if( $nIndex >= count( $ReplacerChars ) )
      $nIndex = count( $ReplacerChars )-1;
     

    if( $bReject )
     $Result .= $ReplacerChars[$nIndex];
    else
     $Result .= $ValueChar;

  }

  return $Result;


}

function RmCRLF( $Value )
{
  $Chars = str_split( $Value );
  $Result = '';
  foreach( $Chars as $Char )
  {
    $Code = ord( $Char );
    if( $Code == ord("\n" ) )
      $Result .= ' ';
    else  if( $Code ==  ord( "\r" ) )
      $Result .= ' ';
    else
      $Result .= $Char;
  }

  return $Result;
}


function HTML( $Value )
{
  $Chars = str_split( $Value );
  $Result = '';
  $WasBackslash = 0;
  foreach( $Chars as $Char )
  {
    $Code = ord( $Char );

    if( $WasBackslash  )
    {
      $WasBackslash = 0;
      if( $Code == ord('n') )
      {
        $Result .= "<br>\n";
        continue;
      }
      else
        $Result .= '\\';
    }


  if( $Code == ord( "'" ) )
      $Result .= '&apos;'; else
 if( $Code == ord( '"' ) )
      $Result .= '&quot;';
    else if( $Code == ord( '>' ) )
      $Result .= '&gt;';
    else if( $Code == ord( '<' ) )
      $Result .= '&lt;';
    else if( $Code == ord( '&' ) )
      $Result .= '&amp;';
    else if( $Code == ord( '\\' ) )
      $WasBackslash = 1;
    else
      $Result .= $Char;

  }
  
  return $Result;
}

function Unquot( $Text )
{
  $Chars = str_split( $Text );
  $Result = '';
  $WasQuot = 0;
  
  foreach( $Chars as $Char )
  {
    $Skip = 0;
    
    if( $Char == '\\' && !$WasQuot )
	{
	  $Skip = 1;
	  $WasQuot = 1;
	}
	else
	  $WasQuot = 0;
	  
	 if( ! $Skip )
	   $Result .= $Char;

  }
  
  return $Result;
  
}



function INCH2MM( $Inches )
{
  return $Inches * 25.4;
}

function MM2INCH( $MM )
{
  return $MM / 25.4;
}


function LBS2KG( $Lbs )
{
  return $Lbs * 0.4535;
}

function KG2LBS( $Kg )
{
  return $Kg / 0.4535;
}


function SQINCH2SQMM( $SqInches )
{
  return $SqInches * 25.4 * 25.4;
}


function ConvertEMSDimensionsInchToMM( $InchDim )
{


  $Results = array( 'dim1xdim2xdim3', 'dima1mm^2xdim2mm(Overall dim3xdim4mm)', 'D=dim1;dim2mm long', 'dima1mm^2xdim2mm' );
  $RExps = array( '/(\d*\.\d+|\d+)\s*x\s*(\d*\.\d+|\d+)\s*x\s*(\d*\.\d+|\d+)\s*(\"|in)/i',
                '/(\d*\.\d+|\d+)\s*in\^2\s*x\s*(\d*\.\d+|\d+)\s*in\s*\(.+?(\d*\.\d+|\d+)\s*x\s*(\d*\.\d+|\d+)\s*(\"|in)/i',
                '/(\d*\.\d+|\d+)\,\s*(\d*\.\d+|\d+)\s*\"\s+long/i',
                '/(\d*\.\d+|\d+)\s*in\^2\s*x\s*(\d*\.\d+|\d+)\s*in\s*$/i' );

  $patterns = array( '/dim1/', '/dim2/', '/dim3/', '/dim4/', '/dim5/', '/dim6/' );
  $patterns_sqin = array( '/dima1/', '/dima2/', '/dima3/', '/dima4/', '/dima5/', '/dima6/' );



  $Index = 0;
  foreach( $RExps as $RExp )
  {
   $Matches = array();
   if( preg_match( $RExp, $InchDim,  $Matches ) )
   {
     $Res = $Results[$Index];
     
     $MatchIdx = 1;
     foreach( $patterns as $pattern )
     {
       $Dim_mm = sprintf( "%.0f", floor(INCH2MM($Matches[$MatchIdx]*1000.0))/1000.0 );
       
       $Res = preg_replace( $pattern, $Dim_mm, $Res );

       $Dim_mm2 = sprintf( "%.0f", floor(SQINCH2SQMM($Matches[$MatchIdx]*100.0))/100.0 );

       $Res = preg_replace( $patterns_sqin[$MatchIdx-1], $Dim_mm2, $Res );

       $MatchIdx++;
     }
     
     return $Res;

   }
   $Index++;

  }

  return $InchDim;

}

function ClipAt( $String, $Pos )
{
  $Result = trim( $String );
  $Matches = array();
  if( preg_match( '/^\s*?(.{'.$Pos.'})/i', $Result, $Matches ) )
    $Result = trim($Matches[1]).'...';

  return $Result;
  
}


function ClearDir( $Dir, $RmDir=0, $TimeThreshould=0 )
{
  $Result = 1; //Success
  if( ! ($handle = @opendir( $Dir  )) )
    return 0;
	
  while (false !== ($file = @readdir($handle))) 
  {
 	 if( $file == '.' || $file == '..' )
	   continue;
	   
	  $file = $Dir.'/'.$file;
	  
	  if( $TimeThreshould )
	  {
	    $time = filemtime( $file );
	    if( $time > $TimeThreshould )
	       continue;
	  }
   
     @chmod( $file, 0777 );
	 if( @is_dir( $file ) )
	 {
	   if( ! ClearDir( $file, 1 ) )
	     $Result = 0;
	 }
	 else
	 {
       if( ! @unlink( $file ) )
	     $Result = 0;
	 }
  }
	
  @closedir( $handle );
  
  if( $RmDir )
    if( ! @rmdir( $Dir ) )
	  $Result = 0;
	  
  
  return $Result;
}

function IsValidEmail( $Email )
{
  return preg_match( '/^ *[_A-Za-z0-9\-\.\+\=\(\)\~\^\&\,]+\@[_A-Za-z0-9\.\-\+\=\(\)\~\^\&\,]{3,} *$/', $Email );
}

function IsPOBox( $Address )
{
  return preg_match( '/^\s*po\s+box/i',$Address );
}

?>