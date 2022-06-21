import { html, LitElement } from 'lit';
import { customElement, property } from 'lit/decorators.js';
import { UmbContextConsumerMixin } from '../../core/context';
import { UmbPropertyActionElement } from './property-action-element.model';

@customElement('umb-property-action-clear')
export default class UmbPropertyActionClear extends UmbContextConsumerMixin(LitElement) implements UmbPropertyActionElement {
  
  @property()
  value = '';

  constructor () {
    super();

    // TODO implement a property context
    this.consumeContext('umbProperty', (property) => {
      console.log('PROPERTY', property);
    });
  }

  private _handleLabelClick () {
    this._clearValue();
    // TODO: how do we want to close the menu? Testing an event based approach
    this.dispatchEvent(new CustomEvent('close', { bubbles: true, composed: true }));
  }

  private _clearValue () {
    this.value = '';
    this.dispatchEvent(new CustomEvent('property-editor-change', { bubbles: true, composed: true }));
  }

  render() {
    return html`
      <uui-menu-item label="Clear" @click-label="${this._handleLabelClick}">
        <uui-icon slot="icon" name="delete"></uui-icon>
      </uui-menu-item>`;
  }
}

declare global {
  interface HTMLElementTagNameMap {
    'umb-property-action-clear': UmbPropertyActionClear;
  }
}
