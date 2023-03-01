import { html } from 'lit';
import { customElement } from 'lit/decorators.js';
import { UmbNotificationDefaultData } from '../layouts/default';
import {
	UmbNotificationColor,
	UmbNotificationOptions,
	UmbNotificationService,
	UMB_NOTIFICATION_SERVICE_CONTEXT_TOKEN
} from '..';
import { UmbLitElement } from '@umbraco-cms/element';


@customElement('story-notification-default-example')
export class StoryNotificationDefaultExampleElement extends UmbLitElement {
	private _notificationService?: UmbNotificationService;

	connectedCallback(): void {
		super.connectedCallback();

		this.consumeContext(UMB_NOTIFICATION_SERVICE_CONTEXT_TOKEN, (notificationService) => {
			this._notificationService = notificationService;
		});
	}

	private _handleNotification = (color: UmbNotificationColor) => {
		const options: UmbNotificationOptions<UmbNotificationDefaultData> = {
			data: {
				headline: 'Headline',
				message: 'Lorem ipsum dolor sit amet, consectetur adipiscing elit',
			},
		};
		this._notificationService?.peek(color, options);
	};

	render() {
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
