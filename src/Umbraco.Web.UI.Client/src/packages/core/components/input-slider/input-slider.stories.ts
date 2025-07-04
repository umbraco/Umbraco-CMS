import type { UmbInputSliderElement } from './input-slider.element.js';
import type { Meta, StoryObj } from '@storybook/web-components-vite';
import { html } from '@umbraco-cms/backoffice/external/lit';
import './input-slider.element.js';

const meta: Meta<UmbInputSliderElement> = {
	title: 'Generic Components/Inputs/Slider',
	component: 'umb-input-slider',
	render: (args) =>
		html`<umb-input-slider
			.min="${args.min}"
			.max="${args.max}"
			.step="${args.step}"
			.valueLow="${args.valueLow}"
			.valueHigh="${args.valueHigh}"
			?enableRange="${args.enableRange}"></umb-input-slider>`,
};

export default meta;
type Story = StoryObj<UmbInputSliderElement>;

export const Docs: Story = {
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
