import { UMB_DICTIONARY_OVERVIEW_DASHBOARD_PATH } from '../dashboard/index.js';
import { html, customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

@customElement('umb-dictionary-workspace-editor')
export class UmbDictionaryWorkspaceEditorElement extends UmbLitElement {
	override render() {
		return html`
			<umb-workspace-editor back-path=${UMB_DICTIONARY_OVERVIEW_DASHBOARD_PATH}>
				<umb-workspace-header-name-editable slot="header"></umb-workspace-header-name-editable>
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
