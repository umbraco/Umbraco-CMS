import { customElement, property } from '@umbraco-cms/backoffice/external/lit';
import type { ManifestTreeItem } from '@umbraco-cms/backoffice/extension-registry';
import { UmbExtensionInitializerElementBase, umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';

@customElement('umb-tree-item')
export class UmbTreeItemElement extends UmbExtensionInitializerElementBase<ManifestTreeItem> {
	_entityType?: string;
	@property({ type: String, reflect: true })
	get entityType() {
		return this._entityType;
	}
	set entityType(newVal) {
		this._entityType = newVal;
		this.#observeManifest();
	}

	#observeManifest() {
		if (!this._entityType) return;

		const filterByEntityType = (manifest: ManifestTreeItem) => {
			if (!this._entityType) return false;
			return manifest.meta.entityTypes.includes(this._entityType);
		};

		this.observe(
			umbExtensionsRegistry.byTypeAndFilter(this.getExtensionType(), filterByEntityType),
			(manifests) => {
				if (!manifests) return;
				// TODO: what should we do if there are multiple tree items for an entity type?
				const manifest = manifests[0];
				this.createApi(manifest);
				this.createElement(manifest);
			},
			'umbObserveTreeManifest',
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
