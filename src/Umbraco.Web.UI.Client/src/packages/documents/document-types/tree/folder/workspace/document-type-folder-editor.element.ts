import { html, customElement, css } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';

const elementName = 'umb-document-type-folder-workspace-editor';
@customElement(elementName)
export class UmbDocumentTypeFolderWorkspaceEditorElement extends UmbLitElement {
	override render() {
		return html`<umb-workspace-editor>
			<umb-icon id="icon" slot="header" name="icon-folder"></umb-icon>
			<umb-workspace-header-name-editable slot="header"></umb-workspace-header-name-editable>
		</umb-workspace-editor>`;
	}

	static override styles = [
		UmbTextStyles,
		css`
			#icon {
				display: inline-block;
				font-size: var(--uui-size-6);
				margin-right: var(--uui-size-space-4);
			}
		`,
	];
}

export { UmbDocumentTypeFolderWorkspaceEditorElement as element };

declare global {
	interface HTMLElementTagNameMap {
		[elementName]: UmbDocumentTypeFolderWorkspaceEditorElement;
	}
}
