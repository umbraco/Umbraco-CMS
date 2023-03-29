import { UmbWorkspaceContext } from '../../../shared/components/workspace/workspace-context/workspace-context';
import { UmbEntityWorkspaceContextInterface } from '../../../shared/components/workspace/workspace-context/workspace-entity-context.interface';
import { UmbDocumentTypeRepository } from '../repository/document-type.repository';
import { UmbWorkspacePropertyStructureManager } from '../../../shared/components/workspace/workspace-context/workspace-property-structure-manager.class';
import type { DocumentTypeResponseModel } from '@umbraco-cms/backoffice/backend-api';
import { UmbControllerHostInterface } from '@umbraco-cms/backoffice/controller';
import { ObjectState } from '@umbraco-cms/backoffice/observable-api';

type EntityType = DocumentTypeResponseModel;
export class UmbWorkspaceDocumentTypeContext
	extends UmbWorkspaceContext<UmbDocumentTypeRepository>
	implements UmbEntityWorkspaceContextInterface<EntityType | undefined>
{
	#draft = new ObjectState<EntityType | undefined>(undefined);
	data = this.#draft.asObservable();
	name = this.#draft.getObservablePart((data) => data?.name);

	readonly structure;

	constructor(host: UmbControllerHostInterface) {
		super(host, new UmbDocumentTypeRepository(host));

		this.structure = new UmbWorkspacePropertyStructureManager(this.host, this.repository);
	}

	public setPropertyValue(alias: string, value: unknown) {
		throw new Error('setPropertyValue is not implemented for UmbWorkspaceDocumentTypeContext');
	}

	getData() {
		return this.#draft.getValue();
	}

	getEntityKey() {
		return this.getData()?.key || '';
	}

	getEntityType() {
		return 'document-type';
	}

	setName(name: string) {
		this.#draft.update({ name });
	}

	// TODO => manage setting icon color
	setIcon(icon: string) {
		this.#draft.update({ icon });
	}

	async createScaffold(documentTypeKey: string) {
		const { data } = await this.repository.createScaffold(documentTypeKey);
		if (!data) return undefined;

		this.setIsNew(true);
		//this.#draft.next(data);
		return data || undefined;
	}

	async load(entityKey: string) {
		this.structure.loadType(entityKey);

		const { data } = await this.repository.requestByKey(entityKey);
		if (!data) return undefined;

		this.setIsNew(false);
		//this.#draft.next(data);
		return data || undefined;
	}

	async save() {
		if (!this.#draft.value) return;
		this.repository.save(this.#draft.value);
	}

	public destroy(): void {
		this.#draft.complete();
	}
}
