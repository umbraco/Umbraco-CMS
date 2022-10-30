/*-----------------------------------------------------------------------
* Sa11y, the accessibility quality assurance assistant.
* @version: 2.3.5
* @author: Development led by Adam Chaboryk, CPWA
* @acknowledgements: https://this.netlify.app/acknowledgements/
* @license: https://github.com/ryersondmp/sa11y/blob/master/LICENSE.md
* Copyright (c) 2020 - 2022 Toronto Metropolitan University (formerly Ryerson University).
* The above copyright notice shall be included in all copies or
substantial portions of the Software.
------------------------------------------------------------------------*/

import tippy from 'tippy.js';

/* Translation object */
const Lang = {
  langStrings: {},
  addI18n(strings) {
    this.langStrings = strings;
  },
  _(string) {
    return this.translate(string);
  },
  sprintf(string, ...args) {
    let transString = this._(string);
    if (args && args.length) {
      args.forEach((arg) => {
        transString = transString.replace(/%\([a-zA-z]+\)/, arg);
      });
    }
    return transString;
  },
  translate(string) {
    return this.langStrings[string] || string;
  },
};

class Sa11yCustomChecks {
  setSa11y(sa11y) {
    this.sa11y = sa11y;
  }

  check() {}
}

class Sa11y {
  constructor(options) {
    const defaultOptions = {
      checkRoot: 'body',
      containerIgnore: '.sa11y-ignore',
      contrastIgnore: '.sr-only',
      outlineIgnore: '',
      headerIgnore: '',
      imageIgnore: '',
      linkIgnore: 'nav *, [role="navigation"] *',
      linkIgnoreSpan: '',
      linksToFlag: '',
      nonConsecutiveHeadingIsError: true,
      flagLongHeadings: true,
      showGoodLinkButton: true,
      detectSPArouting: false,
      doNotRun: '',

      // Readability
      readabilityPlugin: true,
      readabilityRoot: 'body',
      readabilityLang: 'en',
      readabilityIgnore: '',

      // Other plugins
      contrastPlugin: true,
      formLabelsPlugin: true,
      linksAdvancedPlugin: true,
      customChecks: true,

      // QA rulesets
      badLinksQA: true,
      strongItalicsQA: true,
      pdfQA: true,
      langQA: true,
      blockquotesQA: true,
      tablesQA: true,
      allCapsQA: true,
      fakeHeadingsQA: true,
      fakeListQA: true,
      duplicateIdQA: true,
      underlinedTextQA: true,
      pageTitleQA: true,
      subscriptQA: true,

      // Embedded content rulesets
      embeddedContentAll: true,
      embeddedContentAudio: true,
      embeddedContentVideo: true,
      embeddedContentDataViz: true,
      embeddedContentTitles: true,
      embeddedContentGeneral: true,

      // Embedded content
      videoContent: 'youtube.com, vimeo.com, yuja.com, panopto.com',
      audioContent: 'soundcloud.com, simplecast.com, podbean.com, buzzsprout.com, blubrry.com, transistor.fm, fusebox.fm, libsyn.com',
      dataVizContent: 'datastudio.google.com, tableau',
      embeddedContent: '',
    };
    defaultOptions.embeddedContent = `${defaultOptions.videoContent}, ${defaultOptions.audioContent}, ${defaultOptions.dataVizContent}`;

    const option = {
      ...defaultOptions,
      ...options,
    };

    // Global constants for annotations.
    const ERROR = Lang._('ERROR');
    const WARNING = Lang._('WARNING');
    const GOOD = Lang._('GOOD');

    this.initialize = () => {
      // Do not run Sa11y if any supplied elements detected on page.
      const checkRunPrevent = () => {
        const { doNotRun } = option;
        return doNotRun.trim().length > 0 ? document.querySelector(doNotRun) : false;
      };

      // Only call Sa11y once page has loaded.
      const documentLoadingCheck = (callback) => {
        if (document.readyState === 'complete') {
          callback();
        } else {
          window.addEventListener('load', callback);
        }
      };

      if (!checkRunPrevent()) {
        this.globals();
        this.utilities();

        // Once document has fully loaded.
        documentLoadingCheck(() => {
          this.buildSa11yUI();
          this.settingPanelToggles();
          this.mainToggle();
          this.skipToIssueTooltip();
          this.detectPageChanges();

          // Pass Sa11y instance to custom checker
          if (option.customChecks && option.customChecks.setSa11y) {
            option.customChecks.setSa11y(this);
          }

          // Check page once page is done loading.
          document.getElementById('sa11y-toggle').disabled = false;
          if (this.store.getItem('sa11y-remember-panel') === 'Closed' || !this.store.getItem('sa11y-remember-panel')) {
            this.panelActive = true;
            this.checkAll();
          }
        });
      }
    };

    this.buildSa11yUI = () => {
      // Icon on the main toggle.
      const MainToggleIcon = "<svg role='img' focusable='false' width='35px' height='35px' aria-hidden='true' xmlns='http://www.w3.org/2000/svg' viewBox='0 0 512 512'><path fill='#ffffff' d='M256 48c114.953 0 208 93.029 208 208 0 114.953-93.029 208-208 208-114.953 0-208-93.029-208-208 0-114.953 93.029-208 208-208m0-40C119.033 8 8 119.033 8 256s111.033 248 248 248 248-111.033 248-248S392.967 8 256 8zm0 56C149.961 64 64 149.961 64 256s85.961 192 192 192 192-85.961 192-192S362.039 64 256 64zm0 44c19.882 0 36 16.118 36 36s-16.118 36-36 36-36-16.118-36-36 16.118-36 36-36zm117.741 98.023c-28.712 6.779-55.511 12.748-82.14 15.807.851 101.023 12.306 123.052 25.037 155.621 3.617 9.26-.957 19.698-10.217 23.315-9.261 3.617-19.699-.957-23.316-10.217-8.705-22.308-17.086-40.636-22.261-78.549h-9.686c-5.167 37.851-13.534 56.208-22.262 78.549-3.615 9.255-14.05 13.836-23.315 10.217-9.26-3.617-13.834-14.056-10.217-23.315 12.713-32.541 24.185-54.541 25.037-155.621-26.629-3.058-53.428-9.027-82.141-15.807-8.6-2.031-13.926-10.648-11.895-19.249s10.647-13.926 19.249-11.895c96.686 22.829 124.283 22.783 220.775 0 8.599-2.03 17.218 3.294 19.249 11.895 2.029 8.601-3.297 17.219-11.897 19.249z'/></svg>";

      const sa11ycontainer = document.createElement('div');
      sa11ycontainer.setAttribute('id', 'sa11y-container');
      sa11ycontainer.setAttribute('role', 'region');
      sa11ycontainer.setAttribute('lang', Lang._('LANG_CODE'));
      sa11ycontainer.setAttribute('aria-label', Lang._('CONTAINER_LABEL'));

      const loadContrastPreference = this.store.getItem('sa11y-remember-contrast') === 'On';
      const loadLabelsPreference = this.store.getItem('sa11y-remember-labels') === 'On';
      const loadChangeRequestPreference = this.store.getItem('sa11y-remember-links-advanced') === 'On';
      const loadReadabilityPreference = this.store.getItem('sa11y-remember-readability') === 'On';

      sa11ycontainer.innerHTML = `<button type="button" aria-expanded="false" id="sa11y-toggle" aria-describedby="sa11y-notification-badge" aria-label="${Lang._('MAIN_TOGGLE_LABEL')}" disabled>
                    ${MainToggleIcon}
                    <div id="sa11y-notification-badge">
                        <span id="sa11y-notification-count"></span>
                        <span id="sa11y-notification-text" class="sa11y-visually-hidden"></span>
                    </div>
                </button>`
        // Start of main container.
        + '<div id="sa11y-panel">'

        // Page Outline tab.
        + `<div id="sa11y-outline-panel" role="tabpanel" aria-labelledby="sa11y-outline-header">
                <div id="sa11y-outline-header" class="sa11y-header-text">
                    <h2 tabindex="-1">${Lang._('PAGE_OUTLINE')}</h2>
                </div>
                <div id="sa11y-outline-content">
                    <ul id="sa11y-outline-list" tabindex="0" role="list" aria-label="${Lang._('PAGE_OUTLINE')}"></ul>
                </div>`

        // Readability tab.
        + `<div id="sa11y-readability-panel">
                    <div id="sa11y-readability-content">
                        <h2 class="sa11y-header-text-inline">${Lang._('LANG_READABILITY')}</h2>
                        <p id="sa11y-readability-info"></p>
                        <ul id="sa11y-readability-details"></ul>
                    </div>
                </div>
            </div>`// End of Page Outline tab.

        // Settings tab.
        + `<div id="sa11y-settings-panel" role="tabpanel" aria-labelledby="sa11y-settings-header">
                <div id="sa11y-settings-header" class="sa11y-header-text">
                    <h2 tabindex="-1">${Lang._('SETTINGS')}</h2>
                </div>
                <div id="sa11y-settings-content">
                    <ul id="sa11y-settings-options">
                        <li id="sa11y-contrast-li">
                            <label id="sa11y-check-contrast" for="sa11y-contrast-toggle">${Lang._('CONTRAST')}</label>
                            <button id="sa11y-contrast-toggle"
                            aria-labelledby="sa11y-check-contrast"
                            class="sa11y-settings-switch"
                            aria-pressed="${loadContrastPreference ? 'true' : 'false'}">${loadContrastPreference ? Lang._('ON') : Lang._('OFF')}</button></li>
                        <li id="sa11y-form-labels-li">
                            <label id="sa11y-check-labels" for="sa11y-labels-toggle">${Lang._('FORM_LABELS')}</label>
                            <button id="sa11y-labels-toggle" aria-labelledby="sa11y-check-labels" class="sa11y-settings-switch"
                            aria-pressed="${loadLabelsPreference ? 'true' : 'false'}">${loadLabelsPreference ? Lang._('ON') : Lang._('OFF')}</button>
                        </li>
                        <li id="sa11y-links-advanced-li">
                            <label id="check-changerequest" for="sa11y-links-advanced-toggle">${Lang._('LINKS_ADVANCED')} <span class="sa11y-badge">AAA</span></label>
                            <button id="sa11y-links-advanced-toggle" aria-labelledby="check-changerequest" class="sa11y-settings-switch"
                            aria-pressed="${loadChangeRequestPreference ? 'true' : 'false'}">${loadChangeRequestPreference ? Lang._('ON') : Lang._('OFF')}</button>
                        </li>
                        <li id="sa11y-readability-li">
                            <label id="check-readability" for="sa11y-readability-toggle">${Lang._('LANG_READABILITY')} <span class="sa11y-badge">AAA</span></label>
                            <button id="sa11y-readability-toggle" aria-labelledby="check-readability" class="sa11y-settings-switch"
                            aria-pressed="${loadReadabilityPreference ? 'true' : 'false'}">${loadReadabilityPreference ? Lang._('ON') : Lang._('OFF')}</button>
                        </li>
                        <li>
                            <label id="sa11y-dark-mode" for="sa11y-theme-toggle">${Lang._('DARK_MODE')}</label>
                            <button id="sa11y-theme-toggle" aria-labelledby="sa11y-dark-mode" class="sa11y-settings-switch"></button>
                        </li>
                    </ul>
                </div>
            </div>`

          // Console warning messages.
          + `<div id="sa11y-panel-alert">
                <div class="sa11y-header-text">
                    <button id="sa11y-close-alert" class="sa11y-close-btn" aria-label="${Lang._('ALERT_CLOSE')}" aria-describedby="sa11y-alert-heading sa11y-panel-alert-text"></button>
                    <h2 id="sa11y-alert-heading">${Lang._('ALERT_TEXT')}</h2>
                </div>
                <p id="sa11y-panel-alert-text"></p>
                <div id="sa11y-panel-alert-preview"></div>
            </div>`

        // Main panel that conveys state of page.
        + `<div id="sa11y-panel-content">
                <button id="sa11y-cycle-toggle" type="button" aria-label="${Lang._('SHORTCUT_SCREEN_READER')}">
                    <div class="sa11y-panel-icon"></div>
                </button>
                <div id="sa11y-panel-text"><h1 class="sa11y-visually-hidden">${Lang._('PANEL_HEADING')}</h1>
                <p id="sa11y-status" aria-live="polite"></p>
                </div>
            </div>`

        // Show Outline & Show Settings button.
        + `<div id="sa11y-panel-controls" role="tablist" aria-orientation="horizontal">
                <button type="button" role="tab" aria-expanded="false" id="sa11y-outline-toggle" aria-controls="sa11y-outline-panel">
                    ${Lang._('SHOW_OUTLINE')}
                </button>
                <button type="button" role="tab" aria-expanded="false" id="sa11y-settings-toggle" aria-controls="sa11y-settings-panel">
                    ${Lang._('SHOW_SETTINGS')}
                </button>
                <div style="width:40px;"></div>
            </div>`

      // End of main container.
      + '</div>';

      const pagebody = document.getElementsByTagName('BODY')[0];
      pagebody.prepend(sa11ycontainer);
    };

    this.globals = () => {
      // Readability root
      if (!option.readabilityRoot) {
        option.readabilityRoot = option.checkRoot;
      }

      // Supported readability languages. Turn module off if not supported.
      const supportedLang = ['en', 'fr', 'es', 'de', 'nl', 'it', 'sv', 'fi', 'da', 'no', 'nb', 'nn'];
      const pageLang = document.querySelector('html').getAttribute('lang');

      // If lang attribute is missing.
      if (!pageLang) {
        option.readabilityPlugin = false;
      } else {
        const pageLangLowerCase = pageLang.toLowerCase();
        if (!supportedLang.some(($el) => pageLangLowerCase.includes($el))) {
          option.readabilityPlugin = false;
        }
      }

      /* Exclusions */
      // Container ignores apply to self and children.
      if (option.containerIgnore) {
        const containerSelectors = option.containerIgnore.split(',').map(($el) => `${$el} *, ${$el}`);
        option.containerIgnore = `[aria-hidden], [data-tippy-root] *, #sa11y-container *, #wpadminbar *, ${containerSelectors.join(', ')}`;
      } else {
        option.containerIgnore = '[aria-hidden], [data-tippy-root] *, #sa11y-container *, #wpadminbar *';
      }
      this.containerIgnore = option.containerIgnore;

      // Contrast exclusions
      this.contrastIgnore = `${this.containerIgnore}, .sa11y-heading-label, script`;
      if (option.contrastIgnore) {
        this.contrastIgnore = `${option.contrastIgnore}, ${this.contrastIgnore}`;
      }

      // Ignore specific regions for readability module.
      this.readabilityIgnore = `${this.containerIgnore}, nav li, [role="navigation"] li`;
      if (option.readabilityIgnore) {
        this.readabilityIgnore = `${option.readabilityIgnore}, ${this.readabilityIgnore}`;
      }

      // Ignore specific headings
      this.headerIgnore = this.containerIgnore;
      if (option.headerIgnore) {
        this.headerIgnore = `${option.headerIgnore}, ${this.headerIgnore}`;
      }

      // Don't add heading label or include in panel.
      if (option.outlineIgnore) {
        this.outlineIgnore = `${option.outlineIgnore}, #sa11y-container h1, #sa11y-container h2`;
      }

      // Ignore specific images.
      this.imageIgnore = `${this.containerIgnore}, [role='presentation'], [src^='https://trck.youvisit.com']`;
      if (option.imageIgnore) {
        this.imageIgnore = `${option.imageIgnore}, ${this.imageIgnore}`;
      }

      // Ignore specific links
      this.linkIgnore = `${this.containerIgnore}, [aria-hidden="true"], .anchorjs-link`;
      if (option.linkIgnore) {
        this.linkIgnore = `${option.linkIgnore}, ${this.linkIgnore}`;
      }

      // Ignore specific classes within links.
      if (option.linkIgnoreSpan) {
        const linkIgnoreSpanSelectors = option.linkIgnoreSpan.split(',').map(($el) => `${$el} *, ${$el}`);
        option.linkIgnoreSpan = `noscript, ${linkIgnoreSpanSelectors.join(', ')}`;
      } else {
        option.linkIgnoreSpan = 'noscript';
      }

      /* Embedded content sources */
      // Video sources.
      if (option.videoContent) {
        const videoContent = option.videoContent.split(/\s*[\s,]\s*/).map(($el) => `[src*='${$el}']`);
        option.videoContent = `video, ${videoContent.join(', ')}`;
      } else {
        option.videoContent = 'video';
      }

      // Audio sources.
      if (option.audioContent) {
        const audioContent = option.audioContent.split(/\s*[\s,]\s*/).map(($el) => `[src*='${$el}']`);
        option.audioContent = `audio, ${audioContent.join(', ')}`;
      } else {
        option.audioContent = 'audio';
      }

      // Data viz sources.
      if (option.dataVizContent) {
        const dataVizContent = option.dataVizContent.split(/\s*[\s,]\s*/).map(($el) => `[src*='${$el}']`);
        option.dataVizContent = dataVizContent.join(', ');
      } else {
        option.dataVizContent = 'datastudio.google.com, tableau';
      }

      // Embedded content all
      if (option.embeddedContent) {
        const embeddedContent = option.embeddedContent.split(/\s*[\s,]\s*/).map(($el) => `[src*='${$el}']`);
        option.embeddedContent = embeddedContent.join(', ');
      }

      // A11y: Determine scroll behaviour
      let reducedMotion = false;
      if (typeof window.matchMedia === 'function') {
        reducedMotion = window.matchMedia('(prefers-reduced-motion: reduce)');
      }
      this.scrollBehaviour = (!reducedMotion || reducedMotion.matches) ? 'auto' : 'smooth';
    };

    this.mainToggle = () => {
      // Keeps checker active when navigating between pages until it is toggled off.
      const sa11yToggle = document.getElementById('sa11y-toggle');
      sa11yToggle.addEventListener('click', (e) => {
        if (this.store.getItem('sa11y-remember-panel') === 'Opened') {
          this.store.setItem('sa11y-remember-panel', 'Closed');
          sa11yToggle.classList.remove('sa11y-on');
          sa11yToggle.setAttribute('aria-expanded', 'false');
          this.resetAll();
          this.updateBadge();
          e.preventDefault();
        } else {
          this.store.setItem('sa11y-remember-panel', 'Opened');
          sa11yToggle.classList.add('sa11y-on');
          sa11yToggle.setAttribute('aria-expanded', 'true');
          this.checkAll();
          // Don't show badge when panel is opened.
          document.getElementById('sa11y-notification-badge').style.display = 'none';
          e.preventDefault();
        }
      });

      // Remember to leave it open
      if (this.store.getItem('sa11y-remember-panel') === 'Opened') {
        sa11yToggle.classList.add('sa11y-on');
        sa11yToggle.setAttribute('aria-expanded', 'true');
      }

      // Crudely give time to load any other content or slow post-rendered JS, iFrames, etc.
      if (sa11yToggle.classList.contains('sa11y-on')) {
        sa11yToggle.classList.toggle('loading-sa11y');
        sa11yToggle.setAttribute('aria-expanded', 'true');
        setTimeout(this.checkAll, 400);
      }

      document.onkeydown = (e) => {
        const evt = e || window.event;
        if (evt.key === 'Escape' && document.getElementById('sa11y-panel').classList.contains('sa11y-active')) {
          sa11yToggle.setAttribute('aria-expanded', 'false');
          sa11yToggle.classList.remove('sa11y-on');
          sa11yToggle.click();
          this.resetAll();
        }

        // Alt + A to enable accessibility checker.
        if (evt.altKey && evt.code === 'KeyA') {
          sa11yToggle.click();
          sa11yToggle.focus();
          evt.preventDefault();
        }
      };
    };

    // ============================================================
    // Helpers: Sanitize HTML and compute ARIA for hyperlinks
    // ============================================================
    this.utilities = () => {
      this.isElementHidden = ($el) => {
        if ($el.getAttribute('hidden') || ($el.offsetWidth === 0 && $el.offsetHeight === 0)) {
          return true;
        }
        const compStyles = getComputedStyle($el);
        return compStyles.getPropertyValue('display') === 'none';
      };

      // Helper: Escape HTML, encode HTML symbols.
      this.escapeHTML = (text) => {
        const $div = document.createElement('div');
        $div.textContent = text;
        return $div.innerHTML.replaceAll('"', '&quot;').replaceAll("'", '&#039;').replaceAll('`', '&#x60;');
      };

      // Helper: Help clean up HTML characters for tooltips and outline panel.
      this.sanitizeForHTML = (string) => {
        const entityMap = {
          '&': '&amp;',
          '<': '&lt;',
          '>': '&gt;',
          '"': '&quot;',
          "'": '&#39;',
          '/': '&#x2F;',
          '`': '&#x60;',
          '=': '&#x3D;',
        };
        return String(string).replace(/[&<>"'`=/]/g, (s) => entityMap[s]);
      };

      // Helper: Compute alt text on images within a text node.
      this.computeTextNodeWithImage = ($el) => {
        const imgArray = Array.from($el.querySelectorAll('img'));
        let returnText = '';
        // No image, has text.
        if (imgArray.length === 0 && $el.textContent.trim().length > 1) {
          returnText = $el.textContent.trim();
        } else if (imgArray.length && $el.textContent.trim().length === 0) {
          // Has image.
          const imgalt = imgArray[0].getAttribute('alt');
          if (!imgalt || imgalt === ' ' || imgalt === '') {
            returnText = ' ';
          } else if (imgalt !== undefined) {
            returnText = imgalt;
          }
        } else if (imgArray.length && $el.textContent.trim().length) {
          // Has image and text.
          // To-do: This is a hack? Any way to do this better?
          imgArray.forEach((element) => {
            element.insertAdjacentHTML('afterend', ` <span class='sa11y-clone-image-text' aria-hidden='true'>${imgArray[0].getAttribute('alt')}</span>`);
          });
          returnText = $el.textContent.trim();
        }
        return returnText;
      };

      // Utility: https://www.joshwcomeau.com/snippets/javascript/debounce/
      this.debounce = (callback, wait) => {
        let timeoutId = null;
        return (...args) => {
          window.clearTimeout(timeoutId);
          timeoutId = window.setTimeout(() => {
            callback(...args);
          }, wait);
        };
      };

      // Helper: Used to ignore child elements within an anchor.
      this.fnIgnore = (element, selector) => {
        const $clone = element.cloneNode(true);
        const $exclude = Array.from(selector ? $clone.querySelectorAll(selector) : $clone.children);
        $exclude.forEach(($c) => {
          $c.parentElement.removeChild($c);
        });
        return $clone;
      };

      // Helper: Handle ARIA labels for Link Text module.
      this.computeAriaLabel = ($el) => {
        // aria-label
        if ($el.matches('[aria-label]')) {
          return $el.getAttribute('aria-label');
        }
        // aria-labeledby.
        if ($el.matches('[aria-labelledby]')) {
          const target = $el.getAttribute('aria-labelledby').split(/\s+/);
          if (target.length > 0) {
            let returnText = '';
            target.forEach((x) => {
              const targetSelector = document.querySelector(`#${x}`);
              if (targetSelector === null) {
                returnText += ' ';
              } else if (targetSelector.hasAttribute('aria-label')) {
                returnText += `${targetSelector.getAttribute('aria-label')}`;
              } else {
                returnText += `${targetSelector.firstChild.nodeValue} `;
              }
            });
            return returnText;
          }
          return '';
        }
        // Child with aria-label
        if (Array.from($el.children).filter((x) => x.matches('[aria-label]')).length > 0) {
          const child = Array.from($el.childNodes);
          let returnText = '';

          // Process each child within node.
          child.forEach((x) => {
            if (x.nodeType === 1) {
              if (x.ariaLabel === null) {
                returnText += x.innerText;
              } else {
                returnText += x.getAttribute('aria-label');
              }
            } else {
              returnText += x.nodeValue;
            }
          });
          return returnText;
        }
        // Child with aria-labelledby
        if (Array.from($el.children).filter((x) => x.matches('[aria-labelledby]')).length > 0) {
          const child = Array.from($el.childNodes);
          let returnText = '';

          // Process each child within node.
          child.forEach((y) => {
            if (y.nodeType === 8) {
              // Ignore HTML comments
            } else if (y.nodeType === 3) {
              returnText += y.nodeValue;
            } else {
              const target = y.getAttribute('aria-labelledby').split(/\s+/);
              if (target.length > 0) {
                let returnAria = '';
                target.forEach((z) => {
                  if (document.querySelector(`#${z}`) === null) {
                    returnAria += ' ';
                  } else {
                    returnAria += `${document.querySelector(`#${z}`).firstChild.nodeValue} `;
                  }
                });
                returnText += returnAria;
              }
            }
            return '';
          });
          return returnText;
        }
        return 'noAria';
      };

      // Mini function: Find visibible parent of hidden element.
      this.findVisibleParent = (element, property, value) => {
        let $el = element;
        while ($el !== null) {
          const style = window.getComputedStyle($el);
          const propValue = style.getPropertyValue(property);
          if (propValue === value) {
            return $el;
          }
          $el = $el.parentElement;
        }
        return null;
      };

      // Mini function: Calculate top of element.
      this.offsetTop = ($el) => {
        const rect = $el.getBoundingClientRect();
        const scrollTop = window.pageYOffset || document.documentElement.scrollTop;
        return {
          top: rect.top + scrollTop,
        };
      };

      // Utility: Custom localStorage utility with fallback to sessionStorage.
      this.store = {
        getItem(key) {
          try {
            if (localStorage.getItem(key) === null) {
              return sessionStorage.getItem(key);
            }
            return localStorage.getItem(key);
          } catch (error) {
            // Cookies totally disabled.
            return false;
          }
        },
        setItem(key, value) {
          try {
            localStorage.setItem(key, value);
          } catch (error) {
            sessionStorage.setItem(key, value);
          }
          return true;
        },
      };

      // Utility: Add & remove pulsing border.
      this.addPulse = ($el) => {
        const border = 'sa11y-pulse-border';
        document.querySelectorAll(`.${border}`).forEach((el) => el.classList.remove(border));
        $el.classList.add(border);
        setTimeout(() => {
          $el.classList.remove(border);
        }, 2500);
      };

      // Utility: Send an alert to main panel.
      this.createAlert = (alertMessage, errorPreview) => {
        const $alertPanel = document.getElementById('sa11y-panel-alert');
        const $alertText = document.getElementById('sa11y-panel-alert-text');
        const $alertPreview = document.getElementById('sa11y-panel-alert-preview');
        const $closeAlertToggle = document.getElementById('sa11y-close-alert');
        const $skipBtn = document.getElementById('sa11y-cycle-toggle');

        $alertPanel.classList.add('sa11y-active');
        $alertText.innerHTML = alertMessage;
        if (errorPreview) {
          $alertPreview.classList.add('sa11y-panel-alert-preview');
          $alertPreview.innerHTML = errorPreview;
        }
        setTimeout(() => {
          $closeAlertToggle.focus();
        }, 500);

        // Closing alert sets focus back to Skip to Issue toggle.
        $closeAlertToggle.addEventListener('click', () => {
          this.removeAlert();
          $skipBtn.focus();
        });
      };

      // Utility: Destory alert.
      this.removeAlert = () => {
        const $alertPanel = document.getElementById('sa11y-panel-alert');
        const $alertText = document.getElementById('sa11y-panel-alert-text');
        const $alertPreview = document.getElementById('sa11y-panel-alert-preview');
        $alertPanel.classList.remove('sa11y-active');
        $alertPreview.classList.remove('sa11y-panel-alert-preview');
        while ($alertText.firstChild) $alertText.removeChild($alertText.firstChild);
        while ($alertPreview.firstChild) $alertPreview.removeChild($alertPreview.firstChild);
      };

      // Utility: Replace newlines and double spaces with a single space.
      this.getText = ($el) => $el.textContent.replace(/[\n\r]+|[\s]{2,}/g, ' ').trim();

      // Utility: Get next sibling.
      this.getNextSibling = (elem, selector) => {
        let sibling = elem.nextElementSibling;
        if (!selector) return sibling;
        while (sibling) {
          if (sibling.matches(selector)) return sibling;
          sibling = sibling.nextElementSibling;
        }
        return '';
      };
    };

    //----------------------------------------------------------------------
    // Setting's panel: Additional ruleset toggles.
    //----------------------------------------------------------------------
    this.settingPanelToggles = () => {
      // Toggle: Contrast
      const $contrastToggle = document.getElementById('sa11y-contrast-toggle');
      $contrastToggle.onclick = async () => {
        if (this.store.getItem('sa11y-remember-contrast') === 'On') {
          this.store.setItem('sa11y-remember-contrast', 'Off');
          $contrastToggle.textContent = `${Lang._('OFF')}`;
          $contrastToggle.setAttribute('aria-pressed', 'false');
          this.resetAll(false);
          await this.checkAll();
        } else {
          this.store.setItem('sa11y-remember-contrast', 'On');
          $contrastToggle.textContent = `${Lang._('ON')}`;
          $contrastToggle.setAttribute('aria-pressed', 'true');
          this.resetAll(false);
          await this.checkAll();
        }
      };

      // Toggle: Form labels
      const $labelsToggle = document.getElementById('sa11y-labels-toggle');
      $labelsToggle.onclick = async () => {
        if (this.store.getItem('sa11y-remember-labels') === 'On') {
          this.store.setItem('sa11y-remember-labels', 'Off');
          $labelsToggle.textContent = `${Lang._('OFF')}`;
          $labelsToggle.setAttribute('aria-pressed', 'false');
          this.resetAll(false);
          await this.checkAll();
        } else {
          this.store.setItem('sa11y-remember-labels', 'On');
          $labelsToggle.textContent = `${Lang._('ON')}`;
          $labelsToggle.setAttribute('aria-pressed', 'true');
          this.resetAll(false);
          await this.checkAll();
        }
      };

      // Toggle: Links (Advanced)
      const $linksToggle = document.getElementById('sa11y-links-advanced-toggle');
      $linksToggle.onclick = async () => {
        if (this.store.getItem('sa11y-remember-links-advanced') === 'On') {
          this.store.setItem('sa11y-remember-links-advanced', 'Off');
          $linksToggle.textContent = `${Lang._('OFF')}`;
          $linksToggle.setAttribute('aria-pressed', 'false');
          this.resetAll(false);
          await this.checkAll();
        } else {
          this.store.setItem('sa11y-remember-links-advanced', 'On');
          $linksToggle.textContent = `${Lang._('ON')}`;
          $linksToggle.setAttribute('aria-pressed', 'true');
          this.resetAll(false);
          await this.checkAll();
        }
      };

      // Toggle: Readability
      const $readabilityToggle = document.getElementById('sa11y-readability-toggle');
      $readabilityToggle.onclick = async () => {
        if (this.store.getItem('sa11y-remember-readability') === 'On') {
          this.store.setItem('sa11y-remember-readability', 'Off');
          $readabilityToggle.textContent = `${Lang._('OFF')}`;
          $readabilityToggle.setAttribute('aria-pressed', 'false');
          document.getElementById('sa11y-readability-panel').classList.remove('sa11y-active');
          this.resetAll(false);
          await this.checkAll();
        } else {
          this.store.setItem('sa11y-remember-readability', 'On');
          $readabilityToggle.textContent = `${Lang._('ON')}`;
          $readabilityToggle.setAttribute('aria-pressed', 'true');
          document.getElementById('sa11y-readability-panel').classList.add('sa11y-active');
          this.resetAll(false);
          await this.checkAll();
        }
      };

      if (this.store.getItem('sa11y-remember-readability') === 'On') {
        document.getElementById('sa11y-readability-panel').classList.add('sa11y-active');
      }

      // Toggle: Dark mode. (Credits: https://derekkedziora.com/blog/dark-mode-revisited)
      const systemInitiatedDark = window.matchMedia('(prefers-color-scheme: dark)');
      const $themeToggle = document.getElementById('sa11y-theme-toggle');
      const html = document.querySelector('html');

      if (systemInitiatedDark.matches) {
        $themeToggle.textContent = `${Lang._('ON')}`;
        $themeToggle.setAttribute('aria-pressed', 'true');
      } else {
        $themeToggle.textContent = `${Lang._('OFF')}`;
        $themeToggle.setAttribute('aria-pressed', 'false');
      }

      const prefersColorTest = () => {
        if (systemInitiatedDark.matches) {
          html.setAttribute('data-sa11y-theme', 'dark');
          $themeToggle.textContent = `${Lang._('ON')}`;
          $themeToggle.setAttribute('aria-pressed', 'true');
          this.store.setItem('sa11y-remember-theme', '');
        } else {
          html.setAttribute('data-sa11y-theme', 'light');
          $themeToggle.textContent = `${Lang._('OFF')}`;
          $themeToggle.setAttribute('aria-pressed', 'false');
          this.store.setItem('sa11y-remember-theme', '');
        }
      };

      systemInitiatedDark.addEventListener('change', prefersColorTest);
      $themeToggle.onclick = async () => {
        const theme = this.store.getItem('sa11y-remember-theme');
        if (theme === 'dark') {
          html.setAttribute('data-sa11y-theme', 'light');
          this.store.setItem('sa11y-remember-theme', 'light');
          $themeToggle.textContent = `${Lang._('OFF')}`;
          $themeToggle.setAttribute('aria-pressed', 'false');
        } else if (theme === 'light') {
          html.setAttribute('data-sa11y-theme', 'dark');
          this.store.setItem('sa11y-remember-theme', 'dark');
          $themeToggle.textContent = `${Lang._('ON')}`;
          $themeToggle.setAttribute('aria-pressed', 'true');
        } else if (systemInitiatedDark.matches) {
          html.setAttribute('data-sa11y-theme', 'light');
          this.store.setItem('sa11y-remember-theme', 'light');
          $themeToggle.textContent = `${Lang._('OFF')}`;
          $themeToggle.setAttribute('aria-pressed', 'false');
        } else {
          html.setAttribute('data-sa11y-theme', 'dark');
          this.store.setItem('sa11y-remember-theme', 'dark');
          $themeToggle.textContent = `${Lang._('ON')}`;
          $themeToggle.setAttribute('aria-pressed', 'true');
        }
      };
      const theme = this.store.getItem('sa11y-remember-theme');
      if (theme === 'dark') {
        html.setAttribute('data-sa11y-theme', 'dark');
        this.store.setItem('sa11y-remember-theme', 'dark');
        $themeToggle.textContent = `${Lang._('ON')}`;
        $themeToggle.setAttribute('aria-pressed', 'true');
      } else if (theme === 'light') {
        html.setAttribute('data-sa11y-theme', 'light');
        this.store.setItem('sa11y-remember-theme', 'light');
        $themeToggle.textContent = `${Lang._('OFF')}`;
        $themeToggle.setAttribute('aria-pressed', 'false');
      }
    };

    //----------------------------------------------------------------------
    // Tooltip for Jump-to-Issue button.
    //----------------------------------------------------------------------
    this.skipToIssueTooltip = () => {
      let keyboardShortcut;
      if (navigator.userAgent.indexOf('Mac') !== -1) {
        keyboardShortcut = '<span class="sa11y-kbd">Option</span> + <span class="sa11y-kbd">S</span>';
      } else {
        keyboardShortcut = '<span class="sa11y-kbd">Alt</span> + <span class="sa11y-kbd">S</span>';
      }

      tippy('#sa11y-cycle-toggle', {
        content: `<div style="text-align:center">${Lang._('SHORTCUT_TOOLTIP')} &raquo;<br>${keyboardShortcut}</div>`,
        allowHTML: true,
        delay: [500, 0],
        trigger: 'mouseenter focusin',
        arrow: true,
        placement: 'top',
        theme: 'sa11y-theme',
        aria: {
          content: null,
          expanded: false,
        },
        appendTo: document.body,
        zIndex: 2147483645,
      });
    };

    //----------------------------------------------------------------------
    // Feature to detect if URL changed for bookmarklet/SPAs.
    //----------------------------------------------------------------------
    this.detectPageChanges = () => {
      // Feature to detect page changes (e.g. SPAs).
      if (option.detectSPArouting === true) {
        let url = window.location.href.split('#')[0];

        const checkURL = this.debounce(async () => {
          if (url !== window.location.href.split('#')[0]) {
            // If panel is closed.
            if (this.store.getItem('sa11y-remember-panel') === 'Closed' || !this.store.getItem('sa11y-remember-panel')) {
              this.panelActive = true;
              this.checkAll();
            }
            // Async scan while panel is open.
            if (this.panelActive === true) {
              this.resetAll(false);
              await this.checkAll();
            }
            // Performance: New URL becomes current.
            url = window.location.href;
          }
        }, 250);
        window.addEventListener('mousemove', checkURL);
        window.addEventListener('keydown', checkURL);
      }
    };

    // ----------------------------------------------------------------------
    // Check all
    // ----------------------------------------------------------------------
    this.checkAll = async () => {
      this.errorCount = 0;
      this.warningCount = 0;

      // Error handling. If specified selector doesn't exist on page, scan the body of page instead.
      const rootTarget = document.querySelector(option.checkRoot);
      if (!rootTarget) {
        this.root = document.querySelector('body');
        this.createAlert(`${Lang.sprintf('ERROR_MISSING_ROOT_TARGET', option.checkRoot)}`);
      } else {
        this.root = document.querySelector(option.checkRoot);
      }

      this.findElements();

      // Ruleset checks
      this.checkHeaders();
      this.checkLinkText();
      this.checkAltText();

      // Contrast plugin
      if (option.contrastPlugin === true) {
        if (this.store.getItem('sa11y-remember-contrast') === 'On') {
          this.checkContrast();
        }
      } else {
        const contrastLi = document.getElementById('sa11y-contrast-li');
        contrastLi.setAttribute('style', 'display: none !important;');
        this.store.setItem('sa11y-remember-contrast', 'Off');
      }

      // Form labels plugin
      if (option.formLabelsPlugin === true) {
        if (this.store.getItem('sa11y-remember-labels') === 'On') {
          this.checkLabels();
        }
      } else {
        const formLabelsLi = document.getElementById('sa11y-form-labels-li');
        formLabelsLi.setAttribute('style', 'display: none !important;');
        this.store.setItem('sa11y-remember-labels', 'Off');
      }

      // Links (Advanced) plugin
      if (option.linksAdvancedPlugin === true) {
        if (this.store.getItem('sa11y-remember-links-advanced') === 'On') {
          this.checkLinksAdvanced();
        }
      } else {
        const linksAdvancedLi = document.getElementById('sa11y-links-advanced-li');
        linksAdvancedLi.setAttribute('style', 'display: none !important;');
        this.store.setItem('sa11y-remember-links-advanced', 'Off');
      }

      // Readability plugin
      if (option.readabilityPlugin === true) {
        if (this.store.getItem('sa11y-remember-readability') === 'On') {
          this.checkReadability();
        }
      } else {
        const readabilityLi = document.getElementById('sa11y-readability-li');
        const readabilityPanel = document.getElementById('sa11y-readability-panel');
        readabilityLi.setAttribute('style', 'display: none !important;');
        readabilityPanel.classList.remove('sa11y-active');
      }

      // Embedded content plugin
      if (option.embeddedContentAll === true) {
        this.checkEmbeddedContent();
      }

      // QA module checks.
      this.checkQA();

      // Custom checks abstracted to seperate class.
      if (option.customChecks && option.customChecks.setSa11y) {
        option.customChecks.check();
      }

      // Update panel
      if (this.panelActive) {
        this.resetAll();
      } else {
        this.updatePanel();
      }
      this.initializeTooltips();
      this.detectOverflow();
      this.nudge();

      // Don't show badge when panel is opened.
      if (!document.getElementsByClassName('sa11y-on').length) {
        this.updateBadge();
      }
    };

    // ============================================================
    // Reset all
    // ============================================================
    this.resetAll = (restartPanel = true) => {
      this.panelActive = false;

      const html = document.querySelector('html');
      html.removeAttribute('data-sa11y-active');

      // Remove eventListeners on the Show Outline and Show Panel toggles.
      const $outlineToggle = document.getElementById('sa11y-outline-toggle');
      const resetOutline = $outlineToggle.cloneNode(true);
      $outlineToggle.parentNode.replaceChild(resetOutline, $outlineToggle);

      const $settingsToggle = document.getElementById('sa11y-settings-toggle');
      const resetSettings = $settingsToggle.cloneNode(true);
      $settingsToggle.parentNode.replaceChild(resetSettings, $settingsToggle);

      // Reset all classes on elements.
      const resetClass = ($el) => {
        $el.forEach((x) => {
          document.querySelectorAll(`.${x}`).forEach((y) => y.classList.remove(x));
        });
      };
      resetClass(['sa11y-error-border', 'sa11y-error-text', 'sa11y-warning-border', 'sa11y-warning-text', 'sa11y-good-border', 'sa11y-good-text', 'sa11y-overflow', 'sa11y-fake-heading']);

      const dataAttr = document.querySelectorAll('[data-sa11y-parent]');
      dataAttr.forEach(($el) => { $el.removeAttribute('data-sa11y-parent'); });

      const allcaps = document.querySelectorAll('.sa11y-warning-uppercase');
      // eslint-disable-next-line no-param-reassign, no-return-assign
      allcaps.forEach(($el) => $el.outerHTML = $el.innerHTML);

      document.getElementById('sa11y-readability-info').innerHTML = '';

      // Remove
      document.querySelectorAll(`
                .sa11y-element,
                .sa11y-instance,
                .sa11y-instance-inline,
                .sa11y-heading-label,
                #sa11y-outline-list li,
                .sa11y-readability-period,
                #sa11y-readability-details li,
                .sa11y-clone-image-text
            `).forEach(($el) => $el.parentNode.removeChild($el));

      // Alert within panel.
      this.removeAlert();

      // Main panel warning and error count.
      const clearStatus = document.querySelector('#sa11y-status');
      while (clearStatus.firstChild) clearStatus.removeChild(clearStatus.firstChild);

      if (restartPanel) {
        document.querySelector('#sa11y-panel').classList.remove('sa11y-active');
      }
    };

    // ============================================================
    // Initialize tooltips for error/warning/pass buttons: (Tippy.js)
    // ============================================================
    this.initializeTooltips = () => {
      tippy('.sa11y-btn', {
        interactive: true,
        trigger: 'mouseenter click focusin', // Focusin trigger to ensure "Jump to issue" button displays tooltip.
        arrow: true,
        delay: [100, 0], // Slight delay to ensure mouse doesn't quickly trigger and hide tooltip.
        theme: 'sa11y-theme',
        placement: 'auto-start',
        allowHTML: true,
        aria: {
          content: 'describedby',
        },
        appendTo: document.body,
        zIndex: 2147483645,
      });
    };

    // ============================================================
    // Detect parent containers that have hidden overflow.
    // ============================================================
    this.detectOverflow = () => {
      const findParentWithOverflow = (element, property, value) => {
        let $el = element;
        while ($el !== null) {
          const style = window.getComputedStyle($el);
          const propValue = style.getPropertyValue(property);
          if (propValue === value) {
            return $el;
          }
          $el = $el.parentElement;
        }
        return null;
      };
      const $findButtons = document.querySelectorAll('.sa11y-btn');
      $findButtons.forEach(($el) => {
        const overflowing = findParentWithOverflow($el, 'overflow', 'hidden');
        if (overflowing !== null) {
          overflowing.classList.add('sa11y-overflow');
        }
      });
    };

    // ============================================================
    // Nudge buttons if they overlap.
    // ============================================================
    this.nudge = () => {
      const sa11yInstance = document.querySelectorAll('.sa11y-instance, .sa11y-instance-inline');
      sa11yInstance.forEach(($el) => {
        const sibling = $el.nextElementSibling;
        if (sibling !== null
          && (sibling.classList.contains('sa11y-instance') || sibling.classList.contains('sa11y-instance-inline'))) {
          sibling.querySelector('button').setAttribute('style', 'margin: -10px -20px !important;');
        }
      });
    };

    // ============================================================
    // Update iOS style notification badge on icon.
    // ============================================================
    this.updateBadge = () => {
      const totalCount = this.errorCount + this.warningCount;
      const notifBadge = document.getElementById('sa11y-notification-badge');
      const notifCount = document.getElementById('sa11y-notification-count');
      const notifText = document.getElementById('sa11y-notification-text');

      if (totalCount === 0) {
        notifBadge.style.display = 'none';
      } else if (this.warningCount > 0 && this.errorCount === 0) {
        notifBadge.style.display = 'flex';
        notifBadge.classList.add('sa11y-notification-badge-warning');
        notifCount.innerText = `${this.warningCount}`;
        notifText.innerText = `${Lang._('PANEL_ICON_WARNINGS')}`;
      } else {
        notifBadge.style.display = 'flex';
        notifBadge.classList.remove('sa11y-notification-badge-warning');
        notifCount.innerText = `${totalCount}`;
        notifText.innerText = Lang._('PANEL_ICON_TOTAL');
      }
    };

    // ----------------------------------------------------------------------
    // Main panel: Display and update panel.
    // ----------------------------------------------------------------------
    this.updatePanel = () => {
      this.panelActive = true;

      this.buildPanel();
      this.skipToIssue();

      const $skipBtn = document.getElementById('sa11y-cycle-toggle');
      $skipBtn.disabled = false;

      const $panel = document.getElementById('sa11y-panel');
      $panel.classList.add('sa11y-active');

      const html = document.querySelector('html');
      html.setAttribute('data-sa11y-active', 'true');

      const $panelContent = document.getElementById('sa11y-panel-content');
      const $status = document.getElementById('sa11y-status');
      const $findButtons = document.querySelectorAll('.sa11y-btn');

      if (this.errorCount > 0 && this.warningCount > 0) {
        $panelContent.setAttribute('class', 'sa11y-errors');
        $status.innerHTML = `${Lang._('ERRORS')} <span class="sa11y-panel-count sa11y-margin-right">${this.errorCount}</span> ${Lang._('WARNINGS')} <span class="sa11y-panel-count">${this.warningCount}</span>`;
      } else if (this.errorCount > 0) {
        $panelContent.setAttribute('class', 'sa11y-errors');
        $status.innerHTML = `${Lang._('ERRORS')} <span class="sa11y-panel-count">${this.errorCount}</span>`;
      } else if (this.warningCount > 0) {
        $panelContent.setAttribute('class', 'sa11y-warnings');
        $status.innerHTML = `${Lang._('WARNINGS')} <span class="sa11y-panel-count">${this.warningCount}</span>`;
      } else {
        $panelContent.setAttribute('class', 'sa11y-good');
        $status.textContent = `${Lang._('PANEL_STATUS_NONE')}`;

        if ($findButtons.length === 0) {
          $skipBtn.disabled = true;
        }
      }
    };

    // ----------------------------------------------------------------------
    // Main panel: Build Show Outline and Settings tabs.
    // ----------------------------------------------------------------------
    this.buildPanel = () => {
      const $outlineToggle = document.getElementById('sa11y-outline-toggle');
      const $outlinePanel = document.getElementById('sa11y-outline-panel');
      const $outlineList = document.getElementById('sa11y-outline-list');
      const $settingsToggle = document.getElementById('sa11y-settings-toggle');
      const $settingsPanel = document.getElementById('sa11y-settings-panel');
      const $settingsContent = document.getElementById('sa11y-settings-content');
      const $headingAnnotations = document.querySelectorAll('.sa11y-heading-label');

      // Show outline panel
      $outlineToggle.addEventListener('click', () => {
        if ($outlineToggle.getAttribute('aria-expanded') === 'true') {
          $outlineToggle.classList.remove('sa11y-outline-active');
          $outlinePanel.classList.remove('sa11y-active');
          $outlineToggle.textContent = `${Lang._('SHOW_OUTLINE')}`;
          $outlineToggle.setAttribute('aria-expanded', 'false');
          this.store.setItem('sa11y-remember-outline', 'Closed');
        } else {
          $outlineToggle.classList.add('sa11y-outline-active');
          $outlinePanel.classList.add('sa11y-active');
          $outlineToggle.textContent = `${Lang._('HIDE_OUTLINE')}`;
          $outlineToggle.setAttribute('aria-expanded', 'true');
          this.store.setItem('sa11y-remember-outline', 'Opened');
        }

        // Set focus on Page Outline heading for accessibility.
        document.querySelector('#sa11y-outline-header > h2').focus();

        // Show heading level annotations.
        $headingAnnotations.forEach(($el) => $el.classList.toggle('sa11y-label-visible'));

        // Close Settings panel when Show Outline is active.
        $settingsPanel.classList.remove('sa11y-active');
        $settingsToggle.classList.remove('sa11y-settings-active');
        $settingsToggle.setAttribute('aria-expanded', 'false');
        $settingsToggle.textContent = `${Lang._('SHOW_SETTINGS')}`;

        // Keyboard accessibility fix for scrollable panel content.
        if ($outlineList.clientHeight > 250) {
          $outlineList.setAttribute('tabindex', '0');
        }
      });

      // Remember to leave outline open
      if (this.store.getItem('sa11y-remember-outline') === 'Opened') {
        $outlineToggle.classList.add('sa11y-outline-active');
        $outlinePanel.classList.add('sa11y-active');
        $outlineToggle.textContent = `${Lang._('HIDE_OUTLINE')}`;
        $outlineToggle.setAttribute('aria-expanded', 'true');
        $headingAnnotations.forEach(($el) => $el.classList.toggle('sa11y-label-visible'));
      }

      // Roving tabindex menu for page outline.
      // Thanks to Srijan for this snippet! https://blog.srij.dev/roving-tabindex-from-scratch
      const children = Array.from($outlineList.querySelectorAll('a'));
      let current = 0;
      const handleKeyDown = (e) => {
        if (!['ArrowUp', 'ArrowDown', 'Space'].includes(e.code)) return;
        if (e.code === 'Space') {
          children[current].click();
          return;
        }
        const selected = children[current];
        selected.setAttribute('tabindex', -1);
        let next;
        if (e.code === 'ArrowDown') {
          next = current + 1;
          if (current === children.length - 1) {
            next = 0;
          }
        } else if ((e.code === 'ArrowUp')) {
          next = current - 1;
          if (current === 0) {
            next = children.length - 1;
          }
        }
        children[next].setAttribute('tabindex', 0);
        children[next].focus();
        current = next;
        e.preventDefault();
      };
      $outlineList.addEventListener('focus', () => {
        if (children.length > 0) {
          $outlineList.setAttribute('tabindex', -1);
          children[current].setAttribute('tabindex', 0);
          children[current].focus();
        }
        $outlineList.addEventListener('keydown', handleKeyDown);
      });
      $outlineList.addEventListener('blur', () => {
        $outlineList.removeEventListener('keydown', handleKeyDown);
      });

      // Show settings panel
      $settingsToggle.addEventListener('click', () => {
        if ($settingsToggle.getAttribute('aria-expanded') === 'true') {
          $settingsToggle.classList.remove('sa11y-settings-active');
          $settingsPanel.classList.remove('sa11y-active');
          $settingsToggle.textContent = `${Lang._('SHOW_SETTINGS')}`;
          $settingsToggle.setAttribute('aria-expanded', 'false');
        } else {
          $settingsToggle.classList.add('sa11y-settings-active');
          $settingsPanel.classList.add('sa11y-active');
          $settingsToggle.textContent = `${Lang._('HIDE_SETTINGS')}`;
          $settingsToggle.setAttribute('aria-expanded', 'true');
        }

        // Set focus on Settings heading for accessibility.
        document.querySelector('#sa11y-settings-header > h2').focus();

        // Close Show Outline panel when Settings is active.
        $outlinePanel.classList.remove('sa11y-active');
        $outlineToggle.classList.remove('sa11y-outline-active');
        $outlineToggle.setAttribute('aria-expanded', 'false');
        $outlineToggle.textContent = `${Lang._('SHOW_OUTLINE')}`;
        $headingAnnotations.forEach(($el) => $el.classList.remove('sa11y-label-visible'));
        this.store.setItem('sa11y-remember-outline', 'Closed');

        // Keyboard accessibility fix for scrollable panel content.
        if ($settingsContent.clientHeight > 350) {
          $settingsContent.setAttribute('tabindex', '0');
          $settingsContent.setAttribute('aria-label', `${Lang._('SETTINGS')}`);
          $settingsContent.setAttribute('role', 'region');
        }
      });

      // Enhanced keyboard accessibility for panel.
      document.getElementById('sa11y-panel-controls').addEventListener('keydown', (e) => {
        const $tab = document.querySelectorAll('#sa11y-outline-toggle[role=tab], #sa11y-settings-toggle[role=tab]');
        if (e.key === 'ArrowRight') {
          for (let i = 0; i < $tab.length; i++) {
            if ($tab[i].getAttribute('aria-expanded') === 'true' || $tab[i].getAttribute('aria-expanded') === 'false') {
              $tab[i + 1].focus();
              e.preventDefault();
              break;
            }
          }
        }
        if (e.key === 'ArrowDown') {
          for (let i = 0; i < $tab.length; i++) {
            if ($tab[i].getAttribute('aria-expanded') === 'true' || $tab[i].getAttribute('aria-expanded') === 'false') {
              $tab[i + 1].focus();
              e.preventDefault();
              break;
            }
          }
        }
        if (e.key === 'ArrowLeft') {
          for (let i = $tab.length - 1; i > 0; i--) {
            if ($tab[i].getAttribute('aria-expanded') === 'true' || $tab[i].getAttribute('aria-expanded') === 'false') {
              $tab[i - 1].focus();
              e.preventDefault();
              break;
            }
          }
        }
        if (e.key === 'ArrowUp') {
          for (let i = $tab.length - 1; i > 0; i--) {
            if ($tab[i].getAttribute('aria-expanded') === 'true' || $tab[i].getAttribute('aria-expanded') === 'false') {
              $tab[i - 1].focus();
              e.preventDefault();
              break;
            }
          }
        }
      });
    };

    // ============================================================
    // Main panel: Skip to issue button.
    // ============================================================

    this.skipToIssue = () => {
      // Constants
      const $findButtons = document.querySelectorAll('[data-sa11y-annotation]');
      const $skipToggle = document.getElementById('sa11y-cycle-toggle');
      const findSa11yBtn = document.querySelectorAll('[data-sa11y-annotation]').length;
      let i = -1;

      // Add pulsing border to visible parent of hidden element.
      const hiddenParent = () => {
        $findButtons.forEach(($el) => {
          const overflowing = this.findVisibleParent($el, 'display', 'none');
          if (overflowing !== null) {
            const hiddenparent = overflowing.previousElementSibling;
            if (hiddenparent) {
              this.addPulse(hiddenparent);
            } else {
              this.addPulse(overflowing.parentNode);
            }
          }
        });
      };

      // Find scroll position.
      const scrollPosition = ($el) => {
        const offsetTopPosition = $el.offsetTop;
        if (offsetTopPosition === 0) {
          const visiblePosition = this.findVisibleParent($el, 'display', 'none');

          // Alert if tooltip is hidden.
          const tooltip = $findButtons[i].getAttribute('data-tippy-content');
          this.createAlert(`${Lang._('NOT_VISIBLE_ALERT')}`, tooltip);

          if (visiblePosition) {
            // Get as close to the hidden parent as possible.
            const prevSibling = visiblePosition.previousElementSibling;
            const { parentNode } = visiblePosition;
            if (prevSibling) {
              return this.offsetTop(prevSibling).top - 150;
            }
            return this.offsetTop(parentNode).top - 150;
          }
        }
        this.removeAlert();
        $skipToggle.focus();
        return this.offsetTop($el).top - 150;
      };

      // Skip to next.
      const next = () => {
        i += 1;
        const $el = $findButtons[i];
        const scrollPos = scrollPosition($el);
        window.scrollTo({
          top: scrollPos,
          behavior: `${this.scrollBehaviour}`,
        });
        if (i >= findSa11yBtn - 1) {
          i = -1;
        }
        hiddenParent();
        $el.focus();
      };

      // Skip to previous.
      const prev = () => {
        i = Math.max(0, i -= 1);
        const $el = $findButtons[i];
        if ($el) {
          const scrollPos = scrollPosition($el);
          window.scrollTo({
            top: scrollPos,
            behavior: `${this.scrollBehaviour}`,
          });
          hiddenParent();
          $el.focus();
        }
      };

      // Jump to issue using keyboard shortcut.
      document.addEventListener('keyup', (e) => {
        e.preventDefault();
        if (findSa11yBtn && (e.altKey && (e.code === 'Period' || e.code === 'KeyS'))) {
          next();
        } else if (findSa11yBtn && (e.altKey && (e.code === 'Comma' || e.code === 'KeyW'))) {
          prev();
        }
      });

      // Jump to issue using click.
      $skipToggle.addEventListener('click', (e) => {
        e.preventDefault();
        next();
      });
    };

    // ============================================================
    // Finds all elements and cache.
    // ============================================================
    this.findElements = () => {
      // Sa11y's panel container
      this.panel = document.getElementById('sa11y-container');

      // Query DOM.
      const find = (selectors, exclude, rootType) => {
        let root;
        if (rootType === 'readability') {
          root = document.querySelector(option.readabilityRoot);
        } else if (rootType === 'document') {
          root = document;
        } else {
          root = document.querySelector(option.checkRoot);
        }
        if (!root) {
          root = document.querySelector('body');
        }
        const exclusions = Array.from(document.querySelectorAll(exclude));
        const queryDOM = Array.from(root.querySelectorAll(selectors));
        const filtered = queryDOM.filter(($el) => !exclusions.includes($el));
        return filtered;
      };

      // Main selectors
      this.contrast = find('*', this.contrastIgnore);
      this.images = find('img', this.imageIgnore);
      this.headings = find('h1, h2, h3, h4, h5, h6, [role="heading"][aria-level]', this.headerIgnore);
      this.headingOne = find('h1, [role="heading"][aria-level="1"]', this.headerIgnore, 'document');
      this.links = find('a[href]', this.linkIgnore);
      this.readability = find('p, li', this.readabilityIgnore, 'readability');

      // Quality assurance module.
      this.language = document.querySelector('html').getAttribute('lang');
      this.paragraphs = find('p', this.containerIgnore);
      this.lists = find('li', this.containerIgnore);
      this.spans = find('span', this.containerIgnore);
      this.blockquotes = find('blockquote', this.containerIgnore);
      this.tables = find('table:not([role="presentation"])', this.containerIgnore);
      this.pdf = find('a[href$=".pdf"]', this.containerIgnore);
      this.strongitalics = find('strong, em', this.containerIgnore);
      this.inputs = find('input, select, textarea', this.containerIgnore);
      this.customErrorLinks = option.linksToFlag ? find(option.linksToFlag, this.containerIgnore) : [];

      // iFrames
      this.iframes = find('iframe, audio, video', this.containerIgnore);
      this.videos = this.iframes.filter(($el) => $el.matches(option.videoContent));
      this.audio = this.iframes.filter(($el) => $el.matches(option.audioContent));
      this.datavisualizations = this.iframes.filter(($el) => $el.matches(option.dataVizContent));
      this.embeddedContent = this.iframes.filter(($el) => !$el.matches(option.embeddedContent));
    };

    //----------------------------------------------------------------------
    // Templating for Error, Warning and Pass buttons.
    //----------------------------------------------------------------------
    this.annotate = (type, content, inline = false) => {
      let message = content;

      const validTypes = [
        ERROR,
        WARNING,
        GOOD,
      ];

      if (validTypes.indexOf(type) === -1) {
        throw Error(`Invalid type [${type}] for annotation`);
      }

      // Update error or warning count.
      [type].forEach(($el) => {
        if ($el === ERROR) {
          this.errorCount += 1;
        } else if ($el === WARNING) {
          this.warningCount += 1;
        }
      });

      const CSSName = {
        [validTypes[0]]: 'error',
        [validTypes[1]]: 'warning',
        [validTypes[2]]: 'good',
      };

      // Make translations easier.
      message = message
        .replaceAll(/<hr>/g, '<hr aria-hidden="true">')
        .replaceAll(/<a[\s]href=/g, '<a target="_blank" rel="noopener noreferrer" href=')
        .replaceAll(/<\/a>/g, `<span class="sa11y-visually-hidden"> (${Lang._('NEW_TAB')})</span></a>`)
        .replaceAll(/{r}/g, 'class="sa11y-red-text"');

      message = this.escapeHTML(message);

      return `<div class=${inline ? 'sa11y-instance-inline' : 'sa11y-instance'}>
                <button data-sa11y-annotation type="button" aria-label="${[type]}" class="sa11y-btn sa11y-${CSSName[type]}-btn${inline ? '-text' : ''}" data-tippy-content="<div lang='${Lang._('LANG_CODE')}'><div class='sa11y-header-text'>${[type]}</div>${message}</div>"></button>
              </div>`;
    };

    //----------------------------------------------------------------------
    // Templating for full-width banners.
    //----------------------------------------------------------------------
    this.annotateBanner = (type, content) => {
      let message = content;

      const validTypes = [
        ERROR,
        WARNING,
        GOOD,
      ];

      if (validTypes.indexOf(type) === -1) {
        throw Error(`Invalid type [${type}] for annotation`);
      }

      const CSSName = {
        [validTypes[0]]: 'error',
        [validTypes[1]]: 'warning',
        [validTypes[2]]: 'good',
      };

      // Update error or warning count.
      [type].forEach(($el) => {
        if ($el === ERROR) {
          this.errorCount += 1;
        } else if ($el === WARNING) {
          this.warningCount += 1;
        }
      });

      // Check if content is a function & make translations easier.
      if (message && {}.toString.call(message) === '[object Function]') {
        message = message
          .replaceAll(/<hr>/g, '<hr aria-hidden="true">')
          .replaceAll(/<a[\s]href=/g, '<a target="_blank" rel="noopener noreferrer" href=')
          .replaceAll(/<\/a>/g, `<span class="sa11y-visually-hidden"> (${Lang._('NEW_TAB')})</span></a>`)
          .replaceAll(/{r}/g, 'class="sa11y-red-text"');
        message = this.escapeHTML(message);
      }

      return `<div class="sa11y-instance sa11y-${CSSName[type]}-message-container"><div role="region" data-sa11y-annotation tabindex="-1" aria-label="${[type]}" class="sa11y-${CSSName[type]}-message" lang="${Lang._('LANG_CODE')}">${message}</div></div>`;
    };

    // ============================================================
    // Rulesets: Check Headings
    // ============================================================
    this.checkHeaders = () => {
      let prevLevel;
      this.headings.forEach(($el, i) => {
        const text = this.computeTextNodeWithImage($el);
        const htext = this.sanitizeForHTML(text);
        let level;

        if ($el.getAttribute('aria-level')) {
          level = +$el.getAttribute('aria-level');
        } else {
          level = +$el.tagName.slice(1);
        }

        const headingLength = $el.textContent.trim().length;
        let error = null;
        let warning = null;

        if (level - prevLevel > 1 && i !== 0) {
          if (option.nonConsecutiveHeadingIsError === true) {
            error = Lang.sprintf('HEADING_NON_CONSECUTIVE_LEVEL', prevLevel, level);
          } else {
            warning = Lang.sprintf('HEADING_NON_CONSECUTIVE_LEVEL', prevLevel, level);
          }
        } else if ($el.textContent.trim().length === 0) {
          if ($el.querySelectorAll('img').length) {
            const imgalt = $el.querySelector('img').getAttribute('alt');
            if (imgalt === null || imgalt === ' ' || imgalt === '') {
              error = Lang.sprintf('HEADING_EMPTY_WITH_IMAGE', level);
              $el.classList.add('sa11y-error-text');
            }
          } else {
            error = Lang.sprintf('HEADING_EMPTY', level);
            $el.classList.add('sa11y-error-text');
          }
        } else if (i === 0 && level !== 1 && level !== 2) {
          error = Lang._('HEADING_FIRST');
        } else if ($el.textContent.trim().length > 170 && option.flagLongHeadings === true) {
          warning = Lang.sprintf('HEADING_LONG', headingLength);
        }

        prevLevel = level;

        // Indicate if heading is totally hidden or visually hidden.
        const headingHidden = this.isElementHidden($el);
        const visibleIcon = (headingHidden === true || ($el.clientHeight === 1 && $el.clientWidth === 1)) ? '<span class="sa11y-hidden-icon"></span><span class="sa11y-visually-hidden">Hidden</span>' : '';
        const visibleStatus = (headingHidden === true || ($el.clientHeight === 1 && $el.clientWidth === 1)) ? 'class="sa11y-hidden-h"' : '';

        // Normal heading.
        const li = `<li class='sa11y-outline-${level}'>
                  <a id="sa11y-link-${i}" tabindex="-1" ${visibleStatus}>
                    <span class='sa11y-badge'>${visibleIcon} ${level}</span>
                    <span class='sa11y-outline-list-item'>${htext}</span>
                  </a>
                </li>`;

        // Error heading.
        const liError = `<li class='sa11y-outline-${level}'>
                  <a id="sa11y-link-${i}" tabindex="-1" ${visibleStatus}>
                    <span class='sa11y-badge sa11y-error-badge'>
                    <span aria-hidden='true'>${visibleIcon} &#33;</span>
                    <span class='sa11y-visually-hidden'>${Lang._('ERROR')}</span> ${level}</span>
                    <span class='sa11y-outline-list-item sa11y-red-text sa11y-bold'>${htext}</span>
                  </a>
                </li>`;

        // Warning heading.
        const liWarning = `<li class='sa11y-outline-${level}'>
                  <a id="sa11y-link-${i}" tabindex="-1" ${visibleStatus}>
                    <span class='sa11y-badge sa11y-warning-badge'>
                    <span aria-hidden='true'>${visibleIcon} &#x3f;</span>
                    <span class='sa11y-visually-hidden'>${Lang._('WARNING')}</span> ${level}</span>
                    <span class='sa11y-outline-list-item sa11y-yellow-text sa11y-bold'>${htext}</span>
                  </a>
                </li>`;

        let ignoreArray = [];
        if (option.outlineIgnore) {
          ignoreArray = Array.from(document.querySelectorAll(this.outlineIgnore));
        }

        const outline = document.querySelector('#sa11y-outline-list');
        if (!ignoreArray.includes($el)) {
          // Append annotations & update panel.
          if (error !== null && $el.closest('a') !== null) {
            $el.classList.add('sa11y-error-border');
            $el.closest('a').insertAdjacentHTML('afterend', this.annotate(ERROR, error, true));
            outline.insertAdjacentHTML('beforeend', liError);
          } else if (error !== null) {
            $el.classList.add('sa11y-error-border');
            $el.insertAdjacentHTML('beforebegin', this.annotate(ERROR, error));
            outline.insertAdjacentHTML('beforeend', liError);
          } else if (warning !== null && $el.closest('a') !== null) {
            $el.closest('a').insertAdjacentHTML('afterend', this.annotate(WARNING, warning));
            outline.insertAdjacentHTML('beforeend', liWarning);
          } else if (warning !== null) {
            $el.insertAdjacentHTML('beforebegin', this.annotate(WARNING, warning));
            outline.insertAdjacentHTML('beforeend', liWarning);
          } else if (error === null || warning === null) {
            outline.insertAdjacentHTML('beforeend', li);
          }
        }

        const sa11yToggle = document.getElementById('sa11y-toggle');
        if (sa11yToggle.classList.contains('sa11y-on')) {
          // Append heading labels. Although if the heading is in a hidden container, place the anchor just before it's most visible parent.
          const hiddenH = this.findVisibleParent($el, 'display', 'none');
          const plainHeadingLabel = `<span class="sa11y-heading-label">H${level}</span>`;
          const anchor = `<span class="sa11y-element" id="sa11y-h${i}"></span>`;
          if (hiddenH !== null) {
            const hiddenParent = hiddenH.previousElementSibling;
            $el.insertAdjacentHTML('beforeend', plainHeadingLabel);
            if (hiddenParent) {
              hiddenParent.insertAdjacentHTML('beforebegin', anchor);
              hiddenParent.setAttribute('data-sa11y-parent', `h${i}`);
            } else {
              hiddenH.parentNode.insertAdjacentHTML('beforebegin', anchor);
              hiddenH.parentNode.setAttribute('data-sa11y-parent', `h${i}`);
            }
          } else {
            // If the heading isn't hidden, then append id on visible label.
            $el.insertAdjacentHTML('beforeend', `<span id="sa11y-h${i}" class="sa11y-heading-label">H${level}</span>`);
          }

          // Make Page Outline clickable.
          setTimeout(() => {
            const outlineLink = document.getElementById(`sa11y-link-${i}`);
            const alertActive = document.getElementById('sa11y-panel-alert');
            const hID = document.getElementById(`sa11y-h${i}`);
            const hParent = document.querySelector(`[data-sa11y-parent="h${i}"]`);
            const smooth = () => hID.scrollIntoView({ behavior: `${this.scrollBehaviour}`, block: 'center' });
            const pulse = () => ((hParent !== null) ? this.addPulse(hParent) : this.addPulse($el));
            const smoothPulse = (e) => {
              if ((e.type === 'keyup' && e.code === 'Enter') || e.type === 'click') {
                smooth();
                pulse();
                if (outlineLink.classList.contains('sa11y-hidden-h')) {
                  this.createAlert(`${Lang._('HEADING_NOT_VISIBLE_ALERT')}`);
                } else if (alertActive.classList.contains('sa11y-active')) {
                  this.removeAlert();
                }
              }
              e.preventDefault();
            };
            outlineLink.addEventListener('click', smoothPulse, false);
            outlineLink.addEventListener('keyup', smoothPulse, false);
          }, 50);
        }
      });

      // Check to see there is at least one H1 on the page.
      if (this.headingOne.length === 0) {
        const updateH1Outline = `<div class='sa11y-instance sa11y-missing-h1'>
                    <span class='sa11y-badge sa11y-error-badge'><span aria-hidden='true'>!</span><span class='sa11y-visually-hidden'>${Lang._('ERROR')}</span></span>
                    <span class='sa11y-red-text sa11y-bold'>${Lang._('PANEL_HEADING_MISSING_ONE')}</span>
                </div>`;
        document.getElementById('sa11y-outline-header').insertAdjacentHTML('afterend', updateH1Outline);
        this.panel.insertAdjacentHTML('afterend', this.annotateBanner(ERROR, Lang._('HEADING_MISSING_ONE')));
      }
    };

    // ============================================================
    // Rulesets: Link text
    // ============================================================
    this.checkLinkText = () => {
      const containsLinkTextStopWords = (textContent) => {
        const urlText = [
          'http',
          '.asp',
          '.htm',
          '.php',
          '.edu/',
          '.com/',
          '.net/',
          '.org/',
          '.us/',
          '.ca/',
          '.de/',
          '.icu/',
          '.uk/',
          '.ru/',
          '.info/',
          '.top/',
          '.xyz/',
          '.tk/',
          '.cn/',
          '.ga/',
          '.cf/',
          '.nl/',
          '.io/',
          '.fr/',
          '.pe/',
          '.nz/',
          '.pt/',
          '.es/',
          '.pl/',
          '.ua/',
        ];

        const hit = [null, null, null];

        // Flag partial stop words.
        Lang._('PARTIAL_ALT_STOPWORDS').forEach((word) => {
          if (
            textContent.length === word.length && textContent.toLowerCase().indexOf(word) >= 0
          ) {
            hit[0] = word;
          }
          return false;
        });

        // Other warnings we want to add.
        Lang._('WARNING_ALT_STOPWORDS').forEach((word) => {
          if (textContent.toLowerCase().indexOf(word) >= 0) {
            hit[1] = word;
          }
          return false;
        });

        // Flag link text containing URLs.
        urlText.forEach((word) => {
          if (textContent.toLowerCase().indexOf(word) >= 0) {
            hit[2] = word;
          }
          return false;
        });
        return hit;
      };

      this.links.forEach(($el) => {
        let linkText = this.computeAriaLabel($el);
        const hasAriaLabelledBy = $el.getAttribute('aria-labelledby');
        const hasAriaLabel = $el.getAttribute('aria-label');
        let childAriaLabelledBy = null;
        let childAriaLabel = null;
        const hasTitle = $el.getAttribute('title');

        if ($el.children.length) {
          const $firstChild = $el.children[0];
          childAriaLabelledBy = $firstChild.getAttribute('aria-labelledby');
          childAriaLabel = $firstChild.getAttribute('aria-label');
        }

        if (linkText === 'noAria') {
          // Plain text content.
          linkText = this.getText($el);
          const $img = $el.querySelector('img');

          // If an image exists within the link. Help with AccName computation.
          if ($img) {
            // Check if there's aria on the image.
            const imgText = this.computeAriaLabel($img);
            if (imgText !== 'noAria') {
              linkText += imgText;
            } else {
              // No aria? Process alt on image.
              linkText += $img ? ($img.getAttribute('alt') || '') : '';
            }
          }
        }

        const error = containsLinkTextStopWords(this.fnIgnore($el, option.linkIgnoreSpan).textContent.replace(/[!*?↣↳→↓»↴]/g, '').trim());

        if ($el.querySelectorAll('img').length) {
          // Do nothing. Don't overlap with Alt Text module.
        } else if ($el.getAttribute('href') && !linkText) {
          // Flag empty hyperlinks.
          if ($el && hasTitle) {
            // If empty but has title attribute.
          } else if ($el.children.length) {
            // Has child elements (e.g. SVG or SPAN) <a><i></i></a>
            $el.classList.add('sa11y-error-border');
            $el.insertAdjacentHTML('afterend', this.annotate(ERROR, Lang._('LINK_EMPTY_LINK_NO_LABEL'), true));
          } else {
            // Completely empty <a></a>
            $el.classList.add('sa11y-error-border');
            $el.insertAdjacentHTML('afterend', this.annotate(ERROR, Lang._('LINK_EMPTY'), true));
          }
        } else if (error[0] != null) {
          // Contains stop words.
          if (hasAriaLabelledBy || hasAriaLabel || childAriaLabelledBy || childAriaLabel) {
            if (option.showGoodLinkButton === true) {
              $el.insertAdjacentHTML(
                'beforebegin',
                this.annotate(GOOD, Lang.sprintf('LINK_LABEL', linkText), true),
              );
            }
          } else if ($el.getAttribute('aria-hidden') === 'true' && $el.getAttribute('tabindex') === '-1') {
            // Do nothing.
          } else {
            $el.classList.add('sa11y-error-text');
            $el.insertAdjacentHTML(
              'afterend',
              this.annotate(ERROR, Lang.sprintf('LINK_STOPWORD', error[0]), true),
            );
          }
        } else if (error[1] != null) {
          // Contains warning words.
          $el.classList.add('sa11y-warning-text');
          $el.insertAdjacentHTML(
            'afterend',
            this.annotate(WARNING, Lang.sprintf('LINK_BEST_PRACTICES', error[1]), true),
          );
        } else if (error[2] != null) {
          // Contains URL in link text.
          if (linkText.length > 40) {
            $el.classList.add('sa11y-warning-text');
            $el.insertAdjacentHTML('afterend', this.annotate(WARNING, Lang._('LINK_URL'), true));
          }
        } else if (hasAriaLabelledBy || hasAriaLabel || childAriaLabelledBy || childAriaLabel) {
          // If the link has any ARIA, append a "Good" link button.
          if (option.showGoodLinkButton === true) {
            $el.insertAdjacentHTML(
              'beforebegin',
              this.annotate(GOOD, Lang.sprintf('LINK_LABEL', linkText), true),
            );
          }
        }
      });
    };

    // ============================================================
    // Rulesets: Links (Advanced)
    // ============================================================
    this.checkLinksAdvanced = () => {
      const seen = {};
      this.links.forEach(($el) => {
        let linkText = this.computeAriaLabel($el);
        const $img = $el.querySelector('img');

        if (linkText === 'noAria') {
          // Plain text content.
          linkText = this.getText($el);

          // If an image exists within the link.
          if ($img) {
            // Check if there's aria on the image.
            const imgText = this.computeAriaLabel($img);
            if (imgText !== 'noAria') {
              linkText += imgText;
            } else {
              // No aria? Process alt on image.
              linkText += $img ? ($img.getAttribute('alt') || '') : '';
            }
          }
        }

        // Remove whitespace, special characters, etc.
        const linkTextTrimmed = linkText.replace(/'|"|-|\.|\s+/g, '').toLowerCase();

        // Links with identical accessible names have equivalent purpose.
        const href = $el.getAttribute('href');

        if (linkText.length !== 0) {
          if (seen[linkTextTrimmed] && linkTextTrimmed.length !== 0) {
            if (seen[href]) {
              // Nothing
            } else {
              $el.classList.add('sa11y-warning-text');
              $el.insertAdjacentHTML(
                'afterend',
                this.annotate(WARNING, Lang.sprintf('LINK_IDENTICAL_NAME', linkText), true),
              );
            }
          } else {
            seen[linkTextTrimmed] = true;
            seen[href] = true;
          }
        }

        // New tab or new window.
        const containsNewWindowPhrases = Lang._('NEW_WINDOW_PHRASES').some((pass) => {
          if (linkText.trim().length === 0 && !!$el.getAttribute('title')) {
            linkText = $el.getAttribute('title');
          }
          return linkText.toLowerCase().indexOf(pass) >= 0;
        });

        // Link that points to a file type indicates that it does.
        const containsFileTypePhrases = Lang._('FILE_TYPE_PHRASES').some((pass) => linkText.toLowerCase().indexOf(pass) >= 0);

        const fileTypeMatch = $el.matches(`
          a[href$='.pdf'],
          a[href$='.doc'],
          a[href$='.docx'],
          a[href$='.zip'],
          a[href$='.mp3'],
          a[href$='.txt'],
          a[href$='.exe'],
          a[href$='.dmg'],
          a[href$='.rtf'],
          a[href$='.pptx'],
          a[href$='.ppt'],
          a[href$='.xls'],
          a[href$='.xlsx'],
          a[href$='.csv'],
          a[href$='.mp4'],
          a[href$='.mov'],
          a[href$='.avi']
        `);

        if ($el.getAttribute('target') === '_blank' && !fileTypeMatch && !containsNewWindowPhrases) {
          $el.classList.add('sa11y-warning-text');
          $el.insertAdjacentHTML(
            'afterend',
            this.annotate(WARNING, Lang._('NEW_TAB_WARNING'), true),
          );
        }

        if (fileTypeMatch && !containsFileTypePhrases) {
          $el.classList.add('sa11y-warning-text');
          $el.insertAdjacentHTML(
            'beforebegin',
            this.annotate(WARNING, Lang._('FILE_TYPE_WARNING'), true),
          );
        }
      });
    };

    // ============================================================
    // Ruleset: Alternative text
    // ============================================================
    this.checkAltText = () => {
      this.containsAltTextStopWords = (alt) => {
        const altUrl = [
          '.png',
          '.jpg',
          '.jpeg',
          '.webp',
          '.gif',
          '.tiff',
          '.svg',
        ];

        const hit = [null, null, null];
        altUrl.forEach((word) => {
          if (alt.toLowerCase().indexOf(word) >= 0) {
            hit[0] = word;
          }
        });
        Lang._('SUSPICIOUS_ALT_STOPWORDS').forEach((word) => {
          if (alt.toLowerCase().indexOf(word) >= 0) {
            hit[1] = word;
          }
        });
        Lang._('PLACEHOLDER_ALT_STOPWORDS').forEach((word) => {
          if (alt.length === word.length && alt.toLowerCase().indexOf(word) >= 0) {
            hit[2] = word;
          }
        });
        return hit;
      };

      this.images.forEach(($el) => {
        const alt = $el.getAttribute('alt');
        if (alt === null) {
          if ($el.closest('a[href]')) {
            if (this.fnIgnore($el.closest('a[href]'), 'noscript').textContent.trim().length >= 1) {
              $el.classList.add('sa11y-error-border');
              $el.closest('a[href]').insertAdjacentHTML('beforebegin', this.annotate(ERROR, Lang._('MISSING_ALT_LINK_BUT_HAS_TEXT_MESSAGE')));
            } else if (this.fnIgnore($el.closest('a[href]'), 'noscript').textContent.trim().length === 0) {
              $el.classList.add('sa11y-error-border');
              $el.closest('a[href]').insertAdjacentHTML('beforebegin', this.annotate(ERROR, Lang._('MISSING_ALT_LINK_MESSAGE')));
            }
          } else {
            // General failure message if image is missing alt.
            $el.classList.add('sa11y-error-border');
            $el.insertAdjacentHTML('beforebegin', this.annotate(ERROR, Lang._('MISSING_ALT_MESSAGE')));
          }
        } else {
          // If alt attribute is present, further tests are done.
          const altText = this.sanitizeForHTML(alt); // Prevent tooltip from breaking.
          const error = this.containsAltTextStopWords(altText);
          const altLength = alt.length;

          if ($el.closest('a[href]') && $el.closest('a[href]').getAttribute('tabindex') === '-1' && $el.closest('a[href]').getAttribute('aria-hidden') === 'true') {
            // Do nothing if link has aria-hidden and negative tabindex.
          } else if (error[0] !== null && $el.closest('a[href]')) {
            // Image fails if a stop word was found.
            $el.classList.add('sa11y-error-border');
            $el.closest('a[href]').insertAdjacentHTML('beforebegin', this.annotate(ERROR, Lang.sprintf('LINK_IMAGE_BAD_ALT_MESSAGE', error[0], altText)));
          } else if (error[2] !== null && $el.closest('a[href]')) {
            $el.classList.add('sa11y-error-border');
            $el.closest('a[href]').insertAdjacentHTML('beforebegin', this.annotate(ERROR, Lang.sprintf('LINK_IMAGE_PLACEHOLDER_ALT_MESSAGE', altText)));
          } else if (error[1] !== null && $el.closest('a[href]')) {
            $el.classList.add('sa11y-warning-border');
            $el.closest('a[href]').insertAdjacentHTML('beforebegin', this.annotate(WARNING, Lang.sprintf('LINK_IMAGE_SUS_ALT_MESSAGE', error[1], altText)));
          } else if (error[0] !== null) {
            $el.classList.add('sa11y-error-border');
            $el.insertAdjacentHTML('beforebegin', this.annotate(ERROR, Lang.sprintf('LINK_ALT_HAS_BAD_WORD_MESSAGE', error[0], altText)));
          } else if (error[2] !== null) {
            $el.classList.add('sa11y-error-border');
            $el.insertAdjacentHTML('beforebegin', this.annotate(ERROR, Lang.sprintf('ALT_PLACEHOLDER_MESSAGE', altText)));
          } else if (error[1] !== null) {
            $el.classList.add('sa11y-warning-border');
            $el.insertAdjacentHTML('beforebegin', this.annotate(WARNING, Lang.sprintf('ALT_HAS_SUS_WORD', error[1], altText)));
          } else if ((alt === '' || alt === ' ') && $el.closest('a[href]')) {
            if ($el.closest('a[href]').getAttribute('tabindex') === '-1' && $el.closest('a[href]').getAttribute('aria-hidden') === 'true') {
              // Do nothing.
            } else if ($el.closest('a[href]').getAttribute('aria-hidden') === 'true') {
              $el.classList.add('sa11y-error-border');
              $el.closest('a[href]').insertAdjacentHTML('beforebegin', this.annotate(ERROR, Lang._('LINK_IMAGE_ARIA_HIDDEN')));
            } else if (this.fnIgnore($el.closest('a[href]'), 'noscript').textContent.trim().length === 0) {
              $el.classList.add('sa11y-error-border');
              $el.closest('a[href]').insertAdjacentHTML('beforebegin', this.annotate(ERROR, Lang._('LINK_IMAGE_NO_ALT_TEXT')));
            } else {
              $el.closest('a[href]').insertAdjacentHTML('beforebegin', this.annotate(GOOD, Lang._('LINK_IMAGE_HAS_TEXT')));
            }
          } else if (alt.length > 250 && $el.closest('a[href]')) {
            // Link and contains alt text.
            $el.classList.add('sa11y-warning-border');
            $el.closest('a[href]').insertAdjacentHTML('beforebegin', this.annotate(WARNING, Lang.sprintf('LINK_IMAGE_LONG_ALT', altLength, altText)));
          } else if (alt !== '' && $el.closest('a[href]') && this.fnIgnore($el.closest('a[href]'), 'noscript').textContent.trim().length === 0) {
            // Link and contains an alt text.
            $el.classList.add('sa11y-warning-border');
            $el.closest('a[href]').insertAdjacentHTML('beforebegin', this.annotate(WARNING, Lang.sprintf('LINK_IMAGE_ALT_WARNING', altText)));
          } else if (alt !== '' && $el.closest('a[href]') && this.fnIgnore($el.closest('a[href]'), 'noscript').textContent.trim().length >= 1) {
            // Contains alt text & surrounding link text.
            $el.classList.add('sa11y-warning-border');
            $el.closest('a[href]').insertAdjacentHTML('beforebegin', this.annotate(WARNING, Lang.sprintf('LINK_IMAGE_ALT_AND_TEXT_WARNING', altText)));
          } else if (alt === '' || alt === ' ') {
            // Decorative alt and not a link.
            if ($el.closest('figure')) {
              const figcaption = $el.closest('figure').querySelector('figcaption');
              if (figcaption !== null && figcaption.textContent.trim().length >= 1) {
                $el.classList.add('sa11y-warning-border');
                $el.insertAdjacentHTML('beforebegin', this.annotate(WARNING, Lang._('IMAGE_FIGURE_DECORATIVE')));
              } else {
                $el.classList.add('sa11y-warning-border');
                $el.insertAdjacentHTML('beforebegin', this.annotate(WARNING, Lang._('IMAGE_DECORATIVE')));
              }
            } else {
              $el.classList.add('sa11y-warning-border');
              $el.insertAdjacentHTML('beforebegin', this.annotate(WARNING, Lang._('IMAGE_DECORATIVE')));
            }
          } else if (alt.length > 250) {
            $el.classList.add('sa11y-warning-border');
            $el.insertAdjacentHTML('beforebegin', this.annotate(WARNING, Lang.sprintf('IMAGE_ALT_TOO_LONG', altLength, altText)));
          } else if (alt !== '') {
            // Figure element has same alt and caption text.
            if ($el.closest('figure')) {
              const figcaption = $el.closest('figure').querySelector('figcaption');
              if (!!figcaption
                && (figcaption.textContent.trim().toLowerCase() === altText.trim().toLowerCase())) {
                $el.classList.add('sa11y-warning-border');
                $el.insertAdjacentHTML('beforebegin', this.annotate(WARNING, Lang.sprintf('IMAGE_FIGURE_DUPLICATE_ALT', altText)));
              } else {
                $el.insertAdjacentHTML('beforebegin', this.annotate(GOOD, Lang.sprintf('IMAGE_PASS', altText)));
              }
            } else {
              // If image has alt text - pass!
              $el.insertAdjacentHTML('beforebegin', this.annotate(GOOD, Lang.sprintf('IMAGE_PASS', altText)));
            }
          }
        }
      });
    };

    // ============================================================
    // Rulesets: Labels
    // ============================================================
    this.checkLabels = () => {
      this.inputs.forEach(($el) => {
        // Ignore hidden inputs.
        if (this.isElementHidden($el) !== true) {
          let ariaLabel = this.computeAriaLabel($el);
          const type = $el.getAttribute('type');
          const tabindex = $el.getAttribute('tabindex');

          // If button type is submit or button: pass
          if (type === 'submit' || type === 'button' || type === 'hidden' || tabindex === '-1') {
            // Do nothing
          } else if (type === 'image') {
            // Inputs where type="image".
            const imgalt = $el.getAttribute('alt');
            if (!imgalt || imgalt === ' ') {
              if ($el.getAttribute('aria-label')) {
                // Good.
              } else {
                $el.classList.add('sa11y-error-border');
                $el.insertAdjacentHTML('afterend', this.annotate(ERROR, Lang._('LABELS_MISSING_IMAGE_INPUT_MESSAGE'), true));
              }
            }
          } else if (type === 'reset') {
            // Recommendation to remove reset buttons.
            $el.classList.add('sa11y-warning-border');
            $el.insertAdjacentHTML('afterend', this.annotate(WARNING, Lang._('LABELS_INPUT_RESET_MESSAGE'), true));
          } else if ($el.getAttribute('aria-label') || $el.getAttribute('aria-labelledby') || $el.getAttribute('title')) {
            // Uses ARIA. Warn them to ensure there's a visible label.
            if ($el.getAttribute('title')) {
              ariaLabel = $el.getAttribute('title');
              $el.classList.add('sa11y-warning-border');
              $el.insertAdjacentHTML('afterend', this.annotate(WARNING, Lang.sprintf('LABELS_ARIA_LABEL_INPUT_MESSAGE', ariaLabel), true));
            } else {
              $el.classList.add('sa11y-warning-border');
              $el.insertAdjacentHTML('afterend', this.annotate(WARNING, Lang.sprintf('LABELS_ARIA_LABEL_INPUT_MESSAGE', ariaLabel), true));
            }
          } else if ($el.closest('label') && $el.closest('label').textContent.trim()) {
            // Implicit labels.
            // Do nothing if label has text.
          } else if ($el.getAttribute('id')) {
            // Has an ID but doesn't have a matching FOR attribute.
            const $labels = this.root.querySelectorAll('label');
            let hasFor = false;

            $labels.forEach(($l) => {
              if (hasFor) return;
              if ($l.getAttribute('for') === $el.getAttribute('id')) {
                hasFor = true;
              }
            });

            if (!hasFor) {
              $el.classList.add('sa11y-error-border');
              const id = $el.getAttribute('id');
              $el.insertAdjacentHTML('afterend', this.annotate(ERROR, Lang.sprintf('LABELS_NO_FOR_ATTRIBUTE_MESSAGE', id), true));
            }
          } else {
            $el.classList.add('sa11y-error-border');
            $el.insertAdjacentHTML('afterend', this.annotate(ERROR, Lang._('LABELS_MISSING_LABEL_MESSAGE'), true));
          }
        }
      });
    };

    // ============================================================
    // Rulesets: Embedded content.
    // ============================================================
    this.checkEmbeddedContent = () => {
      // Warning: Audio content.
      if (option.embeddedContentAudio === true) {
        this.audio.forEach(($el) => {
          $el.classList.add('sa11y-warning-border');
          $el.insertAdjacentHTML('beforebegin', this.annotate(WARNING, Lang._('EMBED_AUDIO')));
        });
      }

      // Warning: Video content.
      if (option.embeddedContentVideo === true) {
        this.videos.forEach(($el) => {
          const track = $el.getElementsByTagName('TRACK');
          if ($el.tagName === 'VIDEO' && track.length) {
            // Pass if track element found.
          } else {
            $el.classList.add('sa11y-warning-border');
            $el.insertAdjacentHTML('beforebegin', this.annotate(WARNING, Lang._('EMBED_VIDEO')));
          }
        });
      }

      // Warning: Data visualizations.
      if (option.embeddedContentDataViz === true) {
        this.datavisualizations.forEach(($el) => {
          $el.classList.add('sa11y-warning-border');
          $el.insertAdjacentHTML('beforebegin', this.annotate(WARNING, Lang._('EMBED_DATA_VIZ')));
        });
      }

      // Error: iFrame is missing accessible name.
      if (option.embeddedContentTitles === true) {
        this.iframes.forEach(($el) => {
          if ($el.tagName === 'VIDEO'
          || $el.tagName === 'AUDIO'
          || $el.getAttribute('aria-hidden') === 'true'
          || $el.getAttribute('hidden') !== null
          || $el.style.display === 'none'
          || $el.getAttribute('role') === 'presentation') {
            // Ignore if hidden.
          } else if ($el.getAttribute('title') === null || $el.getAttribute('title') === '') {
            if ($el.getAttribute('aria-label') === null || $el.getAttribute('aria-label') === '') {
              if ($el.getAttribute('aria-labelledby') === null) {
                // Make sure red error border takes precedence
                if ($el.classList.contains('sa11y-warning-border')) {
                  $el.classList.remove('sa11y-warning-border');
                }
                $el.classList.add('sa11y-error-border');
                $el.insertAdjacentHTML('beforebegin', this.annotate(ERROR, Lang._('EMBED_MISSING_TITLE')));
              }
            }
          } else {
            // Nothing
          }
        });
      }

      // Warning: general warning for iFrames
      if (option.embeddedContentGeneral === true) {
        this.embeddedContent.forEach(($el) => {
          if ($el.tagName === 'VIDEO'
          || $el.tagName === 'AUDIO'
          || $el.getAttribute('aria-hidden') === 'true'
          || $el.getAttribute('hidden') !== null
          || $el.style.display === 'none'
          || $el.getAttribute('role') === 'presentation'
          || $el.getAttribute('tabindex') === '-1') {
            // Ignore if hidden.
          } else {
            $el.classList.add('sa11y-warning-border');
            $el.insertAdjacentHTML('beforebegin', this.annotate(WARNING, Lang._('EMBED_GENERAL_WARNING')));
          }
        });
      }
    };

    // ============================================================
    // Rulesets: QA
    // ============================================================
    this.checkQA = () => {
      // Error: Find all links pointing to development environment.
      if (option.badLinksQA === true) {
        this.customErrorLinks.forEach(($el) => {
          $el.classList.add('sa11y-error-text');
          $el.insertAdjacentHTML('afterend', this.annotate(ERROR, Lang.sprintf('QA_BAD_LINK', $el), true));
        });
      }

      // Warning: Excessive bolding or italics.
      if (option.strongItalicsQA === true) {
        this.strongitalics.forEach(($el) => {
          const strongItalicsText = $el.textContent.trim().length;
          if (strongItalicsText > 400) {
            $el.insertAdjacentHTML('beforebegin', this.annotate(WARNING, Lang._('QA_BAD_ITALICS')));
            $el.parentNode.classList.add('sa11y-warning-border');
          }
        });
      }

      // Warning: Find all PDFs.
      if (option.pdfQA === true) {
        this.pdf.forEach(($el, i) => {
          const pdfCount = this.pdf.length;

          // Highlight all PDFs.
          if (pdfCount > 0) {
            $el.classList.add('sa11y-warning-text');
          }
          // Only append warning button to first PDF.
          if ($el && i === 0) {
            $el.insertAdjacentHTML('afterend', this.annotate(WARNING, Lang.sprintf('QA_PDF', pdfCount), true));
            if ($el.querySelector('img')) {
              $el.classList.remove('sa11y-warning-text');
            }
          }
        });
      }

      // Error: Missing language tag. Lang should be at least 2 characters.
      if (option.langQA === true) {
        if (!this.language || this.language.length < 2) {
          this.panel.insertAdjacentHTML('afterend', this.annotateBanner(ERROR, Lang._('QA_PAGE_LANGUAGE')));
        }
      }

      // Warning: Find blockquotes used as headers.
      if (option.blockquotesQA === true) {
        this.blockquotes.forEach(($el) => {
          const bqHeadingText = $el.textContent;
          if (bqHeadingText.trim().length < 25) {
            $el.classList.add('sa11y-warning-border');
            $el.insertAdjacentHTML('beforebegin', this.annotate(WARNING, Lang.sprintf('QA_BLOCKQUOTE_MESSAGE', bqHeadingText)));
          }
        });
      }

      // Tables check.
      if (option.tablesQA === true) {
        this.tables.forEach(($el) => {
          const findTHeaders = $el.querySelectorAll('th');
          const findHeadingTags = $el.querySelectorAll('h1, h2, h3, h4, h5, h6');
          if (findTHeaders.length === 0) {
            $el.classList.add('sa11y-error-border');
            $el.insertAdjacentHTML('beforebegin',
              this.annotate(ERROR, Lang._('TABLES_MISSING_HEADINGS')));
          }
          if (findHeadingTags.length > 0) {
            findHeadingTags.forEach(($a) => {
              $a.classList.add('sa11y-error-border');
              $a.insertAdjacentHTML('beforebegin', this.annotate(ERROR, Lang._('TABLES_SEMANTIC_HEADING')));
            });
          }
          findTHeaders.forEach(($b) => {
            if ($b.textContent.trim().length === 0) {
              $b.classList.add('sa11y-error-border');
              $b.insertAdjacentHTML('afterbegin', this.annotate(ERROR, Lang._('TABLES_EMPTY_HEADING')));
            }
          });
        });
      }

      // Warning: Detect fake headings.
      if (option.fakeHeadingsQA === true) {
        this.paragraphs.forEach(($el) => {
          const brAfter = $el.innerHTML.indexOf('</strong><br>');
          const brBefore = $el.innerHTML.indexOf('<br></strong>');
          let boldtext;

          // Check paragraphs greater than x characters.
          if ($el && $el.textContent.trim().length >= 300) {
            const { firstChild } = $el;

            // If paragraph starts with <strong> tag and ends with <br>.
            if (firstChild.tagName === 'STRONG' && (brBefore !== -1 || brAfter !== -1)) {
              boldtext = firstChild.textContent;

              if (!/[*]$/.test(boldtext) && !$el.closest('table') && boldtext.length <= 120) {
                firstChild.classList.add('sa11y-fake-heading', 'sa11y-warning-border');
                $el.insertAdjacentHTML('beforebegin', this.annotate(WARNING, Lang.sprintf('QA_FAKE_HEADING', boldtext)));
              }
            }
          }

          // If paragraph only contains <p><strong>...</strong></p>.
          if (/^<(strong)>.+<\/\1>$/.test($el.innerHTML.trim())) {
            // Although only flag if it:
            // 1) Has less than 120 characters (typical heading length).
            // 2) The previous element is not a heading.
            const prevElement = $el.previousElementSibling;
            let tagName = '';
            boldtext = $el.textContent;

            if (prevElement !== null) {
              tagName = prevElement.tagName;
            }

            if (!/[*]$/.test(boldtext) && !$el.closest('table') && boldtext.length <= 120 && tagName.charAt(0) !== 'H') {
              $el.classList.add('sa11y-fake-heading', 'sa11y-warning-border');
              $el.insertAdjacentHTML('beforebegin',
                this.annotate(WARNING, Lang.sprintf('QA_FAKE_HEADING', boldtext)));
            }
          }
        });
      }

      // Warning: Detect paragraphs that should be lists.
      // Thanks to John Jameson from PrincetonU for this ruleset!
      if (option.fakeListQA === true) {
        this.paragraphs.forEach(($el) => {
          let activeMatch = '';
          const prefixDecrement = {
            b: 'a',
            B: 'A',
            2: '1',
            б: 'а',
            Б: 'А',
          };
          const prefixMatch = /a\.|a\)|A\.|A\)|а\.|а\)|А\.|А\)|1\.|1\)|\*\s|-\s|--|•\s|→\s|✓\s|✔\s|✗\s|✖\s|✘\s|❯\s|›\s|»\s/;
          const decrement = (el) => el.replace(/^b|^B|^б|^Б|^2/, (match) => prefixDecrement[match]);
          let hit = false;
          const firstPrefix = $el.textContent.substring(0, 2);
          if (
            firstPrefix.trim().length > 0
            && firstPrefix !== activeMatch
            && firstPrefix.match(prefixMatch)
          ) {
            const hasBreak = $el.innerHTML.indexOf('<br>');
            if (hasBreak !== -1) {
              const subParagraph = $el
                .innerHTML
                .substring(hasBreak + 4)
                .trim();
              const subPrefix = subParagraph.substring(0, 2);
              if (firstPrefix === decrement(subPrefix)) {
                hit = true;
              }
            }

            // Decrement the second p prefix and compare .
            if (!hit) {
              const $second = this.getNextSibling($el, 'p');
              if ($second) {
                const secondPrefix = decrement($el.nextElementSibling.textContent.substring(0, 2));
                if (firstPrefix === secondPrefix) {
                  hit = true;
                }
              }
            }
            if (hit) {
              $el.insertAdjacentHTML('beforebegin', this.annotate(WARNING, Lang.sprintf('QA_SHOULD_BE_LIST', firstPrefix)));
              $el.classList.add('sa11y-warning-border');
              activeMatch = firstPrefix;
            } else {
              activeMatch = '';
            }
          } else {
            activeMatch = '';
          }
        });
      }

