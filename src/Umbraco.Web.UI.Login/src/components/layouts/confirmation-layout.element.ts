import { CSSResultGroup, LitElement, css, html } from 'lit';
import { customElement, property } from 'lit/decorators.js';

@customElement('umb-confirmation-layout')
export default class UmbConfirmationLayoutElement extends LitElement {
  @property({ type: String })
  header = '';

  @property({ type: String })
  message = '';

  render() {
    return html`
      <header id="header">
        <h1>${this.header}</h1>
        <span>${this.message}</span>
      </header>

      <umb-back-to-login-button></umb-back-to-login-button>

      <slot></slot>
    `;
  }

  static styles: CSSResultGroup = [
    css`
      :host {
        display: flex;
        flex-direction: column;
        gap: var(--uui-size-layout-1);
      }

      #header {
        text-align: center;
        display: flex;
        flex-direction: column;
        gap: var(--uui-size-space-5);
      }

      #header span {
        color: var(--uui-color-text-alt); /* TODO Change to uui color when uui gets a muted text variable */
        font-size: 14px;
      }

      #header h1 {
        margin: 0;
        font-weight: 400;
        font-size: var(--header-secondary-font-size);
        color: var(--uui-color-interactive);
        line-height: 1.2;
      }

      uui-button {
        width: 100%;
        margin-top: var(--uui-size-space-5);
        --uui-button-padding-top-factor: 1.5;
        --uui-button-padding-bottom-factor: 1.5;
      }
    `,
  ];
}

declare global {
  interface HTMLElementTagNameMap {
    'umb-confirmation-layout': UmbConfirmationLayoutElement;
  }
}
