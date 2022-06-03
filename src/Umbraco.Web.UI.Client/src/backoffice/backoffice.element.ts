import './components/section-sidebar.element';
import './components/backoffice-header.element';
import './components/backoffice-main.element';

import { defineElement } from '@umbraco-ui/uui-base/lib/registration';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { css, html, LitElement } from 'lit';
import { state } from 'lit/decorators.js';
import { repeat } from 'lit/directives/repeat.js';
import { Subscription } from 'rxjs';

import { UmbContextProviderMixin } from '../core/context';
import { UmbNotificationService } from '../core/service/notifications.store';
import { UmbDataTypeStore } from '../core/stores/data-type.store';
import { UmbNodeStore } from '../core/stores/node.store';

@defineElement('umb-backoffice')
export default class UmbBackoffice extends UmbContextProviderMixin(LitElement) {
  static styles = [
    UUITextStyles,
    css`
      :host {
        display: flex;
        flex-direction: column;
        height: 100%;
        width: 100%;
        color: var(--uui-color-text);
        font-size: 14px;
        box-sizing: border-box;
      }

      #notifications {
        position: absolute;
        top: 0;
        left: 0;
        right: 0;
        bottom: 70px;
        height: auto;
        padding: var(--uui-size-layout-1);
      }
    `,
  ];

  private _notificationService: UmbNotificationService = new UmbNotificationService();
  private _notificationSubscribtion?: Subscription;

  @state()
  private _notifications: any[] = [];

  constructor() {
    super();

    this.provideContext('umbNodeStore', new UmbNodeStore());
    this.provideContext('umbDataTypeStore', new UmbDataTypeStore());

    this._notificationService = new UmbNotificationService();
    this.provideContext('umbNotificationService', this._notificationService);
  }

  protected firstUpdated(_changedProperties: Map<string | number | symbol, unknown>): void {
    super.firstUpdated(_changedProperties);

    this._notificationSubscribtion = this._notificationService.notifications.subscribe((notifications: Array<any>) => {
      this._notifications = notifications;
    });

    // TODO: listen to close event and remove notification from store.
  }

  disconnectedCallback(): void {
    super.disconnectedCallback();

    this._notificationSubscribtion?.unsubscribe();
  }

  render() {
    return html`
      <umb-backoffice-header></umb-backoffice-header>
      <umb-backoffice-main></umb-backoffice-main>
      <uui-toast-notification-container auto-close="7000" bottom-up id="notifications">
        ${repeat(
          this._notifications,
          (notification) => notification.key,
          (notification) => html`<uui-toast-notification color="positive">
            <uui-toast-notification-layout .headline=${notification.headline}> </uui-toast-notification-layout>
          </uui-toast-notification>`
        )}
      </uui-toast-notification-container>
    `;
  }
}

declare global {
  interface HTMLElementTagNameMap {
    'umb-backoffice': UmbBackoffice;
  }
}
