import type { Meta, StoryObj } from '@storybook/web-components';
import './default/default-tree.element.js';
import type { UmbDefaultTreeElement } from './default/default-tree.element.js';

const meta: Meta<UmbDefaultTreeElement> = {
	title: 'Components/Tree/Tree',
	component: 'umb-tree',
};

export default meta;
type Story = StoryObj<UmbDefaultTreeElement>;

// TODO: This does not display anything - need help
export const Overview: Story = {
	args: {
		alias: 'Umb.Tree.Document',
	},
};
