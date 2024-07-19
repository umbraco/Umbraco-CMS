import type { Meta, StoryObj } from '@storybook/web-components';
import './template-field-dropdown-list.element.js';
import type { UmbTemplateFieldDropdownListElement } from './template-field-dropdown-list.element.js';

const meta: Meta<UmbTemplateFieldDropdownListElement> = {
	title: 'Components/Inputs/Field Dropdown List',
	component: 'umb-template-field-dropdown-list',
};

export default meta;
type Story = StoryObj<UmbTemplateFieldDropdownListElement>;

export const Overview: Story = {
	args: {
		excludeMediaType: false,
	},
};
