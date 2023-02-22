import { UmbWorkspaceContext } from '../../../shared/components/workspace/workspace-context/workspace-context';
import { UmbWorkspaceEntityContextInterface } from '../../../shared/components/workspace/workspace-context/workspace-entity-context.interface';
import { UmbDocumentTypeRepository } from '../repository/document-type.repository';
import type { DocumentTypeModel } from '@umbraco-cms/backend-api';
import { UmbControllerHostInterface } from '@umbraco-cms/controller';
import { ObjectState } from '@umbraco-cms/observable-api';

type EntityType = DocumentTypeModel;
export class UmbWorkspaceDocumentTypeContext
	extends UmbWorkspaceContext
	implements UmbWorkspaceEntityContextInterface<EntityType | undefined>
{
	#host: UmbControllerHostInterface;
	#repo: UmbDocumentTypeRepository;

	#data = new ObjectState<EntityType | undefined>(undefined);
	data = this.#data.asObservable();
	name = this.#data.getObservablePart((data) => data?.name);

	constructor(host: UmbControllerHostInterface) {
		super(host);
		this.#host = host;
		this.#repo = new UmbDocumentTypeRepository(this.#host);
	}
<<<<<<< HEAD
	setIcon(icon: string) {
		this.#manager.state.update({ icon: icon });
	}
	getEntityType = this.#manager.getEntityType;
	getUnique = this.#manager.getEntityKey;
	getEntityKey = this.#manager.getEntityKey;
	getStore = this.#manager.getStore;
	getData = this.#manager.getData as any; // TODO: fix this type mismatch, but this will be done when we move to repositories.
	load = this.#manager.load;
	create = this.#manager.create;
	save = this.#manager.save;
	destroy = this.#manager.destroy;
=======
>>>>>>> origin/feature/mocks-for-doc-types

	public setPropertyValue(alias: string, value: unknown) {
		throw new Error('setPropertyValue is not implemented for UmbWorkspaceDocumentTypeContext');
	}

	getData() {
		return this.#data.getValue();
	}

	getEntityKey() {
		return this.getData()?.key || '';
	}

	getEntityType() {
		return 'document-type';
	}

	setName(name: string) {
		this.#data.update({ name });
	}

	// TODO => manage setting icon color
	setIcon(icon: string) {
		this.#data.update({ icon });
	}

	async load(entityKey: string) {
		const { data } = await this.#repo.requestByKey(entityKey);
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
