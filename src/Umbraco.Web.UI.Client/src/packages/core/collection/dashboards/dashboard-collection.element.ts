import { UUITextStyles } from '@umbraco-cms/backoffice/external/uui';
import { css, html, customElement, state, ifDefined } from '@umbraco-cms/backoffice/external/lit';
import { UMB_COLLECTION_CONTEXT_TOKEN, UmbCollectionContext } from '@umbraco-cms/backoffice/collection';
import type { ManifestDashboardCollection } from '@umbraco-cms/backoffice/extension-registry';
import type { FolderTreeItemResponseModel } from '@umbraco-cms/backoffice/backend-api';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';

import '../collection.element.js';

@customElement('umb-dashboard-collection')
export class UmbDashboardCollectionElement extends UmbLitElement {
	// TODO: Use the right type here:
	private _collectionContext?: UmbCollectionContext<FolderTreeItemResponseModel, any>;

	public manifest!: ManifestDashboardCollection;

	@state()
	private _entityType?: string;

	connectedCallback(): void {
		super.connectedCallback();

		if (!this._collectionContext) {
			const repositoryAlias = this.manifest.meta.repositoryAlias;
			this._entityType = this.manifest.conditions.entityType;
			this._collectionContext = new UmbCollectionContext(this, this._entityType, repositoryAlias);
			this.provideContext(UMB_COLLECTION_CONTEXT_TOKEN, this._collectionContext);
		}
	}

	render() {
		return html`<umb-collection entity-type=${ifDefined(this._entityType)}></umb-collection>`;
	}

	static styles = [
		UUITextStyles,
		css`
			:host {
				display: flex;
				flex-direction: column;
				box-sizing: border-box;
				gap: var(--uui-size-space-5);
				height: 100%;
			}
		`,
	];
}

export default UmbDashboardCollectionElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-dashboard-collection': UmbDashboardCollectionElement;
	}
}
