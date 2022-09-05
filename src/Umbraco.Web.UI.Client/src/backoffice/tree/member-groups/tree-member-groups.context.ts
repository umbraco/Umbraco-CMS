import { map } from 'rxjs';
import { UmbTreeContextBase } from '../tree.context';

export class UmbTreeMemberGroupsContext extends UmbTreeContextBase {
	private _rootKey = '575645a5-0f25-4671-b9a0-be515096ad6b';

	public fetchRoot() {
		const data = {
			key: this._rootKey,
			name: 'Member Groups',
			hasChildren: true,
			type: 'memberGroupRoot',
			icon: 'favorite',
			parentKey: '',
		};

		this.entityStore.update([data]);
		return this.entityStore.entities.pipe(map((items) => items.filter((item) => item.key === this._rootKey)));
	}

	public fetchChildren(key: string) {
		// TODO: figure out url structure
		fetch(`/umbraco/backoffice/entities/member-groups?parentKey=${key}`)
			.then((res) => res.json())
			.then((data) => {
				this.entityStore.update(data);
			});

		return this.entityStore.entities.pipe(map((items) => items.filter((item) => item.parentKey === key)));
	}
}
