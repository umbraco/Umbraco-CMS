import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { css, CSSResultGroup, html, LitElement } from 'lit';
import { customElement } from 'lit/decorators.js';

@customElement('umb-backoffice-header-tools')
export class UmbBackofficeHeaderTools extends LitElement {
  static styles: CSSResultGroup = [
    UUITextStyles,
    css`
      #tools {
        display: flex;
        align-items: center;
        gap: var(--uui-size-space-2);
      }

      .tool {
        font-size: 18px;
      }
    `,
  ];

  render() {
    return html`
      <div id="tools">
        <uui-button class="tool" look="primary" label="Search" compact>
          <uui-icon name="search"></uui-icon>
        </uui-button>
        <uui-button class="tool" look="primary" label="Help" compact>
          <uui-icon name="favorite"></uui-icon>
        </uui-button>
        <uui-button look="primary" style="font-size: 14px;" label="User" compact>
          <uui-avatar name="Mr Rabbit"></uui-avatar>
        </uui-button>
      </div>
    `;
  }
}

declare global {
  interface HTMLElementTagNameMap {
    'umb-backoffice-header-tools': UmbBackofficeHeaderTools;
  }
}
