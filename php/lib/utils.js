var Digits =  "0123456789";

var Message = new Array();
var MsgParams = new Array();

function ShowMessage( MsgID )
{
  var Msg =  new String( Message[MsgID] );
  
  
  for( var MsgParam in MsgParams )
  {
    var regExp = new RegExp(MsgParam, "ig");
    Msg = Msg.replace( regExp, MsgParams[MsgParam] );
  }

  alert( Msg );
}


function FormatOk(strAllowedChars, strValue)
{
  var checkOK = strAllowedChars;
  var checkStr = strValue;
  var allValid = true;
  for (i = 0;  i < checkStr.length;  i++)
  {
    ch = checkStr.charAt(i);
    for (j = 0;  j < checkOK.length;  j++)
      if (ch == checkOK.charAt(j))
        break;
    if (j == checkOK.length)
    {
      allValid = false;
      break;
    }
  }
  return allValid;
}

function GetTextBlock( strCheck, BlockNum, strDivider )
{
  var nPos = 0;
  var Result = "";
  while(  nPos <  strCheck.length )
  {
     var ch =   strCheck.charAt(nPos);
     if( ch == strDivider )
     {
       BlockNum--;
       if( BlockNum == 0 )
         return Result;
       else
         Result = "";
     }
     else
       Result += ch;
     nPos++;
  }
  if( BlockNum == 1 )
    return Result;
  else
    return "";
}

function CheckDate( strDate )
{
  var Month = GetTextBlock(strDate, 1, "/");
  var Day =  GetTextBlock(strDate, 2, "/");
  var Year =  GetTextBlock(strDate, 3, "/");

  if( Month == "" || Day == "" || Year == "" )
    return (false);

  if( !FormatOk( Digits, Month ) || !FormatOk( Digits, Day) || !FormatOk(Digits, Year) )
    return (false);

  if( Month < 1 || Month > 12 || Day < 1 || Day > 31 || Year < 2005 )
    return (false);

  return (true);

}

function Trim( str )
{
 return str.replace(/(^\s+|\s+$)/g, '');
}



//--- Encode decode object  -----

var sp = '|';
var ech = '\\';


function Escape( str )
{
  var Result = '';
  st = new String( str );
  var i = 0;
  while( i < st.length )
  {
    if( st.charAt(i) == sp )
      Result += ech + sp;
    else if( st.charAt(i) == ech )
      Result += ech + ech;
    else 
      Result += st.charAt(i);
    i++;
  }  
  return Result;  
}

function UnEscape( str )
{
  var Result = '';
  var i = 0;
  var chPrior = '';
  while( i < str.length )
  {
    if( ! ( str.charAt(i) == ech && i < str.length-1 && chPrior != ech ) )
      Result += str.charAt(i);
    chPrior = str.charAt(i);
    i++;
    
  }
  return Result;
}


function EncodeElements()
{
  this.encoded_str = "";
  var sep = '';
  for( i=0; i < this.elements_count; i++ )
  {
   
   this.encoded_str += sep + Escape( this.elements[i] );
   sep = sp;
  }
}

function DecodeElements()
{
  var i = 0;
  var charPrior = '';
  var CurrElem = '';
  var j = -1;
  while( i < this.encoded_str.length )
  {
    var charThis = this.encoded_str.charAt(i);
    if( charThis == sp && charPrior != ech )
    {
      j++;
      this.elements[j] = UnEscape(CurrElem);
      CurrElem = '';
    }
    else
      CurrElem += charThis;

   charPrior = charThis;

   i++; 


  }

  j++;
  this.elements[j] = UnEscape(CurrElem);
  

  this.elements_count = j+1;
  if( this.elements_count > this.maxelements_count )
    this.maxelements_count = this.elements_count;
  else  
    this.elements_count = this.maxelements_count;

}

function PushElem( str )
{
  this.elements[this.elements_count] = str;
  this.elements_count++;
}

function PopElem()
{
  this.elements_count--;
  return this.elements[this.elements_count];
}


function strencoderdecoder(maxelements_count)
{
  this.encoded_str = "";
  this.elements = new Array( maxelements_count );
  for( i=0; i<maxelements_count; i++ ) this.elements[i] = '';
  
  this.maxelements_count = maxelements_count;
  this.elements_count = 0;
  this.Encode = EncodeElements;
  this.Decode = DecodeElements;
  this.Push = PushElem;
  this.Pop = PopElem;
}

       
function SetCookie(cname, value) 
{
  if (cname == "") return;
  var ExpDate = new Date();
  var FiveYears = new Date(75,1,1,1);
  ExpDate.setTime(ExpDate.getTime() + FiveYears.getTime());
  document.cookie = cname + "=" + escape(value) + ";Expires=" + ExpDate.toGMTString(); 
}

function GetCookie(cname) 
{
  if( document.cookie.length == 0 ) return "";
  
  var CharBeforeName;

  
  var nAttempts = 1;
  while( nAttempts <= 2 )
  {

    CharBeforeName = ( nAttempts == 1 )? ' ' : ';';
  
  
    var searched = CharBeforeName + cname + "=";
    var Cookie = CharBeforeName + document.cookie;
  
  
    var nstart = Cookie.indexOf(searched);
    if (nstart != -1)  
    {
      nstart = nstart+searched.length;
      var nend = Cookie.indexOf(";", nstart );
      if (nend == -1) nend = Cookie.length;
      var rv = unescape(Cookie.substring(nstart , nend));
      if (rv.length != "")  return rv; 
    }
    nAttempts++;
    
   }

  return "";
}
	
function IsCookieEnabled()
{
  SetCookie('MICLOG_TEST_ABSBS121212', 'test');
  return GetCookie('MICLOG_TEST_ABSBS121212') == 'test';
}

function CheckEmail( Email ) 
{
  return Email.indexOf ('@',0) != -1 &&
         Email.indexOf ('.',0) != -1 &&
         Email.length > 4;
}

function GetFormByName( formname )
{
  var i;
  var form;
  for(i=0; i< document.forms.length; i++ )
  {
    if( document.forms[i].name == formname )
      return document.forms[i];
  }
  
  return 0;
}


function trim(string)
{
  return string.replace(/(^\s+)|(\s+$)/g, "");
}


function GetFormElementByName(form,  ElementName )
{
 var Element;
 for(  i=0; i < form.elements.length; i++ )
   if( form.elements[i].name == ElementName )
   {
     Element = form.elements[i];
     break;
   }

 return Element;

}

function IsRadioButtonChecked( Radios )
{
  for( i=0; i< Radios.length; i++ )
    if( Radios[i].checked )
	  return true;
	  
  return false;
}

function IsMax( oTextArea )
{
   return oTextArea.value.length > oTextArea.getAttribute('maxlength');
}


function DisableSubmitButtons( form, disable )
{
for(  i=0; i < form.elements.length; i++ )
   if( form.elements[i].type == 'submit' || form.elements[i].type == 'reset' )
     form.elements[i].disabled = disable;
}
function IsNumber( Number )
{
  return ! isNaN(Number * 1);
}

function atof( val )
{
  val = Trim( val );
  if( val == '' )
    return 0;
	
  if( isNaN(val*1) )
    return 0;

  return parseFloat( val );
}

function atoi( val )
{
  val = Trim( val );
  if( val == '' )
    return 0;
	
  if( isNaN(val*1) )
    return 0;

  return parseInt( val );
}


//--- END OF Encode decode object  -----



