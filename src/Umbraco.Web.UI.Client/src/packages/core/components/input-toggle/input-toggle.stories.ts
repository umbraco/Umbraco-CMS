import type { Meta, StoryObj } from '@storybook/web-components';
import './input-toggle.element.js';
import type { UmbInputToggleElement } from './input-toggle.element.js';

const meta: Meta<UmbInputToggleElement> = {
	title: 'Components/Inputs/Toggle',
	component: 'umb-input-toggle',
};

export default meta;
type Story = StoryObj<UmbInputToggleElement>;

export const Overview: Story = {
	args: {
		checked: true,
		showLabels: true,
		labelOn: 'On',
		labelOff: 'Off',
	},
};

export const WithDifferentLabels: Story = {
	args: {
		checked: false,
		showLabels: true,
		labelOn: 'Hide the treasure',
		labelOff: 'Show the way to the treasure',
	},
};

export const WithNoLabels: Story = {
	args: {
		checked: true,
		showLabels: false,
	},
};
