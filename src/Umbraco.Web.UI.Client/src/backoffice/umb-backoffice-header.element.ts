import { css, CSSResultGroup, html, LitElement } from 'lit';
import { customElement, state } from 'lit/decorators.js';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';

@customElement('umb-backoffice-header')
export class UmbBackofficeHeader extends LitElement {
  static styles: CSSResultGroup = [
    UUITextStyles,
    css`
      :host {
        width: 100%;
      }
      #appHeader {
        background-color: var(--uui-look-primary-surface);
        display: flex;
        align-items: center;
        justify-content: space-between;
        padding: 0 var(--uui-size-space-5);
      }

      #logo {
        --uui-button-padding-top-factor: 1;
        --uui-button-padding-bottom-factor: 0.5;
        margin-right: var(--uui-size-space-2);
      }

      #logo img {
        height: var(--uui-size-10);
        width: var(--uui-size-10);
      }

      #sections {
        flex: 1 1 auto;
        display: flex;
        align-items: center;
        gap: var(--uui-size-space-2);
      }

      #tabs {
        color: var(--uui-look-primary-contrast);
        height: 60px;
        font-size: 16px;
        --uui-tab-text: var(--uui-look-primary-contrast);
        --uui-tab-text-hover: var(--uui-look-primary-contrast-hover);
        --uui-tab-text-active: var(--uui-interface-active);
        --uui-tab-background: var(--uui-look-primary-surface);
      }

      #tools {
        display: flex;
        align-items: center;
        gap: var(--uui-size-space-2);
      }

      .tool {
        font-size: 18px;
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
  private _sections: Array<string> = ['Content', 'Media', 'Members', 'Settings', 'Packages'];

  @state()
  private _activeSection: string = this._sections[0];

  @state()
  private _visibleSections: Array<string> = ['Content', 'Media', 'Members'];

  @state()
  private _extraSections: Array<string> = ['Settings', 'Packages'];

  private _handleMore(e: MouseEvent) {
    e.stopPropagation();
    this._open = !this._open;
  }

  private _handleTabClick(e: MouseEvent) {
    const tab = e.currentTarget as any;

    // TODO: we need to be able to prevent the tab from setting the active state
    if (tab.id === 'moreTab') {
      return;
    }

    this._activeSection = tab.label;
  }

  private _handleLabelClick(e: MouseEvent) {
    const label = (e.target as any).label;
    this._activeSection = label;

    const moreTab = this.shadowRoot?.getElementById('moreTab');
    moreTab?.setAttribute('active', 'true');

    this._open = false;
  }

  render() {
    return html`
      <div id="appHeader">
        <uui-button id="logo" look="primary" label="Umbraco" compact>
          <img src="/umbraco_logomark_white.svg" alt="Umbraco" />
        </uui-button>

        <div id="sections">
          <uui-tab-group id="tabs">
            ${this._visibleSections.map(
              (section) => html`
                <uui-tab
                  ?active="${this._activeSection === section}"
                  label="${section}"
                  @click="${this._handleTabClick}"></uui-tab>
              `
            )}

            <uui-tab id="moreTab" @click="${this._handleTabClick}">
              <uui-popover .open=${this._open} placement="bottom-start" @close="${() => (this._open = false)}">
                <uui-button slot="trigger" look="primary" label="More" @click="${this._handleMore}" compact>
                  <uui-symbol-more></uui-symbol-more>
                </uui-button>

                <div slot="popover" id="dropdown">
                  ${this._extraSections.map(
                    (section) => html`
                      <uui-menu-item
                        ?active="${this._activeSection === section}"
                        label="${section}"
                        @click-label="${this._handleLabelClick}"></uui-menu-item>
                    `
                  )}
                </div>
              </uui-popover>
            </uui-tab>
          </uui-tab-group>
        </div>

        <div id="tools">
          <uui-button class="tool" look="primary" label="Search" compact>
            <uui-icon name="search"></uui-icon>
          </uui-button>
          <uui-button class="tool" look="primary" label="Help" compact>
            <uui-icon name="favorite"></uui-icon>
          </uui-button>
          <uui-button look="primary" style="font-size: 14px;" label="User" compact>
            <uui-avatar name="Mads Rasmussen"></uui-avatar>
          </uui-button>
        </div>
      </div>
    `;
  }
}

declare global {
  interface HTMLElementTagNameMap {
    'umb-backoffice-header': UmbBackofficeHeader;
  }
}
