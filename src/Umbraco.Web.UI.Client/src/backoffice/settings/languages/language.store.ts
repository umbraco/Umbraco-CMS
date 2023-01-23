import { map, Observable } from 'rxjs';
import { UmbDataStoreBase } from '../../../core/stores/store';
import type { LanguageDetails } from '@umbraco-cms/models';
import { LanguageResource } from '@umbraco-cms/backend-api';
import { tryExecuteAndNotify } from '@umbraco-cms/resources';
import { UmbContextToken } from '@umbraco-cms/context-api';

export type UmbLanguageStoreItemType = LanguageDetails;

// TODO: research how we write names of global consts.
export const STORE_ALIAS = 'umbLanguageStore';

/**
 * @export
 * @class UmbLanguageStore
 * @extends {UmbDataStoreBase<UmbLanguageStoreItemType>}
 * @description - Data Store for languages
 */
export class UmbLanguageStore extends UmbDataStoreBase<UmbLanguageStoreItemType> {
	public readonly storeAlias = STORE_ALIAS;

	/**
	 * @description - Request a Data Type by key. The Data Type is added to the store and is returned as an Observable.
	 * @param {string} key
	 * @return {*}  {(Observable<DataTypeDetails | null>)}
	 * @memberof UmbDataTypesStore
	 */
	getByKey(key: string): Observable<LanguageDetails | null> {
		// TODO: use backend cli when available.
		fetch(`/umbraco/management/api/v1/language/${key}`)
			.then((res) => res.json())
			.then((data) => {
				this.updateItems([data]);
			});

		return this.items.pipe(map((languages) => languages.find((language) => language.key === key) || null));
	}

	getAll(): Observable<Array<LanguageDetails>> {
		tryExecuteAndNotify(this.host, LanguageResource.getLanguage({ skip: 0, take: 100 })).then(({ data }) => {
			if (data) {
				this.updateItems(data.items as Array<LanguageDetails>);
			}
		});

		return this.items;
	}

	async save(language: LanguageDetails): Promise<void> {
		if (language.id && language.key) {
			tryExecuteAndNotify(this.host, LanguageResource.putLanguageById({ id: language.id, requestBody: language })).then(
				(response) => {
					if (response.data) {
						this.updateItems([response.data]);
					}
				}
			);
		} else {
			tryExecuteAndNotify(this.host, LanguageResource.postLanguage({ requestBody: language })).then((response) => {
				if (response.data) {
					this.updateItems([response.data]);
				}
			});
		}
	}
}

export const UMB_LANGUAGE_STORE_CONTEXT_TOKEN = new UmbContextToken<UmbLanguageStore>(STORE_ALIAS);
