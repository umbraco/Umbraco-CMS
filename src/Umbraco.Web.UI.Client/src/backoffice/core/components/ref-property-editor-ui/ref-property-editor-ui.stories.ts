import { Meta, StoryObj } from '@storybook/web-components';
import { html } from 'lit';
import './ref-property-editor-ui.element';
import type { UmbRefPropertyEditorUIElement } from './ref-property-editor-ui.element';

const meta: Meta<UmbRefPropertyEditorUIElement> = {
	title: 'Components/Ref Property Editor UI',
	component: 'umb-ref-property-editor-ui',
};

export default meta;
type Story = StoryObj<UmbRefPropertyEditorUIElement>;

export const Overview: Story = {
	args: {
		name: 'Custom Property Editor UI',
		alias: 'Umb.PropertyEditorUI.CustomUI',
		propertyEditorAlias: 'Umbraco.JSON',
	},
};

export const WithDetail: Story = {
	args: {
		name: 'Custom Property Editor UI',
		alias: 'Umb.PropertyEditorUI.CustomUI',
		propertyEditorAlias: 'Umbraco.JSON',
		detail: 'With some custom details',
	},
};

export const WithSlots: Story = {
	args: {
		name: 'Custom Property Editor UI',
		alias: 'Umb.PropertyEditorUI.CustomUI',
		propertyEditorAlias: 'Umbraco.JSON',
		detail: 'With some custom details',
	},
	render: (args) => html`
		<umb-ref-property-editor-ui
			.name=${args.name}
			.alias=${args.alias}
			.propertyEditorAlias=${args.propertyEditorAlias}
			.detail=${args.detail}>
			<div slot="tag"><uui-tag color="positive">10</uui-tag></div>
			<div slot="actions">
				<uui-action-bar>
					<uui-button label="delete" look="primary" color="danger" compact>
						<uui-icon name="umb:delete"></uui-icon>
					</uui-button>
				</uui-action-bar>
			</div>
		</umb-ref-property-editor-ui>
	`,
};
