(function () {
  'use strict';

  function platformService() {
    const userAgentRules = [
      ['Aol', /AOLShield\/([0-9\._]+)/],
      ['Edge', /Edge\/([0-9\._]+)/],
      ['Edge-ios', /EdgiOS\/([0-9\._]+)/],
      ['Yandexbrowser', /YaBrowser\/([0-9\._]+)/],
      ['Kakaotalk', /KAKAOTALK\s([0-9\.]+)/],
      ['Samsung', /SamsungBrowser\/([0-9\.]+)/],
      ['Silk', /\bSilk\/([0-9._-]+)\b/],
      ['MiUI', /MiuiBrowser\/([0-9\.]+)$/],
      ['Beaker', /BeakerBrowser\/([0-9\.]+)/],
      ['Edge-chromium', /EdgA?\/([0-9\.]+)/],
      ['chromium-webview', /(?!Chrom.*OPR)wv\).*Chrom(?:e|ium)\/([0-9\.]+)(:?\s|$)/],
      ['Chrome', /(?!Chrom.*OPR)Chrom(?:e|ium)\/([0-9\.]+)(:?\s|$)/],
      ['PhantomJS', /PhantomJS\/([0-9\.]+)(:?\s|$)/],
      ['Crios', /CriOS\/([0-9\.]+)(:?\s|$)/],
      ['Firefox', /Firefox\/([0-9\.]+)(?:\s|$)/],
      ['FxiOS', /FxiOS\/([0-9\.]+)/],
      ['Opera-mini', /Opera Mini.*Version\/([0-9\.]+)/],
      ['Opera', /Opera\/([0-9\.]+)(?:\s|$)/],
      ['Opera', /OPR\/([0-9\.]+)(:?\s|$)/],
      ['IE', /Trident\/7\.0.*rv\:([0-9\.]+).*\).*Gecko$/],
      ['IE', /MSIE\s([0-9\.]+);.*Trident\/[4-7].0/],
      ['IE', /MSIE\s(7\.0)/],
      ['BB10', /BB10;\sTouch.*Version\/([0-9\.]+)/],
      ['Android', /Android\s([0-9\.]+)/],
      ['iOS', /Version\/([0-9\._]+).*Mobile.*Safari.*/],
      ['Safari', /Version\/([0-9\._]+).*Safari/],
      ['Facebook', /FB[AS]V\/([0-9\.]+)/],
      ['Instagram', /Instagram\s([0-9\.]+)/],
      ['iOS-webview', /AppleWebKit\/([0-9\.]+).*Mobile/],
      ['iOS-webview', /AppleWebKit\/([0-9\.]+).*Gecko\)$/],
      ['Curl', /^curl\/([0-9\.]+)$/]
    ];

    function isMac() {
      return navigator.platform.toUpperCase().indexOf('MAC') >= 0;
    }

    function getBrowserInfo(){
      let data = matchUserAgent(navigator.userAgent);
      console.log(data);
      if(data){
        const test = data[1];
        return {
          name : data[0],
          version : test[1]
        };
      }
      return null;
    }

    function matchUserAgent(ua) {
      return (ua !== '' && userAgentRules.reduce (
          (matched, [browser, regex]) => {
            if (matched) {
              return matched;
            }
            const uaMatch = regex.exec(ua);
            return !!uaMatch && [browser, uaMatch];
          },
          false
        )
      );
    }

    ////////////

    var service = {
      isMac: isMac,
      getBrowserInfo : getBrowserInfo
    };

    return service;

  }

  angular.module('umbraco.services').factory('platformService', platformService);


})();
