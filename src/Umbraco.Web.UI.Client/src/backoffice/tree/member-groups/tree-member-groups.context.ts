import { UmbTreeContextBase } from '../tree.context';

export class UmbTreeMemberGroupsContext extends UmbTreeContextBase {
	public rootKey = '575645a5-0f25-4671-b9a0-be515096ad6b';

	public rootChanges() {
		const data = {
			key: this.rootKey,
			name: 'Member Groups',
			hasChildren: true,
			type: 'memberGroupRoot',
			icon: 'folder',
			parentKey: '',
			isTrashed: false,
		};

		this.entityStore.update([data]);
		return super.rootChanges();
	}

	public childrenChanges(key: string) {
		// TODO: figure out url structure
		fetch(`/umbraco/backoffice/entities/member-groups?parentKey=${key}`)
			.then((res) => res.json())
			.then((data) => {
				this.entityStore.update(data);
			});
		return super.childrenChanges(key);
	}
}
