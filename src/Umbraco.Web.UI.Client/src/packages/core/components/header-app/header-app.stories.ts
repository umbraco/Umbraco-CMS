import type { Meta, StoryObj } from '@storybook/web-components';
import './header-app-button.element.js';
import type { UmbHeaderAppButtonElement } from './header-app-button.element.js';

const meta: Meta<UmbHeaderAppButtonElement> = {
	title: 'Components/Header App Button',
	component: 'umb-header-app-button',
};

export default meta;
type Story = StoryObj<UmbHeaderAppButtonElement>;

export const Overview: Story = {
	args: {
		manifest: {
			name: 'Some Manifest',
			alias: 'someManifestAlias',
			type: 'headerApp',
			kind: 'button',
			meta: {
				label: 'Some Header',
				icon: 'icon-home',
				href: '/some/path',
			},
		},
	},
};
