import { html } from 'lit';
import { customElement } from 'lit/decorators.js';
import { UmbEntityStore } from '../../../core/stores/entity.store';
import { UmbTreeBase } from '../shared/tree-base.element';
import { UmbTreeDocumentDataContext } from './tree-documents-data.context';
import { UmbContextConsumerMixin, UmbContextProviderMixin } from '@umbraco-cms/context-api';
import type { ManifestTreeItemAction, ManifestWithLoader } from '@umbraco-cms/models';

import '../shared/tree-navigator.element';
import { umbExtensionsRegistry } from '@umbraco-cms/extensions-registry';
@customElement('umb-tree-document')
export class UmbTreeDocumentElement extends UmbContextProviderMixin(UmbContextConsumerMixin(UmbTreeBase)) {
	constructor() {
		super();

		this._registerTreeItemActions();

		this.consumeContext('umbEntityStore', (entityStore: UmbEntityStore) => {
			this._entityStore = entityStore;
			if (!this._entityStore || !this.tree) return;

			this._treeDataContext = new UmbTreeDocumentDataContext(this._entityStore);
			this.provideContext('umbTreeDataContext', this._treeDataContext);
		});
	}

	private _registerTreeItemActions() {
		const dashboards: Array<ManifestWithLoader<ManifestTreeItemAction>> = [
			{
				type: 'treeItemAction',
				alias: 'Umb.TreeItemAction.Document.Create',
				name: 'Document Tree Item Action Create',
				loader: () => import('./actions/action-document-create.element'),
				weight: 100,
				meta: {
					trees: ['Umb.Tree.Documents'],
					label: 'Create',
					icon: 'add',
				},
			},
			{
				type: 'treeItemAction',
				alias: 'Umb.TreeItemAction.Document.Delete',
				name: 'Document Tree Item Action Delete',
				loader: () => import('./actions/action-document-delete.element'),
				weight: 100,
				meta: {
					trees: ['Umb.Tree.Documents'],
					label: 'Delete',
					icon: 'delete',
				},
			},
			{
				type: 'treeItemAction',
				alias: 'Umb.TreeItemAction.Document.Paged',
				name: 'Document Tree Item Action Paged',
				loader: () => import('./actions/action-document-paged.element'),
				weight: 100,
				meta: {
					trees: ['Umb.Tree.Documents'],
					label: 'Paged',
					icon: 'favorite',
				},
			},
		];

		dashboards.forEach((dashboard) => {
			if (umbExtensionsRegistry.isRegistered(dashboard.alias)) return;
			umbExtensionsRegistry.register(dashboard);
		});
	}

	render() {
		return html`<umb-tree-navigator></umb-tree-navigator>`;
	}
}

export default UmbTreeDocumentElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-tree-document': UmbTreeDocumentElement;
	}
}
