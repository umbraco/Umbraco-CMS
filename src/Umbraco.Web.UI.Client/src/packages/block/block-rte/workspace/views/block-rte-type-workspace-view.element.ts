import { css, html, customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { UmbWorkspaceViewElement } from '@umbraco-cms/backoffice/workspace';

@customElement('umb-block-rte-type-workspace-view-settings')
export class UmbBlockRteTypeWorkspaceViewSettingsElement extends UmbLitElement implements UmbWorkspaceViewElement {
	override render() {
		return html`
			<uui-box headline="Editor Appearance">
				<umb-property
					label="Label"
					alias="label"
					property-editor-ui-alias="Umb.PropertyEditorUi.TextBox"></umb-property>
				<umb-property
					label="Display Inline"
					alias="displayInline"
					property-editor-ui-alias="Umb.PropertyEditorUi.Toggle"></umb-property>
				<umb-property
					label="Overlay size"
					alias="editorSize"
					property-editor-ui-alias="Umb.PropertyEditorUi.OverlaySize"></umb-property>
			</uui-box>
			<uui-box headline="Data models">
				<!-- TODO: implement readonly mode for umb-property -->
				<umb-property
					label="Content Model"
					alias="contentElementTypeKey"
					property-editor-ui-alias="Umb.PropertyEditorUi.DocumentTypePicker"
					readonly
					.config=${[
						{
							alias: 'validationLimit',
							value: { min: 0, max: 1 },
						},
						{
							alias: 'onlyPickElementTypes',
							value: true,
						},
					]}></umb-property>
				<umb-property
					label="Settings Model"
					alias="settingsElementTypeKey"
					property-editor-ui-alias="Umb.PropertyEditorUi.DocumentTypePicker"
					.config=${[
						{
							alias: 'validationLimit',
							value: { min: 0, max: 1 },
						},
						{
							alias: 'onlyPickElementTypes',
							value: true,
						},
					]}></umb-property>
			</uui-box>
			<uui-box headline="Catalogue appearance">
				<umb-property
					label="Background color"
					alias="backgroundColor"
					property-editor-ui-alias="Umb.PropertyEditorUi.EyeDropper"
					.config=${[{ alias: 'showAlpha', value: true }]}></umb-property>
				<umb-property
					label="Icon color"
					alias="iconColor"
					property-editor-ui-alias="Umb.PropertyEditorUi.EyeDropper"
					.config=${[{ alias: 'showAlpha', value: true }]}></umb-property>
				<umb-property
					label="Thumbnail"
					alias="thumbnail"
					property-editor-ui-alias="Umb.PropertyEditorUi.StaticFilePicker"
					.config=${[
						{
							alias: 'singleItemMode',
							value: true,
						},
					]}></umb-property>
			</uui-box>
		`;
	}

	static override styles = [
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

export default UmbBlockRteTypeWorkspaceViewSettingsElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-block-rte-type-workspace-view-settings': UmbBlockRteTypeWorkspaceViewSettingsElement;
	}
}
