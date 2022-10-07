import { map, Observable } from 'rxjs';
import type { PropertyEditor } from '../../models';
import { getPropertyEditorsList, getPropertyEditor } from '../../api/fetcher';
import { UmbDataStoreBase } from '../store';

/**
 * @export
 * @class UmbPropertyEditorStore
 * @extends {UmbDataStoreBase<PropertyEditor>}
 * @description - Data Store for Property Editors
 */
export class UmbPropertyEditorStore extends UmbDataStoreBase<PropertyEditor> {
	/**
	 * @description - Request all Property Editors. The Property Editors are added to the store and are returned as an Observable.
	 * @return {*}  {(Observable<Array<PropertyEditor> | []>)}
	 * @memberof UmbPropertyEditorStore
	 */
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

	/**
	 * Request a Property Editor by alias. The Property Editor is added to the store and is returned as an Observable.
	 * @param {string} alias
	 * @return {*}  {(Observable<PropertyEditor | undefined>)}
	 * @memberof UmbPropertyEditorStore
	 */
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
