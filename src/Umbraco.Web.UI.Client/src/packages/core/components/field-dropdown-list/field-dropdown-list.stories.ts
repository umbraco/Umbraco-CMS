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
		excludeMediaType: false,
	},
};
