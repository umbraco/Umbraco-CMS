import { UmbWorkspaceContext } from '../../../shared/components/workspace/workspace-context/workspace-context';
import { UmbWorkspaceEntityContextInterface } from '../../../shared/components/workspace/workspace-context/workspace-entity-context.interface';
import { UmbMemberTypeRepository } from '../repository/member-type.repository';
import { ObjectState } from '@umbraco-cms/observable-api';
import { UmbControllerHostInterface } from '@umbraco-cms/controller';

// TODO => use correct tpye
type EntityType = any;

export class UmbWorkspaceMemberTypeContext
	extends UmbWorkspaceContext
	implements UmbWorkspaceEntityContextInterface<EntityType | undefined>
{
	isNew = false;
	#host: UmbControllerHostInterface;
	#dataTypeRepository: UmbMemberTypeRepository;

	#data = new ObjectState<EntityType | undefined>(undefined);
	name = this.#data.getObservablePart((data) => data?.name);

	constructor(host: UmbControllerHostInterface) {
		super(host);
		this.#host = host;
		this.#dataTypeRepository = new UmbMemberTypeRepository(this.#host);
	}

	async load(entityKey: string) {
		const { data } = await this.#dataTypeRepository.requestByKey(entityKey);
		if (data) {
			this.isNew = false;
			this.#data.next(data);
		}
	}

	async createScaffold() {
		const { data } = await this.#dataTypeRepository.createScaffold();
		if (!data) return;
		this.isNew = true;
		this.#data.next(data);
	}

	getData() {
		return this.#data.getValue();
	}

	getEntityKey() {
		return this.getData()?.key || '';
	}

	getEntityType() {
		return 'member-type';
	}

	setName(name: string) {
		this.#data.update({ name });
	}

	setPropertyValue(alias: string, value: unknown) {
		// Not implemented
	}

	async save() {
		if (!this.#data.value) return;
		if (this.isNew) {
			await this.#dataTypeRepository.create(this.#data.value);
		} else {
			await this.#dataTypeRepository.save(this.#data.value);
		}
		// If it went well, then its not new anymore?.
		this.isNew = false;
	}

	async delete(key: string) {
		await this.#dataTypeRepository.delete(key);
	}

	public destroy(): void {
		this.#data.complete();
	}
}
