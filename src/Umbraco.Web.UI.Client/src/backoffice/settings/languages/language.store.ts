import { Observable } from 'rxjs';
import { Culture, CultureResource, Language, LanguageResource } from '@umbraco-cms/backend-api';
import { tryExecuteAndNotify } from '@umbraco-cms/resources';
import { UmbContextToken } from '@umbraco-cms/context-api';
import { UmbStoreBase } from '@umbraco-cms/store';
import { ArrayState } from '@umbraco-cms/observable-api';
import { UmbControllerHostInterface } from '@umbraco-cms/controller';

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
	#availableLanguages = new ArrayState<Culture>([], (x) => x.name);

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

	getAvailableCultures() {
		tryExecuteAndNotify(this._host, CultureResource.getCulture({ skip: 0, take: 1000 })).then(({ data }) => {
			if (!data) return;
			this.#availableLanguages.append(data.items);
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
