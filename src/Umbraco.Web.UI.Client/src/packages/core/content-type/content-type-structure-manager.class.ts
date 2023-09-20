import { UmbDetailRepository } from '@umbraco-cms/backoffice/repository';
import { UmbId } from '@umbraco-cms/backoffice/id';
import {
	DocumentTypePropertyTypeResponseModel,
	PropertyTypeContainerModelBaseModel,
	PropertyTypeModelBaseModel,
	DocumentTypeResponseModel,
} from '@umbraco-cms/backoffice/backend-api';
import { UmbControllerHostElement, UmbController } from '@umbraco-cms/backoffice/controller-api';
import {
	UmbArrayState,
	UmbObserverController,
	MappingFunction,
	partialUpdateFrozenArray,
	appendToFrozenArray,
	filterFrozenArray,
} from '@umbraco-cms/backoffice/observable-api';
import { incrementString } from '@umbraco-cms/backoffice/utils';

export type PropertyContainerTypes = 'Group' | 'Tab';

type T = DocumentTypeResponseModel;

// TODO: get this type from the repository, or use some generic type.
export class UmbContentTypePropertyStructureManager<R extends UmbDetailRepository<T> = UmbDetailRepository<T>> {
	#host: UmbControllerHostElement;
	#init!: Promise<unknown>;

	#contentTypeRepository: R;

