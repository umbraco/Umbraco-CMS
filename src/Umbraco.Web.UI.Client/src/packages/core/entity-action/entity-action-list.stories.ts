import type { UmbEntityActionListElement } from './entity-action-list.element.js';
import type { Meta, StoryObj } from '@storybook/web-components-vite';
import { html } from '@umbraco-cms/backoffice/external/lit';
import './entity-action-list.element.js';

const meta: Meta<UmbEntityActionListElement> = {
	title: 'Extension Type/Entity Action/Components/Entity Action List',
	component: 'umb-entity-action-list',
	render: (args) =>
		html` <umb-entity-action-list .entityType=${args.entityType} .unique=${args.unique}></umb-entity-action-list>`,
};

export default meta;
type Story = StoryObj<UmbEntityActionListElement>;

export const Docs: Story = {
	args: {
		entityType: 'document',
		unique: '1234',
	},
};
