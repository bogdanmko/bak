<?

include_once $b.'../lib/utils.php';

//----------------------------------------------------------------
//Template operations
//----------------------------------------------------------------

$CurrParamValue = '';

function replacer_callback( $matches )
{
  global $CurrParamValue;
  return $CurrParamValue;
}

function TextReplace( $ParamName, $ParamValue, &$Text )
{
  global $CurrParamValue;

  $ParamName = preg_quote( $ParamName, '/' );
  $CurrParamValue = $ParamValue;
  $Text =  preg_replace_callback( '/'.$ParamName.'/i',

  "replacer_callback",


   $Text );
  
}


class CTemplate extends CObject
{
  var $templ_file_name;
  var $text;
  var $markerA = 'THIS_IS_A_SERVER_';
  var $markerB = 'SCRIPT_MICLOG_TEMPLATE_FILE';
  var $params  = array();
  var $CurrParamValue;

function CTemplate( $TemplateFile = '' )
{
  parent::CObject();
  
  if( $TemplateFile != '' )
  {
    $this->templ_file_name = $this->b.$TemplateFile;
    $this->Load();
  }
}

function SetText( $Text )
{
  $this->text = $Text;
}


function Replace( $ParamName, $ParamValue )
{
  TextReplace(  $ParamName, $ParamValue, $this->text );
}

function UnHTML()
{
  $this->Replace( '&apos;', "'" );
  $this->Replace( '&amp;', '&' );
  $this->Replace( '&lt;', '<' );
  $this->Replace( '&gt;', '>' );
}

function ReplaceBtwMarkers( $MarkerStart, $MarkerStop, $Replacer )
{
  $this->text = preg_replace( '/'.$MarkerStart.'.+?'.$MarkerStop.'/ims', $Replacer,  $this->text );
}

function ReplaceWithRegExp( $Expression, $Replacer )
{
  $this->text = preg_replace( '/'.$Expression.'/i', $Replacer,  $this->text );
}


function Prnt()
{
  print $this->text;
}

function Load()
{
  $marker = $this->markerA.$this->markerB;
  $file = @fopen( $this->templ_file_name, "rb" ) or die( "Failed to open template file ".$this->templ_file_name );
  $this->text = fread( $file, filesize( $this->templ_file_name) ) or die( "Failed to read template file ".$this->templ_file_name );

  $MarkerExpression = '/'.$marker.'.*?^/ims';
  if( !preg_match( $MarkerExpression, $this->text ) )
    die( "The file '$this->templ_file_name' does not contain template marker '$marker'" );

  $this->text = preg_replace( $MarkerExpression, '', $this->text );


  fclose( $file );
}

function GetParam( $Name )
{
  
  $Result = $this->params[$Name];

  if( Result != '')
  {
    $NameQuoted = preg_quote( $Name, '/' );
    $Matches = array();
    $Expression = '/<'.$NameQuoted.'>(.+?)<\/>/i';

    if( preg_match( $Expression, $this->text, $Matches ) )
    {
      $Result = $Matches[1];
      $this->params[$Name] = $Result;
      $this->text = preg_replace( $Expression, '', $this->text );
    }
    else
    {

      //Another type of expression
      $Expression = '/'.$NameQuoted.'\=(.*?)$/ims';
      if( preg_match( $Expression, $this->text, $Matches ) )
      {
        $Result = $Matches[1];
        $this->params[$Name] = $Result;
        $Expression = '/'.$NameQuoted.'\=(.*?)^/ims';
        $this->text = preg_replace( $Expression, '', $this->text );
      }
    }


   }
   

   return $Result;
}


};



?>