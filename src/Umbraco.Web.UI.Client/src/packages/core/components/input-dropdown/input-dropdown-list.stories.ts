import type { Meta, StoryObj } from '@storybook/web-components';
import './input-dropdown-list.element.js';
import type { UmbInputDropdownListElement } from './input-dropdown-list.element.js';

const meta: Meta<UmbInputDropdownListElement> = {
	title: 'Components/Inputs/Dropdown List',
	component: 'umb-input-dropdown-list',
};

export default meta;
type Story = StoryObj<UmbInputDropdownListElement>;

export const Overview: Story = {
	args: {
		options: [
			{
				name: 'One',
				value: 'One',
			},
			{
				name: 'Two',
				value: 'Two',
			},
			{
				name: 'Three',
				value: 'Three',
			},
		],
	},
};

export const WithSelectedValue: Story = {
	args: {
		options: [
			{
				name: 'One',
				value: 'One',
			},
			{
				name: 'Two',
				value: 'Two',
				selected: true,
			},
			{
				name: 'Three',
				value: 'Three',
			},
		],
	},
};
