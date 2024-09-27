import { html, customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

@customElement('umb-dictionary-workspace-editor')
export class UmbDictionaryWorkspaceEditorElement extends UmbLitElement {
	override render() {
		return html`
			<umb-workspace-editor back-path="section/dictionary/dashboard">
				<umb-workspace-name slot="header"></umb-workspace-name>
			</umb-workspace-editor>
		`;
	}
}

export default UmbDictionaryWorkspaceEditorElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-dictionary-workspace-editor': UmbDictionaryWorkspaceEditorElement;
	}
}