      // Warning: Detect uppercase. Updated logic thanks to Editoria11y!
      if (option.allCapsQA === true) {
        const checkCaps = ($el) => {
          let thisText = '';
          if ($el.tagName === 'LI') {
            // Prevent recursion through nested lists.
            $el.childNodes.forEach((node) => {
              if (node.nodeType === 3) {
                thisText += node.textContent;
              }
            });
          } else {
            thisText = this.getText($el);
          }
          const uppercasePattern = /([A-Z]{2,}[ ])([A-Z]{2,}[ ])([A-Z]{2,}[ ])([A-Z]{2,})/g;
          const detectUpperCase = thisText.match(uppercasePattern);

          if (detectUpperCase && detectUpperCase[0].length > 10) {
            const parentClickable = $el.closest('a, button');
            if (parentClickable) {
              parentClickable.insertAdjacentHTML('beforebegin', this.annotate(WARNING, Lang._('QA_UPPERCASE_WARNING')));
              parentClickable.classList.add('sa11y-warning-border');
            } else {
              $el.classList.add('sa11y-warning-border');
              $el.insertAdjacentHTML('beforebegin', this.annotate(WARNING, Lang._('QA_UPPERCASE_WARNING')));
            }
          }
        };
        this.paragraphs.forEach(($el) => checkCaps($el));
        this.headings.forEach(($el) => checkCaps($el));
        this.lists.forEach(($el) => checkCaps($el));
        this.blockquotes.forEach(($el) => checkCaps($el));
      }

