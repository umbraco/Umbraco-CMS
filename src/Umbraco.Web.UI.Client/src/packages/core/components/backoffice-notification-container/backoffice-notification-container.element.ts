import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import type { CSSResultGroup } from '@umbraco-cms/backoffice/external/lit';
import { css, html, customElement, state, repeat, query } from '@umbraco-cms/backoffice/external/lit';
import type { UmbNotificationHandler, UmbNotificationContext } from '@umbraco-cms/backoffice/notification';
import { UMB_NOTIFICATION_CONTEXT } from '@umbraco-cms/backoffice/notification';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

@customElement('umb-backoffice-notification-container')
export class UmbBackofficeNotificationContainerElement extends UmbLitElement {
	@query('#notifications')
	private _notificationsElement?: HTMLElement;

	@state()
	private _notifications?: UmbNotificationHandler[];

	private _notificationContext?: UmbNotificationContext;

	constructor() {
		super();

		this.consumeContext(UMB_NOTIFICATION_CONTEXT, (instance) => {
			this._notificationContext = instance;
			this._observeNotifications();
		});
	}

	private _observeNotifications() {
		if (!this._notificationContext) return;

		this.observe(this._notificationContext.notifications, (notifications) => {
			this._notifications = notifications;

			// Close and instantly open the popover again to make sure it stays on top of all other content on the page
			// TODO: This ignorer is just needed for JSON SCHEMA TO WORK, As its not updated with latest TS jet.
			// eslint-disable-next-line @typescript-eslint/ban-ts-comment
			// @ts-ignore
			this._notificationsElement?.hidePopover?.(); // To prevent issues in FireFox I added `?.` before `()`  [NL]
			// TODO: This ignorer is just needed for JSON SCHEMA TO WORK, As its not updated with latest TS jet.
			// eslint-disable-next-line @typescript-eslint/ban-ts-comment
			// @ts-ignore
			this._notificationsElement?.showPopover?.(); // To prevent issues in FireFox I added `?.` before `()`  [NL]
		});
	}

	override render() {
		return html`
			<uui-toast-notification-container bottom-up id="notifications" popover="manual">
				${this._notifications
					? repeat(
							this._notifications,
							(notification: UmbNotificationHandler) => notification.key,
							(notification) => html`${notification.element}`,
						)
					: ''}
			</uui-toast-notification-container>
		`;
	}

	static override styles: CSSResultGroup = [
		UmbTextStyles,
		css`
			#notifications {
				top: 0;
				left: 0;
				right: 0;
				bottom: 45px;
				height: auto;
				padding: var(--uui-size-layout-1);

				position: fixed;
				width: 100vw;
				background: 0;
				outline: 0;
				border: 0;
				margin: 0;
			}
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-backoffice-notification-container': UmbBackofficeNotificationContainerElement;
	}
}
