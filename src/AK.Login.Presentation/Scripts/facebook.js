/*******************************************************************************************************************************
 * AK.Login.Presentation.Scripts.facebook
 * Copyright © 2014 Aashish Koirala <http://aashishkoirala.github.io>
 * 
 * This file is part of AK-Login.
 *  
 * AK-Login is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 * 
 * AK-Login is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with AK-Login.  If not, see <http://www.gnu.org/licenses/>.
 * 
 *******************************************************************************************************************************/

var FB = null;

function checkLoginState() {
    FB.getLoginStatus(function (response) {
        if (response.status != 'connected') return;

        $('#facebookSignedRequest').val(response.authResponse.signedRequest);
        $('#facebookUserId').val(response.authResponse.userID);

        $('#facebookForm').submit();
    });
}

window.fbAsyncInit = function () {
    FB.init({
        appId: '227825953941981',
        xfbml: true,
        version: 'v2.1'
    });
};

(function (d, s, id) {
    var js, fjs = d.getElementsByTagName(s)[0];
    if (d.getElementById(id)) { return; }
    js = d.createElement(s); js.id = id;
    js.src = "//connect.facebook.net/en_US/sdk.js";
    fjs.parentNode.insertBefore(js, fjs);
}(document, 'script', 'facebook-jssdk'));