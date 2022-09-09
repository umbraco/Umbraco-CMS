import { BehaviorSubject, Observable } from 'rxjs';
import { DocumentTypeEntity } from '../../../mocks/data/document-type.data';

export class UmbDocumentTypeContext {
	// TODO: figure out how fine grained we want to make our observables.
	private _data = new BehaviorSubject<DocumentTypeEntity>({
		key: '',
		name: '',
		icon: '',
		type: '',
		hasChildren: false,
		parentKey: '',
		isTrashed: false,
		alias: '',
		properties: [],
	});
	public readonly data: Observable<DocumentTypeEntity> = this._data.asObservable();

	constructor(documentType?: DocumentTypeEntity) {
		if (!documentType) return;
		this._data.next(documentType);
	}

	// TODO: figure out how we want to update data
	public update(data: Partial<DocumentTypeEntity>) {
		this._data.next({ ...this._data.getValue(), ...data });
	}

	public getData() {
		return this._data.getValue();
	}
}
