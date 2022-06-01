import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { css, CSSResultGroup, html, LitElement } from 'lit';
import { customElement, state } from 'lit/decorators.js';
import { when } from 'lit/directives/when.js';
import { Subscription } from 'rxjs';

import { getUserSections } from '../core/api/fetcher';
import { UmbContextConsumerMixin } from '../core/context';
import { UmbExtensionManifestSection } from '../core/extension';
import { UmbRouteLocation, UmbRouter } from '../core/router';
import { UmbSectionContext } from '../section.context';

@customElement('umb-backoffice-header-sections')
export class UmbBackofficeHeaderSections extends UmbContextConsumerMixin(LitElement) {
  static styles: CSSResultGroup = [
    UUITextStyles,
    css`
      #tabs {
        color: var(--uui-look-primary-contrast);
        height: 60px;
        font-size: 16px;
        --uui-tab-text: var(--uui-look-primary-contrast);
        --uui-tab-text-hover: var(--uui-look-primary-contrast-hover);
        --uui-tab-text-active: var(--uui-interface-active);
        --uui-tab-background: var(--uui-look-primary-surface);
      }

      #dropdown {
        background-color: white;
        border-radius: var(--uui-border-radius);
        width: 100%;
        height: 100%;
        box-sizing: border-box;
        box-shadow: var(--uui-shadow-depth-3);
        min-width: 200px;
        color: black; /* Change to variable */
      }
    `,
  ];

  @state()
  private _open = false;

  @state()
  private _allowedSection: Array<string> = [];

  @state()
  private _sections: Array<UmbExtensionManifestSection> = [];

  @state()
  private _visibleSections: Array<UmbExtensionManifestSection> = [];

  @state()
  private _extraSections: Array<UmbExtensionManifestSection> = [];

  @state()
  private _currentSectionAlias = '';

  private _router?: UmbRouter;
  private _sectionContext?: UmbSectionContext;
  private _sectionSubscription?: Subscription;
  private _currentSectionSubscription?: Subscription;
  private _locationSubscription?: Subscription;
  private _location?: UmbRouteLocation;

  constructor() {
    super();

    this.consumeContext('umbRouter', (_instance: UmbRouter) => {
      this._router = _instance;
      this._useLocation();
    });

    this.consumeContext('umbSectionContext', (_instance: UmbSectionContext) => {
      this._sectionContext = _instance;
      this._useCurrentSection();
      this._useSections();
    });
  }

  private _handleMore(e: MouseEvent) {
    e.stopPropagation();
    this._open = !this._open;
  }

  private _handleTabClick(e: PointerEvent, section: UmbExtensionManifestSection) {
    const tab = e.currentTarget as HTMLElement;

    // TODO: we need to be able to prevent the tab from setting the active state
    if (tab.id === 'moreTab') {
      return;
    }

    // TODO: this could maybe be handled by an anchor tag
    this._router?.push(`/section/${section.meta.pathname}`);
    this._sectionContext?.setCurrent(section.alias);
  }

  private _handleLabelClick(e: PointerEvent) {
    const label = (e.target as any).label;

    // TODO: set current section
    //this._sectionContext?.setCurrent(section.alias);

    const moreTab = this.shadowRoot?.getElementById('moreTab');
    moreTab?.setAttribute('active', 'true');

    this._open = false;
  }

  private _useLocation() {
    this._locationSubscription?.unsubscribe();

    this._locationSubscription = this._router?.location.subscribe((location: UmbRouteLocation) => {
      this._location = location;
    });
  }

  private _useCurrentSection() {
    this._currentSectionSubscription?.unsubscribe();

    this._currentSectionSubscription = this._sectionContext?.getCurrent().subscribe((section) => {
      this._currentSectionAlias = section.alias;
    });
  }

  private async _useSections() {
    this._sectionSubscription?.unsubscribe();

    const { data } = await getUserSections({});
    this._allowedSection = data.sections;

    this._sectionSubscription = this._sectionContext?.getSections().subscribe((sectionExtensions) => {
      this._sections = sectionExtensions.filter((section) => this._allowedSection.includes(section.alias));
      this._visibleSections = this._sections;
      const currentSectionAlias = this._sections.find(
        (section) => section.meta.pathname === this._location?.params?.section
      )?.alias;
      if (!currentSectionAlias) return;
      this._sectionContext?.setCurrent(currentSectionAlias);
    });
  }

  disconnectedCallback(): void {
    super.disconnectedCallback();
    this._locationSubscription?.unsubscribe();
    this._sectionSubscription?.unsubscribe();
    this._currentSectionSubscription?.unsubscribe();
  }

  render() {
    return html`
      <uui-tab-group id="tabs">
        ${this._visibleSections.map(
          (section) => html`
            <uui-tab
              ?active="${this._currentSectionAlias === section.alias}"
              label="${section.name}"
              @click="${(e: PointerEvent) => this._handleTabClick(e, section)}"></uui-tab>
          `
        )}
        ${this._renderExtraSections()}
      </uui-tab-group>
    `;
  }

  private _renderExtraSections() {
    return when(
      this._extraSections.length > 0,
      () => html`
        <uui-tab id="moreTab" @click="${this._handleTabClick}">
          <uui-popover .open=${this._open} placement="bottom-start" @close="${() => (this._open = false)}">
            <uui-button slot="trigger" look="primary" label="More" @click="${this._handleMore}" compact>
              <uui-symbol-more></uui-symbol-more>
            </uui-button>

            <div slot="popover" id="dropdown">
              ${this._extraSections.map(
                (section) => html`
                  <uui-menu-item
                    ?active="${this._currentSectionAlias === section.alias}"
                    label="${section.name}"
                    @click-label="${this._handleLabelClick}"></uui-menu-item>
                `
              )}
            </div>
          </uui-popover>
        </uui-tab>
      `
    );
  }
}

declare global {
  interface HTMLElementTagNameMap {
    'umb-backoffice-header-sections': UmbBackofficeHeaderSections;
  }
}
