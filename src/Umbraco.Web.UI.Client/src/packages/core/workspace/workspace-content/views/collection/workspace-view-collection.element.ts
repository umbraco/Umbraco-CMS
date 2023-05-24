import { css, html } from '@umbraco-cms/backoffice/external/lit';
import { UUITextStyles } from '@umbraco-cms/backoffice/external/uui';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { ifDefined } from '@umbraco-cms/backoffice/external/lit';
import { UmbCollectionContext, UMB_COLLECTION_CONTEXT_TOKEN } from '@umbraco-cms/backoffice/collection';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import type { FolderTreeItemResponseModel } from '@umbraco-cms/backoffice/backend-api';
import type { ManifestWorkspaceViewCollection } from '@umbraco-cms/backoffice/extension-registry';
import { UMB_ENTITY_WORKSPACE_CONTEXT } from '@umbraco-cms/backoffice/workspace';

import '../../../../collection/dashboards/dashboard-collection.element.js';

@customElement('umb-workspace-view-collection')
export class UmbWorkspaceViewCollectionElement extends UmbLitElement {
	public manifest!: ManifestWorkspaceViewCollection;

	private _workspaceContext?: typeof UMB_ENTITY_WORKSPACE_CONTEXT.TYPE;

	// TODO: add type for the collection context.
	private _collectionContext?: UmbCollectionContext<FolderTreeItemResponseModel, any>;

	constructor() {
		super();

		this.consumeContext(UMB_ENTITY_WORKSPACE_CONTEXT, (nodeContext) => {
			this._workspaceContext = nodeContext;
			this._provideWorkspace();
		});
	}

	protected _provideWorkspace() {
		const entityId = this._workspaceContext?.getEntityId();
		const entityType = this._workspaceContext?.getEntityType();

		if (entityId != null && entityType != null) {
			const manifestMeta = this.manifest.meta;

			this._collectionContext = new UmbCollectionContext(this, entityType, manifestMeta.repositoryAlias);
			this.provideContext(UMB_COLLECTION_CONTEXT_TOKEN, this._collectionContext);
		}
	}

	render() {
		return html`<umb-collection entity-type=${ifDefined(this._workspaceContext?.getEntityType())}></umb-collection>`;
	}

	static styles = [
		UUITextStyles,
		css`
			:host {
				display: block;
				height: 100%;
			}
		`,
	];
}

export default UmbWorkspaceViewCollectionElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-workspace-view-collection': UmbWorkspaceViewCollectionElement;
	}
}
