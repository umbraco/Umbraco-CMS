import type { Meta, StoryObj } from '@storybook/web-components';
import './input-content.element.js';
import type { UmbInputContentElement } from './input-content.element.js';

const meta: Meta<UmbInputContentElement> = {
	title: 'Components/Inputs/Content',
	component: 'umb-input-content',
};

export default meta;
type Story = StoryObj<UmbInputContentElement>;

export const Overview: Story = {
	args: {},
};
