import type { UmbEntityActionListElement } from './entity-action-list.element.js';
import type { Meta, StoryObj } from '@storybook/web-components-vite';
import { html } from '@umbraco-cms/backoffice/external/lit';
import { UmbEntityContext, UMB_ENTITY_CONTEXT } from '@umbraco-cms/backoffice/entity';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import '@umbraco-cms/backoffice/context-api';

import './entity-action-list.element.js';

const meta: Meta<UmbEntityActionListElement> = {
	title: 'Extension Type/Entity Action/Components/Entity Action List',
	component: 'umb-entity-action-list',
	decorators: [
		(story) =>
			html`<umb-context-provider
				.key=${UMB_ENTITY_CONTEXT}
				.create=${(host: UmbControllerHost) => {
					const context = new UmbEntityContext(host);
					context.setEntityType('document');
					context.setUnique('1234');
					return context;
				}}
				>${story()}</umb-context-provider
			>`,
	],
	render: () => html`<umb-entity-action-list></umb-entity-action-list>`,
};

export default meta;
type Story = StoryObj<UmbEntityActionListElement>;

export const Docs: Story = {};
