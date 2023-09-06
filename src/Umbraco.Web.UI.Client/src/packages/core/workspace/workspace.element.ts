import { UmbTextStyles } from "@umbraco-cms/backoffice/style";
import { css, html, nothing, customElement, property } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { ManifestWorkspace } from '@umbraco-cms/backoffice/extension-registry';

@customElement('umb-workspace')
export class UmbWorkspaceElement extends UmbLitElement {
	@property({ type: String, attribute: 'entity-type' })
	entityType = '';

	render() {
		if (!this.entityType) return nothing;
		return html`<umb-extension-slot
			type="workspace"
			.filter=${(manifest: ManifestWorkspace) => manifest.meta.entityType === this.entityType}></umb-extension-slot>`;
	}

	static styles = [UmbTextStyles, css``];
}

export default UmbWorkspaceElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-workspace': UmbWorkspaceElement;
	}
}
