//NOTE: I don't think this is used anywhere!

angular.module('umbraco.mocks.resources')
.factory('userResource', function () {

    var _currentUser, _authenticated = (jQuery.cookie('authed') === "authenticated");
    var _mockedU = {
        name: "Per Ploug",
        avatar: "assets/img/avatar.jpeg",
        id: 0,
        authenticated: true,
        locale: 'da-DK'
    };

    if (_authenticated) {
        _currentUser = _mockedU;
    }

    return {
        isAuthenticated: _authenticated,
        currentUser: _currentUser,

        authenticate: function (login, password) {
            _authenticated = true;
            _currentUser = _mockedU;

            jQuery.cookie('authed', "authenticated", { expires: 1 });
            return _authenticated;
        },

        logout: function () {
            $rootScope.$apply(function () {
                _authenticated = false;
                jQuery.cookie('authed', null);
                _currentUser = undefined;
            });
        },

        getCurrentUser: function () {
            return _currentUser;
        }
    };

});
