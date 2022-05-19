import { defineElement } from '@umbraco-ui/uui-base/lib/registration';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { css, html, LitElement } from 'lit';

@defineElement('umb-backoffice-sidebar')
export class UmbBackofficeSidebar extends LitElement {
  static styles = [
    UUITextStyles,
    css`
      :host {
        flex: 0 0 300px;
        background-color: var(--uui-color-surface);
        height: 100%;
        border-right: 1px solid var(--uui-color-border);
        font-weight: 500;
        display: flex;
        flex-direction: column;
      }

      h3 {
        padding: var(--uui-size-4) var(--uui-size-8);
      }
    `,
  ];

  render() {
    return html`
      <h3>Content</h3>
      <div class="nav-list">
        <uui-menu-item label="Hello World">
          <uui-icon slot="icon" name="document"></uui-icon>
        </uui-menu-item>
        <uui-menu-item label="Home" active has-children show-children>
          <uui-icon slot="icon" name="document"></uui-icon>
          <uui-menu-item label="Products">
            <uui-icon slot="icon" name="document"></uui-icon>
          </uui-menu-item>
          <uui-menu-item label="People">
            <uui-icon slot="icon" name="document"></uui-icon>
          </uui-menu-item>
          <uui-menu-item label="About Us" disabled has-children>
            <uui-icon slot="icon" name="document"></uui-icon>
            <uui-menu-item label="History">
              <uui-icon slot="icon" name="document"></uui-icon>
            </uui-menu-item>
            <uui-menu-item label="Team">
              <uui-icon slot="icon" name="document"></uui-icon>
            </uui-menu-item>
          </uui-menu-item>
          <uui-menu-item label="MyMenuItem" selected has-children>
            <uui-icon slot="icon" name="document"></uui-icon>
            <uui-menu-item label="History">
              <uui-icon slot="icon" name="document"></uui-icon>
            </uui-menu-item>
            <uui-menu-item label="Team">
              <uui-icon slot="icon" name="document"></uui-icon>
            </uui-menu-item>
          </uui-menu-item>
          <uui-menu-item label="Blog">
            <uui-icon slot="icon" name="calendar"></uui-icon>
          </uui-menu-item>
          <uui-menu-item label="Contact"></uui-menu-item>
        </uui-menu-item>
        <uui-menu-item label="Recycle Bin">
          <uui-icon slot="icon" name="delete"></uui-icon>
        </uui-menu-item>
      </div>
    `;
  }
}

declare global {
  interface HTMLElementTagNameMap {
    'umb-backoffice-sidebar': UmbBackofficeSidebar;
  }
}
