import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { css, CSSResultGroup, html, LitElement } from 'lit';
import { customElement, state } from 'lit/decorators.js';
import { repeat } from 'lit/directives/repeat.js';
import type { UmbNotificationHandler, UmbNotificationService } from '../../test/core/services/notification';
import { UmbObserverMixin } from '@umbraco-cms/observable-api';
import { UmbContextConsumerMixin } from '@umbraco-cms/context-api';

@customElement('umb-backoffice-notification-container')
export class UmbBackofficeNotificationContainer extends UmbContextConsumerMixin(UmbObserverMixin(LitElement)) {
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

	constructor() {
		super();

		this.consumeContext('umbNotificationService', (notificationService: UmbNotificationService) => {
			this._notificationService = notificationService;
			this._observeNotifications();
		});
	}

	private _observeNotifications() {
		if (!this._notificationService) return;

		this.observe<UmbNotificationHandler[]>(this._notificationService.notifications, (notifications) => {
			this._notifications = notifications;
		});
	}

	render() {
		return html`
			<uui-toast-notification-container bottom-up id="notifications">
				${repeat(
					this._notifications,
					(notification: UmbNotificationHandler) => notification.key,
					(notification) => html`${notification.element}`
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
