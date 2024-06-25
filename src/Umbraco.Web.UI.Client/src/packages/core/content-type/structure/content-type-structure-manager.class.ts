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

type UmbPropertyTypeId = UmbPropertyTypeModel['id'];

/**
 * Manages a structure of a Content Type and its properties and containers.
 * This loads and merges the structures of the Content Type and its inherited and composed Content Types.
 * To help manage the data, there is two helper classes:
 * - {@link UmbContentTypePropertyStructureHelper} for managing the structure of properties, optional of another container or root.
 * - {@link UmbContentTypeContainerStructureHelper} for managing the structure of containers, optional of another container or root.
 */
export class UmbContentTypeStructureManager<
	T extends UmbContentTypeModel = UmbContentTypeModel,
> extends UmbControllerBase {
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
	readonly contentTypeUniques = this.#contentTypes.asObservablePart((x) => x.map((y) => y.unique));
	readonly contentTypeAliases = this.#contentTypes.asObservablePart((x) => x.map((y) => y.alias));

	#containers: UmbArrayState<UmbPropertyTypeContainerModel> = new UmbArrayState<UmbPropertyTypeContainerModel>(
		[],
		(x) => x.id,
	);
	containerById(id: string) {
		return this.#containers.asObservablePart((x) => x.find((y) => y.id === id));
	}

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
		if (!contentType || !contentType.unique) throw new Error('Could not find the Content Type to save');

		const { error, data } = await this.#repository.save(contentType);
		if (error || !data) return { error, data };

		// Update state with latest version:
		this.#contentTypes.updateOne(contentType.unique, data);

		return { error, data };
	}

	/**
	 * Create the owner content type. Notice this is for a Content Type that is NOT already stored on the server.
	 * @returns boolean
	 */
	public async create(parentUnique: string | null) {
		const contentType = this.getOwnerContentType();
		if (!contentType || !contentType.unique) {
			throw new Error('Could not find the Content Type to create');
		}

		const { data } = await this.#repository.create(contentType, parentUnique);
		if (!data) return Promise.reject();

		// Update state with latest version:
		this.#contentTypes.updateOne(contentType.unique, data);

		// Start observe the new content type in the store, as we did not do that when it was a scaffold/local-version.
		this._observeContentType(data);
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
		const { data, asObservable } = await this.#repository.requestByUnique(unique);
		if (!data) return {};

		await this._observeContentType(data);
		return { data, asObservable };
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

	getOwnerContentTypeUnique() {
		return this.#ownerContentTypeUnique;
	}

	updateOwnerContentType(entry: Partial<T>) {
		this.#contentTypes.updateOne(this.#ownerContentTypeUnique, entry);
	}

	getContentTypes() {
		return this.#contentTypes.getValue();
	}
	getContentTypeUniques() {
		return this.#contentTypes.getValue().map((x) => x.unique);
	}
	getContentTypeAliases() {
		return this.#contentTypes.getValue().map((x) => x.alias);
	}

	// TODO: We could move the actions to another class?

	/**
	 * Ensure a container exists for a specific Content Type. Otherwise clone it.
	 * @param containerId - The container to ensure exists on the given ContentType.
	 * @param contentTypeUnique - The content type to ensure the container for.
	 * @returns Promise<UmbPropertyTypeContainerModel | undefined>
	 */
	async ensureContainerOf(
		containerId: string,
		contentTypeUnique: string,
	): Promise<UmbPropertyTypeContainerModel | undefined> {
		await this.#init;
		const contentType = this.#contentTypes.getValue().find((x) => x.unique === contentTypeUnique);
		if (!contentType) {
			throw new Error('Could not find the Content Type to ensure containers for');
		}
		const containers = contentType?.containers;
		const container = containers?.find((x) => x.id === containerId);
		if (!container) {
			return this.cloneContainerTo(containerId, contentTypeUnique);
		}
		return container;
	}

	/**
	 * Clone a container to a specific Content Type.
	 * @param containerId - The container to clone, assuming it does not already exist on the given Content Type.
	 * @param toContentTypeUnique - The content type to clone to.
	 * @returns Promise<UmbPropertyTypeContainerModel | undefined>
	 */
	async cloneContainerTo(
		containerId: string,
		toContentTypeUnique?: string,
	): Promise<UmbPropertyTypeContainerModel | undefined> {
		await this.#init;
		toContentTypeUnique = toContentTypeUnique ?? this.#ownerContentTypeUnique!;

		// Find container.
		const container = this.#containers.getValue().find((x) => x.id === containerId);
		if (!container) throw new Error('Container to clone was not found');

		const clonedContainer: UmbPropertyTypeContainerModel = {
			...container,
			id: UmbId.new(),
		};
		if (container.parent) {
			// Investigate parent container. (See if we have one that matches if not, then clone it.)
			const parentContainer = await this.ensureContainerOf(container.parent.id, toContentTypeUnique);
			if (!parentContainer) {
				throw new Error('Parent container for cloning could not be found or created');
			}
			// Clone container.
			clonedContainer.parent = { id: parentContainer.id };
		}
		// Spread containers, so we can append to it, and then update the specific content-type with the new set of containers: [NL]
		// Correction the spread is removed now, cause we do a filter: [NL]
		// And then we remove the existing one, to have the more local one replacing it. [NL]
		const containers = [
			...(this.#contentTypes.getValue().find((x) => x.unique === toContentTypeUnique)?.containers ?? []),
		];
		//.filter((x) => x.name !== clonedContainer.name && x.type === clonedContainer.type);
		containers.push(clonedContainer);

		// eslint-disable-next-line @typescript-eslint/ban-ts-comment
		// @ts-ignore
		// TODO: fix TS partial complaint [NL]
		this.#contentTypes.updateOne(toContentTypeUnique, { containers });

		return clonedContainer;
	}

	ensureContainerNames(
		contentTypeUnique: string | null,
		type: UmbPropertyContainerTypes,
		parentId: string | null = null,
	) {
		contentTypeUnique = contentTypeUnique ?? this.#ownerContentTypeUnique!;
		this.getOwnerContainers(type, parentId)?.forEach((container) => {
			if (container.name === '') {
				const newName = 'Unnamed';
				this.updateContainer(null, container.id, {
					name: this.makeContainerNameUniqueForOwnerContentType(container.id, newName, type, parentId) ?? newName,
				});
			}
		});
	}

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

		// Ensure
		this.ensureContainerNames(contentTypeUnique, type, parentId);

		const contentTypes = this.#contentTypes.getValue();
		const containers = [...(contentTypes.find((x) => x.unique === contentTypeUnique)?.containers ?? [])];
		containers.push(container);

		// eslint-disable-next-line @typescript-eslint/ban-ts-comment
		// @ts-ignore
		// TODO: fix TS partial complaint
		this.#contentTypes.updateOne(contentTypeUnique, { containers });

		return container;
	}

	/*async insertContainer(contentTypeUnique: string | null, container: UmbPropertyTypeContainerModel) {
		await this.#init;
		contentTypeUnique = contentTypeUnique ?? this.#ownerContentTypeUnique!;

		// If we have a parent, we need to ensure it exists, and then update the parent property with the new container id.
		if (container.parent) {
			const parentContainer = await this.ensureContainerOf(container.parent.id, contentTypeUnique);
			if (!parentContainer) {
				throw new Error('Container for inserting property could not be found or created');
			}
			container.parent.id = parentContainer.id;
		}

		const frozenContainers =
			this.#contentTypes.getValue().find((x) => x.unique === contentTypeUnique)?.containers ?? [];

		const containers = appendToFrozenArray(frozenContainers, container, (x) => x.id === container.id);

		// eslint-disable-next-line @typescript-eslint/ban-ts-comment
		// @ts-ignore
		// TODO: fix TS partial complaint
		this.#contentTypes.updateOne(contentTypeUnique, { containers });
	}*/

	makeEmptyContainerName(
		containerId: string,
		containerType: UmbPropertyContainerTypes,
		parentId: string | null = null,
	) {
		return (
			this.makeContainerNameUniqueForOwnerContentType(containerId, 'Unnamed', containerType, parentId) ?? 'Unnamed'
		);
	}
	makeContainerNameUniqueForOwnerContentType(
		containerId: string,
		newName: string,
		containerType: UmbPropertyContainerTypes,
		parentId: string | null = null,
	) {
		const ownerRootContainers = this.getOwnerContainers(containerType, parentId); //getRootContainers() can't differentiates between compositions and locals
		if (!ownerRootContainers) {
			return null;
		}

		let changedName = newName;
		while (ownerRootContainers.find((con) => con.name === changedName && con.id !== containerId)) {
			changedName = incrementString(changedName);
		}

		return changedName === newName ? null : changedName;
	}

	async updateContainer(
		contentTypeUnique: string | null,
		containerId: string,
		partialUpdate: Partial<UmbPropertyTypeContainerModel>,
	) {
		await this.#init;
		contentTypeUnique = contentTypeUnique ?? this.#ownerContentTypeUnique!;

		/*
		// If we have a container, we need to ensure it exists, and then update the container with the new parent id.
		if (containerId) {
			const container = await this.ensureContainerOf(containerId, contentTypeUnique);
			if (!container) {
				throw new Error('Container for inserting property could not be found or created');
			}
			// Correct containerId to the local one: [NL]
			containerId = container.id;
		}
		*/

		const frozenContainers =
			this.#contentTypes.getValue().find((x) => x.unique === contentTypeUnique)?.containers ?? [];

		const ownerContainer = frozenContainers.find((x) => x.id === containerId);
		if (!ownerContainer) {
			console.error(
				'We do not have this container on the requested id, we should clone the container and append the change to it. [NL]',
			);
		}

		const containers = partialUpdateFrozenArray(frozenContainers, partialUpdate, (x) => x.id === containerId);

		// eslint-disable-next-line @typescript-eslint/ban-ts-comment
		// @ts-ignore
		// TODO: fix TS partial complaint
		this.#contentTypes.updateOne(contentTypeUnique, { containers });
	}

	async removeContainer(contentTypeUnique: string | null, containerId: string | null = null) {
		await this.#init;
		contentTypeUnique = contentTypeUnique ?? this.#ownerContentTypeUnique!;

		const contentType = this.#contentTypes.getValue().find((x) => x.unique === contentTypeUnique);
		if (!contentType) {
			throw new Error('Could not find the Content Type to remove container from');
		}
		const frozenContainers = contentType.containers ?? [];
		const removedContainerIds = frozenContainers
			.filter((x) => x.id === containerId || x.parent?.id === containerId)
			.map((x) => x.id);
		const containers = frozenContainers.filter((x) => x.id !== containerId && x.parent?.id !== containerId);

		const frozenProperties = contentType.properties;
		const properties = frozenProperties.filter((x) =>
			x.container ? !removedContainerIds.some((ids) => ids === x.container?.id) : true,
		);

		// eslint-disable-next-line @typescript-eslint/ban-ts-comment
		// @ts-ignore
		// TODO: fix TS partial complaint
		this.#contentTypes.updateOne(contentTypeUnique, { containers, properties });
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

		// If we have a container, we need to ensure it exists, and then update the container with the new parent id. [NL]
		if (containerId) {
			const container = await this.ensureContainerOf(containerId, contentTypeUnique);
			if (!container) {
				throw new Error('Container for inserting property could not be found or created');
			}
			// Correct containerId to the local one: [NL]
			containerId = container.id;
		}

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

		// If we have a container, we need to ensure it exists, and then update the container with the new parent id. [NL]
		if (property.container) {
			const container = await this.ensureContainerOf(property.container.id, contentTypeUnique);
			if (!container) {
				throw new Error('Container for inserting property could not be found or created');
			}
			// Unfreeze object, while settings container.id
			property = { ...property, container: { id: container.id } };
		}

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

	ownerContainersOf(containerType: UmbPropertyContainerTypes, parentId: string | null) {
		return this.ownerContentTypeObservablePart(
			(x) =>
				x.containers?.filter(
					(x) => (parentId ? x.parent?.id === parentId : x.parent === null) && x.type === containerType,
				) ?? [],
		);
	}

	getOwnerContainers(containerType: UmbPropertyContainerTypes, parentId: string | null) {
		return this.getOwnerContentType()?.containers?.filter(
			(x) => (parentId ? x.parent?.id === parentId : x.parent === null) && x.type === containerType,
		);
	}

	isOwnerContainer(containerId: string) {
		return this.getOwnerContentType()?.containers?.filter((x) => x.id === containerId);
	}

	containersOfParentId(parentId: string, containerType: UmbPropertyContainerTypes) {
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

	containersByNameAndTypeAndParent(
		name: string,
		containerType: UmbPropertyContainerTypes,
		parentName: string | null,
		parentType?: UmbPropertyContainerTypes,
	) {
		return this.#containers.asObservablePart((data) => {
			return data.filter(
				(x) =>
					// Match name and type:
					x.name === name &&
					x.type === containerType &&
					// If we look for a parent name, then we need to match that as well:
					(parentName !== null
						? // And we have a parent on this container, then we need to match the parent name and type as well
							x.parent
							? data.some((y) => x.parent!.id === y.id && y.name === parentName && y.type === parentType)
							: false
						: // if we do not have a parent then its not a match
							x.parent === null), // it parentName === null then we expect the container parent to be null.
			);
		});
	}

	getContentTypeOfContainer(containerId: string) {
		return this.#contentTypes
			.getValue()
			.find((contentType) => contentType.containers.some((c) => c.id === containerId));
	}

	contentTypeOfProperty(propertyId: UmbPropertyTypeId) {
		return this.#contentTypes.asObservablePart((contentTypes) =>
			contentTypes.find((contentType) => contentType.properties.some((p) => p.id === propertyId)),
		);
	}

	private _reset() {
		this.#contentTypeObservers.forEach((observer) => observer.destroy());
		this.#contentTypeObservers = [];
		this.#contentTypes.setValue([]);
		this.#containers.setValue([]);
	}
	public override destroy() {
		this.#contentTypes.destroy();
		this.#containers.destroy();
		super.destroy();
	}
}
