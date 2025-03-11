import type { ManifestTreeItem } from '../extensions/types.js';
import { customElement, property } from '@umbraco-cms/backoffice/external/lit';
import {
	UmbExtensionElementAndApiSlotElementBase,
	umbExtensionsRegistry,
} from '@umbraco-cms/backoffice/extension-registry';
import { createObservablePart } from '@umbraco-cms/backoffice/observable-api';

@customElement('umb-tree-item')
export class UmbTreeItemElement extends UmbExtensionElementAndApiSlotElementBase<ManifestTreeItem> {
	_entityType?: string;
	@property({ type: String, reflect: true })
	get entityType() {
		return this._entityType;
	}
	set entityType(newVal) {
		this._entityType = newVal;
		this.#observeEntityType();
	}

	#observeEntityType() {
		if (!this._entityType) return;

		const filterByEntityType = (manifest: ManifestTreeItem) => {
			if (!this._entityType) return false;
			return manifest.forEntityTypes.includes(this._entityType);
		};

		this.observe(
			// TODO: what should we do if there are multiple tree items for an entity type?
			// This method gets all extensions based on a type, then filters them based on the entity type. and then we get the alias of the first one [NL]
			createObservablePart(
				umbExtensionsRegistry.byTypeAndFilter(this.getExtensionType(), filterByEntityType),
				(x) => x[0].alias,
			),
			(alias) => {
				this.alias = alias;
			},
			'umbObserveAlias',
		);
	}

	getExtensionType() {
		return 'treeItem';
	}

	getDefaultElementName() {
		return 'umb-default-tree-item';
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-tree-item': UmbTreeItemElement;
	}
}
