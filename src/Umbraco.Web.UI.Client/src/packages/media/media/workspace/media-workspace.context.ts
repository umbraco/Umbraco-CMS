import { UmbMediaDetailRepository } from '../repository/detail/index.js';
import type { UmbMediaDetailModel } from '../index.js';
import type { UmbSaveableWorkspaceContextInterface } from '@umbraco-cms/backoffice/workspace';
import { UmbEditableWorkspaceContextBase } from '@umbraco-cms/backoffice/workspace';
import { appendToFrozenArray, UmbObjectState } from '@umbraco-cms/backoffice/observable-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';

type EntityType = UmbMediaDetailModel;
export class UmbMediaWorkspaceContext
	extends UmbEditableWorkspaceContextBase<EntityType>
	implements UmbSaveableWorkspaceContextInterface, UmbApi
{
	//
	public readonly repository = new UmbMediaDetailRepository(this);

	#data = new UmbObjectState<EntityType | undefined>(undefined);
	data = this.#data.asObservable();
	// TODO: get correct variant name
	name = this.#data.asObservablePart((data) => data?.variants[0].name);

	constructor(host: UmbControllerHost) {
		super(host, 'Umb.Workspace.Media');
	}

	getData() {
		return this.#data.getValue();
	}

	// TODO: this should be async because it can only return the id if the data is loaded.
	getEntityId() {
		return this.getData()?.unique;
	}

	getEntityType() {
		return 'media';
	}

	setPropertyValue(alias: string, value: unknown) {
		const entry = { alias: alias, value: value };

		const currentData = this.#data.value;
		if (currentData) {
			const newDataSet = appendToFrozenArray(currentData.values, entry, (x) => x.alias);

			this.#data.update({ values: newDataSet });
		}
	}

	async load(unique: string) {
		const { data } = await this.repository.requestByUnique(unique);
		if (data) {
			this.setIsNew(false);
			this.#data.setValue(data);
		}
	}

	async create(parentUnique: string | null) {
		const { data } = await this.repository.createScaffold(parentUnique);
		if (!data) return;
		this.setIsNew(true);
		// TODO: This is a hack to get around the fact that the data is not typed correctly.
		// Create and response models are different. We need to look into this.
		this.#data.setValue(data as unknown as UmbMediaDetailModel);
	}

	async save() {
		if (!this.#data.value) return;
		if (this.isNew) {
			// eslint-disable-next-line @typescript-eslint/ban-ts-comment
			// @ts-ignore
			await this.repository.create(this.#data.value);
		} else {
			// eslint-disable-next-line @typescript-eslint/ban-ts-comment
			// @ts-ignore
			await this.repository.save(this.#data.value.id, this.#data.value);
		}
		// If it went well, then its not new anymore?.
		this.setIsNew(false);
	}

	async delete(id: string) {
		await this.repository.delete(id);
	}

	public destroy(): void {
		this.#data.destroy();
	}
}
export const api = UmbMediaWorkspaceContext;

export const UMB_MEDIA_WORKSPACE_CONTEXT = new UmbContextToken<
	UmbSaveableWorkspaceContextInterface,
	UmbMediaWorkspaceContext
>(
	'UmbWorkspaceContext',
	undefined,
	(context): context is UmbMediaWorkspaceContext => context.getEntityType?.() === 'media',
);
