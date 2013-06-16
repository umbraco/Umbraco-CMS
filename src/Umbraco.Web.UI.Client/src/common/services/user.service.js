angular.module('umbraco.services')
.factory('userService', function (authResource, $q) {

  var _currentUser,_authenticated = (jQuery.cookie('UMB_UCONTEXT') != "");       

  //var _mockedU = {
  //  name: "Per Ploug", 
  //  avatar: "assets/img/avatar.jpeg", 
  //  id: 0,
  //  authenticated: true,
  //  locale: 'da-DK' 
  //};

  //if(_authenticated){
  //  _currentUser = _mockedU; 
  //}

  return {
    authenticated: _authenticated,
    currentUser: _currentUser,
    
    authenticate: function(login, password) {

        var deferred = $q.defer();

        authResource.performLogin(login, password)
            .then(function (data) {
                _currentUser = data;
                _authenticated = true;
                deferred.resolve({user: data, authenticated: true});                
            }, 
            function (reason) {
                deferred.reject(reason);
                _authenticated = false;                
            });

        return deferred;
    },
    
    logout: function(){
      $rootScope.$apply(function() {
        _authenticated = false;
        jQuery.cookie('authed', null);
        _currentUser = undefined;
      });
    },

    getCurrentUser: function(){
      return _currentUser;
    }
  };
  
});
