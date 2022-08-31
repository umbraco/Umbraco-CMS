import { map } from 'rxjs';
import { UmbEntityStore } from '../../../core/stores/entity.store';
import { UmbTreeContext } from '../tree.context';
import type { ManifestTree } from '../../../core/models';

export class UmbTreeExtensionsContext implements UmbTreeContext {
	public tree: ManifestTree;
	public entityStore: UmbEntityStore;

	private _entityType = 'extension';

	constructor(tree: ManifestTree, entityStore: UmbEntityStore) {
		this.tree = tree;
		this.entityStore = entityStore;
	}

	public fetchRoot() {
		const data = {
			id: -1,
			key: 'fd32ea8b-893b-4ee9-b1d0-72f41c4a6d38',
			name: 'Extensions',
			hasChildren: false,
			type: 'extensionsList',
			icon: 'favorite',
			parentKey: '',
		};
		this.entityStore.update([data]);

		return this.entityStore.entities.pipe(
			map((items) => items.filter((item) => item.type === 'extensionsList' && item.parentKey === ''))
		);
	}

	public fetchChildren(key: string) {
		fetch(`/umbraco/backoffice/entities?type=${this._entityType}&parentKey=${key}`)
			.then((res) => res.json())
			.then((data) => {
				this.entityStore.update(data);
			});

		return this.entityStore.entities.pipe(map((items) => items.filter((item) => item.parentKey === key)));
	}
}
