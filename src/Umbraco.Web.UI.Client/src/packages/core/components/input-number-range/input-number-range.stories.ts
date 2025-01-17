import type { Meta, StoryObj } from '@storybook/web-components';
import './input-number-range.element.js';
import type { UmbInputNumberRangeElement } from './input-number-range.element.js';

const meta: Meta<UmbInputNumberRangeElement> = {
	title: 'Components/Inputs/Number Range Picker',
	component: 'umb-input-number-range',
};

export default meta;
type Story = StoryObj<UmbInputNumberRangeElement>;

export const Overview: Story = {
	args: {},
};

export const WithMinMax: Story = {
	args: {
		minValue: 0,
		maxValue: 40,
	},
};
