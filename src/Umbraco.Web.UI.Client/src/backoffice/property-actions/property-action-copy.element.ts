import { html, LitElement } from 'lit';
import { customElement, property } from 'lit/decorators.js';
import { UmbContextConsumerMixin } from '../../core/context';
import { UmbNotificationService } from '../../core/services/notification.service';

// eslint-disable-next-line @typescript-eslint/no-empty-interface
interface UmbPropertyActionElement {
  value: string;
}

@customElement('umb-property-action-copy')
export default class UmbPropertyActionCopy extends UmbContextConsumerMixin(LitElement) implements UmbPropertyActionElement {
  
  @property()
  value = '';

  private _notificationService?: UmbNotificationService;
  
  constructor () {
    super();

    this.consumeContext('umbProperty', (property) => {
      console.log('PROPERTY', property);
    });

    this.consumeContext('umbNotificationService', (notificationService: UmbNotificationService) => {
      this._notificationService = notificationService;
    });
  }

  private _handleLabelClick () {
    this._notificationService?.peek('Copied to clipboard');
    // TODO: how do we want to close the menu? Testing an event based approach
    this.dispatchEvent(new CustomEvent('close', { bubbles: true, composed: true }));
  }

  render() {
    return html`
      <uui-menu-item label="Copy" @click-label="${this._handleLabelClick}">
        <uui-icon slot="icon" name="copy"></uui-icon>
      </uui-menu-item>`;
  }
}

declare global {
  interface HTMLElementTagNameMap {
    'umb-property-action-copy': UmbPropertyActionCopy;
  }
}
