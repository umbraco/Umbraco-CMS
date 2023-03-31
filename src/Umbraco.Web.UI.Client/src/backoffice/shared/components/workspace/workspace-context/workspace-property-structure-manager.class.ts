import { UmbDocumentTypeRepository } from '../../../../documents/document-types/repository/document-type.repository';
import {
	DocumentTypeResponseModel,
	DocumentTypePropertyTypeResponseModel,
	PropertyTypeContainerResponseModelBaseModel,
	ContentTypeResponseModelBaseDocumentTypePropertyTypeResponseModelDocumentTypePropertyTypeContainerResponseModel,
} from '@umbraco-cms/backoffice/backend-api';
import { UmbControllerHostElement, UmbControllerInterface } from '@umbraco-cms/backoffice/controller';
import { ArrayState, UmbObserverController, MappingFunction } from '@umbraco-cms/backoffice/observable-api';

export type PropertyContainerTypes = 'Group' | 'Tab';

// TODO: get this type from the repository, or use some generic type.
type T = DocumentTypeResponseModel;

// TODO: make general interface for NodeTypeRepository, to replace UmbDocumentTypeRepository:
export class UmbWorkspacePropertyStructureManager<R extends UmbDocumentTypeRepository = UmbDocumentTypeRepository> {
	#host: UmbControllerHostElement;

	#documentTypeRepository: R;

	#rootDocumentTypeKey?: string;
	#documentTypeObservers = new Array<UmbControllerInterface>();
	#documentTypes = new ArrayState<T>([], (x) => x.key);

	#containers = new ArrayState<PropertyTypeContainerResponseModelBaseModel>([], (x) => x.key);

	constructor(host: UmbControllerHostElement, typeRepository: R) {
		this.#host = host;
		this.#documentTypeRepository = typeRepository;
	}

	/**
	 * loadType will load the node type and all inherited and composed types.
	 * This will give us all the structure for properties and containers.
	 */
	public async loadType(key?: string) {
		this._reset();

		this.#rootDocumentTypeKey = key;

		return await this._loadType(key);
	}

	public async createScaffold(parentKey: string) {
		this._reset();

		if (!parentKey) return {};

		const { data } = await this.#documentTypeRepository.createScaffold(parentKey);
		if (!data) return {};

		this.#rootDocumentTypeKey = data.key;

		await this._observeDocumentType(data);
		return { data };
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
					this.#documentTypes.appendOne(docType);
					this._initDocumentTypeContainers(docType);
					this._loadDocumentTypeCompositions(docType);
				}
			})
		);
	}

	private async _loadDocumentTypeCompositions(documentType: T) {
		documentType.compositions?.forEach((composition) => {
			this._loadType(composition.key);
		});
	}

	private async _initDocumentTypeContainers(documentType: T) {
		documentType.containers?.forEach((container) => {
			this.#containers.appendOne(container);
		});
	}

	/** Public methods for consuming structure: */

	rootDocumentType() {
		return this.#documentTypes.getObservablePart((x) => x.find((y) => y.key === this.#rootDocumentTypeKey));
	}
	getRootDocumentType() {
		return this.#documentTypes.getValue().find((y) => y.key === this.#rootDocumentTypeKey);
	}
	updateRootDocumentType(entry: T) {
		return this.#documentTypes.updateOne(this.#rootDocumentTypeKey, entry);
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

	hasPropertyStructuresOf(containerKey: string | null) {
		return this.#documentTypes.getObservablePart((docTypes) => {
			return (
				docTypes.find((docType) => {
					return docType.properties?.find((property) => property.containerKey === containerKey);
				}) !== undefined
			);
		});
	}
	rootPropertyStructures() {
		return this.propertyStructuresOf(null);
	}
	propertyStructuresOf(containerKey: string | null) {
		return this.#documentTypes.getObservablePart((docTypes) => {
			const props: DocumentTypePropertyTypeResponseModel[] = [];
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

	rootContainers(containerType: PropertyContainerTypes) {
		return this.#containers.getObservablePart((data) => {
			return data.filter((x) => x.parentKey === null && x.type === containerType);
		});
	}

	hasRootContainers(containerType: PropertyContainerTypes) {
		return this.#containers.getObservablePart((data) => {
			return data.filter((x) => x.parentKey === null && x.type === containerType).length > 0;
		});
	}

	containersOfParentKey(
		parentKey: PropertyTypeContainerResponseModelBaseModel['parentKey'],
		containerType: PropertyContainerTypes
	) {
		return this.#containers.getObservablePart((data) => {
			return data.filter((x) => x.parentKey === parentKey && x.type === containerType);
		});
	}

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
