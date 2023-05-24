import { Meta, StoryObj } from '@storybook/web-components';
import { html } from '@umbraco-cms/backoffice/external/lit';
import './ref-data-type.element';
import type { UmbRefDataTypeElement } from './ref-data-type.element.js';

const meta: Meta<UmbRefDataTypeElement> = {
	title: 'Components/Ref Data Type',
	component: 'umb-ref-property-editor-ui',
};

export default meta;
type Story = StoryObj<UmbRefDataTypeElement>;

export const Overview: Story = {
	args: {
		name: 'Custom Data Type',
		propertyEditorUiAlias: 'Umb.DataTypeInput.CustomUI',
		propertyEditorModelAlias: 'Umbraco.JSON',
	},
};

export const WithDetail: Story = {
	args: {
		name: 'Custom Data Type',
		propertyEditorUiAlias: 'Umb.DataType.CustomUI',
		propertyEditorModelAlias: 'UmbracoInput.JSON',
		detail: 'With some custom details',
	},
};

export const WithSlots: Story = {
	args: {
		name: 'Custom Data Type',
		propertyEditorUiAlias: 'Umb.DataTypeInput.CustomUI',
		propertyEditorModelAlias: 'Umbraco.JSON',
		detail: 'With some custom details',
	},
	render: (args) => html`
		<umb-ref-data-type
			.name=${args.name}
			.propertyEditorUiAlias=${args.propertyEditorUiAlias}
			.propertyEditorModelAlias=${args.propertyEditorModelAlias}
			.detail=${args.detail}>
			<div slot="tag"><uui-tag color="positive">10</uui-tag></div>
			<div slot="actions">
				<uui-action-bar>
					<uui-button label="delete" look="primary" color="danger" compact>
						<uui-icon name="umb:delete"></uui-icon>
					</uui-button>
				</uui-action-bar>
			</div>
		</umb-ref-data-type>
	`,
};
