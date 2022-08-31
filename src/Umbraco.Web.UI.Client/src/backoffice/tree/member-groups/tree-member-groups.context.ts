import { map } from 'rxjs';
import { UmbEntityStore } from '../../../core/stores/entity.store';
import { UmbTreeContext } from '../tree.context';
import type { ManifestTree } from '../../../core/models';

export class UmbTreeMemberGroupsContext implements UmbTreeContext {
	public tree: ManifestTree;
	public entityStore: UmbEntityStore;

	private _entityType = 'memberGroup';

	constructor(tree: ManifestTree, entityStore: UmbEntityStore) {
		this.tree = tree;
		this.entityStore = entityStore;
	}

	public fetchRoot() {
		const data = {
			id: -1,
			key: 'd46d144e-33d8-41e3-bf7a-545287e16e3c',
			name: 'Member Groups',
			hasChildren: true,
			type: 'memberGroup',
			icon: 'favorite',
			parentKey: '',
		};

		this.entityStore.update([data]);
		return this.entityStore.entities.pipe(
			map((items) => items.filter((item) => item.type === this._entityType && item.parentKey === ''))
		);
	}

	public fetchChildren(key: string) {
		// TODO: figure out url structure
		fetch(`/umbraco/backoffice/entities?type=${this._entityType}&parentKey=${key}`)
			.then((res) => res.json())
			.then((data) => {
				this.entityStore.update(data);
			});

		return this.entityStore.entities.pipe(map((items) => items.filter((item) => item.parentKey === key)));
	}
}
