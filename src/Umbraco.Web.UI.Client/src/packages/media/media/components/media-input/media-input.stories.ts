import { Meta, StoryObj } from '@storybook/web-components';
import './media-input.element.js';
import type { UmbMediaInputElement } from './media-input.element.js';

const meta: Meta<UmbMediaInputElement> = {
	title: 'Components/Inputs/Media',
	component: 'umb-media-input',
};

export default meta;
type Story = StoryObj<UmbMediaInputElement>;

export const Overview: Story = {
	args: {},
};
