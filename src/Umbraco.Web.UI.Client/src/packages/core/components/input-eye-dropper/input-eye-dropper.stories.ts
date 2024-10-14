import type { Meta, StoryObj } from '@storybook/web-components';
import './input-eye-dropper.element.js';
import type { UmbInputEyeDropperElement } from './input-eye-dropper.element.js';

const meta: Meta<UmbInputEyeDropperElement> = {
	title: 'Components/Inputs/Eye Dropper',
	component: 'umb-input-eye-dropper',
};

export default meta;
type Story = StoryObj<UmbInputEyeDropperElement>;

export const Overview: Story = {
	args: {},
};

export const WithOpacity: Story = {
	args: {
		opacity: true,
	},
};

export const WithSwatches: Story = {
	args: {
		showPalette: true,
		swatches: ['#000000', '#ffffff', '#ff0000', '#00ff00', '#0000ff'],
	},
};

export const ShowPalette: Story = {
	args: {
		showPalette: true,
	},
};
