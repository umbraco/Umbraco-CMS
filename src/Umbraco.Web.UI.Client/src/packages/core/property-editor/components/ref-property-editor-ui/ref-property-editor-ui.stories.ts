import { UMB_PROPERTY_EDITOR_SCHEMA_ALIAS_DEFAULT } from '../../constants.js';
import type { UmbRefPropertyEditorUIElement } from './ref-property-editor-ui.element.js';
import type { Meta, StoryObj } from '@storybook/web-components';
import { html } from '@umbraco-cms/backoffice/external/lit';
import './ref-property-editor-ui.element.js';

const meta: Meta<UmbRefPropertyEditorUIElement> = {
	title: 'Components/Ref Property Editor UI',
	component: 'umb-ref-property-editor-ui',
};

export default meta;
type Story = StoryObj<UmbRefPropertyEditorUIElement>;

export const Overview: Story = {
	args: {
		name: 'Custom Property Editor UI',
		alias: 'Umb.PropertyEditorUi.CustomUI',
		propertyEditorSchemaAlias: UMB_PROPERTY_EDITOR_SCHEMA_ALIAS_DEFAULT,
	},
};

export const WithDetail: Story = {
	args: {
		name: 'Custom Property Editor UI',
		alias: 'Umb.PropertyEditorUi.CustomUI',
		propertyEditorSchemaAlias: UMB_PROPERTY_EDITOR_SCHEMA_ALIAS_DEFAULT,
		detail: 'With some custom details',
	},
};

export const WithSlots: Story = {
	args: {
		name: 'Custom Property Editor UI',
		alias: 'Umb.PropertyEditorUi.CustomUI',
		propertyEditorSchemaAlias: UMB_PROPERTY_EDITOR_SCHEMA_ALIAS_DEFAULT,
		detail: 'With some custom details',
	},
	render: (args) => html`
		<umb-ref-property-editor-ui
			.name=${args.name}
			.alias=${args.alias}
			.propertyEditorSchemaAlias=${args.propertyEditorSchemaAlias}
			.detail=${args.detail}>
			<div slot="tag"><uui-tag color="positive">10</uui-tag></div>
			<div slot="actions">
				<uui-action-bar>
					<uui-button label="delete" look="primary" color="danger" compact>
						<uui-icon name="icon-delete"></uui-icon>
					</uui-button>
				</uui-action-bar>
			</div>
		</umb-ref-property-editor-ui>
	`,
};