      // Error: Duplicate IDs
      if (option.duplicateIdQA === true) {
        const ids = Array.from(this.root.querySelectorAll('[id]'));
        const allIds = {};
        ids.forEach(($el) => {
          const { id } = $el;
          if (id) {
            if (allIds[id] === undefined) {
              allIds[id] = 1;
            } else {
              $el.classList.add('sa11y-error-border');
              $el.insertAdjacentHTML('afterend', this.annotate(ERROR, Lang.sprintf('QA_DUPLICATE_ID', id), true));
            }
          }
        });
      }

      // Warning: Flag underline text.
      if (option.underlinedTextQA === true) {
        // Find all <u> tags.
        const underline = Array.from(this.root.querySelectorAll('u'));
        underline.forEach(($el) => {
          $el.classList.add('sa11y-warning-text');
          $el.insertAdjacentHTML('beforebegin', this.annotate(WARNING, Lang._('QA_TEXT_UNDERLINE_WARNING'), true));
        });
        // Find underline based on computed style.
        const computeUnderline = ($el) => {
          const style = getComputedStyle($el);
          const decoration = style.textDecorationLine;
          if (decoration === 'underline') {
            $el.classList.add('sa11y-warning-text');
            $el.insertAdjacentHTML('beforebegin', this.annotate(WARNING, Lang._('QA_TEXT_UNDERLINE_WARNING')));
          }
        };
        this.paragraphs.forEach(($el) => computeUnderline($el));
        this.headings.forEach(($el) => computeUnderline($el));
        this.lists.forEach(($el) => computeUnderline($el));
        this.blockquotes.forEach(($el) => computeUnderline($el));
        this.spans.forEach(($el) => computeUnderline($el));
      }

