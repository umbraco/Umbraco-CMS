import { html, nothing, customElement, property } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { ManifestWorkspace } from '@umbraco-cms/backoffice/extension-registry';
import type { UmbApiConstructorArgumentsMethodType } from '@umbraco-cms/backoffice/extension-api';

const apiArgsCreator: UmbApiConstructorArgumentsMethodType<unknown> = (manifest: unknown) => {
	return [{ manifest }];
};

@customElement('umb-workspace')
export class UmbWorkspaceElement extends UmbLitElement {
	@property({ type: String, attribute: 'entity-type' })
	entityType = '';

	render() {
		if (!this.entityType) return nothing;
		return html`<umb-extension-with-api-slot
			type="workspace"
			.defaultApi=${() => import('./contexts/default-workspace.context.js')}
			.apiArgs=${apiArgsCreator}
			.filter=${(manifest: ManifestWorkspace) =>
				manifest.meta.entityType === this.entityType}></umb-extension-with-api-slot>`;
	}
}

export { UmbWorkspaceElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-workspace': UmbWorkspaceElement;
	}
}
