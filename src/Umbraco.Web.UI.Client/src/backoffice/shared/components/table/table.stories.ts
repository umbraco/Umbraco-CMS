import { Meta, Story } from '@storybook/web-components';
import { html } from 'lit-html';
import { v4 as uuidv4 } from 'uuid';

import type { UmbTableElement, UmbTableColumn, UmbTableConfig, UmbTableItem } from './table.element';

import './table.element';

export default {
	title: 'Components/Table',
	component: 'umb-table',
	id: 'umb-table',
} as Meta;

const columns: Array<UmbTableColumn> = [
	{
		name: 'Name',
		alias: 'name',
	},
	{
		name: 'Date',
		alias: 'date',
	},
];

const today = new Intl.DateTimeFormat('en-US').format(new Date());

const items: Array<UmbTableItem> = [
	{
		key: uuidv4(),
		icon: 'umb:wand',
		data: [
			{
				columnAlias: 'name',
				value: 'Item 1',
			},
			{
				columnAlias: 'date',
				value: today,
			},
		],
	},
	{
		key: uuidv4(),
		icon: 'umb:document',
		data: [
			{
				columnAlias: 'name',
				value: 'Item 2',
			},
			{
				columnAlias: 'date',
				value: today,
			},
		],
	},
	{
		key: uuidv4(),
		icon: 'umb:user',
		data: [
			{
				columnAlias: 'name',
				value: 'Item 3',
			},
			{
				columnAlias: 'date',
				value: today,
			},
		],
	},
];

const config: UmbTableConfig = {
	allowSelection: true,
	hideIcon: false,
};

export const AAAOverview: Story<UmbTableElement> = () =>
	html`<umb-table .items=${items} .columns=${columns} .config=${config}></umb-table>`;
AAAOverview.storyName = 'Overview';
