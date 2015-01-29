<?

include_once $b.'../lib/utils.php';
include_once $b.'../lib/template.php';

//----------------------------------------------------------------
//Base class for all forms CForm start
class CFormDataHolder extends CObject
{
  var $F = array();
  
  function CFormDataHolder()
  {
    parent::CObject();
	$this->SetDataFromGetAndPost();
  }
  
function SetDataFromGetAndPost()
{
  if( $this->got_data )
    return;

  foreach( $_GET as $Name => $Value )
  {
    $this->F[$Name] = Unquot( $Value );
  }
   
  foreach( $_POST as $Name => $Value )
  {
      $this->F[$Name] = Unquot( $Value );
  }

   $this->got_data = 1;
}
  
};


//----------------------------------------------------------------
class CForm extends CFormDataHolder
{
  var $Error = 0;
  var $Errors = array();
  var $CADInfo;
  var $FP = array();
  var $Printed = 0;
  var $Action = "";
  var $Cancelled = 0;
  var $JavaScriptFile = '';
  var $JavaScript = '';
  var $form_name = '';
  var $error_msgs = array();
  var $template_file_name = '';
  var $templ;
  var $autofield_names = array();
  var $ft_checkbox = 'checkbox';
  var $ft_text = 'text';
  var $ft_radio = 'radio';
  var $ft_combo = 'combo';
  var $got_data = 0;


//Define Field
function DF( $Name, $Type, $Auto=0, $Clean=0, $MaxChars=0 )
{
  $this->FP[$Name]['type'] = $Type;
  $this->FP[$Name]['auto'] = $Auto;
  $this->FP[$Name]['clean'] = $Clean;
  $this->FP[$Name]['maxchars'] = $MaxChars;
}

function GetFieldType( $Name )
{
  return $this->FP[$Name]['type'];
}

function ValidateFields()
{
   $this->SetAutoFieldsFromOldFieldsArray();

  foreach( $this->FP as $FieldName => $FieldDef )
  {
    $MaxChars = $FieldDef['maxchars'];
    if( $MaxChars == 0 )
      continue;
	  
	$this->F[$FieldName] = substr( $this->F[$FieldName], 0, $MaxChars );
	  
	
  }
   
}
  
function PrepareFormTemplate()
{
  $nested_template_files = $this->template_file_name.'|';
  
  
  
  //Get dat file names
  $NestedFilesArr = preg_split( "/\|/", $nested_template_files );
  
  $NestLevel = 0;
 
  
  foreach( $NestedFilesArr  as $TemplateFile )
  {
    if( strlen($TemplateFile) == 0 )
	  continue;
	  
	
    $templ = new CTemplate( $TemplateFile );
	if( $NestLevel == 0 ) //Main template
      $this->templ = $templ;
	else
	  $this->templ->Replace( '{CONTENT}', $templ->text );
	  
	 $NestLevel++;
  }
  
 
  $this->templ->Replace( '{form_name}', $this->form_name );
  $this->templ->Replace( '{form_action}', $this->Action );
  $this->templ->Replace( '{base}', $this->b );
  
}
  
function CForm( $Action = "" )
{
  
  parent::CFormDataHolder();

  $this->Action = $Action;
  
  $this->ValidateFields();
	
  if( $this->template_file_name )
    $this->PrepareFormTemplate();
  	
  $this->LoadJavaScript();	

  
}

function SetAutoFieldsFromOldFieldsArray()
{
  foreach( $this->autofield_names as $FieldName )
    $this->FP[$FieldName]['auto'] = 1;
}

function ClearFieldsToBeClean()
{
  $this->SetAutoFieldsFromOldFieldsArray();

  foreach( $this->FP as $FieldName => $FieldDef )
   if( $FieldDef['clean'] )
     $this->F[$FieldName] = '';  
}

function ProcessAutoFields()
{

  $this->SetAutoFieldsFromOldFieldsArray();


  foreach( $this->FP as $FieldName => $FieldDef )
  {
    if( !$FieldDef['auto'] )
      continue;

     $this->templ->Replace( '{'.$FieldName.'_error}', $this->FormatError( $this->Errors[$FieldName] ) );

     $this->templ->Replace( '{'.$FieldName.'_error_msg}', $this->error_msgs[$FieldName] );
	 
	 $this->templ->Replace( '{'.$FieldName.'_maxchars}', $FieldDef['maxchars'] );
	 

    if( $FieldDef['type'] == $this->ft_checkbox )
    {
      $Checked = '';
      if( $this->F[$FieldName] )
        $Checked = 'checked';
      $this->templ->Replace('{'.$FieldName.'_checked}', $Checked );
    }
    else if( $FieldDef['type'] == $this->ft_radio )
    {
      $this->templ->Replace('{'.$FieldName.'_'.$this->F[$FieldName].'_checked}', 'checked' );
      $this->templ->ReplaceWithRegExp( '{'.$FieldName.'_.+?_checked}', '' );
   
    }
    else 
      $this->templ->Replace('{'.$FieldName.'_value}', HTML($this->F[$FieldName]) );

	
  }
}


function PrintCheckBox($Name, $Checked, $Caption, $ExtraParams='' )
{
  $this->PrintErrorIfAny( $Name );

  print "<input type='checkbox' name='$Name' $Checked $ExtraParams > $Caption\n";
}


function PrintJavaScript()
{
  print $this->JavaScript;
}

function ReplaceJavaScriptParam( $ParamName, $Value )
{
  if( $this->templ ) //If template - javascript is built-into template
    $this->templ->Replace( $ParamName, $Value );
  else
    $this->ReplaceParam( $ParamName, $Value, $this->JavaScript );
}

function ReplaceParam( $ParamName, $Value, &$Text )
{
  $ParamName = preg_quote( $ParamName, '/' );
  $Text = preg_replace( '/'.$ParamName.'/i', $Value , $Text );
}



function GenerateSaveLoadCookieJSCode( $FieldsInvolved )
{
  $SaveCookie = '';
  $LoadCookie  = '';
  foreach( $FieldsInvolved as $Field )
  {
    $SaveCookie .= "  SetCookie( '$Field', form.$Field.value );\n";
	if( ! $this->IsSubmitted() )
	  $LoadCookie .= "  form.$Field.value = GetCookie( '$Field' );\n";
  }
  
  $this->ReplaceJavaScriptParam( '///LOAD_FROM_COOKIE_CODE', $LoadCookie);
  $this->ReplaceJavaScriptParam( '///SAVE_TO_COOKIE_CODE', $SaveCookie );
  
}

function LoadJavaScript()
{
  global $b;
  if( $this->JavaScriptFile == '' )
    return;

  $this->JavaScript = $this->GetJavaScriptFromFile( $this->JavaScriptFile );
  
  $this->ReplaceJavaScriptParam( '_base_', $b );
  $this->ReplaceJavaScriptParam( '<base>', $b );
  $this->ReplaceJavaScriptParam( '_formname_', $this->form_name );
 
  
}

function GetJavaScriptFromFile( $filename )
{
  $JavaScriptHandle = fopen( $filename, "r" );
  
  if( !$JavaScriptHandle )
  {
    $this->PrintError( "Error reading file: $JSFile" );
	return;
  }
  
  $Size = filesize( $filename );
  $JavaScriptText = fread( $JavaScriptHandle , $Size );
  fclose( $JavaScriptHandle );
  
     
  return $JavaScriptText;
  

}




function CheckIfEmpty( $FieldName, $ErrorMessage = "" )
{
  if( IsEmpty( $this->F[$FieldName] ) )
    $this->SetError( $FieldName, $ErrorMessage );
}

  
function SetError( $Context, $Message = "" )
{
  if( $Message == "" )
    $Message = $this->error_msgs[$Context];
  $this->Errors[$Context] = $Message;
  $this->Error = 1;
}

function IsError()
{
  return $this->Error > 0;
}

function IsSubmitted()
{
  $ClassName = get_class( $this );
  print "$ClassName: Method IsSubmitted() must be overrided.";
}

function Check()
{
}


function Prnt()
{
  print "Method Prnt() must be overriden";
}

function Process()
{
  print "Method Process() must be overriden";
}

function FormatError( $Message )
{
  if( $Message )
    return "<span style='color:red;font-weight:bold'>$Message</span><br>";
  else
    return "";
}

function PrintError( $Message )
{
  print $this->FormatError( $Message );
}

function GetErrorIfAny($Context )
{
   if( $this->Errors[$Context] )
     return  $this->FormatError( $this->Errors[$Context] );
   else
     return '';
}

function PrintErrorIfAny( $Context )
{
   if( $this->Errors[$Context] )
     $this->PrintError( $this->Errors[$Context] );
}

function GetAllErrors()
{
  $result = '';
  foreach( $this->Errors as $FieldID => $Message );
    $result = $result."$FieldID - $Message; ";

  return $result;
    
}


function Execute()
{

if( $this->Cancelled )
  return;
  
if( $this->IsSubmitted() )
{
  $this->Check();
  if(  $this->IsError() )
    $this->Prnt();
  else
    $this->Process();
}
else 
  $this->Prnt();

}

function XMLProp( $XMLPropertyName, $FormFieldName )
{
  return ' '.$XMLPropertyName.'="'.$this->F[$FormFieldName].'"';
}

};
//----------------------------------------------------------------
// CForm end
//----------------------------------------------------------------

