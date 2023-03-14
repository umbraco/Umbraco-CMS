import { Meta, Story } from '@storybook/web-components';
import { html } from 'lit';
import type { UmbNotificationLayoutDefaultElement } from './notification-layout-default.element';

import './notification-layout-default.element';

export default {
	title: 'API/Notifications/Layouts/Default',
	component: 'umb-notification-layout-default',
	id: 'notification-layout-default',
	args: {
		data: {
			headline: 'Headline',
			message: 'This is a default notification',
		},
	},
} as Meta<UmbNotificationLayoutDefaultElement>;

const Template: Story<UmbNotificationLayoutDefaultElement> = (props) => html`
	<uui-toast-notification .open=${true}>
		<umb-notification-layout-default .data=${props.data}></umb-notification-layout-default>
	</uui-toast-notification>
`;

export const Default = Template.bind({});
