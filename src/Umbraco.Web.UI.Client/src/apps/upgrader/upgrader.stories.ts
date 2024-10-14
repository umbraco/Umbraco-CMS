import './upgrader-view.element.js';

import type { UmbUpgraderViewElement } from './upgrader-view.element.js';
import type { Meta, StoryObj } from '@storybook/web-components';
import { html } from '@umbraco-cms/backoffice/external/lit';

export default {
	title: 'Apps/Upgrader/States',
	component: 'umb-upgrader-view',
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
	decorators: [
		(story) =>
			html`<div
				style="margin:2rem; max-width:400px;border:1px solid #ccc;border-radius:30px 0px 0px 30px;padding:var(--uui-size-layout-4) var(--uui-size-layout-4) var(--uui-size-layout-2) var(--uui-size-layout-4);">
				${story()}
			</div>`,
	],
} satisfies Meta<UmbUpgraderViewElement>;

type Story = StoryObj<UmbUpgraderViewElement>;

export const Overview: Story = {};

export const Upgrading: Story = {
	args: {
		upgrading: true,
	},
};

export const Fetching: Story = {
	args: {
		fetching: true,
	},
};

export const Error: Story = {
	args: {
		errorMessage: 'Something went wrong',
	},
};
