import { html, customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';

const elementName = 'umb-data-type-folder-workspace-editor';
@customElement(elementName)
export class UmbDataTypeFolderWorkspaceEditorElement extends UmbLitElement {
	override render() {
		return html`<umb-workspace-editor>
			<umb-workspace-editable-name-header slot="header"></umb-workspace-editable-name-header>
		</umb-workspace-editor>`;
	}

	static override styles = [UmbTextStyles];
}

export { UmbDataTypeFolderWorkspaceEditorElement as element };

declare global {
	interface HTMLElementTagNameMap {
		[elementName]: UmbDataTypeFolderWorkspaceEditorElement;
	}
}
