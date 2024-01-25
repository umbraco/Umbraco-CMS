import type { Meta, StoryObj } from '@storybook/web-components';
import './input-media.element.js';
import type { UmbInputMediaElement } from './input-media.element.js';

const meta: Meta<UmbInputMediaElement> = {
	title: 'Components/Inputs/Media',
	component: 'umb-input-media',
};

export default meta;
type Story = StoryObj<UmbInputMediaElement>;

export const Overview: Story = {
	args: {},
};
