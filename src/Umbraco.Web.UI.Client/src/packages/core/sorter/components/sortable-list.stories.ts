import type { UmbSortableListElement } from './sortable-list.element.js';
import type { Meta, StoryObj } from '@storybook/web-components-vite';
import { html } from '@umbraco-cms/backoffice/external/lit';

import './sortable-list.element.js';
import './sortable-list-item.element.js';

const items = ['Item 1', 'Item 2', 'Item 3'];

const meta: Meta<UmbSortableListElement<string>> = {
	title: 'Generic Components/Sortable List',
	component: 'umb-sortable-list',
	args: {
		disabled: false,
	},
	render: (args) => html`
		<umb-sortable-list
			.items=${items}
			.getUnique=${(item: string) => item}
			.renderMethod=${(item: string) => html`
				<umb-sortable-list-item .unique=${item} ?disabled=${args.disabled}>
					<uui-input .value=${item} ?disabled=${args.disabled}></uui-input>
					<uui-action-bar slot="actions">
						<uui-button label="Remove"><uui-icon name="icon-trash"></uui-icon></uui-button>
					</uui-action-bar>
				</umb-sortable-list-item>
			`}
			?disabled=${args.disabled}>
		</umb-sortable-list>
	`,
};

export default meta;
type Story = StoryObj<UmbSortableListElement<string>>;

export const Default: Story = {};

export const Disabled: Story = {
	args: {
		disabled: true,
	},
};
