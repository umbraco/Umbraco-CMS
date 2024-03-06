import type { Meta, StoryObj } from '@storybook/web-components';
import './icon.element.js';
import type { UmbIconElement } from './icon.element.js';

const meta: Meta<UmbIconElement> = {
	title: 'Components/Inputs/Icon',
	component: 'umb-icon',
};

export default meta;
type Story = StoryObj<UmbIconElement>;

export const Overview: Story = {
	args: {
		name: 'icon-wand',
		color: 'color-pink',
	},
};
