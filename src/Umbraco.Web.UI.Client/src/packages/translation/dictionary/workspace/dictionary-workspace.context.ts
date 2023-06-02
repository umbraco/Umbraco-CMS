import { UmbDictionaryRepository } from '../repository/dictionary.repository.js';
import { UmbEntityWorkspaceContextInterface, UmbWorkspaceContext } from '@umbraco-cms/backoffice/workspace';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { UmbObjectState } from '@umbraco-cms/backoffice/observable-api';
import { DictionaryItemResponseModel } from '@umbraco-cms/backoffice/backend-api';

export class UmbDictionaryWorkspaceContext
	extends UmbWorkspaceContext<UmbDictionaryRepository, DictionaryItemResponseModel>
	implements UmbEntityWorkspaceContextInterface<DictionaryItemResponseModel | undefined>
{
	#data = new UmbObjectState<DictionaryItemResponseModel | undefined>(undefined);
	data = this.#data.asObservable();

	name = this.#data.getObservablePart((data) => data?.name);
	dictionary = this.#data.getObservablePart((data) => data);

	constructor(host: UmbControllerHostElement) {
		super(host, new UmbDictionaryRepository(host));
	}

	getData() {
		return this.#data.getValue();
	}

	getEntityId() {
		return this.getData()?.id || '';
	}

	getEntityType() {
		return 'dictionary-item';
	}

	setName(name: string) {
		this.#data.update({ name });
	}

	setPropertyValue(isoCode: string, translation: string) {
		if (!this.#data.value) return;

		// update if the code already exists
		const updatedValue =
			this.#data.value.translations?.map((translationItem) => {
				if (translationItem.isoCode === isoCode) {
					return { ...translationItem, translation };
				}
				return translationItem;
			}) ?? [];

		// if code doesn't exist, add it to the new value set
		if (!updatedValue?.find((x) => x.isoCode === isoCode)) {
			updatedValue?.push({ isoCode, translation });
		}

		this.#data.next({ ...this.#data.value, translations: updatedValue });
	}

	async load(entityId: string) {
		const { data } = await this.repository.requestById(entityId);
		if (data) {
			this.setIsNew(false);
			this.#data.next(data);
		}
	}

	async createScaffold(parentId: string | null) {
		const { data } = await this.repository.createScaffold(parentId);
		if (!data) return;
		this.setIsNew(true);
		// TODO: This is a hack to get around the fact that the data is not typed correctly.
		// Create and response models are different. We need to look into this.
		this.#data.next(data as unknown as DictionaryItemResponseModel);
	}

	async save() {
		if (!this.#data.value) return;
		if (!this.#data.value.id) return;
		await this.repository.save(this.#data.value.id, this.#data.value);
		this.setIsNew(false);
	}

	public destroy(): void {
		this.#data.complete();
	}
}
