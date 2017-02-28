function cmdDisplayBanks_onclick() {

    var zip = $("input[@name=txtZipCode]").val();
    var rad = $("#cboRadius option[@selected]").text();
    
    $.blockUI('<h3><img src="images/busy.gif" /> Finding banks within ' + rad + ' mile(s) of zipcode ' + zip + '</h3>');

    $("#side_bar").empty();

    $.get('BankFinder.svc/GetBanksForZipcode',
      {zipcode: zip, radius: rad},
      OnSuccess
      );
}

var markersArray = [];
var infowindow = new google.maps.InfoWindow();

function clearOverlays() {
    for (var i = 0; i < markersArray.length; i++) {
        markersArray[i].setMap(null);
    }
    markersArray.length = 0;
}

function createMarker(point, displayinfo, bankName) {

    var marker = new google.maps.Marker({
        position: point,
        title: bankName,
        visible: true
    });

    marker.addListener('click', function () {
        infowindow.setContent(displayinfo);
        infowindow.open(map, marker);
    });

    markersArray.push(marker);
    return marker;
}

function OnSuccess(transport){
    var sideBarHtml ;
    var lastBankName;
    var banks = JSON.parse(transport)["d"];

    if(banks.length > 0)
    {    
        if (markersArray.length > 0)
            { clearOverlays(); }
        
        var bounds = new google.maps.LatLngBounds();

        sideBarHtml = '';
        for (var i=0;i<banks.length;i++)
        {
          var latitude    = banks[i].Latitude;
          var longitude   = banks[i].Longitude;
          var fulladdress = banks[i].FullAddress;
          var bankname    = banks[i].Name;
          var point = new google.maps.LatLng(latitude, longitude);
          var marker = createMarker(point, "<b>" + bankname + "</b><br>" + fulladdress, bankname );
          marker.setMap(map);
          
          if (lastBankName != bankname)
          {
            if (i>0)
                {sideBarHtml += '</ul>';}

            sideBarHtml += '<ul><b>' + bankname + '</b><li><a href="javascript:ShowMarker('+ i + ')">' + fulladdress + '</a></li>';
          }
          else
          {
            sideBarHtml += '<li><a href="javascript:ShowMarker('+ i + ')">' + fulladdress + '</a></li>';
          }

          lastBankName = bankname;
          
          bounds.extend(point);
        }
        $("#side_bar").append(sideBarHtml);
        map.fitBounds(bounds);
        
        $("ul").Treeview({ speed: "fast", collapsed: true});    
    }
    else
    {
        alert('No banks found!');
    } 
   
    /*
    //Monitor the window resize event and let the map know when it occurs  
    if (window.attachEvent) 
        { window.attachEvent("onresize", function() {this.map.onResize()} ); } 
    else 
        {  window.addEventListener("resize", function() {this.map.onResize()} , false);  }
    */
}

function ShowMarker(markerid) {
    google.maps.event.trigger(markersArray[markerid], 'click');
}


