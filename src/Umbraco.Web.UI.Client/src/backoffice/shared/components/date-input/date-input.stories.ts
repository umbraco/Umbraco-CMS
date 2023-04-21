import { Meta, StoryObj } from '@storybook/web-components';
import './date-input.element';
import type { UmbDateInputElement } from './date-input.element';

const meta: Meta<UmbDateInputElement> = {
	title: 'Components/Inputs/Date',
	component: 'umb-date-input',
};

export default meta;
type Story = StoryObj<UmbDateInputElement>;

export const Overview: Story = {
	args: {},
};

export const WithOpacity: Story = {
	args: {},
};

export const WithSwatches: Story = {
	args: {},
};
