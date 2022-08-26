import { map } from 'rxjs';
import { UmbExtensionManifestTree } from '../../../core/extension';
import { UmbEntityStore } from '../../../core/stores/entity.store';
import { UmbTreeContext } from '../tree.context';

export class UmbTreeMemberGroupsContext implements UmbTreeContext {
	public tree: UmbExtensionManifestTree;
	public entityStore: UmbEntityStore;

	constructor(tree: UmbExtensionManifestTree, entityStore: UmbEntityStore) {
		this.tree = tree;
		this.entityStore = entityStore;
	}

	public fetchRoot() {
		const data = {
			id: -1,
			key: 'd46d144e-33d8-41e3-bf7a-545287e16e3c',
			name: 'Member Groups',
			hasChildren: true,
			type: 'member-group',
			icon: 'favorite',
			parentKey: '',
		};

		this.entityStore.update([data]);
		return this.entityStore.entities.pipe(map((items) => items.filter((item) => item.key === data.key)));
	}

	public fetchChildren(key: string) {
		// TODO: figure out url structure
		fetch(`/umbraco/backoffice/trees/member-groups/${key}`)
			.then((res) => res.json())
			.then((data) => {
				this.entityStore.update(data);
			});

		return this.entityStore.entities.pipe(map((items) => items.filter((item) => item.key === key)));
	}
}
