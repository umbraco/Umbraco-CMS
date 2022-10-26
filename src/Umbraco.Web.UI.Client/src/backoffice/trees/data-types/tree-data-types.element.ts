import { html } from 'lit';
import { customElement } from 'lit/decorators.js';
import { UmbEntityStore } from '../../../core/stores/entity.store';
import { UmbTreeBase } from '../shared/tree-base.element';
import { UmbTreeDataTypesDataContext } from './tree-data-types-data.context';
import { UmbContextConsumerMixin, UmbContextProviderMixin } from '@umbraco-cms/context-api';
import { umbExtensionsRegistry } from '@umbraco-cms/extensions-registry';
import type { ManifestTreeItemAction, ManifestWithLoader } from '@umbraco-cms/models';

import '../shared/tree-navigator.element';

@customElement('umb-tree-data-types')
export class UmbTreeDataTypesElement extends UmbContextProviderMixin(UmbContextConsumerMixin(UmbTreeBase)) {
	constructor() {
		super();

		this._registerTreeItemActions();

		this.consumeContext('umbEntityStore', (entityStore: UmbEntityStore) => {
			this._entityStore = entityStore;
			if (!this._entityStore) return;

			this._treeDataContext = new UmbTreeDataTypesDataContext(this._entityStore);
			this.provideContext('umbTreeDataContext', this._treeDataContext);
		});
	}

	private _registerTreeItemActions() {
		const dashboards: Array<ManifestWithLoader<ManifestTreeItemAction>> = [
			{
				type: 'treeItemAction',
				alias: 'Umb.TreeItemAction.DataType.Create',
				name: 'Tree Item Action Create',
				loader: () => import('./actions/create/action-data-type-create.element'),
				weight: 200,
				meta: {
					trees: ['Umb.Tree.DataTypes'],
					label: 'Create',
					icon: 'umb:add',
				},
			},
			{
				type: 'treeItemAction',
				alias: 'Umb.TreeItemAction.DataType.Delete',
				name: 'Tree Item Action Delete',
				loader: () => import('./actions/delete/action-data-type-delete.element'),
				weight: 100,
				meta: {
					trees: ['Umb.Tree.DataTypes'],
					label: 'Delete',
					icon: 'umb:delete',
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

export default UmbTreeDataTypesElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-tree-data-types': UmbTreeDataTypesElement;
	}
}
