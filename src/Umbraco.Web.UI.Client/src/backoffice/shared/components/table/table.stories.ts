import { Meta, StoryObj } from '@storybook/web-components';
import './table.element';
import { v4 as uuidv4 } from 'uuid';
import type { UmbTableElement, UmbTableColumn, UmbTableConfig, UmbTableItem } from './table.element'

const meta: Meta<UmbTableElement> = {
    title: 'Components/Table',
    component: 'umb-table',
};
  
export default meta;
type Story = StoryObj<UmbTableElement>;

const today = new Intl.DateTimeFormat('en-US').format(new Date());

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

export const Overview: Story = {
    args: {
		items: items,
		columns: columns,
		config: {
			allowSelection: true,
			hideIcon: false,
		}
    }
};


export const WithDisallowedSelections: Story = {
    args: {
		items: items,
		columns: columns,
		config: {
			allowSelection: false,
			hideIcon: false,
		}
    }
};

export const WithHiddenIcons: Story = {
    args: {
		items: items,
		columns: columns,
		config: {
			allowSelection: true,
			hideIcon: true,
		}
    }
};