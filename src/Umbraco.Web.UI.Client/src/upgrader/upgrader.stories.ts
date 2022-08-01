import '.';

import { Meta, Story } from '@storybook/web-components';
import { html } from 'lit-html';

import { UmbUpgraderView } from './upgrader-view.element';

export default {
	title: 'Upgrader/Upgrader',
	args: {
		errorMessage: '',
		upgrading: false,
		fetching: false,
		settings: {
			currentState: '2b20c6e7',
			newState: '2b20c6e8',
			oldVersion: '12.0.0',
			newVersion: '13.0.0',
			reportUrl: 'https://our.umbraco.com/download/releases/1000',
		},
	},
	parameters: {
		actions: {
			handles: ['onAuthorizeUpgrade'],
		},
	},
} as Meta<UmbUpgraderView>;

const Template: Story<UmbUpgraderView> = ({ upgrading, errorMessage, settings, fetching }) =>
	html`<umb-upgrader-view
		.upgrading=${upgrading}
		.errorMessage=${errorMessage}
		.settings=${settings}
		.fetching=${fetching}></umb-upgrader-view>`;

export const Overview = Template.bind({});

export const Upgrading = Template.bind({});
Upgrading.args = {
	upgrading: true,
};

export const fetching = Template.bind({});
fetching.args = {
	fetching: true
}

export const Error = Template.bind({});
Error.args = {
	errorMessage: 'Something went wrong',
};

