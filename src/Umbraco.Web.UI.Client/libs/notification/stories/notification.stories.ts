import '../layouts/default';

import { Meta, Story } from '@storybook/web-components';
import { html } from 'lit';

import { UmbNotificationContext } from '..';

export default {
	title: 'API/Notifications/Overview',
	component: 'ucp-notification-layout-default',
	decorators: [
		(story) =>
			html`<umb-context-provider key="umbNotificationService" .value=${new UmbNotificationContext()}>
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

this._notificationService?.peek('positive', options);
`,
		},
	},
};
