import { Meta, StoryObj } from '@storybook/web-components';
import './input-multi-url.element';
import type { UmbInputMultiUrlElement } from './input-multi-url.element';

const meta: Meta<UmbInputMultiUrlElement> = {
	title: 'Components/Inputs/Multi URL',
	component: 'umb-input-multi-url',
};

export default meta;
type Story = StoryObj<UmbInputMultiUrlElement>;

export const Overview: Story = {
	args: {},
};
