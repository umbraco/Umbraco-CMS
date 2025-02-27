import type { Meta, StoryObj } from '@storybook/web-components';
import './input-slider.element.js';
import type { UmbInputSliderElement } from './input-slider.element.js';

const meta: Meta<UmbInputSliderElement> = {
	title: 'Components/Inputs/Slider',
	component: 'umb-input-slider',
};

export default meta;
type Story = StoryObj<UmbInputSliderElement>;

export const Overview: Story = {
	args: {
		min: 0,
		max: 100,
		step: 10,
		valueLow: 20,
	},
};

export const WithRange: Story = {
	args: {
		min: 0,
		max: 100,
		step: 10,
		valueLow: 20,
		valueHigh: 80,
		enableRange: true,
	},
};

export const WithSmallStep: Story = {
	args: {
		min: 0,
		max: 5,
		step: 1,
		valueLow: 4,
	},
};

export const WithLargeMinMax: Story = {
	args: {
		min: 0,
		max: 100,
		step: 1,
		valueLow: 86,
	},
};
