import { css, html } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement } from 'lit/decorators.js';
import { ifDefined } from 'lit-html/directives/if-defined.js';
import {
	UmbCollectionContext,
	UMB_COLLECTION_CONTEXT_TOKEN,
} from '../../../../../../shared/collection/collection.context';

import '../../../../../../shared/collection/dashboards/dashboard-collection.element';
import type { UmbEntityWorkspaceContextInterface } from '../../../workspace-context/workspace-entity-context.interface';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import type { FolderTreeItemResponseModel } from '@umbraco-cms/backoffice/backend-api';
import type { ManifestWorkspaceViewCollection } from '@umbraco-cms/backoffice/extensions-registry';

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

	private _workspaceContext?: UmbEntityWorkspaceContextInterface;

	// TODO: add type for the collection context.
	private _collectionContext?: UmbCollectionContext<FolderTreeItemResponseModel, any>;

	constructor() {
		super();

		// TODO: Figure out how to get the magic string for the workspace context.
		this.consumeContext<UmbEntityWorkspaceContextInterface>('umbWorkspaceContext', (nodeContext) => {
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
