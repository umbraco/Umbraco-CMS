import { Meta, Story } from '@storybook/web-components';
import { LitElement, html } from 'lit';
import { customElement } from 'lit/decorators.js';
import { UmbNotificationService, UmbNotificationOptions, UmbNotificationColor } from '.';
import type { UmbNotificationDefaultData } from './layouts/default';
import { UmbContextConsumerMixin } from '../../context';

import '../../context/context-provider.element';
import '../../../backoffice/components/backoffice-notification-container.element';
import './layouts/default';

export default {
	title: 'API/Notifications/Overview',
	component: 'ucp-notification-layout-default',
	decorators: [
		(story) =>
			html`<umb-context-provider key="umbNotificationService" .value=${new UmbNotificationService()}>
				${story()}
			</umb-context-provider>`,
	],
} as Meta;

@customElement('story-notification-default-example')
class StoryNotificationDefaultExampleElement extends UmbContextConsumerMixin(LitElement) {
	private _notificationService?: UmbNotificationService;

	connectedCallback(): void {
		super.connectedCallback();

		this.consumeContext('umbNotificationService', (notificationService) => {
			this._notificationService = notificationService;
			console.log('notification service ready');
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

const Template: Story = () => html`<story-notification-default-example></story-notification-default-example>`;

export const Default = Template.bind({});
Default.parameters = {
	docs: {
		source: {
			language: 'js',
			code: `
const options: UmbNotificationOptions<UmbNotificationDefaultData> = {
  data: {
    headline: 'Headline',
    message: 'Lorem ipsum dolor sit amet, consectetur adipiscing elit'
  }
};

this._notificationService?.peek('positive', options);
`,
		},
	},
};
