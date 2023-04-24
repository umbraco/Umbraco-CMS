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
	args: {
		type: 'datetime-local',
		value: '2023-04-01T10:00:00Z',
		offsetTime: true,
	},
};
