import { Meta, StoryObj } from '@storybook/web-components';
import './tree-item-base.element';
import type { UmbTreeItemBaseElement } from './tree-item-base.element';

// TODO: provide tree item context to make this element render properly
const meta: Meta<UmbTreeItemBaseElement> = {
	title: 'Components/Tree/Tree Item',
	component: 'umb-tree-item',
};

export default meta;
type Story = StoryObj<UmbTreeItemBaseElement>;

export const Overview: Story = {
	args: {},
};

export const WithChildren: Story = {
	args: {},
};