	#ownerContentTypeId?: string;
	#contentTypeObservers = new Array<UmbController>();
	#contentTypes = new UmbArrayState<T>([], (x) => x.id);
	readonly contentTypes = this.#contentTypes.asObservable();
	private readonly _contentTypeContainers = this.#contentTypes.asObservablePart((x) =>
		x.flatMap((x) => x.containers ?? []),
	);

	#containers: UmbArrayState<PropertyTypeContainerModelBaseModel> =
		new UmbArrayState<PropertyTypeContainerModelBaseModel>([], (x) => x.id);

	constructor(host: UmbControllerHostElement, typeRepository: R) {
		this.#host = host;
		this.#contentTypeRepository = typeRepository;

		new UmbObserverController(host, this.contentTypes, (contentTypes) => {
			contentTypes.forEach((contentType) => {
				this._loadContentTypeCompositions(contentType);
			});
		});
		new UmbObserverController(host, this._contentTypeContainers, (contentTypeContainers) => {
			this.#containers.next(contentTypeContainers);
		});
	}

	/**
	 * loadType will load the node type and all inherited and composed types.
	 * This will give us all the structure for properties and containers.
	 */
	public async loadType(id?: string) {
		this._reset();

		this.#ownerContentTypeId = id;

		const promiseResult = this._loadType(id);
		this.#init = promiseResult;
		await this.#init;
		return promiseResult;
	}

	public async createScaffold(parentId: string | null) {
		this._reset();

		if (parentId === undefined) return {};

		const { data } = await this.#contentTypeRepository.createScaffold(parentId);
		if (!data) return {};

		this.#ownerContentTypeId = data.id;

		this.#init = this._observeContentType(data);
		await this.#init;
		return { data };
	}

	public async save() {
		const contentType = this.getOwnerContentType();
		if (!contentType || !contentType.id) return false;

		await this.#contentTypeRepository.save(contentType.id, contentType);

		return true;
	}

	public async create() {
		const contentType = this.getOwnerContentType();
		if (!contentType || !contentType.id) return false;

		const { data } = await this.#contentTypeRepository.create(contentType);
		if (!data) return false;

		await this.loadType(data.id);

		return true;
	}

	private async _ensureType(id?: string) {
		if (!id) return;
		if (this.#contentTypes.getValue().find((x) => x.id === id)) return;
		await this._loadType(id);
	}

	private async _loadType(id?: string) {
		if (!id) return {};

		const { data } = await this.#contentTypeRepository.requestById(id);
		if (!data) return {};

		await this._observeContentType(data);
		return { data };
	}

	public async _observeContentType(data: T) {
		if (!data.id) return;

		// Load inherited and composed types:
		this._loadContentTypeCompositions(data);

		this.#contentTypeObservers.push(
			new UmbObserverController(this.#host, await this.#contentTypeRepository.byId(data.id), (docType) => {
				if (docType) {
					// TODO: Handle if there was changes made to the owner document type in this context.
					/*
					possible easy solutions could be to notify user wether they want to update(Discard the changes to accept the new ones).
					 */
					this.#contentTypes.appendOne(docType);
				}
			}),
		);
	}

	private async _loadContentTypeCompositions(contentType: T) {
		contentType.compositions?.forEach((composition) => {
			this._ensureType(composition.id);
		});
	}

	/** Public methods for consuming structure: */

	ownerContentType() {
		return this.#contentTypes.asObservablePart((x) => x.find((y) => y.id === this.#ownerContentTypeId));
	}

	getOwnerContentType() {
		return this.#contentTypes.getValue().find((y) => y.id === this.#ownerContentTypeId);
	}

	updateOwnerContentType(entry: T) {
		this.#contentTypes.updateOne(this.#ownerContentTypeId, entry);
	}

	// We could move the actions to another class?

	async createContainer(
		contentTypeId: string | null,
		parentId: string | null = null,
		type: PropertyContainerTypes = 'Group',
		sortOrder?: number,
	) {
		await this.#init;
		contentTypeId = contentTypeId ?? this.#ownerContentTypeId!;

		const container: PropertyTypeContainerModelBaseModel = {
			id: UmbId.new(),
			parentId: parentId ?? null,
			name: '',
			type: type,
			sortOrder: sortOrder ?? 0,
		};

		const containers = [...(this.#contentTypes.getValue().find((x) => x.id === contentTypeId)?.containers ?? [])];
		containers.push(container);

		this.#contentTypes.updateOne(contentTypeId, { containers });

		return container;
	}

	makeContainerNameUniqueForOwnerContentType(
		newName: string,
		containerType: PropertyContainerTypes = 'Tab',
		parentId: string | null = null,
	) {
		const ownerRootContainers = this.getOwnerContainers(containerType); //getRootContainers() can't differentiates between compositions and locals

		let changedName = newName;
		if (ownerRootContainers) {
			while (ownerRootContainers.find((tab) => tab.name === changedName && tab.id !== parentId)) {
				changedName = incrementString(changedName);
			}

			return changedName === newName ? null : changedName;
		}
		return null;
	}

	async updateContainer(
		contentTypeId: string | null,
		containerId: string,
		partialUpdate: Partial<PropertyTypeContainerModelBaseModel>,
	) {
		await this.#init;
		contentTypeId = contentTypeId ?? this.#ownerContentTypeId!;

		const frozenContainers = this.#contentTypes.getValue().find((x) => x.id === contentTypeId)?.containers ?? [];

		const containers = partialUpdateFrozenArray(frozenContainers, partialUpdate, (x) => x.id === containerId);

		this.#contentTypes.updateOne(contentTypeId, { containers });
	}

	async removeContainer(contentTypeId: string | null, containerId: string | null = null) {
		await this.#init;
		contentTypeId = contentTypeId ?? this.#ownerContentTypeId!;

		const frozenContainers = this.#contentTypes.getValue().find((x) => x.id === contentTypeId)?.containers ?? [];
		const containers = frozenContainers.filter((x) => x.id !== containerId);

		this.#contentTypes.updateOne(contentTypeId, { containers });
	}

	createPropertyScaffold(containerId: string | null = null, sortOrder?: number) {
		const property: PropertyTypeModelBaseModel = {
			id: UmbId.new(),
			containerId: containerId,
			alias: '',
			name: '',
			description: '',
			dataTypeId: '',
			variesByCulture: false,
			variesBySegment: false,
			validation: {
				mandatory: false,
				mandatoryMessage: null,
				regEx: null,
				regExMessage: null,
			},
			appearance: {
				labelOnTop: false,
			},
			sortOrder: sortOrder ?? 0,
		} as any; // Sort order was not allowed when this was written.

		return property;
	}

	async createProperty(contentTypeId: string | null, containerId: string | null = null, sortOrder?: number) {
		await this.#init;
		contentTypeId = contentTypeId ?? this.#ownerContentTypeId!;

		const property: PropertyTypeModelBaseModel = this.createPropertyScaffold(containerId, sortOrder);

		const properties = [...(this.#contentTypes.getValue().find((x) => x.id === contentTypeId)?.properties ?? [])];
		properties.push(property);

		this.#contentTypes.updateOne(contentTypeId, { properties });

		return property;
	}

	async insertProperty(contentTypeId: string | null, property: PropertyTypeModelBaseModel) {
		await this.#init;
		contentTypeId = contentTypeId ?? this.#ownerContentTypeId!;

		const frozenProperties = this.#contentTypes.getValue().find((x) => x.id === contentTypeId)?.properties ?? [];

		const properties = appendToFrozenArray(frozenProperties, property, (x) => x.id === property.id);

		this.#contentTypes.updateOne(contentTypeId, { properties });
	}

	async removeProperty(contentTypeId: string | null, propertyId: string) {
		await this.#init;
		contentTypeId = contentTypeId ?? this.#ownerContentTypeId!;

		const frozenProperties = this.#contentTypes.getValue().find((x) => x.id === contentTypeId)?.properties ?? [];

		const properties = filterFrozenArray(frozenProperties, (x) => x.id !== propertyId);

		this.#contentTypes.updateOne(contentTypeId, { properties });
	}

	async updateProperty(
		contentTypeId: string | null,
		propertyId: string,
		partialUpdate: Partial<PropertyTypeModelBaseModel>,
	) {
		await this.#init;
		contentTypeId = contentTypeId ?? this.#ownerContentTypeId!;

		const frozenProperties = this.#contentTypes.getValue().find((x) => x.id === contentTypeId)?.properties ?? [];

		const properties = partialUpdateFrozenArray(frozenProperties, partialUpdate, (x) => x.id === propertyId);

		this.#contentTypes.updateOne(contentTypeId, { properties });
	}

	// TODO: Refactor: These property methods, should maybe be named without structure in their name.
	async propertyStructureById(propertyId: string) {
		await this.#init;
		return this.#contentTypes.asObservablePart((docTypes) => {
			for (const docType of docTypes) {
				const foundProp = docType.properties?.find((property) => property.id === propertyId);
				if (foundProp) {
					return foundProp;
				}
			}
			return undefined;
		});
	}
	async propertyStructureByAlias(propertyAlias: string) {
		await this.#init;
		return this.#contentTypes.asObservablePart((docTypes) => {
			for (const docType of docTypes) {
				const foundProp = docType.properties?.find((property) => property.alias === propertyAlias);
				if (foundProp) {
					return foundProp;
				}
			}
			return undefined;
		});
	}

	async getPropertyStructureById(propertyId: string) {
		await this.#init;
		for (const docType of this.#contentTypes.getValue()) {
			const foundProp = docType.properties?.find((property) => property.id === propertyId);
			if (foundProp) {
				return foundProp;
			}
		}
		return undefined;
	}

	async getPropertyStructureByAlias(propertyAlias: string) {
		await this.#init;
		for (const docType of this.#contentTypes.getValue()) {
			const foundProp = docType.properties?.find((property) => property.alias === propertyAlias);
			if (foundProp) {
				return foundProp;
			}
		}
		return undefined;
	}

	ownerContentTypeObservablePart<PartResult>(mappingFunction: MappingFunction<T, PartResult>) {
		return this.#contentTypes.asObservablePart((docTypes) => {
			const docType = docTypes.find((x) => x.id === this.#ownerContentTypeId);
			return docType ? mappingFunction(docType) : undefined;
		});
	}

	hasPropertyStructuresOf(containerId: string | null) {
		return this.#contentTypes.asObservablePart((docTypes) => {
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
		return this.#contentTypes.asObservablePart((docTypes) => {
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
		return this.#containers.asObservablePart((data) => {
			return data.filter((x) => x.parentId === null && x.type === containerType);
		});
	}

	getRootContainers(containerType: PropertyContainerTypes) {
		return this.#containers.getValue().filter((x) => x.parentId === null && x.type === containerType);
	}

	hasRootContainers(containerType: PropertyContainerTypes) {
		return this.#containers.asObservablePart((data) => {
			return data.filter((x) => x.parentId === null && x.type === containerType).length > 0;
		});
	}

	ownerContainersOf(containerType: PropertyContainerTypes) {
		return this.ownerContentTypeObservablePart((x) => x.containers?.filter((x) => x.type === containerType) ?? []);
	}

	getOwnerContainers(containerType: PropertyContainerTypes, parentId: string | null = null) {
		return this.getOwnerContentType()?.containers?.filter((x) => x.parentId === parentId && x.type === containerType);
	}

	isOwnerContainer(containerId: string) {
		return this.getOwnerContentType()?.containers?.filter((x) => x.id === containerId);
	}

	containersOfParentKey(
		parentId: PropertyTypeContainerModelBaseModel['parentId'],
		containerType: PropertyContainerTypes,
	) {
		return this.#containers.asObservablePart((data) => {
			return data.filter((x) => x.parentId === parentId && x.type === containerType);
		});
	}

	// In future this might need to take parentName(parentId lookup) into account as well? otherwise containers that share same name and type will always be merged, but their position might be different and they should not be merged.
	containersByNameAndType(name: string, containerType: PropertyContainerTypes) {
		return this.#containers.asObservablePart((data) => {
			return data.filter((x) => x.name === name && x.type === containerType);
		});
	}

	private _reset() {
		this.#contentTypeObservers.forEach((observer) => observer.destroy());
		this.#contentTypeObservers = [];
		this.#contentTypes.next([]);
		this.#containers.next([]);
	}
	public destroy() {
		this._reset();
		this.#contentTypes.complete();
		this.#containers.complete();
	}
}
