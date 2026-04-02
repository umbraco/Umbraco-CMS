import type { UmbInputDateElement } from './input-date.element.js';
import type { Meta, StoryObj } from '@storybook/web-components-vite';
import { html } from '@umbraco-cms/backoffice/external/lit';
import './input-date.element.js';

const meta: Meta<UmbInputDateElement> = {
	title: 'Generic Components/Inputs/Date',
	component: 'umb-input-date',
	render: (args) => html`<umb-input-date type=${args.type} value=${args.value}></umb-input-date>`,
};

export default meta;
type Story = StoryObj<UmbInputDateElement>;

export const Docs: Story = {
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
