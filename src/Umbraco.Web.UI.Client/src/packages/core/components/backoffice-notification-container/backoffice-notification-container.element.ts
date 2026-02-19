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

	#srContainer?: HTMLDivElement;

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

	override connectedCallback(): void {
		super.connectedCallback();
		this.#createSrContainer();
	}

	override disconnectedCallback(): void {
		super.disconnectedCallback();
		this.#destroySrContainer();
	}

	#createSrContainer(): void {
		if (this.#srContainer) return;

		// Check if a global announcer container already exists
		const existing = document.getElementById('umb-sr-announcer');
		if (existing) {
			this.#srContainer = existing as HTMLDivElement;
			return;
		}

		this.#srContainer = document.createElement('div');
		this.#srContainer.id = 'umb-sr-announcer';

		// Visually hidden but accessible to screen readers
		Object.assign(this.#srContainer.style, {
			position: 'absolute',
			width: '1px',
			height: '1px',
			padding: '0',
			margin: '-1px',
			overflow: 'hidden',
			whiteSpace: 'nowrap',
			clip: 'rect(0, 0, 0, 0)',
			clipPath: 'inset(50%)',
			border: '0',
		});

		document.body.appendChild(this.#srContainer);
	}

	#destroySrContainer(): void {
		this.#srContainer = undefined;
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

			// Announce the newest notification
			this._announceNewest(this._notifications);
		});
	}

	private _getNotificationText(notificationData: UmbNotificationHandler): string {
		// Trick to access the data (notification message) inside a private property
		const data = (notificationData as any)._data ?? {};
		const notificationText = data.message ?? '';
		return notificationText;
	}

	private _announce(message: string) {
		if (!this.#srContainer || !message) return;

		// Create a new element for each announcement - this is more reliable
		// than updating text content of an existing live region
		const alert = document.createElement('div');
		alert.setAttribute('role', 'alert');
		alert.setAttribute('aria-live', 'assertive');
		alert.setAttribute('aria-atomic', 'true');
		alert.textContent = message;

		this.#srContainer.appendChild(alert);

		// Clean up after announcement (screen readers will have captured it)
		setTimeout(() => {
			alert.remove();
		}, 1000);
	}

	private _announceNewest(list?: UmbNotificationHandler[]) {
		const newest = list?.[list.length - 1];
		if (!newest) return;
		this._announce(this._getNotificationText(newest));
	}

	override render() {
		return html`
			<uui-toast-notification-container bottom-up id="notifications" popover="manual">
				${this._notifications
					? repeat(
							this._notifications,
							(notification: UmbNotificationHandler) => notification.key,
							(notification) => html`${notification.element} `,
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
