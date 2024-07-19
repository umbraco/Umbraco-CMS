import type { UmbInputDateElement } from './input-date.element.js';
import type { Meta, StoryObj } from '@storybook/web-components';
import './input-date.element.js';

const meta: Meta<UmbInputDateElement> = {
	title: 'Components/Inputs/Date',
	component: 'umb-input-date',
};

export default meta;
type Story = StoryObj<UmbInputDateElement>;

export const Overview: Story = {
	args: {
		type: 'datetime-local',
		value: '2023-04-01T10:00:00Z',
	},
};

export const Date: Story = {
	args: {
		type: 'date',
		value: '2023-04-01',
	},
};

export const Time: Story = {
	args: {
		type: 'time',
		value: '10:00',
	},
};

export const Datetimelocal: Story = {
	args: {
		type: 'datetime-local',
		value: '2023-04-01T10:00:00',
	},
};
