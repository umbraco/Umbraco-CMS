import { map } from 'rxjs';
import { UmbTreeContextBase } from '../tree.context';

export class UmbTreeMembersContext extends UmbTreeContextBase {
	private _rootKey = '8f974b62-392b-4ddd-908c-03c2e03ab1a6';

	public fetchRoot() {
		const data = {
			key: this._rootKey,
			parentKey: '',
			name: 'Members',
			hasChildren: true,
			type: 'member',
			icon: 'favorite',
			isTrashed: false,
		};
		this.entityStore.update([data]);
		return this.entityStore.items.pipe(map((items) => items.filter((item) => item.key === this._rootKey)));
	}

	public fetchChildren(key: string) {
		// TODO: figure out url structure
		fetch(`/umbraco/backoffice/entities/members?parentKey=${key}`)
			.then((res) => res.json())
			.then((data) => {
				this.entityStore.update(data);
			});

		return this.entityStore.items.pipe(map((items) => items.filter((item) => item.parentKey === key)));
	}
}
