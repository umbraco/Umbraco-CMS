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

      p {
        color: var(--uui-color-text-alt);
      }
    `,
  ];

  @property()
  property: any; // TODO: property data model interface..

  @property()
  value?: string;

  // TODO: maybe a bit messy with all the event listeners on the different levels:
  private _onPropertyDataTypeChange = (e: CustomEvent) => {
    this.value = (e.target as any).value;
    this.dispatchEvent(new CustomEvent('property-value-change', { bubbles: true, composed: true }));
    // No need for this event to leave scope.
    e.stopPropagation();
  };

  render() {
    return html`
      <umb-editor-property-layout>
        <div slot="header">
          <uui-label>${this.property.label}</uui-label>
          <p>${this.property.description}</p>
        </div>
        <div slot="editor">
          <umb-node-property-data-type
            .dataTypeKey=${this.property.dataTypeKey}
            .value=${this.value}
            @property-data-type-value-change=${this._onPropertyDataTypeChange}></umb-node-property-data-type>
        </div>
      </umb-editor-property-layout>
    `;
  }
}

declare global {
  interface HTMLElementTagNameMap {
    'umb-node-property': UmbNodeProperty;
  }
}
