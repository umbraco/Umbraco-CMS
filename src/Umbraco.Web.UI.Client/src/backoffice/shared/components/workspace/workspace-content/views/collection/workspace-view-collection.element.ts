import { css, html } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement } from 'lit/decorators.js';
import { ifDefined } from 'lit-html/directives/if-defined.js';
import { UmbMediaTreeStore } from '../../../../../../media/media/media.tree.store';
import {
	UmbCollectionContext,
	UMB_COLLECTION_CONTEXT_TOKEN,
} from '../../../../../../shared/collection/collection.context';

import '../../../../../../shared/components/content-property/content-property.element';
import '../../../../../../shared/collection/dashboards/dashboard-collection.element';
import { UmbWorkspaceEntityContextInterface } from '../../../workspace-context/workspace-entity-context.interface';
import { UmbLitElement } from '@umbraco-cms/element';
import { FolderTreeItem } from '@umbraco-cms/backend-api';
import { ManifestWorkspaceViewCollection } from '@umbraco-cms/extensions-registry';

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

	private _workspaceContext?: UmbWorkspaceEntityContextInterface;

	private _collectionContext?: UmbCollectionContext<FolderTreeItem, UmbMediaTreeStore>;

	constructor() {
		super();

		// TODO: Figure out how to get the magic string for the workspace context.
		this.consumeContext<UmbWorkspaceEntityContextInterface>('umbWorkspaceContext', (nodeContext) => {
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
