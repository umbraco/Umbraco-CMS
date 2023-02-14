import { UmbDictionaryRepository } from '../repository/dictionary.repository';
import { UmbWorkspaceContext } from '../../../../backoffice/shared/components/workspace/workspace-context/workspace-context';
import { UmbWorkspaceEntityContextInterface } from '../../../../backoffice/shared/components/workspace/workspace-context/workspace-entity-context.interface';
import { UmbControllerHostInterface } from '@umbraco-cms/controller';
import { ObjectState } from '@umbraco-cms/observable-api';
import type { DictionaryDetails } from '@umbraco-cms/models';

type EntityType = DictionaryDetails;
export class UmbWorkspaceDictionaryContext
	extends UmbWorkspaceContext
	implements UmbWorkspaceEntityContextInterface<EntityType | undefined>
{
	#host: UmbControllerHostInterface;
	#repo: UmbDictionaryRepository;

	#data = new ObjectState<DictionaryDetails | undefined>(undefined);
	data = this.#data.asObservable();
	name = this.#data.getObservablePart((data) => data?.name);
	dictionary = this.#data.getObservablePart((data) => data);

	constructor(host: UmbControllerHostInterface) {
		super(host);
		this.#host = host;
		this.#repo = new UmbDictionaryRepository(this.#host);
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
					return { ...translationItem, translation};
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
		const { data } = await this.#repo.requestDetails(entityKey);
		if (data) {
			this.#data.next(data);
		}
	}

	async createScaffold(parentKey: string | null) {
		const { data } = await this.#repo.createDetailsScaffold(parentKey);
		if (!data) return;
		this.#data.next(data);
	}

	async save() {
		if (!this.#data.value) return;
		this.#repo.saveDetail(this.#data.value);
	}
	
	public destroy(): void {
		this.#data.complete();
	}
}
