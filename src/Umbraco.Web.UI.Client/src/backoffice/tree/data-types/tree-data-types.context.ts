import { map } from 'rxjs';
import { UmbExtensionManifestTree } from '../../../core/extension';
import { UmbEntityStore } from '../../../core/stores/entity.store';
import { UmbTreeContext } from '../tree.context';

export class UmbTreeDataTypesContext implements UmbTreeContext {
	public tree: UmbExtensionManifestTree;
	public entityStore: UmbEntityStore;

	constructor(tree: UmbExtensionManifestTree, entityStore: UmbEntityStore) {
		this.tree = tree;
		// TODO: temp solution until we know where to get tree data from
		this.entityStore = entityStore;
	}

	public fetchRoot() {
		const data = {
			id: -1,
			key: '3fd3eba5-c893-4d3c-af67-f574e6eded38',
			name: 'Data Types',
			hasChildren: true,
			type: 'data-type',
			icon: 'favorite',
			parentKey: '',
		};
		this.entityStore.update([data]);
		return this.entityStore.entities.pipe(map((items) => items.filter((item) => item.key === data.key)));
	}

	public fetchChildren(key: string) {
		// TODO: figure out url structure
		fetch(`/umbraco/backoffice/trees/data-types/${key}`)
			.then((res) => res.json())
			.then((data) => {
				this.entityStore.update(data);
			});

		return this.entityStore.entities.pipe(map((items) => items.filter((item) => item.key === key)));
	}
}
