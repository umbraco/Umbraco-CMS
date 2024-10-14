import type { UmbUserGroupInputElement } from './user-group-input.element.js';
import type { Meta, StoryObj } from '@storybook/web-components';
import './user-group-input.element.js';

const meta: Meta<UmbUserGroupInputElement> = {
	title: 'User Group/Components/User Group Input',
	component: 'umb-user-group-input',
	argTypes: {
		// modalType: {
		// 	control: 'inline-radio',
		// 	options: ['dialog', 'sidebar'],
		// 	defaultValue: 'sidebar',
		// 	description: 'The type of modal to use when selecting user groups',
		// },
		// modalSize: {
		// 	control: 'select',
		// 	options: ['small', 'medium', 'large', 'full'],
		// 	defaultValue: 'small',
		// 	description: 'The size of the modal to use when selecting user groups, only applicable to sidebar not dialog',
		// },
	},
};

export default meta;
type Story = StoryObj<UmbUserGroupInputElement>;

export const Overview: Story = {
	args: {},
};
