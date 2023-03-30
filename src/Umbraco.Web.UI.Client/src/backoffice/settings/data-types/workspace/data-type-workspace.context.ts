import { UmbWorkspaceContext } from '../../../shared/components/workspace/workspace-context/workspace-context';
import { UmbDataTypeRepository } from '../repository/data-type.repository';
import { UmbEntityWorkspaceContextInterface } from '@umbraco-cms/backoffice/workspace';
import type { DataTypeResponseModel } from '@umbraco-cms/backoffice/backend-api';
import { appendToFrozenArray, ObjectState } from '@umbraco-cms/backoffice/observable-api';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller';

export class UmbDataTypeWorkspaceContext
	extends UmbWorkspaceContext<UmbDataTypeRepository, DataTypeResponseModel>
	implements UmbEntityWorkspaceContextInterface<DataTypeResponseModel | undefined>
{
	#data = new ObjectState<DataTypeResponseModel | undefined>(undefined);
	data = this.#data.asObservable();
	name = this.#data.getObservablePart((data) => data?.name);
	key = this.#data.getObservablePart((data) => data?.key);

	constructor(host: UmbControllerHostElement) {
		super(host, new UmbDataTypeRepository(host));
	}

	async load(key: string) {
		const { data } = await this.repository.requestByKey(key);
		if (data) {
			this.setIsNew(false);
			this.#data.update(data);
		}
	}

	async createScaffold(parentKey: string | null) {
		const { data } = await this.repository.createScaffold(parentKey);
		this.setIsNew(true);
		this.#data.next(data);
		return { data };
	}

	getData() {
		return this.#data.getValue();
	}

	getEntityKey() {
		return this.getData()?.key || '';
	}

	getEntityType() {
		return 'data-type';
	}

	setName(name: string) {
		this.#data.update({ name });
	}

	setPropertyEditorAlias(alias?: string) {
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
		if (this.isNew) {
			await this.repository.create(this.#data.value);
		} else {
			await this.repository.save(this.#data.value);
		}
		// If it went well, then its not new anymore?.
		this.setIsNew(false);
	}

	async delete(key: string) {
		await this.repository.delete(key);
	}

	public destroy(): void {
		this.#data.complete();
	}
}
