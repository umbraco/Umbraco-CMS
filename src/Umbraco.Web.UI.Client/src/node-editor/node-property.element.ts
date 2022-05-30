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
  property:any; // TODO: property data model interface..

  @property()
  value?:string;

  // TODO: maybe a bit messy with all the event listeners on the different levels:
  private _onPropertyDataTypeChange = (e :CustomEvent) => {
    this.value = (e.target as any).value;
  }

  render() {
    return html`
      <div class="property">
        <div class="header">
          <uui-label>${this.property.label}</uui-label>
          <p>${this.property.description}</p>
        </div>
        <div class="editor">
          <umb-node-property-data-type .dataTypeAlias=${this.property.dataTypeAlias} .value=${this.value} @property-data-type-change=${this._onPropertyDataTypeChange}></umb-node-property-data-type>
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
