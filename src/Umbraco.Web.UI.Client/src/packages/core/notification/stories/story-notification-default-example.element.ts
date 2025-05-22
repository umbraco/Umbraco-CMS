import { UMB_NOTIFICATION_CONTEXT, type UmbNotificationContext } from '../notification.context.js';
import type { UmbNotificationColor, UmbNotificationOptions } from '../types.js';
import { html, customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

@customElement('umb-story-notification-default-example')
export class UmbStoryNotificationDefaultExampleElement extends UmbLitElement {
	private _notificationContext?: UmbNotificationContext;

	override connectedCallback(): void {
		super.connectedCallback();

		this.consumeContext(UMB_NOTIFICATION_CONTEXT, (instance) => {
			this._notificationContext = instance;
		});
	}

	private _handleNotification = (color: UmbNotificationColor) => {
		const options: UmbNotificationOptions = {
			data: {
				headline: 'Headline',
				message: 'Lorem ipsum dolor sit amet, consectetur adipiscing elit',
			},
		};
		this._notificationContext?.peek(color, options);
	};

	override render() {
		return html`
			<uui-button @click="${() => this._handleNotification('default')}" label="Default"></uui-button>
			<uui-button
				@click="${() => this._handleNotification('positive')}"
				label="Positive"
				look="primary"
				color="positive"></uui-button>
			<uui-button
				@click="${() => this._handleNotification('warning')}"
				label="Warning"
				look="primary"
				color="warning"></uui-button>
			<uui-button
				@click="${() => this._handleNotification('danger')}"
				label="Danger"
				look="primary"
				color="danger"></uui-button>

			<umb-backoffice-notification-container></umb-backoffice-notification-container>
		`;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-story-notification-default-example': UmbStoryNotificationDefaultExampleElement;
	}
}
