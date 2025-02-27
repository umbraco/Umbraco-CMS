import type { Meta, StoryObj } from '@storybook/web-components';
import './input-member.element.js';
import type { UmbInputMemberElement } from './input-member.element.js';

const meta: Meta<UmbInputMemberElement> = {
	title: 'Components/Inputs/Member',
	component: 'umb-input-member',
};

export default meta;
type Story = StoryObj<UmbInputMemberElement>;
export const Overview: Story = {
	args: {},
};
