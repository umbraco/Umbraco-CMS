import { html, customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';

const elementName = 'umb-media-type-folder-workspace-editor';
@customElement(elementName)
export class UmbMediaTypeFolderWorkspaceEditorElement extends UmbLitElement {
	override render() {
		return html`<umb-workspace-editor>
			<umb-workspace-header-name-editable slot="header"></umb-workspace-header-name-editable>
		</umb-workspace-editor>`;
	}

	static override styles = [UmbTextStyles];
}

export { UmbMediaTypeFolderWorkspaceEditorElement as element };

declare global {
	interface HTMLElementTagNameMap {
		[elementName]: UmbMediaTypeFolderWorkspaceEditorElement;
	}
}
