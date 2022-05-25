import { defineElement } from '@umbraco-ui/uui-base/lib/registration';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { css, html, LitElement } from 'lit';

@defineElement('umb-backoffice-content')
export class UmbBackofficeContent extends LitElement {
  static styles = [
    UUITextStyles,
    css`
      :host {
        flex: 1 1 auto;
        background-color: var(--uui-color-background);
        width: 100%;
        height: 100%;
        display: flex;
        flex-direction: column;
      }

      #top-bar {
        background-color: var(--uui-color-surface);
        width: 100%;
        display: flex;
        flex: none;
        gap: 16px;
        align-items: center;
        border-bottom: 1px solid var(--uui-color-border);
      }

      #top-bar uui-input {
        width: 100%;
        margin-left: 16px;
      }

      #top-bar uui-tab-group {
        --uui-tab-divider: var(--uui-color-border);
        border-left: 1px solid var(--uui-color-border);
        flex-wrap: nowrap;
        height: 60px;
      }

      #content-content {
        padding: var(--uui-size-6);
        display: flex;
        flex: 1;
        flex-direction: column;
        gap: 16px;
      }

      uui-tab {
        font-size: 0.8rem;
      }

      #bottom-bar {
        display: flex;
        flex: none;
        justify-content: end;
        align-items: center;
        height: 70px;
        width: 100%;
        gap: 16px;
        padding-right: 24px;
        border-top: 1px solid var(--uui-color-border);
        background-color: var(--uui-color-surface);
        box-sizing: border-box;
      }
    `,
  ];

  render() {
    return html`
      <div id="top-bar">
        <uui-input value="Home"></uui-input>
        <uui-tab-group>
          <uui-tab active>Content</uui-tab>
          <uui-tab>Info</uui-tab>
          <uui-tab disabled>Actions</uui-tab>
        </uui-tab-group>
      </div>
      <uui-scroll-container id="content-content"></uui-scroll-container>
      <div id="bottom-bar">
        <uui-button>Save and preview</uui-button>
        <uui-button look="secondary">Save</uui-button>
        <uui-button look="primary" color="positive">Save and publish</uui-button>
      </div>
    `;
  }
}

declare global {
  interface HTMLElementTagNameMap {
    'umb-backoffice-content': UmbBackofficeContent;
  }
}
