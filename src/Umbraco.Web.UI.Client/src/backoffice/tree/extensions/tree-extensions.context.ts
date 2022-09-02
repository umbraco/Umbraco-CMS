import { map } from 'rxjs';
import { UmbEntityStore } from '../../../core/stores/entity.store';
import { UmbTreeContext } from '../tree.context';

export class UmbTreeExtensionsContext implements UmbTreeContext {
	public entityStore: UmbEntityStore;

	private _rootKey = 'fd32ea8b-893b-4ee9-b1d0-72f41c4a6d38';

	constructor(entityStore: UmbEntityStore) {
		this.entityStore = entityStore;
	}

	public fetchRoot() {
		const data = {
			key: this._rootKey,
			name: 'Extensions',
			hasChildren: false,
			type: 'extensionsList',
			icon: 'favorite',
			parentKey: '',
		};
		this.entityStore.update([data]);
		return this.entityStore.entities.pipe(map((items) => items.filter((item) => item.key === this._rootKey)));
	}
}
