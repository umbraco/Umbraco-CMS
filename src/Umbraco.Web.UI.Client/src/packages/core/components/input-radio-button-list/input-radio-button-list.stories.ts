import type { Meta, StoryObj } from '@storybook/web-components';
import './input-radio-button-list.element.js';
import type { UmbInputRadioButtonListElement } from './input-radio-button-list.element.js';

const meta: Meta<UmbInputRadioButtonListElement> = {
	title: 'Components/Inputs/Radio Button List',
	component: 'umb-input-radio-button-list',
};

export default meta;
type Story = StoryObj<UmbInputRadioButtonListElement>;

export const Overview: Story = {
	args: {
		list: [
			{ label: 'One', value: '1' },
			{ label: 'Two', value: '2' },
			{ label: 'Three', value: '3' },
		],
	},
};

export const WithSelectedValue: Story = {
	args: {
		list: [
			{ label: 'One', value: '1' },
			{ label: 'Two', value: '2' },
			{ label: 'Three', value: '3' },
		],
		value: '2',
	},
};

export const SortOrder: Story = {
	args: {
		list: [
			{ label: 'One', value: '1' },
			{ label: 'Two', value: '2' },
			{ label: 'Three', value: '3' },
		],
	},
};
