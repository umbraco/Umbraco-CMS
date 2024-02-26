import type { Meta, StoryObj } from '@storybook/web-components';
import './input-tree.element.js';
import type { UmbInputTreeElement } from './input-tree.element.js';

const meta: Meta<UmbInputTreeElement> = {
	title: 'Components/Inputs/Tree',
	component: 'umb-input-tree',
};

export default meta;
type Story = StoryObj<UmbInputTreeElement>;

export const Overview: Story = {
	args: {},
};
