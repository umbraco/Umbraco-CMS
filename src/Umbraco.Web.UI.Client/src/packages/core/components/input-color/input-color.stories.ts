import type { Meta, StoryObj } from '@storybook/web-components';
import './input-color.element.js';
import type { UmbInputColorElement } from './input-color.element.js';

const meta: Meta<UmbInputColorElement> = {
	title: 'Components/Inputs/Color',
	component: 'umb-input-color',
};

export default meta;
type Story = StoryObj<UmbInputColorElement>;

export const Overview: Story = {
	args: {
		showLabels: true,
		swatches: [
			{
				label: 'Red',
				value: '#ff0000',
			},
			{
				label: 'Green',
				value: '#00ff00',
			},
		],
	},
};

export const WithoutLabels: Story = {
	args: {
		showLabels: false,
		swatches: [
			{
				label: 'Red',
				value: '#ff0000',
			},
			{
				label: 'Green',
				value: '#00ff00',
			},
		],
	},
};

// TODO: This doesn't check the correct swatch when the value is set
// Perhaps a BUG ?
export const WithValueLabels: Story = {
	args: {
		value: '#00ff00',
		showLabels: true,
		swatches: [
			{
				label: 'Red',
				value: '#ff0000',
			},
			{
				label: 'Green',
				value: '#00ff00',
			},
		],
	},
};
