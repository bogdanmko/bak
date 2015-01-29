<?

define( "LOCALHOST", 0 );

define( "BUSINESS_DIVISION_ID", 7 );
define( "BUSINESS_DIVISION_NAME", "Pad2Pad" );

if( LOCALHOST )
{
define( "ROOT_URL", "http://pad2pad" );
define( "SSL_ROOT_URL", "http://pad2pad" );
define( "SSL_URL_PREFIX",  "" );
}
else
{
define( "ROOT_URL", "http://www.pad2pad.com" );
define( "SSL_ROOT_URL", "https://ssl38.pair.com" );
define( "SSL_URL_PREFIX",  "/users/pad2pad" );
}

define( "CC_KEY", 'ags!$&*gan102j-+ajhsndffka' );

$SalesTaxPercent = "7";

?>
