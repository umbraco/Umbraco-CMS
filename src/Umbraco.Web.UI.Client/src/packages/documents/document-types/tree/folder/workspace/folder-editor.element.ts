import { css, html, customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement, umbFocus } from '@umbraco-cms/backoffice/lit-element';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';

const elementName = 'umb-folder-workspace-editor';
@customElement(elementName)
export class UmbFolderWorkspaceEditorElement extends UmbLitElement {
	override render() {
		return html`<umb-workspace-editor alias="Umb.Workspace.DocumentType.Folder">
			<div id="header" slot="header">
				<uui-input label="Folder name" value="" ${umbFocus()}></uui-input>
			</div>
		</umb-workspace-editor>`;
	}

	static override styles = [UmbTextStyles, css``];
}

export { UmbFolderWorkspaceEditorElement as element };

declare global {
	interface HTMLElementTagNameMap {
		[elementName]: UmbFolderWorkspaceEditorElement;
	}
}
