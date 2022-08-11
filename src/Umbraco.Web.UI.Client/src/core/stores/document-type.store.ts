import { BehaviorSubject, map, Observable } from 'rxjs';
import { DocumentTypeEntity, umbDocumentTypeData } from '../../mocks/data/document-type.data';

export class UmbDocumentTypeStore {
	private _documentTypes: BehaviorSubject<Array<DocumentTypeEntity>> = new BehaviorSubject(
		<Array<DocumentTypeEntity>>[]
	);
	public readonly documentTypes: Observable<Array<DocumentTypeEntity>> = this._documentTypes.asObservable();

	getById(id: number): Observable<DocumentTypeEntity | null> {
		// TODO: use Fetcher API.
		// TODO: only fetch if the data type is not in the store?
		fetch(`/umbraco/backoffice/document-type/${id}`)
			.then((res) => res.json())
			.then((data) => {
				this._updateStore(data);
			});

		return this.documentTypes.pipe(
			map(
				(documentTypes: Array<DocumentTypeEntity>) =>
					documentTypes.find((node: DocumentTypeEntity) => node.id === id) || null
			)
		);
	}

	// TODO: temp solution until we know where to get tree data from
	getAll(): Observable<Array<DocumentTypeEntity>> {
		const documentTypes = umbDocumentTypeData.getAll();
		this._documentTypes.next(documentTypes);
		return this.documentTypes;
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
			this._updateStore(json);
		} catch (error) {
			console.error('Save Document Type error', error);
		}
	}

	private _updateStore(fetchedDocumentTypes: Array<DocumentTypeEntity>) {
		const storedDocumentTypes = this._documentTypes.getValue();
		const updated: DocumentTypeEntity[] = [...storedDocumentTypes];

		fetchedDocumentTypes.forEach((fetchedDocumentType) => {
			const index = storedDocumentTypes.map((storedNode) => storedNode.id).indexOf(fetchedDocumentType.id);

			if (index !== -1) {
				// If the data type is already in the store, update it
				updated[index] = fetchedDocumentType;
			} else {
				// If the data type is not in the store, add it
				updated.push(fetchedDocumentType);
			}
		});

		this._documentTypes.next([...updated]);
	}
}
