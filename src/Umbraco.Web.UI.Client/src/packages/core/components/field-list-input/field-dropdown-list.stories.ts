import type { Meta, StoryObj } from '@storybook/web-components';
import './field-dropdown-list.element.js';
import type { UmbFieldDropdownListElement } from './field-dropdown-list.element.js';

const meta: Meta<UmbFieldDropdownListElement> = {
	title: 'Components/Inputs/Field Dropdown List',
	component: 'umb-field-dropdown-list',
};

export default meta;
type Story = StoryObj<UmbFieldDropdownListElement>;

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
