import type { Meta, StoryObj } from '@storybook/web-components';
import './tags-input.element.js';
import type { UmbTagsInputElement } from './tags-input.element.js';

const meta: Meta<UmbTagsInputElement> = {
	title: 'Components/Inputs/Tags',
	component: 'umb-tags-input',
};

export default meta;
type Story = StoryObj<UmbTagsInputElement>;

export const Overview: Story = {
	args: {
		group: 'Fruits',
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
			'Apple',
			'Banana',
			'Dragonfruit',
			'Blackberry',
			'Raspberry',
		],
	},
};