      // Error: Page is missing meta title.
      if (option.pageTitleQA === true) {
        const $title = document.querySelector('title');
        if (!$title || $title.textContent.trim().length === 0) {
          this.panel.insertAdjacentHTML('afterend', this.annotateBanner(ERROR, Lang._('QA_PAGE_TITLE')));
        }
      }

      // Warning: Find inappropriate use of <sup> and <sub> tags.
      if (option.subscriptQA === true) {
        const $subscript = Array.from(this.root.querySelectorAll('sup, sub'));
        $subscript.forEach(($el) => {
          if ($el.textContent.trim().length >= 80) {
            $el.classList.add('sa11y-warning-text');
            $el.insertAdjacentHTML('afterend', this.annotate(WARNING, Lang._('QA_SUBSCRIPT_WARNING'), true));
          }
        });
      }
    };

    // ============================================================
    // Rulesets: Contrast
    // Color contrast plugin by jasonday: https://github.com/jasonday/color-contrast
    // ============================================================
    /* eslint-disable */
    this.checkContrast = () => {
      let contrastErrors = {
        errors: [],
        warnings: [],
      };

      const elements = this.contrast;
      const contrast = {
        // Parse rgb(r, g, b) and rgba(r, g, b, a) strings into an array.
        // Adapted from https://github.com/gka/chroma.js
        parseRgb(css) {
          let i;
          let m;
          let rgb;
          let f;
          let k;
          if (m = css.match(/rgb\(\s*(\-?\d+),\s*(\-?\d+)\s*,\s*(\-?\d+)\s*\)/)) {
            rgb = m.slice(1, 4);
            for (i = f = 0; f <= 2; i = ++f) {
              rgb[i] = +rgb[i];
            }
            rgb[3] = 1;
          } else if (m = css.match(/rgba\(\s*(\-?\d+),\s*(\-?\d+)\s*,\s*(\-?\d+)\s*,\s*([01]|[01]?\.\d+)\)/)) {
            rgb = m.slice(1, 5);
            for (i = k = 0; k <= 3; i = ++k) {
              rgb[i] = +rgb[i];
            }
          }
          return rgb;
        },
        // Based on http://www.w3.org/TR/WCAG20/#relativeluminancedef
        relativeLuminance(c) {
          const lum = [];
          for (let i = 0; i < 3; i++) {
            const v = c[i] / 255;
            // eslint-disable-next-line no-restricted-properties
            lum.push(v < 0.03928 ? v / 12.92 : Math.pow((v + 0.055) / 1.055, 2.4));
          }
          return (0.2126 * lum[0]) + (0.7152 * lum[1]) + (0.0722 * lum[2]);
        },
        // Based on http://www.w3.org/TR/WCAG20/#contrast-ratiodef
        contrastRatio(x, y) {
          const l1 = contrast.relativeLuminance(contrast.parseRgb(x));
          const l2 = contrast.relativeLuminance(contrast.parseRgb(y));
          return (Math.max(l1, l2) + 0.05) / (Math.min(l1, l2) + 0.05);
        },

        getBackground(el) {
          const styles = getComputedStyle(el);
          const bgColor = styles.backgroundColor;
          const bgImage = styles.backgroundImage;
          const rgb = `${contrast.parseRgb(bgColor)}`;
          const alpha = rgb.split(',');

          // if background has alpha transparency, flag manual check
          if (alpha[3] < 1 && alpha[3] > 0) {
            return 'alpha';
          }

          // if element has no background image, or transparent return bgColor
          if (bgColor !== 'rgba(0, 0, 0, 0)' && bgColor !== 'transparent' && bgImage === 'none' && alpha[3] !== '0') {
            return bgColor;
          } if (bgImage !== 'none') {
            return 'image';
          }

          // retest if not returned above
          if (el.tagName === 'HTML') {
            return 'rgb(255, 255, 255)';
          }
          return contrast.getBackground(el.parentNode);
        },
        check() {
          // resets results
          contrastErrors = {
            errors: [],
            warnings: [],
          };

          for (let i = 0; i < elements.length; i++) {
            const elem = elements[i];
            if (contrast) {
              const style = getComputedStyle(elem);
              const { color } = style;
              const { fill } = style;
              const fontSize = parseInt(style.fontSize, 10);
              const pointSize = fontSize * (3 / 4);
              const { fontWeight } = style;
              const htmlTag = elem.tagName;
              const background = contrast.getBackground(elem);
              const textString = [].reduce.call(elem.childNodes, (a, b) => a + (b.nodeType === 3 ? b.textContent : ''), '');
              const text = textString.trim();
              let ratio;
              let error;
              let warning;

              if (htmlTag === 'SVG') {
                ratio = Math.round(contrast.contrastRatio(fill, background) * 100) / 100;
                if (ratio < 3) {
                  error = {
                    elem,
                    ratio: `${ratio}:1`,
                  };
                  contrastErrors.errors.push(error);
                }
              } else if (text.length || htmlTag === 'INPUT' || htmlTag === 'SELECT' || htmlTag === 'TEXTAREA') {
                // does element have a background image - needs to be manually reviewed
                if (background === 'image') {
                  warning = {
                    elem,
                  };
                  contrastErrors.warnings.push(warning);
                } else if (background === 'alpha') {
                  warning = {
                    elem,
                  };
                  contrastErrors.warnings.push(warning);
                } else {
                  ratio = Math.round(contrast.contrastRatio(color, background) * 100) / 100;
                  if (pointSize >= 18 || (pointSize >= 14 && fontWeight >= 700)) {
                    if (ratio < 3) {
                      error = {
                        elem,
                        ratio: `${ratio}:1`,
                      };
                      contrastErrors.errors.push(error);
                    }
                  } else if (ratio < 4.5) {
                    error = {
                      elem,
                      ratio: `${ratio}:1`,
                    };
                    contrastErrors.errors.push(error);
                  }
                }
              }
            }
          }
          return contrastErrors;
        },
      };

      contrast.check();

      contrastErrors.errors.forEach((item) => {
        const name = item.elem;
        const cratio = item.ratio;
        const clone = name.cloneNode(true);
        const removeSa11yHeadingLabel = clone.querySelectorAll('.sa11y-heading-label');
        for (let i = 0; i < removeSa11yHeadingLabel.length; i++) {
          clone.removeChild(removeSa11yHeadingLabel[i]);
        }

        const nodetext = this.fnIgnore(clone, 'script').textContent;
        if (name.tagName === 'INPUT') {
          name.insertAdjacentHTML('beforebegin', this.annotate(ERROR, Lang.sprintf('CONTRAST_INPUT_ERROR', cratio)));
        } else {
          name.insertAdjacentHTML('beforebegin', this.annotate(ERROR, Lang.sprintf('CONTRAST_ERROR', cratio, nodetext)));
        }
      });

      contrastErrors.warnings.forEach((item) => {
        const name = item.elem;
        const clone = name.cloneNode(true);
        const removeSa11yHeadingLabel = clone.querySelectorAll('.sa11y-heading-label');
        for (let i = 0; i < removeSa11yHeadingLabel.length; i++) {
          clone.removeChild(removeSa11yHeadingLabel[i]);
        }
        const nodetext = this.fnIgnore(clone, 'script').textContent;
        name.insertAdjacentHTML('beforebegin', this.annotate(WARNING, Lang.sprintf('CONTRAST_WARNING', nodetext)));
      });
    };
    /* eslint-disable */
    // ============================================================
    // Rulesets: Readability
    // Adapted from Greg Kraus' readability script: https://accessibility.oit.ncsu.edu/it-accessibility-at-nc-state/developers/tools/readability-bookmarklet/
    // ============================================================
    this.checkReadability = () => {
      // Crude hack to add a period to the end of list items to make a complete sentence.
      this.readability.forEach(($el) => {
        const listText = $el.textContent;
        if (listText.length >= 120) {
          if (listText.charAt(listText.length - 1) !== '.') {
            $el.insertAdjacentHTML('beforeend', "<span class='sa11y-readability-period sa11y-visually-hidden'>.</span>");
          }
        }
      });

      // Combine all page text.
      const readabilityarray = [];
      for (let i = 0; i < this.readability.length; i++) {
        const current = this.readability[i];
        if (this.getText(current) !== '') {
          readabilityarray.push(current.textContent);
        }
      }
      const pageText = readabilityarray.join(' ').trim().toString();

      /* Flesch Reading Ease for English, French, German, Dutch, and Italian.
        Reference: https://core.ac.uk/download/pdf/6552422.pdf
        Reference: https://github.com/Yoast/YoastSEO.js/issues/267 */
      if (['en', 'fr', 'de', 'nl', 'it'].includes(option.readabilityLang)) {
        // Compute syllables: http://stackoverflow.com/questions/5686483/how-to-compute-number-of-syllables-in-a-word-in-javascript
        const numberOfSyllables = (el) => {
          let wordCheck = el;
          wordCheck = wordCheck.toLowerCase().replace('.', '').replace('\n', '');
          if (wordCheck.length <= 3) {
            return 1;
          }
          wordCheck = wordCheck.replace(/(?:[^laeiouy]es|ed|[^laeiouy]e)$/, '');
          wordCheck = wordCheck.replace(/^y/, '');
          const syllableString = wordCheck.match(/[aeiouy]{1,2}/g);
          let syllables = 0;

          const syllString = !!syllableString;
          if (syllString) {
            syllables = syllableString.length;
          }
          return syllables;
        };

        // Words
        const wordsRaw = pageText.replace(/[.!?-]+/g, ' ').split(' ');
        let words = 0;
        for (let i = 0; i < wordsRaw.length; i++) {
        // eslint-disable-next-line eqeqeq
          if (wordsRaw[i] != 0) {
            words += 1;
          }
        }

        // Sentences
        const sentenceRaw = pageText.split(/[.!?]+/);
        let sentences = 0;
        for (let i = 0; i < sentenceRaw.length; i++) {
          if (sentenceRaw[i] !== '') {
            sentences += 1;
          }
        }

        // Syllables
        let totalSyllables = 0;
        let syllables1 = 0;
        let syllables2 = 0;
        for (let i = 0; i < wordsRaw.length; i++) {
        // eslint-disable-next-line eqeqeq
          if (wordsRaw[i] != 0) {
            const syllableCount = numberOfSyllables(wordsRaw[i]);
            if (syllableCount === 1) {
              syllables1 += 1;
            }
            if (syllableCount === 2) {
              syllables2 += 1;
            }
            totalSyllables += syllableCount;
          }
        }

        let flesch = false;
        if (option.readabilityLang === 'en') {
          flesch = 206.835 - (1.015 * (words / sentences)) - (84.6 * (totalSyllables / words));
        } else if (option.readabilityLang === 'fr') {
          flesch = 207 - (1.015 * (words / sentences)) - (73.6 * (totalSyllables / words));
        } else if (option.readabilityLang === 'es') {
          flesch = 206.84 - (1.02 * (words / sentences)) - (0.60 * (100 * (totalSyllables / words)));
        } else if (option.readabilityLang === 'de') {
          flesch = 180 - (words / sentences) - (58.5 * (totalSyllables / words));
        } else if (option.readabilityLang === 'nl') {
          flesch = 206.84 - (0.77 * (100 * (totalSyllables / words))) - (0.93 * (words / sentences));
        } else if (option.readabilityLang === 'it') {
          flesch = 217 - (1.3 * (words / sentences)) - (0.6 * (100 * (totalSyllables / words)));
        }

        // Update panel.
        const $readabilityinfo = document.getElementById('sa11y-readability-info');

        if (pageText.length === 0) {
          $readabilityinfo.innerHTML = Lang._('READABILITY_NO_P_OR_LI_MESSAGE');
        } else if (words > 30) {
          // Score must be between 0 and 100%.
          if (flesch > 100) {
            flesch = 100;
          } else if (flesch < 0) {
            flesch = 0;
          }

          const fleschScore = flesch.toFixed(1);
          const avgWordsPerSentence = (words / sentences).toFixed(1);
          const complexWords = Math.round(100 * ((words - (syllables1 + syllables2)) / words));

          // Flesch score: WCAG AAA pass if greater than 60
          if (fleschScore >= 0 && fleschScore < 30) {
            $readabilityinfo.innerHTML = `${fleschScore} <span class="sa11y-readability-score">${Lang._('LANG_VERY_DIFFICULT')}</span>`;
          } else if (fleschScore > 31 && fleschScore < 49) {
            $readabilityinfo.innerHTML = `${fleschScore} <span class="sa11y-readability-score">${Lang._('LANG_DIFFICULT')}</span>`;
          } else if (fleschScore > 50 && fleschScore < 60) {
            $readabilityinfo.innerHTML = `${fleschScore} <span class="sa11y-readability-score">${Lang._('LANG_FAIRLY_DIFFICULT')}</span>`;
          } else {
            $readabilityinfo.innerHTML = `${fleschScore} <span class="sa11y-readability-score">${Lang._('LANG_GOOD')}</span>`;
          }
          // Flesch details
          document.getElementById('sa11y-readability-details').innerHTML = `
          <li><strong>${Lang._('LANG_AVG_SENTENCE')}</strong> ${avgWordsPerSentence}</li>
          <li><strong>${Lang._('LANG_COMPLEX_WORDS')}</strong> ${complexWords}%</li>
          <li><strong>${Lang._('LANG_TOTAL_WORDS')}</strong> ${words}</li>`;
        } else {
          $readabilityinfo.textContent = Lang._('READABILITY_NOT_ENOUGH_CONTENT_MESSAGE');
        }
      }

      /* Lix: Danish, Finnish, Norwegian (Bokmål & Nynorsk), Swedish. To-do: More research needed.
      Reference: https://www.simoahava.com/analytics/calculate-readability-scores-for-content/#commento-58ac602191e5c6dc391015c5a6933cf3e4fc99d1dc92644024c331f1ee9b6093 */
      if (['sv', 'fi', 'da', 'no', 'nb', 'nn'].includes(option.readabilityLang)) {
        const calculateLix = (text) => {
          const lixWords = () => text.replace(/[-'.]/ig, '').split(/[^a-zA-ZöäåÖÄÅÆæØø0-9]/g).filter(Boolean);
          const splitSentences = () => {
            const splitter = /\?|!|\.|\n/g;
            const arrayOfSentences = text.split(splitter).filter(Boolean);
            return arrayOfSentences;
          };
          const wordCount = lixWords(text).length;
          const longWordsCount = lixWords(text).filter((wordsArray) => wordsArray.length > 6).length;
          const sentenceCount = splitSentences(text).length;
          const score = Math.round((wordCount / sentenceCount) + ((longWordsCount * 100) / wordCount));
          const avgWordsPerSentence = (wordCount / sentenceCount).toFixed(1);
          const complexWords = Math.round(100 * (longWordsCount / wordCount));
          return {
            score, avgWordsPerSentence, complexWords, wordCount,
          };
        };

        // Update panel.
        const $readabilityinfo = document.getElementById('sa11y-readability-info');
        const lix = calculateLix(pageText);

        if (pageText.length === 0) {
          $readabilityinfo.innerHTML = Lang._('READABILITY_NO_P_OR_LI_MESSAGE');
        } else if (lix.wordCount > 30) {
          if (lix.score >= 0 && lix.score < 39) {
            $readabilityinfo.innerHTML = `${lix.score} <span class="sa11y-readability-score">${Lang._('LANG_GOOD')}</span>`;
          } else if (lix.score > 40 && lix.score < 50) {
            $readabilityinfo.innerHTML = `${lix.score} <span class="sa11y-readability-score">${Lang._('LANG_FAIRLY_DIFFICULT')}</span>`;
          } else if (lix.score > 51 && lix.score < 61) {
            $readabilityinfo.innerHTML = `${lix.score} <span class="sa11y-readability-score">${Lang._('LANG_DIFFICULT')}</span>`;
          } else {
            $readabilityinfo.innerHTML = `${lix.score} <span class="sa11y-readability-score">${Lang._('LANG_VERY_DIFFICULT')}</span>`;
          }
          // LIX details
          document.getElementById('sa11y-readability-details').innerHTML = `
            <li><strong>${Lang._('LANG_AVG_SENTENCE')}</strong> ${lix.avgWordsPerSentence}</li>
            <li><strong>${Lang._('LANG_COMPLEX_WORDS')}</strong> ${lix.complexWords}%</li>
            <li><strong>${Lang._('LANG_TOTAL_WORDS')}</strong> ${lix.wordCount}</li>`;
        } else {
          $readabilityinfo.textContent = Lang._('READABILITY_NOT_ENOUGH_CONTENT_MESSAGE');
        }
      }
    };
    this.initialize();
  }
}

export {
  Lang,
  Sa11yCustomChecks,
  Sa11y,
};
