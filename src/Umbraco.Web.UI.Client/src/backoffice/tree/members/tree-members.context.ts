import { map } from 'rxjs';
import { UmbExtensionManifestTree } from '../../../core/extension';
import { UmbEntityStore } from '../../../core/stores/entity.store';
import { UmbTreeContext } from '../tree.context';

export class UmbTreeMembersContext implements UmbTreeContext {
	public tree: UmbExtensionManifestTree;
	public entityStore: UmbEntityStore;

	constructor(tree: UmbExtensionManifestTree, entityStore: UmbEntityStore) {
		this.tree = tree;
		this.entityStore = entityStore;
	}

	public fetchRoot() {
		const data = {
			id: -1,
			key: '24fcd88a-d1bb-423b-b794-8a94dcddcb6a',
			parentKey: '',
			name: 'Members',
			hasChildren: true,
			type: 'member',
			icon: 'favorite',
		};
		this.entityStore.update([data]);
		return this.entityStore.entities.pipe(map((items) => items.filter((item) => item.key === data.key)));
	}

	public fetchChildren(key: string) {
		// TODO: figure out url structure
		fetch(`/umbraco/backoffice/trees/members/${key}`)
			.then((res) => res.json())
			.then((data) => {
				this.entityStore.update(data);
			});

		return this.entityStore.entities.pipe(map((items) => items.filter((item) => item.parentKey === key)));
	}
}
