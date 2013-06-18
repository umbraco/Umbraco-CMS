'use strict';
//this controller simply tells the dialogs service to open a mediaPicker window
//with a specified callback, this callback will receive an object with a selection on it
angular.module("umbraco").controller("Umbraco.Editors.GridController", 
  function($rootScope, $scope, dialogService, $log){
    //we most likely will need some iframe-motherpage interop here
    
    //we most likely will need some iframe-motherpage interop here
       $scope.openMediaPicker =function(){
               var d = dialogService.mediaPicker({scope: $scope, callback: renderImages});
       };

       $scope.openPropertyDialog =function(){
               var d = dialogService.property({scope: $scope, callback: renderProperty});
       };

       $scope.openMacroDialog =function(){
               var d = dialogService.macroPicker({scope: $scope, callback: renderMacro});
       };

       function renderProperty(data){
          $scope.currentElement.html("<h1>boom, property!</h1>"); 
       }

       function renderMacro(data){
       //   $scope.currentElement.html( macroFactory.renderMacro(data.macro, -1) ); 
       }
      
       function renderImages(data){
           var list = $("<ul class='thumbnails'></ul>")
           $.each(data.selection, function(i, image) {
               list.append( $("<li class='span2'><img class='thumbnail' src='" + image.src + "'></li>") );
           });

           $scope.currentElement.html( list[0].outerHTML); 
       }

       $(window).bind("umbraco.grid.click", function(event){

           $scope.$apply(function () {
               $scope.currentEditor = event.editor;
               $scope.currentElement = $(event.element);

               if(event.editor == "macro")
                   $scope.openMacroDialog();
               else if(event.editor == "image")
                   $scope.openMediaPicker();
               else
                   $scope.propertyDialog();
           });
       })
});