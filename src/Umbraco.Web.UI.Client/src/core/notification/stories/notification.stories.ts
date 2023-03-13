import './story-notification-default-example.element';

import { Meta, Story } from '@storybook/web-components';
import { html } from 'lit';
import { UmbNotificationContext } from '@umbraco-cms/notification';

export default {
	title: 'API/Notifications/Overview',
	component: 'umb-notification-layout-default',
	decorators: [
		(story) =>
			html`<umb-context-provider key="UmbNotificationContext" .value=${new UmbNotificationContext()}>
				${story()}
			</umb-context-provider>`,
	],
} as Meta;

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

this._notificationContext?.peek('positive', options);
`,
		},
	},
};
