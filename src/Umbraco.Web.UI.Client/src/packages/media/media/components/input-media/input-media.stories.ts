import type { Meta, StoryObj } from '@storybook/web-components-vite';
import './input-media.element.js';
import type { UmbInputMediaElement } from './input-media.element.js';

const meta: Meta<UmbInputMediaElement> = {
	title: 'Entity/Media/Components/Input Media',
	component: 'umb-input-media',
};

export default meta;
type Story = StoryObj<UmbInputMediaElement>;

export const Docs: Story = {
	args: {},
};
