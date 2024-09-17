import { UMB_DISCARD_CHANGES_MODAL } from '../modal/common/discard-changes/discard-changes-modal.tokent.js';
import { html, nothing, customElement, property } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { ManifestWorkspace } from '@umbraco-cms/backoffice/workspace';
import type { UmbApiConstructorArgumentsMethodType } from '@umbraco-cms/backoffice/extension-api';
import { UMB_MODAL_MANAGER_CONTEXT } from '@umbraco-cms/backoffice/modal';

const apiArgsCreator: UmbApiConstructorArgumentsMethodType<unknown> = (manifest: unknown) => {
	return [{ manifest }];
};

@customElement('umb-workspace')
export class UmbWorkspaceElement extends UmbLitElement {
	@property({ type: String, attribute: 'entity-type' })
	entityType = '';

	constructor() {
		super();

		window.addEventListener(
			'willchangestate',
			async (e) => {
				// prevent the navigation
				e.preventDefault();

				const modalManager = await this.getContext(UMB_MODAL_MANAGER_CONTEXT);
				const modal = modalManager.open(this, UMB_DISCARD_CHANGES_MODAL);

				try {
					// navigate to the new url when discarding changes
					await modal.onSubmit();
					history.pushState({}, '', e.detail.url);
					return true;
				} catch {
					return false;
				}
			},
			{ once: true },
		);
	}

	override render() {
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
