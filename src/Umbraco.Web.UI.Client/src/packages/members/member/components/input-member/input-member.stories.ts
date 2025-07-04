import type { Meta, StoryObj } from '@storybook/web-components-vite';
import './input-member.element.js';
import type { UmbInputMemberElement } from './input-member.element.js';

const meta: Meta<UmbInputMemberElement> = {
	title: 'Entity/Member/Components/Input Member',
	component: 'umb-input-member',
};

export default meta;
type Story = StoryObj<UmbInputMemberElement>;
export const Docs: Story = {
	args: {},
};
