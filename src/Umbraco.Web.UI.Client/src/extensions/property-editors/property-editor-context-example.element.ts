import { html, LitElement } from 'lit';
import { customElement } from 'lit/decorators.js';
import { UmbContextConsumerMixin } from '../../core/context';
import { UmbNotificationService } from '../../core/service/notifications.store';

@customElement('umb-property-editor-context-example')
export default class UmbPropertyEditorContextExample extends UmbContextConsumerMixin(LitElement) {
  
  private _notificationService?: UmbNotificationService;

  constructor() {
    super();
    // TODO: how to deal with single consumption, or situation where you dont want to store the service..
    this.consumeContext('umbNotificationService', (service: UmbNotificationService) => {
      this._notificationService = service;
    })
  }
  private _onClick = () => {
    this._notificationService?.peek('Hello from property editor');
  }

  render() {
    return html`<uui-button look='secondary' label="Click to fire notification" @click=${this._onClick}></uui-button>`;
  }
}

declare global {
  interface HTMLElementTagNameMap {
    'umb-property-editor-context-example': UmbPropertyEditorContextExample;
  }
}