//----------------------------------------------------------------
// CFormWithCC begin
//----------------------------------------------------------------


class CFormWithCC extends CForm
{

var $f_cctype = 'CCType';
var $f_ccnumber = 'CCNumber';
var $f_ccexpiremonth = 'CCExpireMonth';
var $f_ccexpireyear = 'CCExpireYear';
var $CCTypes = array( 'Visa' => 'Visa', 'MasterCard' => 'MasterCard', 'Amex' => 'Amex', 'Discover' => 'Discover' );


function CheckCreditCard( $CCTypeFieldName, $CCNumberFieldName, $CCExpireMonthFieldName, $CCExpireYearFieldName )
{
  $this->F[$CCNumberFieldName] = preg_replace( '/\s/', '', $this->F[$CCNumberFieldName] );
  $this->CheckIfEmpty( $CCTypeFieldName, "Please sselect your credit card type" );
  $this->CheckIfEmpty( $CCNumberFieldName, "Please enter your credit card number" );
  
  if(  $this->F[$CCNumberFieldName] != "" )
  {
    if( ! CCNumberIsOK( $this->F[$CCNumberFieldName] ) )
	  $this->SetError( $CCNumberFieldName, "The credit card number is invalid" );
  }

  $Date = getdate();  
  
  $ExpireMonth = $this->F[$CCExpireMonthFieldName];
  $ExpireYear =$this->F[$CCExpireYearFieldName];
  
  $Expired = CheckForExpiration(  $ExpireMonth, $ExpireYear );
  $ExpiresVerySoon = 0;
  
  
  
  //Subt a month to expiration date to check how soon will expire
  if( ! $Expired )
  {
    if( $ExpireMonth == 1 )
    {
      $ExpireYear--;
      $ExpireMonth = 12;
    }
    else
      $ExpireMonth--;
	  
	$ExpiresVerySoon  = CheckForExpiration( $ExpireMonth, $ExpireYear );
   }
   
   if( $Expired )
     $this->SetError( $CCExpireMonthFieldName, "Your credit card has expired" );
   else if( $ExpiresVerySoon )
     $this->SetError( $CCExpireMonthFieldName, "Your credit card will expire very soon" );


}

function GetCCTypeRadios()
{

  $Result = '';
  foreach( $this->CCTypes as $CCTypeID => $CCTypeName )
  {
   $Checked = '';
   if( $this->F[$this->f_cctype] == $CCTypeID )
     $Checked = ' checked ';
    $Result .= "<INPUT type='radio' name='$this->f_cctype' value='$CCTypeID' $Checked> $CCTypeName &nbsp\n";
  }

  return $Result;

}


function GetCCExpirationYearsComboBox()
{

  $Result = "\n<SELECT Name='$this->f_ccexpireyear'>\n";
             
  $CCYears = GetYearsFromCurrent( 10 );

  foreach( $CCYears as $Year )
  {
    $Checked = '';
    if( $Year == $this->F[$this->f_ccexpireyear] )
      $Checked = ' selected ';
      $Result .= "  <OPTION $Checked> $Year\n";
  }

  $Result .= "</SELECT>\n\n";

  return $Result;

}


function GetCCExpirationMonthsComboBox()
{

  $Result = "\n<SELECT Name='$this->f_ccexpiremonth'>\n";

             
  $CCMonths = array(1,2,3,4,5,6,7,8,9,10,11,12);

  foreach( $CCMonths as $Month )
  {
    $Checked = '';
    if( $Month == $this->F[$this->f_ccexpiremonth] )
      $Checked = ' selected ';
      $Result .= "  <OPTION $Checked> $Month\n";
  }

  $Result .= "</SELECT>\n\n";

  return $Result;

}


  
};

