import { UmbDictionaryDetailRepository } from '../repository/index.js';
import type { UmbDictionaryDetailModel } from '../types.js';
import {
	type UmbSaveableWorkspaceContextInterface,
	UmbEditableWorkspaceContextBase,
} from '@umbraco-cms/backoffice/workspace';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbObjectState } from '@umbraco-cms/backoffice/observable-api';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export class UmbDictionaryWorkspaceContext
	extends UmbEditableWorkspaceContextBase<UmbDictionaryDetailModel>
	implements UmbSaveableWorkspaceContextInterface
{
	//
	public readonly detailRepository = new UmbDictionaryDetailRepository(this);

	#data = new UmbObjectState<UmbDictionaryDetailModel | undefined>(undefined);
	readonly data = this.#data.asObservable();

	readonly name = this.#data.asObservablePart((data) => data?.name);
	readonly dictionary = this.#data.asObservablePart((data) => data);

	constructor(host: UmbControllerHost) {
		super(host, 'Umb.Workspace.Dictionary');
	}

	protected resetState(): void {
		super.resetState();
		this.#data.setValue(undefined);
	}

	getData() {
		return this.#data.getValue();
	}

	getUnique() {
		return this.getData()?.unique;
	}

	getEntityType() {
		return 'dictionary';
	}

	setName(name: string) {
		this.#data.update({ name });
	}

	setPropertyValue(isoCode: string, translation: string) {
		if (!this.#data.value) return;

		// TODO: This can use some of our own methods, to make it simpler. see appendToFrozenArray()
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

		this.#data.setValue({ ...this.#data.value, translations: updatedValue });
	}

	async load(unique: string) {
		this.resetState();
		const { data } = await this.detailRepository.requestByUnique(unique);
		if (data) {
			this.setIsNew(false);
			this.#data.setValue(data);
		}
	}

	async create(parentUnique: string | null) {
		this.resetState();
		const { data } = await this.detailRepository.createScaffold(parentUnique);
		if (!data) return;
		this.setIsNew(true);
		this.#data.setValue(data);
	}

	async save() {
		if (!this.#data.value) return;
		if (!this.#data.value.unique) return;

		if (this.getIsNew()) {
			const { error } = await this.detailRepository.create(this.#data.value);
			if (error) {
				return;
			}
			this.setIsNew(false);
		} else {
			await this.detailRepository.save(this.#data.value);
		}

		const data = this.getData();
		if (!data) return;

		this.setIsNew(false);
		this.workspaceComplete(data);
	}

	public destroy(): void {
		this.#data.destroy();
		super.destroy();
	}
}

export const UMB_DICTIONARY_WORKSPACE_CONTEXT = new UmbContextToken<
	UmbSaveableWorkspaceContextInterface,
	UmbDictionaryWorkspaceContext
>(
	'UmbWorkspaceContext',
	undefined,
	(context): context is UmbDictionaryWorkspaceContext => context.getEntityType?.() === 'dictionary',
);
