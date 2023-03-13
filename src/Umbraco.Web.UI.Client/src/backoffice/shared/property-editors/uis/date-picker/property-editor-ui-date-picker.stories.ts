import { Meta, Story } from '@storybook/web-components';
import { html } from 'lit-html';

import type { UmbPropertyEditorUIDatePickerElement } from './property-editor-ui-date-picker.element';
import './property-editor-ui-date-picker.element';

export default {
	title: 'Property Editor UIs/Date Picker',
	component: 'umb-property-editor-ui-date-picker',
	id: 'umb-property-editor-ui-date-picker',
	args: {
		config: [
			{
				alias: 'format',
				value: 'YYYY-MM-DD HH:mm:ss'
			}
		]
	}
} as Meta<UmbPropertyEditorUIDatePickerElement>;

const Template: Story<UmbPropertyEditorUIDatePickerElement> = ({config, value}) => html`<umb-property-editor-ui-date-picker .config=${config} .value=${value}></umb-property-editor-ui-date-picker>`;

export const Overview = Template.bind({});

export const WithDateValue = Template.bind({});
WithDateValue.args = {
	value: '2021-01-24 15:20'
};

export const WithFormat = Template.bind({});
WithFormat.args = {
	config: [
		{
			alias: 'format',
			value: 'dd/MM/yyyy HH:mm:ss'
		}
	]
};

export const TimeOnly = Template.bind({});
TimeOnly.args = {
	config: [
		{
			alias: 'format',
			value: 'HH:mm:ss'
		}
	]
};

export const DateOnly = Template.bind({});
DateOnly.args = {
	config: [
		{
			alias: 'format',
			value: 'dd/MM/yyyy'
		}
	]
};