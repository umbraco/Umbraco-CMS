import { map } from 'rxjs';
import { UmbEntityStore } from '../../../core/stores/entity.store';
import { UmbTreeContext } from '../tree.context';
import type { ManifestTree } from '../../../core/models';

export class UmbTreeMembersContext implements UmbTreeContext {
	public tree: ManifestTree;
	public entityStore: UmbEntityStore;

	private _entityType = 'member';

	constructor(tree: ManifestTree, entityStore: UmbEntityStore) {
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
