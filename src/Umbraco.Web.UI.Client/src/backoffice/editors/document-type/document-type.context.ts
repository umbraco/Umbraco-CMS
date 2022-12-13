import { BehaviorSubject, Observable } from 'rxjs';
import type { DocumentTypeDetails } from '@umbraco-cms/models';

export class UmbDocumentTypeContext {
	// TODO: figure out how fine grained we want to make our observables.
	private _data = new BehaviorSubject<DocumentTypeDetails>({
		key: '',
		name: '',
		icon: '',
		type: '',
		hasChildren: false,
		parentKey: '',
		alias: '',
		properties: [],
	});
	public readonly data: Observable<DocumentTypeDetails> = this._data.asObservable();

	constructor(documentType?: DocumentTypeDetails) {
		if (!documentType) return;
		this._data.next(documentType);
	}

	// TODO: figure out how we want to update data
	public update(data: Partial<DocumentTypeDetails>) {
		this._data.next({ ...this._data.getValue(), ...data });
	}

	public getData() {
		return this._data.getValue();
	}
}
