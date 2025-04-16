import type { Meta, StoryObj } from '@storybook/web-components';
import './input-culture-select.element.js';
import type { UmbInputCultureSelectElement } from './input-culture-select.element.js';

const meta: Meta<UmbInputCultureSelectElement> = {
	title: 'Components/Inputs/Culture Select',
	component: 'umb-input-culture-select',
};

export default meta;
type Story = StoryObj<UmbInputCultureSelectElement>;

export const Overview: Story = {
	args: {
		readonly: false,
		disabled: false,
	},
};

export const ReadOnly: Story = {
	args: {
		readonly: true,
		disabled: false,
	},
};

export const Disabled: Story = {
	args: {
		readonly: false,
		disabled: true,
	},
};

export const WithValue: Story = {
	args: {
		readonly: false,
		disabled: false,
		value: 'da-DK',
	},
};

export const WithValueAndDisabled: Story = {
	args: {
		readonly: false,
		disabled: true,
		value: 'en-US',
	},
};

export const WithValueAndReadOnly: Story = {
	args: {
		readonly: true,
		disabled: false,
		value: 'en-GB',
	},
};
