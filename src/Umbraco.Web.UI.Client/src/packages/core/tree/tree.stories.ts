import type { Meta, StoryObj } from '@storybook/web-components';
import './default/default-tree.element.js';
import type { UmbTreeElement } from './tree.element.js';

const meta: Meta<UmbTreeElement> = {
	title: 'Components/Tree/Tree',
	component: 'umb-tree',
};

export default meta;
type Story = StoryObj<UmbTreeElement>;

// TODO: This does not display anything - need help
export const Overview: Story = {
	args: {
		alias: 'Umb.Tree.Document',
	},
};
