import { html, LitElement } from 'lit';
import { customElement, property } from 'lit/decorators.js';
import { UmbContextConsumerMixin } from '../../core/context';
import { UmbPropertyActionMenuContext } from './property-action-menu/property-action-menu.context';
import { UmbPropertyAction } from './property-action-element.model';

@customElement('umb-property-action-clear')
export default class UmbPropertyActionClearElement extends UmbContextConsumerMixin(LitElement) implements UmbPropertyAction {
  
  @property()
  value = '';

  private _propertyActionMenuContext?: UmbPropertyActionMenuContext;

  constructor () {
    super();

    this.consumeContext('umbPropertyActionMenu', (propertyActionsContext: UmbPropertyActionMenuContext) => {
      this._propertyActionMenuContext = propertyActionsContext;
    });
  }

  private _handleLabelClick () {
    this._clearValue();
    // TODO: how do we want to close the menu? Testing an event based approach and context api approach
    // this.dispatchEvent(new CustomEvent('close', { bubbles: true, composed: true }));
    this._propertyActionMenuContext?.close();
  }

  private _clearValue () {
    // TODO: how do we want to update the value? Testing an event based approach. We need to test an api based approach too.
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
    'umb-property-action-clear': UmbPropertyActionClearElement;
  }
}
