import './story-notification-default-example.element.js';

import { UmbNotificationContext } from '../notification.context.js';
import type { Meta, StoryFn } from '@storybook/web-components';
import { html } from '@umbraco-cms/backoffice/external/lit';

export default {
	title: 'API/Notifications/Overview',
	component: 'umb-notification-layout-default',
	decorators: [
		(story) =>
			html`<umb-context-provider
				key="UmbNotificationContext"
				.create=${(host: any) => new UmbNotificationContext(host)}>
				${story()}
			</umb-context-provider>`,
	],
} as Meta;

const Template: StoryFn = () => html`<umb-story-notification-default-example></umb-story-notification-default-example>`;

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

this._notificationContext?.peek('positive', options);
`,
		},
	},
};
