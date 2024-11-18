import './history-list.element.js';
import './history-item.element.js';

import type { UmbHistoryListElement } from './history-list.element.js';
import type { UmbHistoryItemElement } from './history-item.element.js';
import type { Meta, StoryFn } from '@storybook/web-components';
import { html } from '@umbraco-cms/backoffice/external/lit';

export default {
	title: 'Components/History UI',
	component: 'umb-history-list',
	id: 'umb-history-list',
} as Meta;

export const AAAOverview: StoryFn<UmbHistoryListElement> = () =>
	html` <umb-history-list>
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

export const Node: StoryFn<UmbHistoryItemElement> = () =>
	html`<umb-history-item name="Name attribute" detail="Detail attribute">
		Default slot
		<uui-button slot="actions" label="action">Action slot</uui-button>
	</umb-history-item>`;
