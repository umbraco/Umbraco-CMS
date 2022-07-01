import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { css, CSSResultGroup, html, LitElement } from 'lit';
import { customElement, state } from 'lit/decorators.js';
import { repeat } from 'lit/directives/repeat.js';
import { Subscription } from 'rxjs';
import { UmbContextConsumerMixin } from '../../core/context';
import { UmbNotificationService } from '../../core/services/notification.service';

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
	private _notifications: any[] = [];

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

		this._notificationService?.notifications.subscribe((notifications: Array<any>) => {
			this._notifications = notifications;
		});

		// TODO: listen to close event and remove notification from store.
	}

	disconnectedCallback(): void {
		super.disconnectedCallback();
		this._notificationSubscription?.unsubscribe();
	}

	render() {
		return html`
			<uui-toast-notification-container auto-close="7000" bottom-up id="notifications">
				${repeat(
					this._notifications,
					(notification) => notification.key,
					(notification) => html` <uui-toast-notification color="positive">
						<uui-toast-notification-layout .headline=${notification.headline}> </uui-toast-notification-layout>
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
