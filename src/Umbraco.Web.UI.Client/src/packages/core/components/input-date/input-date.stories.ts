import { Meta, StoryObj } from '@storybook/web-components';
import type { UmbInputDateElement } from './input-date.element.js';
import { html } from '@umbraco-cms/backoffice/external/lit';
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
		offsetTime: true,
	},
};

export const Date: Story = {
	args: {
		type: 'date',
		value: '2023-04-01',
		offsetTime: false,
	},
};

export const Time: Story = {
	args: {
		type: 'time',
		value: '10:00',
		offsetTime: false,
	},
};

export const DatetimelocalOffset: Story = {
	args: {
		type: 'datetime-local',
		value: '2023-04-01T10:00:00',
		offsetTime: true,
		displayValue: '',
	},
	render: (args) =>
		html`<umb-input-date
			.type="${args.type}"
			.value="${args.value}"
			.offsetTime="${args.offsetTime}"
			.displayValue="${args.displayValue}"></umb-input-date>`,
};

export const Datetimelocal: Story = {
	args: {
		type: 'datetime-local',
		value: '2023-04-01T10:00:00',
		offsetTime: false,
		displayValue: '',
	},
	render: (args) =>
		html`<umb-input-date
			.type="${args.type}"
			.value="${args.value}"
			.offsetTime="${args.offsetTime}"
			.displayValue="${args.displayValue}"></umb-input-date>`,
};
