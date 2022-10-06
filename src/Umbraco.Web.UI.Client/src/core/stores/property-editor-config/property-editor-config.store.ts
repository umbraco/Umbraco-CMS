import { map, Observable } from 'rxjs';
import { getPropertyEditorConfig } from '../../api/fetcher';
import type { PropertyEditorConfig } from '../../models';
import { UmbDataStoreBase } from '../store';

export interface PropertyEditorConfigRef {
	propertyEditorAlias: string;
	config: PropertyEditorConfig;
}

/**
 * @export
 * @class UmbPropertyEditorConfigStore
 * @extends {UmbDataStoreBase<PropertyEditorConfigRef>}
 * @description - Data Store for Property Editor Configs
 */
export class UmbPropertyEditorConfigStore extends UmbDataStoreBase<PropertyEditorConfigRef> {
	/**
	 *
	 * @description - Request a Property Editor Config by alias. The Property Editor Config is added to the store and is returned as an Observable.
	 * @param {string} alias
	 * @return {*}  {(Observable<PropertyEditorConfigRef | undefined>)}
	 * @memberof UmbPropertyEditorConfigStore
	 */
	getByAlias(alias: string): Observable<PropertyEditorConfigRef | undefined> {
		// TODO: only fetch if the data type is not in the store?
		getPropertyEditorConfig({ propertyEditorAlias: alias })
			.then((res) => {
				const propertyEditorConfigRef: PropertyEditorConfigRef = { propertyEditorAlias: alias, config: res.data };
				this.update([propertyEditorConfigRef], 'propertyEditorAlias');
			})
			.catch((err) => {
				console.log(err);
			});

		return this.items.pipe(map((items) => items.find((item) => item.propertyEditorAlias === alias)));
	}
}
