import { css, html, LitElement } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, property } from 'lit/decorators.js';

@customElement('umb-node-property')
class UmbNodeProperty extends LitElement {
  static styles = [
    UUITextStyles,
    css`
      :host {
        display: block;
      }
      .property {
        display: grid;
        grid-template-columns: 200px 600px;
        gap: 32px;
      }
      .property > .property-label > p {
        color: var(--uui-color-text-alt);
      }
      .property uui-input,
      .property uui-textarea {
        width: 100%;
      }
    `,
  ];

  @property()
  label = '';

  @property()
  description = '';

  render() {
    return html`
      <div class="property">
        <div class="header">
          <uui-label>${this.label}</uui-label>
          <p>${this.description}</p>
        </div>
        <div class="editor">
          <slot></slot>
        </div>
      </div>
    `;
  }
}

declare global {
  interface HTMLElementTagNameMap {
    'umb-node-property': UmbNodeProperty;
  }
}
