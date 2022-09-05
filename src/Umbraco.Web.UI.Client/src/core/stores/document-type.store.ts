import { map, Observable } from 'rxjs';
import { DocumentTypeEntity } from '../../mocks/data/document-type.data';
import { UmbEntityStore } from './entity.store';
import { UmbDataStoreBase } from './store';

export class UmbDocumentTypeStore extends UmbDataStoreBase<DocumentTypeEntity> {
	private _entityStore: UmbEntityStore;

	constructor(entityStore: UmbEntityStore) {
		super();
		this._entityStore = entityStore;
	}

	getByKey(key: string): Observable<DocumentTypeEntity | null> {
		// TODO: use Fetcher API.
		// TODO: only fetch if the data type is not in the store?
		fetch(`/umbraco/backoffice/document-type/${key}`)
			.then((res) => res.json())
			.then((data) => {
				this.update(data);
			});

		return this.items.pipe(
			map(
				(documentTypes: Array<DocumentTypeEntity>) =>
					documentTypes.find((documentType: DocumentTypeEntity) => documentType.key === key) || null
			)
		);
	}

	async save(documentTypes: Array<DocumentTypeEntity>) {
		// TODO: use Fetcher API.
		try {
			const res = await fetch('/umbraco/backoffice/document-type/save', {
				method: 'POST',
				body: JSON.stringify(documentTypes),
				headers: {
					'Content-Type': 'application/json',
				},
			});
			const json = await res.json();
			this.update(json);
			this._entityStore.update(json);
		} catch (error) {
			console.error('Save Document Type error', error);
		}
	}
}
