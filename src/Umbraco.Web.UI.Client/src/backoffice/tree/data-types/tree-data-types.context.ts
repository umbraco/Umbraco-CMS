import { UmbExtensionManifestTree } from '../../../core/extension';
import { UmbEntityStore } from '../../../core/stores/entity.store';
import { ITreeService } from '../tree.service';

export class UmbTreeDataTypesContext implements ITreeService {
	public tree: UmbExtensionManifestTree;
	public entityStore: UmbEntityStore;

	constructor(tree: UmbExtensionManifestTree, entityStore: UmbEntityStore) {
		this.tree = tree;
		// TODO: temp solution until we know where to get tree data from
		this.entityStore = entityStore;
	}

	public async getRoot() {
		const data = {
			id: -1,
			key: '3fd3eba5-c893-4d3c-af67-f574e6eded38',
			name: 'Data Types',
			hasChildren: true,
			type: 'data-type',
			icon: 'favorite',
		};
		this.entityStore.update([data]);
		return data;
	}

	public async getChildren(id: number) {
		// TODO: figure out url structure
		const res = await fetch(`/umbraco/backoffice/trees/data-types/${id}`);
		const json = await res.json();
		this.entityStore.update(json);
		return json;
	}
}
