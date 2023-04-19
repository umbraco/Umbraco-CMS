import { Meta, StoryObj } from '@storybook/web-components';
import './tag-input.element';
import type { UmbTagInputElement } from './tag-input.element';

const meta: Meta<UmbTagInputElement> = {
	title: 'Components/Inputs/Tags',
	component: 'umb-tag-input',
};

export default meta;
type Story = StoryObj<UmbTagInputElement>;

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
