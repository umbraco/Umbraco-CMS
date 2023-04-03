import { UmbDocumentTypeRepository } from '../../../../documents/document-types/repository/document-type.repository';
import { generateGuid } from '@umbraco-cms/backoffice/utils';
import {
	DocumentTypeResponseModel,
	DocumentTypePropertyTypeResponseModel,
	PropertyTypeContainerResponseModelBaseModel,
	ContentTypeResponseModelBaseDocumentTypePropertyTypeResponseModelDocumentTypePropertyTypeContainerResponseModel,
	PropertyTypeResponseModelBaseModel,
} from '@umbraco-cms/backoffice/backend-api';
import { UmbControllerHostElement, UmbControllerInterface } from '@umbraco-cms/backoffice/controller';
import {
	ArrayState,
	UmbObserverController,
	MappingFunction,
	partialUpdateFrozenArray,
} from '@umbraco-cms/backoffice/observable-api';

export type PropertyContainerTypes = 'Group' | 'Tab';

// TODO: get this type from the repository, or use some generic type.
type T = DocumentTypeResponseModel;

// TODO: make general interface for NodeTypeRepository, to replace UmbDocumentTypeRepository:
export class UmbWorkspacePropertyStructureManager<R extends UmbDocumentTypeRepository = UmbDocumentTypeRepository> {
	#host: UmbControllerHostElement;
	#init!: Promise<unknown>;

	#documentTypeRepository: R;

