import type { UmbPropertyEditorUIDatePickerElement } from './property-editor-ui-date-picker.element.js';
import type { Meta, StoryFn } from '@storybook/web-components';
import { html } from '@umbraco-cms/backoffice/external/lit';

import './property-editor-ui-date-picker.element.js';
import { UmbPropertyEditorConfigCollection } from '@umbraco-cms/backoffice/property-editor';

export default {
	title: 'Property Editor UIs/Date Picker',
	component: 'umb-property-editor-ui-date-picker',
	id: 'umb-property-editor-ui-date-picker',
	args: {
		config: new UmbPropertyEditorConfigCollection([
			{
				alias: 'format',
				value: 'YYYY-MM-DD HH:mm:ss',
			},
		]),
	},
} as Meta<UmbPropertyEditorUIDatePickerElement>;

const Template: StoryFn<UmbPropertyEditorUIDatePickerElement> = ({ config, value }) =>
	html`<umb-property-editor-ui-date-picker .config=${config} .value=${value}></umb-property-editor-ui-date-picker>`;

export const Overview = Template.bind({});

export const WithDateValue = Template.bind({});
WithDateValue.args = {
	value: '2021-01-24 15:20',
};

export const WithFormat = Template.bind({});
WithFormat.args = {
	config: new UmbPropertyEditorConfigCollection([
		{
			alias: 'format',
			value: 'dd/MM/yyyy HH:mm:ss',
		},
	]),
};

export const Timeframe = Template.bind({});
Timeframe.args = {
	config: new UmbPropertyEditorConfigCollection([
		{
			alias: 'format',
			value: 'dd/MM/yyyy HH:mm:ss',
		},
		{
			alias: 'min',
			value: '2021-01-20 00:00',
		},
		{
			alias: 'max',
			value: '2021-01-30 00:00',
		},
	]),
};

export const TimeOnly = Template.bind({});
TimeOnly.args = {
	config: new UmbPropertyEditorConfigCollection([
		{
			alias: 'format',
			value: 'HH:mm:ss',
		},
	]),
};

export const DateOnly = Template.bind({});
DateOnly.args = {
	config: new UmbPropertyEditorConfigCollection([
		{
			alias: 'format',
			value: 'dd/MM/yyyy',
		},
	]),
};
