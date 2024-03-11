import type {
	UmbContentTypeModel,
	UmbPropertyContainerTypes,
	UmbPropertyTypeContainerModel,
	UmbPropertyTypeModel,
	UmbPropertyTypeScaffoldModel,
} from '../types.js';
import type { UmbDetailRepository } from '@umbraco-cms/backoffice/repository';
import { UmbId } from '@umbraco-cms/backoffice/id';
import type { UmbControllerHost, UmbController } from '@umbraco-cms/backoffice/controller-api';
import type { MappingFunction } from '@umbraco-cms/backoffice/observable-api';
import {
	UmbArrayState,
	partialUpdateFrozenArray,
	appendToFrozenArray,
	filterFrozenArray,
	createObservablePart,
} from '@umbraco-cms/backoffice/observable-api';
import { incrementString } from '@umbraco-cms/backoffice/utils';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';

/**
 * Manages a structure of a Content Type and its properties and containers.
 * This loads and merges the structures of the Content Type and its inherited and composed Content Types.
 * To help manage the data, there is two helper classes:
 * - {@link UmbContentTypePropertyStructureHelper} for managing the structure of properties, optional of another container or root.
 * - {@link UmbContentTypeContainerStructureHelper} for managing the structure of containers, optional of another container or root.
 */
export class UmbContentTypePropertyStructureManager<T extends UmbContentTypeModel> extends UmbControllerBase {
	#init!: Promise<unknown>;

	#repository: UmbDetailRepository<T>;

	#ownerContentTypeUnique?: string;
	#contentTypeObservers = new Array<UmbController>();