	#rootDocumentTypeKey?: string;
	#documentTypeObservers = new Array<UmbControllerInterface>();
	#documentTypes = new ArrayState<T>([], (x) => x.key);
	readonly documentTypes = this.#documentTypes.asObservable();
	private readonly _documentTypeContainers = this.#documentTypes.getObservablePart((x) =>
		x.flatMap((x) => x.containers ?? [])
	);

	#containers = new ArrayState<PropertyTypeContainerResponseModelBaseModel>([], (x) => x.key);

	constructor(host: UmbControllerHostElement, typeRepository: R) {
		this.#host = host;
		this.#documentTypeRepository = typeRepository;

		new UmbObserverController(host, this.documentTypes, (documentTypes) => {
			documentTypes.forEach((documentType) => {
				// We could cache by docType Key?
				// TODO: how do we ensure a container goes away?

				//this._initDocumentTypeContainers(documentType);
				this._loadDocumentTypeCompositions(documentType);
			});
		});
		new UmbObserverController(host, this._documentTypeContainers, (documentTypeContainers) => {
			this.#containers.next(documentTypeContainers);
		});
	}

	/**
	 * loadType will load the node type and all inherited and composed types.
	 * This will give us all the structure for properties and containers.
	 */
	public async loadType(key?: string) {
		this._reset();

		this.#rootDocumentTypeKey = key;

		const promiseResult = this._loadType(key);
		this.#init = promiseResult;
		await this.#init;
		return promiseResult;
	}

	public async createScaffold(parentId: string) {
		this._reset();

		if (!parentId) return {};

		const { data } = await this.#documentTypeRepository.createScaffold(parentId);
		if (!data) return {};

		this.#rootDocumentTypeKey = data.key;

		this.#init = this._observeDocumentType(data);
		await this.#init;
		return { data };
	}

	private async _ensureType(key?: string) {
		if (!key) return;
		if (this.#documentTypes.getValue().find((x) => x.key === key)) return;
		await this._loadType(key);
	}

	private async _loadType(key?: string) {
		if (!key) return {};

		const { data } = await this.#documentTypeRepository.requestByKey(key);
		if (!data) return {};

		await this._observeDocumentType(data);
		return { data };
	}

	public async _observeDocumentType(
		data: ContentTypeResponseModelBaseDocumentTypePropertyTypeResponseModelDocumentTypePropertyTypeContainerResponseModel
	) {
		if (!data.key) return;

		// Load inherited and composed types:
		this._loadDocumentTypeCompositions(data);

		this.#documentTypeObservers.push(
			new UmbObserverController(this.#host, await this.#documentTypeRepository.byKey(data.key), (docType) => {
				if (docType) {
					// TODO: Handle if there was changes made to the specific document type in this context.
					/*
					possible easy solutions could be to notify user wether they want to update(Discard the changes to accept the new ones).
					 */
					this.#documentTypes.appendOne(docType);
				}
			})
		);
	}

	private async _loadDocumentTypeCompositions(documentType: T) {
		documentType.compositions?.forEach((composition) => {
			this._ensureType(composition.key);
		});
	}

	/*
	private async _initDocumentTypeContainers(documentType: T) {
		documentType.containers?.forEach((container) => {
			this.#containers.appendOne({ ...container, _ownerDocumentTypeKey: documentType.key });
		});
	}
	*/

	/** Public methods for consuming structure: */

	rootDocumentType() {
		return this.#documentTypes.getObservablePart((x) => x.find((y) => y.key === this.#rootDocumentTypeKey));
	}
	getRootDocumentType() {
		return this.#documentTypes.getValue().find((y) => y.key === this.#rootDocumentTypeKey);
	}
	updateRootDocumentType(entry: T) {
		this.#documentTypes.updateOne(this.#rootDocumentTypeKey, entry);
	}

	// We could move the actions to another class?

	async createContainer(
		documentTypeKey: string | null,
		parentId: string | null = null,
		type: PropertyContainerTypes = 'Group',
		sortOrder?: number
	) {
		await this.#init;
		documentTypeKey = documentTypeKey ?? this.#rootDocumentTypeKey!;

		const container: PropertyTypeContainerResponseModelBaseModel = {
			key: generateGuid(),
			parentId: parentId,
			name: 'New',
			type: type,
			sortOrder: sortOrder ?? 0,
		};

		const containers = [...(this.#documentTypes.getValue().find((x) => x.key === documentTypeKey)?.containers ?? [])];
		containers.push(container);

		this.#documentTypes.updateOne(documentTypeKey, { containers });

		return container;
	}

	async removeContainer(documentTypeKey: string | null, containerId: string | null = null) {
		await this.#init;
		documentTypeKey = documentTypeKey ?? this.#rootDocumentTypeKey!;

		const frozenContainers = this.#documentTypes.getValue().find((x) => x.key === documentTypeKey)?.containers ?? [];
		const containers = frozenContainers.filter((x) => x.key !== containerId);

		this.#documentTypes.updateOne(documentTypeKey, { containers });
	}

	async createProperty(documentTypeKey: string | null, containerId: string | null = null, sortOrder?: number) {
		await this.#init;
		documentTypeKey = documentTypeKey ?? this.#rootDocumentTypeKey!;

		const property: PropertyTypeResponseModelBaseModel = {
			key: generateGuid(),
			containerId: containerId,
			//sortOrder: sortOrder ?? 0,
		};

		const properties = [...(this.#documentTypes.getValue().find((x) => x.key === documentTypeKey)?.properties ?? [])];
		properties.push(property);

		this.#documentTypes.updateOne(documentTypeKey, { properties });

		return property;
	}

	async updateProperty(
		documentTypeKey: string | null,
		propertyKey: string,
		partialUpdate: Partial<DocumentTypePropertyTypeResponseModel>
	) {
		await this.#init;
		documentTypeKey = documentTypeKey ?? this.#rootDocumentTypeKey!;

		const frozenProperties = this.#documentTypes.getValue().find((x) => x.key === documentTypeKey)?.properties ?? [];

		const properties = partialUpdateFrozenArray(frozenProperties, partialUpdate, (x) => x.key === propertyKey!);

		this.#documentTypes.updateOne(documentTypeKey, { properties });
	}

	/*
	rootDocumentTypeName() {
		return this.#documentTypes.getObservablePart((docTypes) => {
			const docType = docTypes.find((x) => x.key === this.#rootDocumentTypeKey);
			return docType?.name ?? '';
		});
	}
	*/

	rootDocumentTypeObservablePart<PartResult>(mappingFunction: MappingFunction<T, PartResult>) {
		return this.#documentTypes.getObservablePart((docTypes) => {
			const docType = docTypes.find((x) => x.key === this.#rootDocumentTypeKey);
			return docType ? mappingFunction(docType) : undefined;
		});
	}
	/*
	nameOfDocumentType(key: string) {
		return this.#documentTypes.getObservablePart((docTypes) => {
			const docType = docTypes.find((x) => x.key === key);
			return docType?.name ?? '';
		});
	}
	*/

	hasPropertyStructuresOf(containerId: string | null) {
		return this.#documentTypes.getObservablePart((docTypes) => {
			return (
				docTypes.find((docType) => {
					return docType.properties?.find((property) => property.containerId === containerId);
				}) !== undefined
			);
		});
	}
	rootPropertyStructures() {
		return this.propertyStructuresOf(null);
	}
	propertyStructuresOf(containerId: string | null) {
		return this.#documentTypes.getObservablePart((docTypes) => {
			const props: DocumentTypePropertyTypeResponseModel[] = [];
			docTypes.forEach((docType) => {
				docType.properties?.forEach((property) => {
					if (property.containerId === containerId) {
						props.push(property);
					}
				});
			});
			return props;
		});
	}

	rootContainers(containerType: PropertyContainerTypes) {
		return this.#containers.getObservablePart((data) => {
			return data.filter((x) => x.parentId === null && x.type === containerType);
		});
	}

	hasRootContainers(containerType: PropertyContainerTypes) {
		return this.#containers.getObservablePart((data) => {
			return data.filter((x) => x.parentId === null && x.type === containerType).length > 0;
		});
	}

	containersOfParentKey(
		parentId: PropertyTypeContainerResponseModelBaseModel['parentId'],
		containerType: PropertyContainerTypes
	) {
		return this.#containers.getObservablePart((data) => {
			return data.filter((x) => x.parentId === parentId && x.type === containerType);
		});
	}

	// TODO: Maybe this must take parentId into account as well?
	containersByNameAndType(name: string, containerType: PropertyContainerTypes) {
		return this.#containers.getObservablePart((data) => {
			return data.filter((x) => x.name === name && x.type === containerType);
		});
	}

	private _reset() {
		this.#documentTypeObservers.forEach((observer) => observer.destroy());
		this.#documentTypeObservers = [];
		this.#documentTypes.next([]);
		this.#containers.next([]);
	}
	public destroy() {
		this._reset();
		this.#documentTypes.complete();
		this.#containers.complete();
	}
}
