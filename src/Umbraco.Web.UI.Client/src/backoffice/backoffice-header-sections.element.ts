import { Subscription, map } from 'rxjs';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { css, CSSResultGroup, html, LitElement } from 'lit';
import { customElement, state } from 'lit/decorators.js';
import { when } from 'lit/directives/when.js';
import { isPathActive, path } from 'router-slot';

import { getUserSections } from '../core/api/fetcher';
import { UmbExtensionRegistry, UmbExtensionManifest, UmbExtensionManifestSection } from '../core/extension';
import { UmbContextConsumerMixin } from '../core/context';

@customElement('umb-backoffice-header-sections')
export class UmbBackofficeHeaderSections extends UmbContextConsumerMixin(LitElement) {
  static styles: CSSResultGroup = [
    UUITextStyles,
    css`
      #tabs {
        color: var(--uui-color-header-contrast);
        height: 60px;
        font-size: 16px;
        --uui-tab-text: var(--uui-color-header-contrast);
        --uui-tab-text-hover: var(--uui-color-header-contrast-emphasis);
        --uui-tab-text-active: var(--uui-color-header-contrast-emphasis);
        --uui-tab-background: var(--uui-color-header-background);
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

  private _extensionRegistry?: UmbExtensionRegistry;

  private _sectionSubscription?: Subscription;

  constructor() {
    super();

    this.consumeContext('umbExtensionRegistry', (extensionRegistry: UmbExtensionRegistry) => {
      this._extensionRegistry = extensionRegistry;
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
    history.pushState(null, '', `/section/${section.meta.pathname}`);
  }

  private _handleLabelClick() {
    const moreTab = this.shadowRoot?.getElementById('moreTab');
    moreTab?.setAttribute('active', 'true');

    this._open = false;
  }

  private async _useSections() {
    this._sectionSubscription?.unsubscribe();

    const { data } = await getUserSections({});
    this._allowedSection = data.sections;

    this._sectionSubscription = this._extensionRegistry?.extensions
      .pipe(
        map((extensions: Array<UmbExtensionManifest>) =>
          extensions
            .filter((extension) => extension.type === 'section')
            .sort((a: any, b: any) => b.meta.weight - a.meta.weight)
        )
      )
      .subscribe((sections: Array<any>) => {
        this._sections = sections.filter((section: any) => this._allowedSection.includes(section.alias));
        this._visibleSections = this._sections;
      });
  }

  disconnectedCallback(): void {
    super.disconnectedCallback();
    this._sectionSubscription?.unsubscribe();
  }

  render() {
    return html`
      <uui-tab-group id="tabs">
        ${this._visibleSections.map(
          (section: any) => html`
            <uui-tab
              ?active="${isPathActive(`/section/${section.meta.pathname}`, path())}"
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
