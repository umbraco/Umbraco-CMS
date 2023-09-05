import { UmbDataTypeRepository } from '../repository/data-type.repository.js';
import { UmbEntityWorkspaceContextInterface, UmbWorkspaceContext } from '@umbraco-cms/backoffice/workspace';
import type { DataTypeResponseModel } from '@umbraco-cms/backoffice/backend-api';
import { appendToFrozenArray, UmbObjectState } from '@umbraco-cms/backoffice/observable-api';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export class UmbDataTypeWorkspaceContext
	extends UmbWorkspaceContext<UmbDataTypeRepository, DataTypeResponseModel>
	implements UmbEntityWorkspaceContextInterface<DataTypeResponseModel | undefined>
{
	// TODO: revisit. temp solution because the create and response models are different.
	#data = new UmbObjectState<DataTypeResponseModel | undefined>(undefined);
	data = this.#data.asObservable();

	name = this.#data.asObservablePart((data) => data?.name);
	id = this.#data.asObservablePart((data) => data?.id);

	constructor(host: UmbControllerHostElement) {
		super(host, 'Umb.Workspace.DataType', new UmbDataTypeRepository(host));
	}

	async load(id: string) {
		const { data } = await this.repository.requestById(id);
		if (data) {
			this.setIsNew(false);
			this.#data.update(data);
		}
	}

	async create(parentId: string | null) {
		let { data } = await this.repository.createScaffold(parentId);
		if (this.modalContext) {
			data = { ...data, ...this.modalContext.data.preset };
		}
		this.setIsNew(true);
		// TODO: This is a hack to get around the fact that the data is not typed correctly.
		// Create and response models are different. We need to look into this.
		this.#data.next(data as unknown as DataTypeResponseModel);
		return { data };
	}

	getData() {
		return this.#data.getValue();
	}

	getEntityId() {
		return this.getData()?.id || '';
	}

	getEntityType() {
		return 'data-type';
	}

	setName(name: string) {
		this.#data.update({ name });
	}

	setPropertyEditorSchemaAlias(alias?: string) {
		this.#data.update({ propertyEditorAlias: alias });
	}
	setPropertyEditorUiAlias(alias?: string) {
		this.#data.update({ propertyEditorUiAlias: alias });
	}

	// TODO: its not called a property in the model, but we do consider this way in our front-end
	setPropertyValue(alias: string, value: unknown) {
		const entry = { alias: alias, value: value };

		const currentData = this.#data.value;
		if (currentData) {
			// TODO: make a partial update method for array of data, (idea/concept, use if this case is getting common)
			const newDataSet = appendToFrozenArray(currentData.values || [], entry, (x) => x.alias);
			this.#data.update({ values: newDataSet });
		}
	}

	async save() {
		if (!this.#data.value) return;
		if (!this.#data.value.id) return;

		if (this.getIsNew()) {
			await this.repository.create(this.#data.value);
		} else {
			await this.repository.save(this.#data.value.id, this.#data.value);
		}

		this.saveComplete(this.#data.value);
	}

	async delete(id: string) {
		await this.repository.delete(id);
	}

	public destroy(): void {
		this.#data.complete();
	}
}

export const UMB_DATA_TYPE_WORKSPACE_CONTEXT = new UmbContextToken<UmbEntityWorkspaceContextInterface, UmbDataTypeWorkspaceContext>(
	'UmbWorkspaceContext',
	(context): context is UmbDataTypeWorkspaceContext => context.getEntityType?.() === 'data-type'
);
