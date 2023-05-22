import { UUITextStyles } from '@umbraco-ui/uui-css';
import { css, html, nothing } from 'lit';
import { customElement, property } from 'lit/decorators.js';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { ManifestWorkspace } from '@umbraco-cms/backoffice/extension-registry';

@customElement('umb-workspace')
export class UmbWorkspaceElement extends UmbLitElement {
	@property({ type: String, attribute: 'entity-type' })
	entityType = '';

	render() {
		if (!this.entityType) nothing;
		return html`<umb-extension-slot
			type="workspace"
			.filter=${(manifest: ManifestWorkspace) => manifest.meta.entityType === this.entityType}></umb-extension-slot>`;
	}

	static styles = [UUITextStyles, css``];
}

export default UmbWorkspaceElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-workspace': UmbWorkspaceElement;
	}
}
