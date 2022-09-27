import { map, Observable } from 'rxjs';
import type { PropertyEditor } from '../../models';
import { getPropertyEditorsList, getPropertyEditor } from '../../api/fetcher';
import { UmbDataStoreBase } from '../store';

export class UmbPropertyEditorStore extends UmbDataStoreBase<PropertyEditor> {
	getAll(): Observable<Array<PropertyEditor> | []> {
		// TODO: only fetch if the data type is not in the store?
		getPropertyEditorsList({})
			.then((res) => {
				this.update(res.data.propertyEditors, 'alias');
			})
			.catch((err) => {
				console.log(err);
			});

		return this.items;
	}

	getByAlias(alias: string): Observable<PropertyEditor | undefined> {
		// TODO: only fetch if the data type is not in the store?
		getPropertyEditor({ propertyEditorAlias: alias })
			.then((res) => {
				this.update([res.data], 'alias');
			})
			.catch((err) => {
				console.log(err);
			});

		return this.items.pipe(map((items) => items.find((item) => item.alias === alias)));
	}
}
