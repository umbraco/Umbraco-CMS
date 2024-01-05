import { css, html, customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { UmbWorkspaceViewElement } from '@umbraco-cms/backoffice/extension-registry';

@customElement('umb-block-grid-type-workspace-view-advanced')
export class UmbBlockGridTypeWorkspaceViewAdvancedElement extends UmbLitElement implements UmbWorkspaceViewElement {
	render() {
		return html`
			<uui-box headline="Advanced">
				<umb-property
					label="Custom view"
					alias="view"
					property-editor-ui-alias="Umb.PropertyEditorUi.StaticFilePicker"></umb-property>
				<umb-property
					label="Custom stylesheet"
					alias="stylesheet"
					property-editor-ui-alias="Umb.PropertyEditorUi.StaticFilePicker"></umb-property>
				<umb-property
					label="Overlay size"
					alias="editorSize"
					property-editor-ui-alias="Umb.PropertyEditorUi.OverlaySize"></umb-property>
				<umb-property
					label="Inline editing"
					alias="inlineEditing"
					property-editor-ui-alias="Umb.PropertyEditorUi.Toggle"></umb-property>
				<umb-property
					label="Hide content editor"
					alias="hideContentEditor"
					property-editor-ui-alias="Umb.PropertyEditorUi.Toggle"></umb-property>
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
					label="Thumbnail"
					alias="thumbnail"
					property-editor-ui-alias="Umb.PropertyEditorUi.StaticFilePicker"></umb-property>
			</uui-box>
		`;
	}

	static styles = [
		UmbTextStyles,
		css`
			:host {
				display: grid;
				grid-template-columns: repeat(auto-fill, minmax(600px, 1fr));
				gap: var(--uui-size-layout-1);
				margin: var(--uui-size-layout-1);
				padding-bottom: var(--uui-size-layout-1); // To enforce some distance to the bottom of the scroll-container.
			}
		`,
	];
}

export default UmbBlockGridTypeWorkspaceViewAdvancedElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-block-grid-type-workspace-view-advanced': UmbBlockGridTypeWorkspaceViewAdvancedElement;
	}
}
