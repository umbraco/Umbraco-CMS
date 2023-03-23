import { Meta, StoryObj } from '@storybook/web-components';
import './tree-item-base.element';
import type { UmbTreeItemBaseElement } from './tree-item-base.element';

const meta: Meta<UmbTreeItemBaseElement> = {
	title: 'Components/Tree/Tree Item',
	component: 'umb-tree-item',
};

export default meta;
type Story = StoryObj<UmbTreeItemBaseElement>;

export const Overview: Story = {
	args: {
		label: 'My Tree Item',
		icon: 'umb:home',
		hasChildren: false,
	},
};

export const WithChildren: Story = {
	args: {
		label: 'My Tree Item',
		icon: 'umb:home',
		hasChildren: true,
	},
};
