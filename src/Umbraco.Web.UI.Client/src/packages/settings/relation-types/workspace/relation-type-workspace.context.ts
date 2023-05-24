import { UmbRelationTypeRepository } from '../repository/relation-type.repository.js';
import { UmbEntityWorkspaceContextInterface, UmbWorkspaceContext } from '@umbraco-cms/backoffice/workspace';
import type { RelationTypeBaseModel, RelationTypeResponseModel } from '@umbraco-cms/backoffice/backend-api';
import { UmbObjectState } from '@umbraco-cms/backoffice/observable-api';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';

export class UmbRelationTypeWorkspaceContext
	extends UmbWorkspaceContext<UmbRelationTypeRepository, RelationTypeResponseModel>
	implements UmbEntityWorkspaceContextInterface<RelationTypeResponseModel | undefined>
{
	#data = new UmbObjectState<RelationTypeResponseModel | undefined>(undefined);
	data = this.#data.asObservable();
	name = this.#data.getObservablePart((data) => data?.name);
	id = this.#data.getObservablePart((data) => data?.id);

	constructor(host: UmbControllerHostElement) {
		super(host, new UmbRelationTypeRepository(host));
	}

	async load(id: string) {
		const { data } = await this.repository.requestById(id);

		if (data) {
			this.setIsNew(false);
			this.#data.update(data);
		}
	}

	async createScaffold(parentId: string | null) {
		const { data } = await this.repository.createScaffold(parentId);
		if (!data) return;
		this.setIsNew(true);
		this.#data.next(data);
	}

	getData() {
		return this.#data.getValue();
	}

	getEntityId() {
		return this.getData()?.id || '';
	}

	getEntityType() {
		return 'relation-type';
	}

	setName(name: string) {
		this.#data.update({ name });
	}

	async save() {
		if (!this.#data.value) return;
		if (!this.#data.value.id) return;

		if (this.isNew) {
			await this.repository.create(this.#data.value);
		} else {
			await this.repository.save(this.#data.value.id, this.#data.value);
		}

		// If it went well, then its not new anymore?.
		this.setIsNew(false);
	}

	update<K extends keyof RelationTypeBaseModel>(id: K, value: RelationTypeBaseModel[K]) {
		this.#data.next({ ...this.#data.value, [id]: value });
	}

	async delete(id: string) {
		await this.repository.delete(id);
	}

	public destroy(): void {
		this.#data.complete();
	}
}
