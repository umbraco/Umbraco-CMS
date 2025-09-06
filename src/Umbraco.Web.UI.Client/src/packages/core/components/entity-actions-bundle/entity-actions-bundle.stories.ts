import type { UmbEntityActionsBundleElement } from './entity-actions-bundle.element.js';
import type { Meta, StoryObj } from '@storybook/web-components-vite';
import { html } from '@umbraco-cms/backoffice/external/lit';
import './entity-actions-bundle.element.js';

const meta: Meta<UmbEntityActionsBundleElement> = {
	title: 'Extension Type/Entity Action/Components/Entity Actions Bundle',
	component: 'umb-entity-actions-bundle',
	render: (args) =>
		html` <umb-entity-actions-bundle
			.entityType=${args.entityType}
			.unique=${args.unique}></umb-entity-actions-bundle>`,
};

export default meta;
type Story = StoryObj<UmbEntityActionsBundleElement>;

export const Docs: Story = {
	args: {
		entityType: 'document',
		unique: '1234',
	},
};
