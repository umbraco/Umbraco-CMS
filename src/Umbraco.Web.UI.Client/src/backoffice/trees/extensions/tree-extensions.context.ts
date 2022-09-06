import { UmbTreeContextBase } from '../tree.context';

export class UmbTreeExtensionsContext extends UmbTreeContextBase {
	public rootKey = 'fd32ea8b-893b-4ee9-b1d0-72f41c4a6d38';

	public rootChanges() {
		const data = {
			key: this.rootKey,
			name: 'Extensions',
			hasChildren: false,
			type: 'extensionsList',
			icon: 'favorite',
			parentKey: '',
			isTrashed: false,
		};
		this.entityStore.update([data]);
		return super.rootChanges();
	}
}
