import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { css, CSSResultGroup, html, LitElement } from 'lit';
import { customElement, state } from 'lit/decorators.js';
import { repeat } from 'lit/directives/repeat.js';
import { Subscription } from 'rxjs';
import { UmbContextConsumerMixin } from '../../core/context';
import type { UmbNotificationService, UmbNotificationHandler } from '../../core/services/notification';

@customElement('umb-backoffice-notification-container')
export class UmbBackofficeNotificationContainer extends UmbContextConsumerMixin(LitElement) {
	static styles: CSSResultGroup = [
		UUITextStyles,
		css`
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

	@state()
	private _notifications: UmbNotificationHandler[] = [];

	private _notificationService?: UmbNotificationService;
	private _notificationSubscription?: Subscription;

	constructor() {
		super();

		this.consumeContext('umbNotificationService', (notificationService: UmbNotificationService) => {
			this._notificationService = notificationService;
			this._useNotifications();
		});
	}

	private _useNotifications() {
		this._notificationSubscription?.unsubscribe();

		this._notificationService?.notifications.subscribe((notifications: Array<UmbNotificationHandler>) => {
			this._notifications = notifications;
		});
	}

	private _closedNotificationHandler(notificationHandler: UmbNotificationHandler) {
    notificationHandler.close();
  }

	disconnectedCallback(): void {
		super.disconnectedCallback();
		this._notificationSubscription?.unsubscribe();
	}

	render() {
		return html`
			<uui-toast-notification-container bottom-up id="notifications">
				${repeat(
					this._notifications,
					(notification: UmbNotificationHandler) => notification.key,
					(notification) => html`<uui-toast-notification color="${notification.color}" .autoClose="${notification.duration}"  @closed="${() => this._closedNotificationHandler(notification)}">
						${notification.element}
					</uui-toast-notification>`
				)}
			</uui-toast-notification-container>
		`;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-backoffice-notification-container': UmbBackofficeNotificationContainer;
	}
}
