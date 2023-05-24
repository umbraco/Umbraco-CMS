import { Meta, StoryObj } from '@storybook/web-components';
import { html } from 'lit';
import './date-input.element';
import type { UmbDateInputElement } from './date-input.element.js';

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
		html`<umb-date-input
			.type="${args.type}"
			.value="${args.value}"
			.offsetTime="${args.offsetTime}"
			.displayValue="${args.displayValue}"></umb-date-input>`,
};

export const Datetimelocal: Story = {
	args: {
		type: 'datetime-local',
		value: '2023-04-01T10:00:00',
		offsetTime: false,
		displayValue: '',
	},
	render: (args) =>
		html`<umb-date-input
			.type="${args.type}"
			.value="${args.value}"
			.offsetTime="${args.offsetTime}"
			.displayValue="${args.displayValue}"></umb-date-input>`,
};
