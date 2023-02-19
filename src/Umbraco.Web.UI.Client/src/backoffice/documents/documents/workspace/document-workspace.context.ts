import { UmbWorkspaceContext } from '../../../shared/components/workspace/workspace-context/workspace-context';
import { UmbDocumentRepository } from '../repository/document.repository';
import type { UmbWorkspaceEntityContextInterface } from '../../../shared/components/workspace/workspace-context/workspace-entity-context.interface';
import { UmbDocumentTypeRepository } from '../../document-types/repository/document-type.repository';
import type {
	DocumentModel,
	DocumentTypeModel,
	DocumentTypePropertyTypeContainerModel,
	DocumentTypePropertyTypeModel,
} from '@umbraco-cms/backend-api';
import {
	partialUpdateFrozenArray,
	ObjectState,
	ArrayState,
	UmbObserverController,
	appendToFrozenArray,
} from '@umbraco-cms/observable-api';
import { UmbControllerHostInterface } from '@umbraco-cms/controller';

// TODO: should this context be called DocumentDraft instead of workspace? or should the draft be part of this?

type EntityType = DocumentModel;
export class UmbDocumentWorkspaceContext
	extends UmbWorkspaceContext
	implements UmbWorkspaceEntityContextInterface<EntityType | undefined>
{
	#isNew = false;
	#host: UmbControllerHostInterface;
	#documentRepository: UmbDocumentRepository;
	#documentTypeRepository: UmbDocumentTypeRepository;
	//#dataTypeRepository: UmbDataTypeRepository;

	#draft = new ObjectState<EntityType | undefined>(undefined);
	documentTypeKey = this.#draft.getObservablePart((data) => data?.contentTypeKey);

	variants = this.#draft.getObservablePart((data) => data?.variants || []);

	#documentTypes = new ArrayState<DocumentTypeModel>([], (x) => x.key);
	documentTypes = this.#documentTypes.asObservable();

	mainDocumentType = this.#documentTypes.asObservable();

	// Notice the DocumentTypePropertyTypeContainerModel is equivalent to PropertyTypeContainerViewModelBaseModel, making it easy to generalize.
	#containers = new ArrayState<DocumentTypePropertyTypeContainerModel>([], (x) => x.key);
	//containers = this.#containers.asObservable();

	constructor(host: UmbControllerHostInterface) {
		super(host);
		this.#host = host;
		this.#documentRepository = new UmbDocumentRepository(this.#host);
		this.#documentTypeRepository = new UmbDocumentTypeRepository(this.#host);
		//this.#dataTypeRepository = new UmbDataTypeRepository(this.#host);

		new UmbObserverController(this._host, this.documentTypeKey, (key) => this._loadDocumentType(key));
	}

	async load(entityKey: string) {
		const { data } = await this.#documentRepository.requestByKey(entityKey);
		if (data) {
			this.#isNew = false;
			this.#draft.next(data);
		}
	}

	async createScaffold(parentKey: string | null) {
		const { data } = await this.#documentRepository.createDetailsScaffold(parentKey);
		if (!data) return;
		this.#isNew = true;
		this.#draft.next(data);
	}

	private async _loadDocumentType(key?: string) {
		if (!key) return;

		const { data } = await this.#documentTypeRepository.requestByKey(key);
		if (!data) return;

		// Load inherited and composed types:
		await data?.compositions?.forEach(async (composition) => {
			if (composition.key) {
				this._loadDocumentType(composition.key);
			}
		});

		new UmbObserverController(this._host, await this.#documentTypeRepository.byKey(key), (docType) => {
			if (docType) {
				this.#documentTypes.appendOne(docType);
				this._initDocumentTypeContainers(docType);
				this._loadDocumentTypeCompositions(docType);
			}
		});
	}

	private async _loadDocumentTypeCompositions(documentType: DocumentTypeModel) {
		documentType.compositions?.forEach((composition) => {
			this._loadDocumentType(composition.key);
		});
	}

	private async _initDocumentTypeContainers(documentType: DocumentTypeModel) {
		documentType.containers?.forEach((container) => {
			this.#containers.appendOne(container);
		});
	}

	/*

	No need for this currently. The data types are loaded by the properties.
	async loadDataTypeOfDocumentType(documentType?: DocumentTypeModel) {
		if (!documentType) return;

		// Load inherited and composed types:
		await documentType?.properties?.forEach(async (property) => {
			if (property.dataTypeKey) {
				this.loadDataType(property.dataTypeKey);
			}
		});
	}

	async loadDataType(key?: string) {
		if (!key) return;

		//const { data } = await this.#dataTypeRepository.requestDetails(key);

		// new UmbObserverController(this._host, await this.#documentTypeRepository.byKey(key), (data) => {
		//	if (data) {
		//		this.#documentTypes.appendOne(data);
		//	}
		//});
	}
	*/

	getData() {
		return this.#draft.getValue() || {};
	}

	/*
	getUnique() {
		return this.#data.getKey();
	}
	*/

	getEntityKey() {
		return this.getData().key;
	}

	getEntityType() {
		return 'document';
	}

	setName(name: string, culture?: string | null, segment?: string | null) {
		const variants = this.#draft.getValue()?.variants || [];
		const newVariants = partialUpdateFrozenArray(
			variants,
			{ name },
			(v) => v.culture == culture && v.segment == segment
		);
		this.#draft.update({ variants: newVariants });
	}
	/*
	getEntityType = this.#manager.getEntityType;
	getUnique = this.#manager.getEntityKey;
	getEntityKey = this.#manager.getEntityKey;

	*/

	/**
	 * Concept for Repository impl.:

	load(entityKey: string) {
		this.#repository.load(entityKey).then((data) => {
			this.#draft.next(data)
		})
	}

	create(parentKey: string | undefined) {
		this.#repository.create(parentKey).then((data) => {
			this.#draft.next(data)
		})
	}
	*/

	propertyValuesOf(culture: string | null, segment: string | null) {
		return this.#draft.getObservablePart((data) =>
			data?.properties?.filter((p) => (culture === p.culture || null) && (segment === p.segment || null))
		);
	}

	propertyValueOfAlias(propertyAlias: string, culture: string | null, segment: string | null) {
		return this.#draft.getObservablePart((data) =>
			data?.properties?.find(
				(p) => propertyAlias === p.alias && (culture === p.culture || null) && (segment === p.segment || null)
			)
		);
	}

	hasPropertyStructuresOf(containerKey: string | null) {
		return this.#documentTypes.getObservablePart((docTypes) => {
			return (
				docTypes.find((docType) => {
					return docType.properties?.find((property) => {
						if (property.containerKey === containerKey) {
							return true;
						}
					});
				}) !== undefined
			);
		});
	}
	rootPropertyStructures() {
		return this.propertyStructuresOf(null);
	}
	propertyStructuresOf(containerKey: string | null) {
		return this.#documentTypes.getObservablePart((docTypes) => {
			const props: DocumentTypePropertyTypeModel[] = [];
			docTypes.forEach((docType) => {
				docType.properties?.forEach((property) => {
					if (property.containerKey === containerKey) {
						props.push(property);
					}
				});
			});
			return props;
		});
	}

	// TODO: Check what of these methods I ended actually using:

	rootContainers(containerType: 'Group' | 'Tab') {
		return this.#containers.getObservablePart((data) => {
			return data.filter((x) => x.parentKey === null && x.type === containerType);
		});
	}

	hasRootContainers(containerType: 'Group' | 'Tab') {
		return this.#containers.getObservablePart((data) => {
			return data.filter((x) => x.parentKey === null && x.type === containerType).length > 0;
		});
	}

	containersOfParentKey(
		parentKey: DocumentTypePropertyTypeContainerModel['parentKey'],
		containerType: 'Group' | 'Tab'
	) {
		return this.#containers.getObservablePart((data) => {
			return data.filter((x) => x.parentKey === parentKey && x.type === containerType);
		});
	}

	containersByNameAndType(name: string, containerType: 'Group' | 'Tab') {
		return this.#containers.getObservablePart((data) => {
			return data.filter((x) => x.name === name && x.type === containerType);
		});
	}
	setPropertyValue(alias: string, value: unknown) {
		const entry = { alias: alias, value: value };

		const currentData = this.#draft.value;
		if (currentData) {
			// TODO: make a partial update method for array of data, (idea/concept, use if this case is getting common)
			const newDataSet = appendToFrozenArray(currentData.properties || [], entry, (x) => x.alias);
			this.#draft.update({ properties: newDataSet });
		}
	}

	async save() {
		if (!this.#draft.value) return;
		if (this.#isNew) {
			await this.#documentRepository.createDetail(this.#draft.value);
		} else {
			await this.#documentRepository.saveDetail(this.#draft.value);
		}
		// If it went well, then its not new anymore?.
		this.#isNew = false;
	}

	async delete(key: string) {
		await this.#documentRepository.delete(key);
	}

	/*
	concept notes:

	public saveAndPublish() {

	}

	public saveAndPreview() {

	}
	*/

	public destroy(): void {
		this.#draft.complete();
	}
}
