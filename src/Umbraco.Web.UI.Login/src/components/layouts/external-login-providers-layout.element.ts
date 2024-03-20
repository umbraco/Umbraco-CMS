import { css, CSSResultGroup, html, nothing, customElement, property, queryAssignedElements } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

@customElement('umb-external-login-providers-layout')
export class UmbExternalLoginProvidersLayoutElement extends UmbLitElement {
  @property({ type: Boolean, attribute: 'divider' })
  showDivider = true;

  @queryAssignedElements({ flatten: true })
  protected slottedElements?: HTMLElement[];

  firstUpdated() {
    !!this.slottedElements?.length ? this.toggleAttribute('empty', false) : this.toggleAttribute('empty', true);
  }

  render() {
    return html`
      ${this.showDivider
        ? html`
          <div id="divider" aria-hidden="true">
            <span>
              <umb-localize key="general_or">or</umb-localize>
            </span>
          </div>
        `
        : nothing}
      <div>
        <slot></slot>
      </div>
    `;
  }

  static styles: CSSResultGroup = [
    css`
      :host {
        margin-top: 16px;
        display: flex;
        flex-direction: column;
      }

      :host([empty]) {
        display: none;
      }

      slot {
        display: flex;
        flex-direction: column;
        gap: var(--uui-size-space-4);
      }

      #divider {
        width: calc(100% - 18px);
        margin: 0 auto 16px;
        text-align: center;
        z-index: 0;
        overflow: hidden;
        text-transform: lowercase;
      }

      #divider span {
        padding-inline: 10px;
        position: relative;
        color: var(--uui-color-border-emphasis);
      }

      #divider span::before,
      #divider span::after {
        content: '';
        display: block;
        width: 500px; /* Arbitrary value, just be bigger than 50% of the max width of the container */
        height: 1px;
        background-color: var(--uui-color-border);
        position: absolute;
        top: calc(50% + 1px);
      }

      #divider span::before {
        right: 100%;
      }

      #divider span::after {
        left: 100%;
      }
    `,
  ];
}

declare global {
  interface HTMLElementTagNameMap {
    'umb-external-login-providers-layout': UmbExternalLoginProvidersLayoutElement;
  }
}
