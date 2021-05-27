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

  UmbPDFViewerController.$inject = ['$scope', '$element', 'assetsService'];

  function UmbPDFViewerController($scope, $element, assetsService) {

    const vm = this;
    const element = $element[0];

    vm.pdf = null;
    vm.pageNumber = 1;
    vm.totalPages = 0;
    vm.pageRendering = false;
    vm.pageNumberPending = null;

    vm.$onInit = onInit;
    vm.nextPage = nextPage;
    vm.prevPage = prevPage;

    function onInit() {

      assetsService.load(['lib/pdfjs-dist/build/pdf.min.js', 'lib/pdfjs-dist/build/pdf.worker.min.js'], $scope)
        .then(function () {

          // TODO: figure out how this works and implement
          // pdfjsLib.GlobalWorkerOptions.workerSrc = '//mozilla.github.io/pdf.js/build/pdf.worker.js';

          loadDocument();
        });
    }

    function nextPage () {
      if (vm.pageNumber >= vm.totalPages) {
        return;
      }

      vm.pageNumber = vm.pageNumber + 1;
      queueRenderPage(vm.pageNumber);
    }

    function prevPage() {
      if (vm.pageNumber <= 1) {
        return;
      }

      vm.pageNumber = vm.pageNumber - 1;
      queueRenderPage(vm.pageNumber);
    }

    function queueRenderPage (pageNumber) {
      if (vm.pageRendering) {
        vm.pageNumberPending = pageNumber;
      } else {
        renderPage(pageNumber);
      }
    }

    function renderPage(pageNumber) {
      vm.pageRendering = true;

      vm.pdf.getPage(pageNumber).then(function(page) {
        const desiredWidth = element.parentElement.clientWidth;
        const viewport = page.getViewport({ scale: 1 });
        const scale = desiredWidth / viewport.width;
        const scaledViewport = page.getViewport({ scale });

        const canvas = element.querySelector('.pdf-canvas');
        const context = canvas.getContext('2d');
        canvas.height = scaledViewport.height;
        canvas.width = scaledViewport.width;

        const renderContext = {
          canvasContext: context,
          viewport: scaledViewport
        };

        const renderTask = page.render(renderContext);

        renderTask.promise.then(function() {
          vm.pageRendering = false;

          if (vm.pageNumberPending !== null) {
            renderPage(vm.pageNumberPending);
            vm.pageNumberPending = null;
          }
        });
      });
    }

    function loadDocument () {
      try {
        const loadingTask = pdfjsLib.getDocument({url: vm.src });

        loadingTask.promise.then((pdf) => {
          vm.pdf = pdf;
          vm.totalPages = pdf.numPages;
          renderPage(vm.pageNumber);
          $scope.$applyAsync();
        });
      } catch (e) {
        console.log('error', e);
      }
    }
  }

})();
