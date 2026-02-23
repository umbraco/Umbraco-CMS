import type { UmbEntityActionsBundleElement } from './entity-actions-bundle.element.js';
import type { Meta, StoryObj } from '@storybook/web-components-vite';
import { html } from '@umbraco-cms/backoffice/external/lit';
import { UmbEntityContext, UMB_ENTITY_CONTEXT } from '@umbraco-cms/backoffice/entity';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import '@umbraco-cms/backoffice/context-api';

import './entity-actions-bundle.element.js';

const meta: Meta<UmbEntityActionsBundleElement> = {
	title: 'Extension Type/Entity Action/Components/Entity Actions Bundle',
	component: 'umb-entity-actions-bundle',
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
	render: () => html`<umb-entity-actions-bundle></umb-entity-actions-bundle>`,
};

export default meta;
type Story = StoryObj<UmbEntityActionsBundleElement>;

export const Docs: Story = {};