	#contentTypes = new UmbArrayState<T>([], (x) => x.unique);
	readonly contentTypes = this.#contentTypes.asObservable();
	readonly ownerContentType = this.#contentTypes.asObservablePart((x) =>
		x.find((y) => y.unique === this.#ownerContentTypeUnique),
	);
	private readonly _contentTypeContainers = this.#contentTypes.asObservablePart((x) =>
		x.flatMap((x) => x.containers ?? []),
	);

	#containers: UmbArrayState<UmbPropertyTypeContainerModel> = new UmbArrayState<UmbPropertyTypeContainerModel>(
		[],
		(x) => x.id,
	);

	constructor(host: UmbControllerHost, typeRepository: UmbDetailRepository<T>) {
		super(host);
		this.#repository = typeRepository;

		this.observe(this.contentTypes, (contentTypes) => {
			contentTypes.forEach((contentType) => {
				this._loadContentTypeCompositions(contentType);
			});
		});
		this.observe(this._contentTypeContainers, (contentTypeContainers) => {
			this.#containers.setValue(contentTypeContainers);
		});
	}

	/**
	 * loadType will load the ContentType and all inherited and composed ContentTypes.
	 * This will give us all the structure for properties and containers.
	 */
	public async loadType(unique?: string) {
		this._reset();

		this.#ownerContentTypeUnique = unique;

		const promise = this._loadType(unique);
		this.#init = promise;
		await this.#init;
		return promise;
	}

	public async createScaffold() {
		this._reset();

		const { data } = await this.#repository.createScaffold();
		if (!data) return {};

		this.#ownerContentTypeUnique = data.unique;

		// Add the new content type to the list of content types, this holds our draft state of this scaffold.
		this.#contentTypes.appendOne(data);
		return { data };
	}

	/**
	 * Save the owner content type. Notice this is for a Content Type that is already stored on the server.
	 * @returns boolean
	 */
	public async save() {
		const contentType = this.getOwnerContentType();
		if (!contentType || !contentType.unique) return false;

		const { data } = await this.#repository.save(contentType);
		if (!data) return false;

		// Update state with latest version:
		this.#contentTypes.updateOne(contentType.unique, data);

		return true;
	}

	/**
	 * Create the owner content type. Notice this is for a Content Type that is NOT already stored on the server.
	 * @returns boolean
	 */
	public async create(parentUnique: string | null) {
		const contentType = this.getOwnerContentType();
		if (!contentType || !contentType.unique) return false;

		const { data } = await this.#repository.create(contentType, parentUnique);
		if (!data) return false;

		// Update state with latest version:
		this.#contentTypes.updateOne(contentType.unique, data);

		// Start observe the new content type in the store, as we did not do that when it was a scaffold/local-version.
		this._observeContentType(data);

		return true;
	}

	private async _loadContentTypeCompositions(contentType: T) {
		contentType.compositions?.forEach((composition) => {
			this._ensureType(composition.contentType.unique);
		});
	}

	private async _ensureType(unique?: string) {
		if (!unique) return;
		if (this.#contentTypes.getValue().find((x) => x.unique === unique)) return;
		await this._loadType(unique);
	}

	private async _loadType(unique?: string) {
		if (!unique) return {};

		// Lets initiate the content type:
		const { data } = await this.#repository.requestByUnique(unique);
		if (!data) return {};

		await this._observeContentType(data);
		return { data };
	}

	private async _observeContentType(data: T) {
		if (!data.unique) return;

		// Notice we do not store the content type in the store here, cause it will happen shortly after when the observations gets its first initial callback. [NL]

		// Load inherited and composed types:
		//this._loadContentTypeCompositions(data);// Should not be necessary as this will be done when appended to the contentTypes state. [NL]

		const ctrl = this.observe(
			// Then lets start observation of the content type:
			await this.#repository.byUnique(data.unique),
			(docType) => {
				if (docType) {
					// TODO: Handle if there was changes made to the owner document type in this context. [NL]
					/*
					possible easy solutions could be to notify user wether they want to update(Discard the changes to accept the new ones). [NL]
					 */
					this.#contentTypes.appendOne(docType);
				}
				// TODO: Do we need to handle the undefined case? [NL]
			},
			'observeContentType_' + data.unique,
		);

		this.#contentTypeObservers.push(ctrl);
	}

	/** Public methods for consuming structure: */

	ownerContentTypePart<R>(mappingFunction: MappingFunction<T | undefined, R>) {
		return createObservablePart(this.ownerContentType, mappingFunction);
	}

	getOwnerContentType() {
		return this.#contentTypes.getValue().find((y) => y.unique === this.#ownerContentTypeUnique);
	}

	updateOwnerContentType(entry: Partial<T>) {
		this.#contentTypes.updateOne(this.#ownerContentTypeUnique, entry);
	}

	// We could move the actions to another class?

	async createContainer(
		contentTypeUnique: string | null,
		parentId: string | null = null,
		type: UmbPropertyContainerTypes = 'Group',
		sortOrder?: number,
	) {
		await this.#init;
		contentTypeUnique = contentTypeUnique ?? this.#ownerContentTypeUnique!;

		const container: UmbPropertyTypeContainerModel = {
			id: UmbId.new(),
			parent: parentId ? { id: parentId } : null,
			name: '',
			type: type,
			sortOrder: sortOrder ?? 0,
		};

		const containers = [
			...(this.#contentTypes.getValue().find((x) => x.unique === contentTypeUnique)?.containers ?? []),
		];
		containers.push(container);

		// eslint-disable-next-line @typescript-eslint/ban-ts-comment
		// @ts-ignore
		// TODO: fix TS partial complaint
		this.#contentTypes.updateOne(contentTypeUnique, { containers });

		return container;
	}

	async insertContainer(contentTypeUnique: string | null, container: UmbPropertyTypeContainerModel) {
		await this.#init;
		contentTypeUnique = contentTypeUnique ?? this.#ownerContentTypeUnique!;

		const frozenContainers =
			this.#contentTypes.getValue().find((x) => x.unique === contentTypeUnique)?.containers ?? [];

		const containers = appendToFrozenArray(frozenContainers, container, (x) => x.id === container.id);

		console.log(frozenContainers, containers);
		// eslint-disable-next-line @typescript-eslint/ban-ts-comment
		// @ts-ignore
		// TODO: fix TS partial complaint
		this.#contentTypes.updateOne(contentTypeUnique, { containers });
	}

	makeContainerNameUniqueForOwnerContentType(
		newName: string,
		containerType: UmbPropertyContainerTypes = 'Tab',
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
		contentTypeUnique: string | null,
		containerId: string,
		partialUpdate: Partial<UmbPropertyTypeContainerModel>,
	) {
		await this.#init;
		contentTypeUnique = contentTypeUnique ?? this.#ownerContentTypeUnique!;

		const frozenContainers =
			this.#contentTypes.getValue().find((x) => x.unique === contentTypeUnique)?.containers ?? [];

		const containers = partialUpdateFrozenArray(frozenContainers, partialUpdate, (x) => x.id === containerId);

		// eslint-disable-next-line @typescript-eslint/ban-ts-comment
		// @ts-ignore
		// TODO: fix TS partial complaint
		this.#contentTypes.updateOne(contentTypeUnique, { containers });
	}

	async removeContainer(contentTypeUnique: string | null, containerId: string | null = null) {
		await this.#init;
		contentTypeUnique = contentTypeUnique ?? this.#ownerContentTypeUnique!;

		const frozenContainers =
			this.#contentTypes.getValue().find((x) => x.unique === contentTypeUnique)?.containers ?? [];
		const containers = frozenContainers.filter((x) => x.id !== containerId);

		// eslint-disable-next-line @typescript-eslint/ban-ts-comment
		// @ts-ignore
		// TODO: fix TS partial complaint
		this.#contentTypes.updateOne(contentTypeUnique, { containers });
	}

	createPropertyScaffold(containerId: string | null = null) {
		const property: UmbPropertyTypeScaffoldModel = {
			id: UmbId.new(),
			container: containerId ? { id: containerId } : null,
			alias: '',
			name: '',
			description: '',
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
			sortOrder: 0,
		};

		return property;
	}

	async createProperty(contentTypeUnique: string | null, containerId: string | null = null, sortOrder?: number) {
		await this.#init;
		contentTypeUnique = contentTypeUnique ?? this.#ownerContentTypeUnique!;

		const property = this.createPropertyScaffold(containerId);
		property.sortOrder = sortOrder ?? 0;

		const properties: Array<UmbPropertyTypeScaffoldModel | UmbPropertyTypeModel> = [
			...(this.#contentTypes.getValue().find((x) => x.unique === contentTypeUnique)?.properties ?? []),
		];

		properties.push(property);

		// eslint-disable-next-line @typescript-eslint/ban-ts-comment
		// @ts-ignore
		// TODO: fix TS partial complaint
		this.#contentTypes.updateOne(contentTypeUnique, { properties });

		return property;
	}

	async insertProperty(contentTypeUnique: string | null, property: UmbPropertyTypeModel) {
		await this.#init;
		contentTypeUnique = contentTypeUnique ?? this.#ownerContentTypeUnique!;

		const frozenProperties =
			this.#contentTypes.getValue().find((x) => x.unique === contentTypeUnique)?.properties ?? [];

		const properties = appendToFrozenArray(frozenProperties, property, (x) => x.id === property.id);

		// eslint-disable-next-line @typescript-eslint/ban-ts-comment
		// @ts-ignore
		// TODO: fix TS partial complaint
		this.#contentTypes.updateOne(contentTypeUnique, { properties });
	}

	async removeProperty(contentTypeUnique: string | null, propertyId: string) {
		await this.#init;
		contentTypeUnique = contentTypeUnique ?? this.#ownerContentTypeUnique!;

		const frozenProperties =
			this.#contentTypes.getValue().find((x) => x.unique === contentTypeUnique)?.properties ?? [];

		const properties = filterFrozenArray(frozenProperties, (x) => x.id !== propertyId);

		// eslint-disable-next-line @typescript-eslint/ban-ts-comment
		// @ts-ignore
		// TODO: fix TS partial complaint
		this.#contentTypes.updateOne(contentTypeUnique, { properties });
	}

	async updateProperty(
		contentTypeUnique: string | null,
		propertyId: string,
		partialUpdate: Partial<UmbPropertyTypeModel>,
	) {
		await this.#init;
		contentTypeUnique = contentTypeUnique ?? this.#ownerContentTypeUnique!;

		const frozenProperties =
			this.#contentTypes.getValue().find((x) => x.unique === contentTypeUnique)?.properties ?? [];

		const properties = partialUpdateFrozenArray(frozenProperties, partialUpdate, (x) => x.id === propertyId);

		// eslint-disable-next-line @typescript-eslint/ban-ts-comment
		// @ts-ignore
		// TODO: fix TS partial complaint
		this.#contentTypes.updateOne(contentTypeUnique, { properties });
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
			const docType = docTypes.find((x) => x.unique === this.#ownerContentTypeUnique);
			return docType ? mappingFunction(docType) : undefined;
		});
	}

	hasPropertyStructuresOf(containerId: string | null) {
		return this.#contentTypes.asObservablePart((docTypes) => {
			return (
				docTypes.find((docType) => {
					return docType.properties?.find((property) => property.container?.id === containerId);
				}) !== undefined
			);
		});
	}

	rootPropertyStructures() {
		return this.propertyStructuresOf(null);
	}

	propertyStructuresOf(containerId: string | null) {
		return this.#contentTypes.asObservablePart((docTypes) => {
			const props: UmbPropertyTypeModel[] = [];
			docTypes.forEach((docType) => {
				docType.properties?.forEach((property) => {
					if (property.container?.id === containerId) {
						props.push(property);
					}
				});
			});
			return props;
		});
	}

	rootContainers(containerType: UmbPropertyContainerTypes) {
		return this.#containers.asObservablePart((data) => {
			return data.filter((x) => x.parent === null && x.type === containerType);
		});
	}

	getRootContainers(containerType: UmbPropertyContainerTypes) {
		return this.#containers.getValue().filter((x) => x.parent === null && x.type === containerType);
	}

	hasRootContainers(containerType: UmbPropertyContainerTypes) {
		return this.#containers.asObservablePart((data) => {
			return data.filter((x) => x.parent === null && x.type === containerType).length > 0;
		});
	}

	ownerContainersOf(containerType: UmbPropertyContainerTypes) {
		return this.ownerContentTypeObservablePart((x) => x.containers?.filter((x) => x.type === containerType) ?? []);
	}

	getOwnerContainers(containerType: UmbPropertyContainerTypes, parentId: string | null = null) {
		return this.getOwnerContentType()?.containers?.filter((x) =>
			parentId ? x.parent?.id === parentId : x.parent === null && x.type === containerType,
		);
	}

	isOwnerContainer(containerId: string) {
		return this.getOwnerContentType()?.containers?.filter((x) => x.id === containerId);
	}

	containersOfParentKey(parentId: string, containerType: UmbPropertyContainerTypes) {
		return this.#containers.asObservablePart((data) => {
			return data.filter((x) => x.parent?.id === parentId && x.type === containerType);
		});
	}

	// In future this might need to take parentName(parentId lookup) into account as well? otherwise containers that share same name and type will always be merged, but their position might be different and they should not be merged. [NL]
	containersByNameAndType(name: string, containerType: UmbPropertyContainerTypes) {
		return this.#containers.asObservablePart((data) => {
			return data.filter((x) => x.name === name && x.type === containerType);
		});
	}

	private _reset() {
		this.#contentTypeObservers.forEach((observer) => observer.destroy());
		this.#contentTypeObservers = [];
		this.#contentTypes.setValue([]);
		this.#containers.setValue([]);
	}
	public destroy() {
		this.#contentTypes.destroy();
		this.#containers.destroy();
		super.destroy();
	}
}
