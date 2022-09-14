import { UmbTreeDataContextBase } from '../tree-data.context';

export class UmbTreeDocumentTypesDataContext extends UmbTreeDataContextBase {
	public rootKey = 'f50eb86d-3ef2-4011-8c5d-c56c04eec0da';

	public rootChanges() {
		const data = {
			key: this.rootKey,
			name: 'Document Types',
			hasChildren: true,
			type: 'documentTypeRoot',
			icon: 'folder',
			parentKey: '',
			isTrashed: false,
		};
		this.entityStore.update([data]);
		return super.rootChanges();
	}

	public childrenChanges(key: string) {
		// TODO: figure out url structure
		fetch(`/umbraco/backoffice/entities/document-types?parentKey=${key}`)
			.then((res) => res.json())
			.then((data) => {
				this.entityStore.update(data);
			});

		return super.childrenChanges(key);
	}
}
