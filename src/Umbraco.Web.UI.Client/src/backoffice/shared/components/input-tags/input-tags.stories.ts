import { Meta, StoryObj } from '@storybook/web-components';
import './input-tags.element';
import type { UmbInputTagsElement } from './input-tags.element';

const meta: Meta<UmbInputTagsElement> = {
	title: 'Components/Inputs/Tags',
	component: 'umb-input-tags',
};

export default meta;
type Story = StoryObj<UmbInputTagsElement>;

export const Overview: Story = {
	args: {
		group: 'default',
		items: [],
	},
};

export const WithTags: Story = {
	args: {
		group: 'default',
		items: ['Flour', 'Eggs', 'Butter', 'Sugar', 'Salt', 'Milk'],
	},
};

export const WithTags2: Story = {
	args: {
		group: 'default',
		items: [
			'Cranberry',
			'Kiwi',
			'Blueberries',
			'Watermelon',
			'Tomato',
			'Mango',
			'Strawberry',
			'Water Chestnut',
			'Papaya',
			'Orange Rind',
			'Olives',
			'Pear',
			'Sultana',
			'Mulberry',
			'Lychee',
			'Lemon',
		],
	},
};
