import { CSSResultGroup, css, html, customElement, property } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from "@umbraco-cms/backoffice/lit-element";

@customElement('umb-error-layout')
export default class UmbErrorLayoutElement extends UmbLitElement {
  @property({ type: String })
  header = '';

  @property({ type: String })
  message = '';

  @property({ type: Boolean, attribute: 'no-back-link' })
  noBackLink = false;

  render() {
    return html`
      <header id="header">
        <h1>${this.header?.length ? this.header : html`<umb-localize key="auth_friendlyGreeting">Hi there</umb-localize>`}</h1>
        <span>${this.message}</span>
      </header>
      <slot></slot>
      ${!this.noBackLink ? html`<umb-back-to-login-button></umb-back-to-login-button>`: ''}
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

      ::slotted(uui-button) {
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
    'umb-error-layout': UmbErrorLayoutElement;
  }
}
