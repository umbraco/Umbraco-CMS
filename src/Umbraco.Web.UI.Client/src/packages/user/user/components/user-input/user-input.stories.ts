import type { Meta, StoryObj } from '@storybook/web-components';
import './user-input.element.js';
import type { UmbUserInputElement } from './user-input.element.js';

const meta: Meta<UmbUserInputElement> = {
	title: 'Components/Inputs/User',
	component: 'umb-user-input',
	argTypes: {
		/*
		modalType: {
			control: 'inline-radio',
			options: ['dialog', 'sidebar'],
			defaultValue: 'sidebar',
			description: 'The type of modal to use when selecting users',
		},
		modalSize: {
			control: 'select',
			options: ['small', 'medium', 'large', 'full'],
			defaultValue: 'small',
			description: 'The size of the modal to use when selecting users, only applicable to sidebar not dialog',
		},
		*/
	},
};

export default meta;
type Story = StoryObj<UmbUserInputElement>;

export const Overview: Story = {
	args: {},
};
