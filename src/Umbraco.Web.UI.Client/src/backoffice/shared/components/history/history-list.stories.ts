import './history-list.element';
import './history-item.element';

import { Meta, Story } from '@storybook/web-components';
import { html } from 'lit-html';

import type { UmbHistoryListElement } from './history-list.element';
import type { UmbHistoryItemElement } from './history-item.element';

export default {
	title: 'Components/History UI',
	component: 'umb-history-list',
	id: 'umb-history-list',
} as Meta;

export const AAAOverview: Story<UmbHistoryListElement> = () => html` <umb-history-list>
	<umb-history-item name="Name attribute" detail="Detail attribute">
		Default slot
		<uui-button slot="actions" label="action">Action slot</uui-button>
	</umb-history-item>
	<umb-history-item name="Name attribute" detail="Detail attribute">
		Default slot
		<uui-button slot="actions" label="action">Action slot</uui-button>
	</umb-history-item>
	<umb-history-item name="Name attribute" detail="Detail attribute">
		Default slot
		<uui-button slot="actions" label="action">Action slot</uui-button>
	</umb-history-item>
</umb-history-list>`;
AAAOverview.storyName = 'Overview';

export const Node: Story<UmbHistoryItemElement> = () => html`<umb-history-item
	name="Name attribute"
	detail="Detail attribute">
	Default slot
	<uui-button slot="actions" label="action">Action slot</uui-button>
</umb-history-item>`;
