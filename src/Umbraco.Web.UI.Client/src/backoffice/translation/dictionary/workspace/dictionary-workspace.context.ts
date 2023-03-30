import { UmbDictionaryRepository } from '../repository/dictionary.repository';
import { UmbWorkspaceContext } from '../../../../backoffice/shared/components/workspace/workspace-context/workspace-context';
import type { DictionaryDetails } from '../';
import { UmbEntityWorkspaceContextInterface } from '@umbraco-cms/backoffice/workspace';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller';
import { ObjectState } from '@umbraco-cms/backoffice/observable-api';

type EntityType = DictionaryDetails;

export class UmbDictionaryWorkspaceContext
	extends UmbWorkspaceContext<UmbDictionaryRepository, EntityType>
	implements UmbEntityWorkspaceContextInterface<EntityType | undefined>
{
	#data = new ObjectState<DictionaryDetails | undefined>(undefined);
	data = this.#data.asObservable();
	name = this.#data.getObservablePart((data) => data?.name);
	dictionary = this.#data.getObservablePart((data) => data);

	constructor(host: UmbControllerHostElement) {
		super(host, new UmbDictionaryRepository(host));
	}

	getData() {
		return this.#data.getValue();
	}

	getEntityKey() {
		return this.getData()?.key || '';
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

	async load(entityKey: string) {
		const { data } = await this.repository.requestByKey(entityKey);
		if (data) {
			this.setIsNew(false);
			this.#data.next(data);
		}
	}

	async createScaffold(parentKey: string | null) {
		const { data } = await this.repository.createScaffold(parentKey);
		if (!data) return;
		this.setIsNew(true);
		this.#data.next(data);
	}

	async save() {
		if (!this.#data.value) return;
		await this.repository.save(this.#data.value);
		this.setIsNew(false);
	}

	public destroy(): void {
		this.#data.complete();
	}
}
