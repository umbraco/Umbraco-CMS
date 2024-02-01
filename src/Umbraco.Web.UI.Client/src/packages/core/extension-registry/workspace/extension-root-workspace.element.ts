import { UMB_EXTENSION_COLLECTION_ALIAS } from '../collection/manifests.js';
import { UMB_EXTENSION_ROOT_WORKSPACE_ALIAS } from './manifests.js';
import { html, customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';

@customElement('umb-extension-root-workspace')
export class UmbExtensionRootWorkspaceElement extends UmbLitElement {
	render() {
		return html`
			<umb-workspace-editor headline="Extensions" alias=${UMB_EXTENSION_ROOT_WORKSPACE_ALIAS} .enforceNoFooter=${true}>
				<umb-collection alias=${UMB_EXTENSION_COLLECTION_ALIAS}></umb-collection>
			</umb-workspace-editor>
		`;
	}
}

export default UmbExtensionRootWorkspaceElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-extension-root-workspace': UmbExtensionRootWorkspaceElement;
	}
}
