(function () {
  'use strict';

  angular
    .module('umbraco.directives')
    .component('umbPdfViewer', {
      templateUrl: 'views/components/media/umbpdfviewer/umb-pdf-viewer.html',
      controller: UmbPDFViewerController,
      controllerAs: 'vm',
      bindings: {
        src: "<"
      }
    });

  UmbPDFViewerController.$inject = ['$scope', 'assetsService'];

  function UmbPDFViewerController($scope, assetsService) {

    var vm = this;

    vm.pdf = null;
    vm.$onInit = onInit;

    function onInit() {
      assetsService.load(['lib/pdfjs-dist/build/pdf.min.js', 'lib/pdfjs-dist/build/pdf.worker.min.js'], $scope)
        .then(function () {
          loadDocument();
        });
    }

    function loadDocument () {
      try {
        const loadingTask = pdfjsLib.getDocument({url: vm.src });

        loadingTask.promise.then((pdf) => {
          vm.pdf = pdf;

          let pageNumber = 1;

          pdf.getPage(pageNumber).then(function(page) {
            const desiredWidth = 500;
            const viewport = page.getViewport({ scale: 1 });
            const scale = desiredWidth / viewport.width;
            const scaledViewport = page.getViewport({ scale });

            const canvas = document.getElementById('pdfCanvas');
            const context = canvas.getContext('2d');
            canvas.height = scaledViewport.height;
            canvas.width = scaledViewport.width;

            const renderContext = {
              canvasContext: context,
              viewport: viewport
            };

            page.render(renderContext);
          });

        });
      } catch (e) {
        console.log('error', e);
      }
    }
  }

})();
