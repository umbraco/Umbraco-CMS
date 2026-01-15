import type { UmbIconElement } from './icon.element.js';
import type { Meta, StoryObj } from '@storybook/web-components-vite';

import './icon.element.js';

const meta: Meta<UmbIconElement> = {
	title: 'Generic Components/Icon/Icon',
	component: 'umb-icon',
};

export default meta;
type Story = StoryObj<UmbIconElement>;

export const Docs: Story = {
	args: {
		name: 'icon-wand',
		color: '',
	},
};
