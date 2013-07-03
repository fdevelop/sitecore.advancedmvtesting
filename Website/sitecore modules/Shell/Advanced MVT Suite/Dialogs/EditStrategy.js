function updateResultValue() {
  var s = '{'; 
  jQuery('input[id^=\"variant_\"]').each(
    function() {
      s += '\"' + $(this).id + '\": \"' + $(this).value + '\",' 
    }
  );
  s = s.substring(0,s.length-1);
  s += '}';
  
  $('result').setValue(s);
}