//----------------------------------------------------------------
// CFormWithCC end
//----------------------------------------------------------------



//----------------------------------------------------------------
// CMiclogForm - micrologic general form
//----------------------------------------------------------------
class CMiclogForm extends CFormWithCC
{

  var $ExtraParamInfo;
  var $ContactURL;
  var $ParamFileLoaded = 0;

function CMiclogForm( $Action = '')
{
  global $ContactURL;
  $this->ContactURL = $ContactURL;
  
  parent::CForm( $Action );
//  $this->LoadParamFile(); 
}

function ComposeContactURL( $URLText )
{
  if( $this->ContactURL == "" )
    return $URLText;
  else
    return "<a href='$this->ContactURL'>$URLText</a>";
}


function GetInternalErrorSolutionText()
{
  return "Try again later or ".$this->ComposeContactURL( "contact" ). ' '. BUSINESS_DIVISION_NAME. ' with the problem';
}


function SetExtraParamInfo( $ExtraParamInfo )
{
  $this->ExtraParamInfo = $ExtraParamInfo;
}

function ParsePriceLine( $PriceLine, &$LeadTime, &$Price, &$AssemblyPrice, &$PriceTimeComboID, &$NotExactPrice )
{
  $Matches = array();
  if( preg_match( '/^(.+):(.+?)\+(.+?)\((.+)\)(.?)/i', $PriceLine, $Matches ) )  
  {
     $Price = $Matches[2];
     $AssemblyPrice = $Matches[3];
     $LeadTime = $Matches[1];
     $PriceTimeComboID = $Matches[4];
     $NotExactPrice = $Matches[5];
     return 1;
  }
  else
    return 0;
}

function GetCADParam( $Name )
{
  $this->LoadParamFile(); 

  $Matches = array();
  $RegularExpr = "/$Name{(.*?)}/is";
  if( preg_match( $RegularExpr, $this->CADInfo, $Matches ) )
    return $Matches[1];
  else
    return "";
}

//Retrieves form field parameters
function GetParamInfo( $Name, &$SelectItems, &$UnitOfMeasure )
{
  $this->LoadParamFile(); 

  $Matches = array();
  $SelectItems = array();
  $RegularExpr = "/$Name{(.*?)}{(.*?)}/is";

  
  if( ! preg_match( $RegularExpr, $this->CADInfo, $Matches ) )
    preg_match( $RegularExpr, $this->ExtraParamInfo, $Matches ) or die( "<span style='color:red;font-weight:bold'>***Param $Name not found in neither CAD info file and params config $this->ExtraParamInfo </span>"  );
  
  $SelectData = $Matches[1];
  $SelectItems = preg_split( "/\|/", $SelectData );
  $UnitOfMeasure = $Matches[2];
  
}

function GetTimeWhenSiteRestores()
{
  $DownTimes = array();
  $this->GetParamInfo( 'ServerDownTime', $DownTimes, $Dummy );
  date_default_timezone_set( "America/New_York" );
  $Time = mktime();
  
  $Result = '';

  
  
  foreach( $DownTimes as $TimeDown )
  {
    $Matches = array();
	if( preg_match( '/^(.+?):(.+?)-(.+?):(.+)/i', $TimeDown, $Matches ) )
	{
	  $StartHour = $Matches[1];
	  $StartMin = $Matches[2];
	  $Start = mktime( $StartHour, $StartMin );
	  $StopHour = $Matches[3];
	  $StopMin = $Matches[4];
	  $Stop = mktime( $StopHour, $StopMin );
	  
//	  $Result .= "$StartHour:$StartMin($Start) - $StopHour:$StopMin($Stop) == $Time<br>";
	  
	  
	  
	  if( $Time >= $Start && $Time <= $Stop )
	    return "$StopHour:$StopMin EST";
	}
    
  }
  
  return '';
  
  
}

function GetParamDisplayValue( $Name, $Value )
{
  $SelectItems = array();
  $Units = '';
  $this->GetParamInfo( $Name, $SelectItems, $Units );
  $Default = $Value;

  foreach( $SelectItems as $Item )
  {
    $ValueAndDisp = array();
    if( preg_match( '/(\*?)(.+?):(.+)/', $Item, $ValueAndDisp ) )
    {
	  if( $ValueAndDisp[1] != '' )
	    $Default =  $ValueAndDisp[3];
      if( $ValueAndDisp[2] == $Value  )
        return $ValueAndDisp[3];
    }
  }

  return $Default;


}

//Translates ON/OFF state of checkbox to values defined in the field info data file
function TranslateCheckBox( $Name )
{
  $Statuses = array();
  $UnitOfMeasure;
  $this->GetParamInfo( $Name, $Statuses, $UnitOfMeasure );
  
  if( count( $Statuses ) < 2 )
    return;

  $Result;	
  
  if( preg_match( "/^on$/i", $this->F[$Name] ) )
    $Result = $Statuses[1];
  else
    $Result = $Statuses[0];
	
  $Matches = array();
  if( preg_match( "/^\*(.+)$/i", $Result, $Matches ) )
    $Result = $Matches[1];
	
return $Result;
	
}

function PrintParamCheckBox($Prefix, $Name, $Suffix, $ExtraHTMLParams='' )
{
  $Statuses = array();
  $UnitOfMeasure;
  $this->GetParamInfo( $Name, $Statuses, $UnitOfMeasure );
  
  $Value = $this->F[$Name];
   
  print $Prefix;
//  $this->PrintErrorIfAny( $Name );
  if( ! $this->IsSubmitted() )
  {
   $Matches = array();
   if( preg_match( "/^\*(.+)$/", $Statuses[0], $Matches ) )
      $Value = "";
	else
	  $Value = "on";
  }
  
  $Checked = (preg_match( "/^ON$/i", $Value, $Matches )) ? "checked" : "";
  
  $this->PrintCheckBox( $Name, $Checked, "$UnitOfMeasure $Suffix\n", $ExtraHTMLParams );
//  print "<input type='checkbox' name='$Name' $Checked> $UnitOfMeasure$Suffix\n";
}

function PrintEditBox($Prefix, $Name, $Suffix, $Size )
{
  $Default = array();
  $UnitOfMeasure;
  $this->GetParamInfo( $Name, $Default, $UnitOfMeasure );
  
  if( $this->IsSubmitted() || (DEBUG_MODE && $this->F[$Name] != "") )
    $Value = $this->F[$Name];
  else
    $Value = $Default[0];
  
 
  print $Prefix;
  $this->PrintErrorIfAny( $Name );
  print "<input type='text' name='$Name' size='$Size' value='".HTML($Value)."'> $UnitOfMeasure$Suffix\n";
}

function GetParamRadioGroup($Prefix, $Name, $Suffix, $AfterEachRadioAdd="<br>" )
{
  return $this->GetParamComboBoxHTML($Prefix, $Name, $Suffix, 1, $AfterEachRadioAdd, '' );
}

function PrintParamRadioGroup($Prefix, $Name, $Suffix, $AfterEachRadioAdd="<br>" )
{
  $this->PrintParamComboBox($Prefix, $Name, $Suffix, 1, $AfterEachRadioAdd );
}

function PrintHidden( $Name )
{
  print "<input type='hidden' name='$Name' value='".HTML($this->F[$Name])."'>\n";
}



function GetParamText( $Prefix, $Name )
{
  $Value = $this->F[$Name];
  $SelectItems = array();
  $UnitOfMeasure;
  $Display = $Value;
  $this->GetParamInfo( $Name, $SelectItems, $UnitOfMeasure );
  $QuotedValue = preg_quote( $Value, '/' );
  $nCount = 0;
  $FirstItem = "";
  $SecondItem = "";
  foreach( $SelectItems as $Item )
  {
       
	  $nCount++;
	  
	  if( $nCount == 1 )
	    $FirstItem = $Item;

	  if( $nCount == 2 )
	    $SecondItem = $Item;
		
	 if( ! preg_match( '/^\*?(.+):(.+)$/', $Item, $Matches ) )
	    continue;
		
	  $ItemValue = $Matches[1];
	  $Caption = $Matches[2];
	  
      if( preg_match( "/^$QuotedValue$/", $ItemValue ) )
	  {
	    $Display = $Caption;
		break;
	  }
	  

	
  }
  
  $CheckBoxRegExpr = '/^\*?(0|1)$/';
  if( $nCount == 2 && preg_match( $CheckBoxRegExpr, $FirstItem ) && preg_match( $CheckBoxRegExpr, $FirstItem ) ) //This is a checkbox
    if( preg_match( "/^on$/i", $Value ) )  $Display = "YES"; else $Display = "NO";

  return "$Prefix $Display $UnitOfMeasure";
  
}

function PrintParamText($Prefix, $Name, $PrintHiddenOnly = 0 )
{
  $this->PrintHidden( $Name );
  
  if( ! $PrintHiddenOnly )
    print GetParamText( $Prefix, $Name )."<br>\n";
  
}

function GetParamComboBoxHTML( $Prefix, $Name, $Suffix, $RadioStyle=0, $AfterEachRadioAdd="<br>", $ExtraHTMLParams='' )
{
//Load select from CADInfo file
//  global $CADInfo;
  
  $Matches = array();
  $SelectItems = array();
  $UnitOfMeasure;
  
  $this->GetParamInfo( $Name, $SelectItems, $UnitOfMeasure );
  
  $SelectMark = $RadioStyle ? " checked" : " selected";
 

  $Result = $Prefix;


  
  $Result .= $this->GetErrorIfAny( $Name );
  if( ! $RadioStyle )
    $Result .= "<SELECT Name='$Name' $ExtraHTMLParams>\n";
  
  $Value = $this->F[$Name];
  $First = 1;
  $Selected = "";
  
  foreach( $SelectItems as $Item )
  {
    preg_match( "/^(.+):(.+)$/", $Item, $Matches );
	$ItemValue = $Matches[1];
	$Caption = $Matches[2];
	$Selected = "";
	if( preg_match( "/^\*(.+)$/", $ItemValue, $Matches ) )
	{
	  if( ! $this->IsSubmitted() )
		  $Selected = $SelectMark ;
	  $ItemValue = $Matches[1];
	}
	
//    if( $First && $Value == "" && $RadioStyle  )
//	  $Selected = $SelectMark;
	if( $this->IsSubmitted() || DEBUG_MODE )
	{
	    $QuotedValue = preg_quote( $Value, '/' );
	    if( preg_match( "/^$QuotedValue$/", $ItemValue ) )
	      $Selected = $SelectMark;
	}
	
	$ControlStart = $RadioStyle ? "<INPUT type='radio' name='$Name' " : "  <OPTION";
	$ControlEnd = ($RadioStyle)? $AfterEachRadioAdd : "";
	
	
	if( !$RadioStyle && preg_match( '/^'. preg_quote($ItemValue, '/').'$/', $Caption ) )
	  $Result .= "$ControlStart $Selected > $Caption $ontrolEnd\n";
	else
	  $Result .= "  $ControlStart value='$ItemValue' $Selected> $Caption $ControlEnd\n";
	  
	$First = 0;
  }
  
  if( ! $RadioStyle )
    $Result .= "</SELECT>";
	
  $Result .= "$UnitOfMeasure\n";
  
  $Result .= $Suffix;
  
  return $Result;
  
}


function PrintParamComboBox($Prefix, $Name, $Suffix, $RadioStyle=0, $AfterEachRadioAdd="<br>", $ExtraHTMLParams='' )
{
  print $this->GetParamComboBoxHTML($Prefix, $Name, $Suffix, $RadioStyle, $AfterEachRadioAdd, $ExtraHTMLParams );
}

//Load definition file - the file contains form field parameters. The file is normally generated by CAD application
function LoadParamFile()
{
  global $CADInfoFileName;
  
  if(  $this->ParamFileLoaded )
    return;
  
  
  $InfoFileNamesString = $CADInfoFileName.'|';
  
  //Get dat file names
  $InfoFiles = preg_split( "/\|/", $InfoFileNamesString );
  $this->CADInfo = '';
  
  foreach( $InfoFiles  as $InfoFile )
  {
    if( strlen($InfoFile) == 0 )
	  continue;
	  
    $file = fopen( $InfoFile , "rb" ) or die( "Failed to open CAD info file ". $InfoFile  );
    $this->CADInfo .= fread( $file, filesize( $InfoFile) ) or die( "Failed to read CAD info file ".$InfoFile );
	$this->CADInfo .= "\n";
    fclose( $file );
  }

  $this->ParamFileLoaded = 1;
}

function PrepareShippingsForExtraParams( $ShippingsLine )
{
  $ParamInfo = '';
  $Shippings = preg_split( "/\|/", $ShippingsLine );
  
//Shipping  
  foreach( $Shippings as $ShipMethod )
  {
     $Matches = array();
     if( preg_match( '/^(.+?):(.+)\((\d+)\)$/i', $ShipMethod, $Matches ) )
	 {
	  if( $ParamInfo != "" )
	    $ParamInfo .= "|";
	  $ParamInfo .= sprintf( '%d:$%.2f - %s', $Matches[3], $Matches[2], $Matches[1] );	
	 }
  }

  return $ParamInfo;
 }

};

//----------------------------------------------------------------
// CMiclogForm END
//----------------------------------------------------------------


?>