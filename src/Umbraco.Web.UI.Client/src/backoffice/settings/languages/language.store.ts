import { Observable } from 'rxjs';
import { LanguageResource } from '@umbraco-cms/backend-api';
import { tryExecuteAndNotify } from '@umbraco-cms/resources';
import { UmbContextToken } from '@umbraco-cms/context-api';
import type { LanguageDetails } from '@umbraco-cms/models';
import { UmbStoreBase } from '@umbraco-cms/store';
import { ArrayState, createObservablePart } from '@umbraco-cms/observable-api';
import { UmbControllerHostInterface } from '@umbraco-cms/controller';

export type UmbLanguageStoreItemType = LanguageDetails;
export const UMB_LANGUAGE_STORE_CONTEXT_TOKEN = new UmbContextToken<UmbLanguageStore>('umbLanguageStore');

/**
 * @export
 * @class UmbLanguageStore
 * @extends {UmbStoreBase}
 * @description - Data Store for languages
 */
export class UmbLanguageStore extends UmbStoreBase {
	#data = new ArrayState<LanguageDetails>([], (x) => x.key);

	constructor(host: UmbControllerHostInterface) {
		super(host, UMB_LANGUAGE_STORE_CONTEXT_TOKEN.toString());
	}

	getByKey(key: string) {
		// TODO: use backend cli when available.
		fetch(`/umbraco/management/api/v1/language/${key}`)
			.then((res) => res.json())
			.then((data) => {
				this.#data.appendOne(data);
			});

		return createObservablePart(this.#data, (items) => items.find((item) => item.key === key));
	}

	getAll(): Observable<Array<LanguageDetails>> {
		tryExecuteAndNotify(this._host, LanguageResource.getLanguage({ skip: 0, take: 1000 })).then(({ data }) => {
			if (data) {
				//TODO: Fix when we have the updated languageResource
				this.#data.append(data.items as Array<LanguageDetails>);
			}
		});

		return this.#data;
	}

	async save(language: LanguageDetails): Promise<void> {
		if (language.id && language.key) {
			tryExecuteAndNotify(
				this._host,
				LanguageResource.putLanguageById({ id: language.id, requestBody: language })
			).then((response) => {
				if (response.data) {
					this.#data.appendOne(response.data);
				}
			});
		} else {
			tryExecuteAndNotify(this._host, LanguageResource.postLanguage({ requestBody: language })).then((response) => {
				if (response.data) {
					this.#data.appendOne(response.data);
				}
			});
		}
	}
}
