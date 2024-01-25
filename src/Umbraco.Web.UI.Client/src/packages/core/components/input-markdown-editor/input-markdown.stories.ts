import type { Meta, StoryObj } from '@storybook/web-components';
import './input-markdown.element.js';
import type { UmbInputMarkdownElement } from './input-markdown.element.js';

const meta: Meta<UmbInputMarkdownElement> = {
	title: 'Components/Inputs/Markdown',
	component: 'umb-input-markdown',
	args: {
		preview: false,
	},
};

export default meta;
type Story = StoryObj<UmbInputMarkdownElement>;

export const Overview: Story = {};
