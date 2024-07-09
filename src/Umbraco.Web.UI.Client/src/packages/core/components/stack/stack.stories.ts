import type { Meta, StoryObj } from '@storybook/web-components';
import './stack.element.js';
import type { UmbStackElement } from './stack.element.js';

const meta: Meta<UmbStackElement> = {
	title: 'Components/Stack',
	component: 'umb-stack',
};

export default meta;

type Story = StoryObj<UmbStackElement>;

export const Default: Story = {
	args: {},
};

export const Divide: Story = {
	args: {
		divide: true,
	},
};

export const Compact: Story = {
	args: {
		look: 'compact',
	},
};
