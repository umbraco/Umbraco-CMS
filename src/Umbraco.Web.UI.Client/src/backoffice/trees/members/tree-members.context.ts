import { UmbTreeContextBase } from '../tree.context';

export class UmbTreeMembersContext extends UmbTreeContextBase {
	public rootKey = '8f974b62-392b-4ddd-908c-03c2e03ab1a6';

	public rootChanges() {
		const data = {
			key: this.rootKey,
			parentKey: '',
			name: 'Members',
			hasChildren: true,
			type: 'member',
			icon: 'folder',
			isTrashed: false,
		};
		this.entityStore.update([data]);
		return super.rootChanges();
	}

	public childrenChanges(key: string) {
		// TODO: figure out url structure
		fetch(`/umbraco/backoffice/entities/members?parentKey=${key}`)
			.then((res) => res.json())
			.then((data) => {
				this.entityStore.update(data);
			});
		return super.childrenChanges(key);
	}
}
