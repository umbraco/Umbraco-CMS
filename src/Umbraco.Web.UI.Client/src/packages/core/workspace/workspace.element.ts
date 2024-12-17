import { html, nothing, customElement, property, type PropertyValueMap } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { ManifestWorkspace } from '@umbraco-cms/backoffice/workspace';
import type { UmbApiConstructorArgumentsMethodType } from '@umbraco-cms/backoffice/extension-api';
import { UMB_MARK_ATTRIBUTE_NAME } from '@umbraco-cms/backoffice/const';

const apiArgsCreator: UmbApiConstructorArgumentsMethodType<unknown> = (manifest: unknown) => {
	return [{ manifest }];
};

@customElement('umb-workspace')
export class UmbWorkspaceElement extends UmbLitElement {
	@property({ type: String, attribute: 'entity-type' })
	entityType = '';

	protected override firstUpdated(_changedProperties: PropertyValueMap<any> | Map<PropertyKey, unknown>): void {
		super.firstUpdated(_changedProperties);
		this.setAttribute(UMB_MARK_ATTRIBUTE_NAME, 'workspace');
	}

	override render() {
		if (!this.entityType) return nothing;
		return html`<umb-extension-with-api-slot
			type="workspace"
			.defaultApi=${() => import('./kinds/default/default-workspace.context.js')}
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
