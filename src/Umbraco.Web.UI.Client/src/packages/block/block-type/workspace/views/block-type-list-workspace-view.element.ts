import { css, html, customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { UmbWorkspaceViewElement } from '@umbraco-cms/backoffice/extension-registry';

@customElement('umb-block-type-list-workspace-view-settings')
export class UmbBlockTypeListWorkspaceViewSettingsElement extends UmbLitElement implements UmbWorkspaceViewElement {
	render() {
		return html`
			<uui-box headline="Editor Appearance">
				<umb-property
					label="Label"
					alias="label"
					property-editor-ui-alias="Umb.PropertyEditorUi.TextBox"></umb-property>
				<umb-property
					label="Custom view"
					alias="view"
					property-editor-ui-alias="Umb.PropertyEditorUi.StaticFile"></umb-property>
				<umb-property
					label="Custom stylesheet"
					alias="stylesheet"
					property-editor-ui-alias="Umb.PropertyEditorUi.StaticFile"></umb-property>
				<umb-property
					label="Overlay size"
					alias="editorSize"
					property-editor-ui-alias="Umb.PropertyEditorUi.OverlaySize"></umb-property>
			</uui-box>
			<uui-box headline="Data models">
				<umb-property
					label="Content Model"
					alias="contentElementTypeKey"
					property-editor-ui-alias="Umb.PropertyEditorUi.ElementTypePicker"></umb-property>
				<umb-property
					label="Settings Model"
					alias="settingsElementTypeKey"
					property-editor-ui-alias="Umb.PropertyEditorUi.ElementTypePicker"></umb-property>
			</uui-box>
			<uui-box headline="Catalogue appearance">
				<umb-property
					label="Background color"
					alias="backgroundColor"
					property-editor-ui-alias="Umb.PropertyEditorUi.TextBox"></umb-property>
				<umb-property
					label="Icon color"
					alias="iconColor"
					property-editor-ui-alias="Umb.PropertyEditorUi.TextBox"></umb-property>
				<umb-property
					label="Custom stylesheet"
					alias="stylesheet"
					property-editor-ui-alias="Umb.PropertyEditorUi.StaticFile"></umb-property>
			</uui-box>
		`;
	}

	static styles = [
		UmbTextStyles,
		css`
			:host {
				display: block;
				margin: var(--uui-size-layout-1);
				padding-bottom: var(--uui-size-layout-1); // To enforce some distance to the bottom of the scroll-container.
			}
			uui-box {
				margin-top: var(--uui-size-layout-1);
			}

			uui-label,
			umb-property-editor-ui-number {
				display: block;
			}

			// TODO: is this necessary?
			uui-toggle {
				display: flex;
			}
		`,
	];
}

export default UmbBlockTypeListWorkspaceViewSettingsElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-block-type-list-workspace-view-settings': UmbBlockTypeListWorkspaceViewSettingsElement;
	}
}
