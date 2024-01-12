import { UmbMediaRepository } from '../repository/media.repository.js';
import type { UmbMediaDetailModel } from '../index.js';
import {
	UmbSaveableWorkspaceContextInterface,
	UmbEditableWorkspaceContextBase,
} from '@umbraco-cms/backoffice/workspace';
import { appendToFrozenArray, UmbObjectState } from '@umbraco-cms/backoffice/observable-api';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import { UmbApi } from '@umbraco-cms/backoffice/extension-api';

type EntityType = UmbMediaDetailModel;
export class UmbMediaWorkspaceContext
	extends UmbEditableWorkspaceContextBase<UmbMediaRepository, EntityType>
	implements UmbSaveableWorkspaceContextInterface, UmbApi
{
	#data = new UmbObjectState<EntityType | undefined>(undefined);
	data = this.#data.asObservable();
	name = this.#data.asObservablePart((data) => data?.name);

	constructor(host: UmbControllerHostElement) {
		super(host, 'Umb.Workspace.Media', new UmbMediaRepository(host));
	}

	getData() {
		return this.#data.getValue();
	}

	// TODO: this should be async because it can only return the id if the data is loaded.
	getEntityId() {
		return this.getData()?.id || '';
	}

	getEntityType() {
		return 'media';
	}

	setName(name: string) {
		this.#data.update({ name });
	}

	setPropertyValue(alias: string, value: unknown) {
		const entry = { alias: alias, value: value };

		const currentData = this.#data.value;
		if (currentData) {
			const newDataSet = appendToFrozenArray(currentData.data, entry, (x) => x.alias);

			this.#data.update({ data: newDataSet });
		}
	}

	async load(entityId: string) {
		const { data } = await this.repository.requestById(entityId);
		if (data) {
			this.setIsNew(false);
			this.#data.next(data);
		}
	}

	async create(parentId: string | null) {
		const { data } = await this.repository.createScaffold(parentId);
		if (!data) return;
		this.setIsNew(true);
		// TODO: This is a hack to get around the fact that the data is not typed correctly.
		// Create and response models are different. We need to look into this.
		this.#data.next(data as unknown as UmbMediaDetailModel);
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
