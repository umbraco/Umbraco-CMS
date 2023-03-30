import { UmbWorkspaceContext } from '../../../shared/components/workspace/workspace-context/workspace-context';
import { UmbDocumentTypeRepository } from '../repository/document-type.repository';
import { UmbWorkspacePropertyStructureManager } from '../../../shared/components/workspace/workspace-context/workspace-property-structure-manager.class';
import { UmbEntityWorkspaceContextInterface } from '@umbraco-cms/backoffice/workspace';
import type { DocumentTypeResponseModel } from '@umbraco-cms/backoffice/backend-api';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller';

type EntityType = DocumentTypeResponseModel;
export class UmbWorkspaceDocumentTypeContext
	extends UmbWorkspaceContext<UmbDocumentTypeRepository, EntityType>
	implements UmbEntityWorkspaceContextInterface<EntityType | undefined>
{
	// Draft is located in structure manager
	readonly data;
	readonly name;

	readonly structure;

	constructor(host: UmbControllerHostElement) {
		super(host, new UmbDocumentTypeRepository(host));

		this.structure = new UmbWorkspacePropertyStructureManager(this.host, this.repository);
		this.data = this.structure.rootDocumentType;
		this.name = this.structure.rootDocumentTypeObservablePart((data) => data?.name);
	}

	public setPropertyValue(alias: string, value: unknown) {
		throw new Error('setPropertyValue is not implemented for UmbWorkspaceDocumentTypeContext');
	}

	getData() {
		return this.structure.getRootDocumentType() || {};
	}

	getEntityKey() {
		return this.getData().key;
	}

	getEntityType() {
		return 'document-type';
	}

	setName(name: string) {
		this.structure.updateRootDocumentType({ name });
	}

	// TODO => manage setting icon color
	setIcon(icon: string) {
		this.structure.updateRootDocumentType({ icon });
	}

	async createScaffold(documentTypeKey: string) {
		const { data } = await this.structure.createScaffold(documentTypeKey);
		if (!data) return undefined;

		this.setIsNew(true);
		//this.#draft.next(data);
		return data || undefined;
	}

	async load(entityKey: string) {
		const { data } = await this.structure.loadType(entityKey);
		if (!data) return undefined;

		this.setIsNew(false);
		//this.#draft.next(data);
		return data || undefined;
	}

	async save() {
		this.repository.save(this.getData());
	}

	public destroy(): void {
		this.structure.destroy();
	}
}
