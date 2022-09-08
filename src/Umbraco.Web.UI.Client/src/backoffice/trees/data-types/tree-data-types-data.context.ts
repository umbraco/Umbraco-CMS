import { UmbTreeDataContextBase } from '../tree-data.context';

export class UmbTreeDataTypesDataContext extends UmbTreeDataContextBase {
	public rootKey = '29d78e6c-c1bf-4c15-b820-d511c237ffae';

	public rootChanges() {
		const data = {
			key: this.rootKey,
			name: 'Data Types',
			hasChildren: true,
			type: 'dataTypeRoot',
			icon: 'folder',
			parentKey: '',
			isTrashed: false,
		};
		this.entityStore.update([data]);
		return super.rootChanges();
	}

	public childrenChanges(key = '') {
		// TODO: figure out url structure
		fetch(`/umbraco/backoffice/entities/data-types?parentKey=${key}`)
			.then((res) => res.json())
			.then((data) => {
				this.entityStore.update(data);
			});

		return super.childrenChanges(key);
	}
}
