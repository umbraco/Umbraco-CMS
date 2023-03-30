import { css, html } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement } from 'lit/decorators.js';
import { ifDefined } from 'lit/directives/if-defined.js';
import {
	UmbCollectionContext,
	UMB_COLLECTION_CONTEXT_TOKEN,
} from '../../../../../../shared/collection/collection.context';

import '../../../../../../shared/collection/dashboards/dashboard-collection.element';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import type { FolderTreeItemResponseModel } from '@umbraco-cms/backoffice/backend-api';
import type { ManifestWorkspaceViewCollection } from '@umbraco-cms/backoffice/extensions-registry';
import { UMB_ENTITY_WORKSPACE_CONTEXT } from '@umbraco-cms/backoffice/context-api';

@customElement('umb-workspace-view-collection')
export class UmbWorkspaceViewCollectionElement extends UmbLitElement {
	static styles = [
		UUITextStyles,
		css`
			:host {
				display: block;
				height: 100%;
			}
		`,
	];

	public manifest!: ManifestWorkspaceViewCollection;

	private _workspaceContext?: typeof UMB_ENTITY_WORKSPACE_CONTEXT.TYPE;

	// TODO: add type for the collection context.
	private _collectionContext?: UmbCollectionContext<FolderTreeItemResponseModel>;

	constructor() {
		super();

		this.consumeContext(UMB_ENTITY_WORKSPACE_CONTEXT, (nodeContext) => {
			this._workspaceContext = nodeContext;
			this._provideWorkspace();
		});
	}

	protected _provideWorkspace() {
		const entityKey = this._workspaceContext?.getEntityKey();
		const entityType = this._workspaceContext?.getEntityType();

		if (entityKey != null && entityType != null) {
			const manifestMeta = this.manifest.meta;

			this._collectionContext = new UmbCollectionContext(
				this,
				entityType,
				entityKey,
				manifestMeta.storeAlias,
				manifestMeta.repositoryAlias
			);
			this.provideContext(UMB_COLLECTION_CONTEXT_TOKEN, this._collectionContext);
		}
	}

	render() {
		return html`<umb-collection entity-type=${ifDefined(this._workspaceContext?.getEntityType())}></umb-collection>`;
	}
}

export default UmbWorkspaceViewCollectionElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-workspace-view-collection': UmbWorkspaceViewCollectionElement;
	}
}
