import { map } from 'rxjs';
import { UmbTreeContextBase } from '../tree.context';

export class UmbTreeDocumentTypesContext extends UmbTreeContextBase {
	private _rootKey = 'f50eb86d-3ef2-4011-8c5d-c56c04eec0da';

	public fetchRoot() {
		const data = {
			key: this._rootKey,
			name: 'Document Types',
			hasChildren: true,
			type: 'documentTypeRoot',
			icon: 'folder',
			parentKey: '',
		};
		this.entityStore.update([data]);
		return this.entityStore.items.pipe(map((items) => items.filter((item) => item.key === this._rootKey)));
	}

	public fetchChildren(key: string) {
		// TODO: figure out url structure
		fetch(`/umbraco/backoffice/entities/document-types?parentKey=${key}`)
			.then((res) => res.json())
			.then((data) => {
				this.entityStore.update(data);
			});

		return this.entityStore.items.pipe(map((items) => items.filter((item) => item.parentKey === key)));
	}
}
