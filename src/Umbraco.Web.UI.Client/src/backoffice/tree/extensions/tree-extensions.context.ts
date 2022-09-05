import { map } from 'rxjs';
import { UmbTreeContextBase } from '../tree.context';

export class UmbTreeExtensionsContext extends UmbTreeContextBase {
	private _rootKey = 'fd32ea8b-893b-4ee9-b1d0-72f41c4a6d38';

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
