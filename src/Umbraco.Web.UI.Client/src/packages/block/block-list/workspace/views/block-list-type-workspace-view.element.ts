import { css, html, customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { UmbWorkspaceViewElement } from '@umbraco-cms/backoffice/workspace';

@customElement('umb-block-list-type-workspace-view-settings')
export class UmbBlockListTypeWorkspaceViewSettingsElement extends UmbLitElement implements UmbWorkspaceViewElement {
	override render() {
		return html`
			<uui-box headline=${this.localize.term('blockEditor_headlineEditorAppearance')}>
				<umb-property
					label=${this.localize.term('general_label')}
					alias="label"
					property-editor-ui-alias="Umb.PropertyEditorUi.TextBox"></umb-property>
				<umb-property
					label=${this.localize.term('blockEditor_labelEditorSize')}
					alias="editorSize"
					property-editor-ui-alias="Umb.PropertyEditorUi.OverlaySize"></umb-property>
			</uui-box>
			<uui-box headline=${this.localize.term('blockEditor_headlineDataModels')}>
				<!-- TODO: implement readonly mode for umb-property -->
				<umb-property
					label=${this.localize.term('blockEditor_labelContentElementType')}
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
					label=${this.localize.term('blockEditor_labelSettingsElementType')}
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
			<uui-box headline=${this.localize.term('blockEditor_headlineCatalogueAppearance')}>
				<umb-property
					label=${this.localize.term('blockEditor_labelBackgroundColor')}
					alias="backgroundColor"
					property-editor-ui-alias="Umb.PropertyEditorUi.EyeDropper"
					.config=${[{ alias: 'showAlpha', value: true }]}></umb-property>
				<umb-property
					label=${this.localize.term('blockEditor_labelIconColor')}
					alias="iconColor"
					property-editor-ui-alias="Umb.PropertyEditorUi.EyeDropper"
					.config=${[{ alias: 'showAlpha', value: true }]}></umb-property>
				<umb-property
					label=${this.localize.term('blockEditor_thumbnail')}
					alias="thumbnail"
					property-editor-ui-alias="Umb.PropertyEditorUi.StaticFilePicker"
					.config=${[
						{
							alias: 'singleItemMode',
							value: true,
						},
					]}></umb-property>
			</uui-box>
			<uui-box headline=${this.localize.term('blockEditor_headlineAdvanced')}>
				<umb-property
					label=${this.localize.term('blockEditor_forceHideContentEditor')}
					alias="forceHideContentEditorInOverlay"
					description="Hide the content edit button and the content editor from the Block Editor overlay."
					property-editor-ui-alias="Umb.PropertyEditorUi.Toggle"></umb-property>
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

export default UmbBlockListTypeWorkspaceViewSettingsElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-block-list-type-workspace-view-settings': UmbBlockListTypeWorkspaceViewSettingsElement;
	}
}
