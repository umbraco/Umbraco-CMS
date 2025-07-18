import type { UmbInputNumberRangeElement } from './input-number-range.element.js';
import type { Meta, StoryObj } from '@storybook/web-components-vite';

import './input-number-range.element.js';

const meta: Meta<UmbInputNumberRangeElement> = {
	title: 'Generic Components/Inputs/Number Range Picker',
	component: 'umb-input-number-range',
};

export default meta;
type Story = StoryObj<UmbInputNumberRangeElement>;

export const Docs: Story = {
	args: {
		minValue: undefined,
		maxValue: undefined,
	},
};

export const WithMinMax: Story = {
	args: {
		minValue: 0,
		maxValue: 40,
	},
};
