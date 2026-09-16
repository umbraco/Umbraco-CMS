import { html, customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

@customElement('umb-dictionary-workspace-editor')
export class UmbDictionaryWorkspaceEditorElement extends UmbLitElement {
	override render() {
		return html`
			<umb-entity-detail-workspace-editor>
				<umb-entity-named-detail-workspace-header slot="header"></umb-entity-named-detail-workspace-header>
			</umb-entity-detail-workspace-editor>
		`;
	}
}

export default UmbDictionaryWorkspaceEditorElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-dictionary-workspace-editor': UmbDictionaryWorkspaceEditorElement;
	}
}
