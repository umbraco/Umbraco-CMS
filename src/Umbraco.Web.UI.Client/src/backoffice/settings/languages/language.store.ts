import { Observable } from 'rxjs';
import { Language, LanguageResource } from '@umbraco-cms/backend-api';
import { tryExecuteAndNotify } from '@umbraco-cms/resources';
import { UmbContextToken } from '@umbraco-cms/context-api';
import { UmbStoreBase } from '@umbraco-cms/store';
import { ArrayState } from '@umbraco-cms/observable-api';
import { UmbControllerHostInterface } from '@umbraco-cms/controller';
import { umbracoPath } from '@umbraco-cms/utils';

export type UmbLanguageStoreItemType = Language;
export const UMB_LANGUAGE_STORE_CONTEXT_TOKEN = new UmbContextToken<UmbLanguageStore>('umbLanguageStore');

/**
 * @export
 * @class UmbLanguageStore
 * @extends {UmbStoreBase}
 * @description - Data Store for languages
 */
export class UmbLanguageStore extends UmbStoreBase {
	#data = new ArrayState<UmbLanguageStoreItemType>([], (x) => x.isoCode);
	#availableLanguages = new ArrayState<UmbLanguageStoreItemType>([], (x) => x.isoCode);

	public readonly availableLanguages = this.#availableLanguages.asObservable();

	constructor(host: UmbControllerHostInterface) {
		super(host, UMB_LANGUAGE_STORE_CONTEXT_TOKEN.toString());
	}

	getByIsoCode(isoCode: string) {
		tryExecuteAndNotify(this._host, LanguageResource.getLanguageByIsoCode({ isoCode })).then(({ data }) => {
			if (data) {
				this.#data.appendOne(data);
			}
		});

		return this.#data.getObservablePart((items) => items.find((item) => item.isoCode === isoCode));
	}

	getAll(): Observable<Array<UmbLanguageStoreItemType>> {
		tryExecuteAndNotify(this._host, LanguageResource.getLanguage({ skip: 0, take: 1000 })).then(({ data }) => {
			this.#data.append(data?.items ?? []);
		});

		return this.#data;
	}

	getAvailable() {
		fetch(umbracoPath('/languages').toString())
			.then((res) => res.json())
			.then((data) => {
				console.log('data', data);

				this.#availableLanguages.append(data);
			});

		return this.availableLanguages;
	}

	async save(language: UmbLanguageStoreItemType): Promise<void> {
		if (language.isoCode) {
			const { data: updatedLanguage } = await tryExecuteAndNotify(
				this._host,
				LanguageResource.putLanguageByIsoCode({ isoCode: language.isoCode, requestBody: language })
			);
			if (updatedLanguage) {
				this.#data.appendOne(updatedLanguage);
			}
		} else {
			const { data: newLanguage } = await tryExecuteAndNotify(
				this._host,
				LanguageResource.postLanguage({ requestBody: language })
			);
			if (newLanguage) {
				this.#data.appendOne(newLanguage);
			}
		}
	}

	async delete(isoCodes: Array<string>) {
		const queue = isoCodes.map((isoCode) =>
			tryExecuteAndNotify(
				this._host,
				LanguageResource.deleteLanguageByIsoCode({ isoCode }).then(() => isoCode)
			)
		);
		const results = await Promise.all(queue);
		const filtered = results.filter((x) => !!x).map((result) => result.data);
		this.#data.remove(filtered);
	}
}
