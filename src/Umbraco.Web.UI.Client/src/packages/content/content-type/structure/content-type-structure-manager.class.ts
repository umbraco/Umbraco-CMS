import type {
	UmbContentTypeModel,
	UmbPropertyContainerTypes,
	UmbPropertyTypeContainerModel,
	UmbPropertyTypeModel,
} from '../types.js';
import {
	UmbRepositoryDetailsManager,
	type UmbDetailRepository,
	type UmbRepositoryResponse,
	type UmbRepositoryResponseWithAsObservable,
} from '@umbraco-cms/backoffice/repository';
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
import { UmbExtensionApiInitializer } from '@umbraco-cms/backoffice/extension-api';
import { umbExtensionsRegistry, type ManifestRepository } from '@umbraco-cms/backoffice/extension-registry';

type UmbPropertyTypeUnique = UmbPropertyTypeModel['unique'];

const UmbFilterDuplicateStrings = (value: string, index: number, array: Array<string>) =>
	array.indexOf(value) === index;

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
	#initResolver?: (result: T) => void;
	#initRejection?: (reason: any) => void;
	#init = new Promise<T>((resolve, reject) => {
		this.#initResolver = resolve;
		this.#initRejection = reject;
	});

	#editedTypes = new UmbArrayState<string, string>([], (x) => x);

	#repository?: UmbDetailRepository<T>;
	#initRepositoryResolver?: (repo: UmbDetailRepository<T>) => void;

	#initRepository = new Promise<UmbDetailRepository<T>>((resolve) => {
		if (this.#repository) {
			resolve(this.#repository);
		} else {
			this.#initRepositoryResolver = resolve;
		}
	});

	#repoManager?: UmbRepositoryDetailsManager<T>;

	async whenLoaded() {
		await this.#init;
		return true;
	}

	#ownerContentTypeUnique?: string;
	#contentTypeObservers = new Array<UmbController>();

	#contentTypes = new UmbArrayState<T>([], (x) => x.unique);
	readonly contentTypes = this.#contentTypes.asObservable();
	readonly ownerContentType = this.#contentTypes.asObservablePart((x) =>
		x.find((y) => y.unique === this.#ownerContentTypeUnique),
	);
	readonly ownerContentTypeAlias = createObservablePart(this.ownerContentType, (x) => x?.alias);
	readonly ownerContentTypeName = createObservablePart(this.ownerContentType, (x) => x?.name);
	readonly ownerContentTypeCompositions = createObservablePart(this.ownerContentType, (x) => x?.compositions);

	readonly contentTypeCompositions = this.#contentTypes.asObservablePart((contentTypes) => {
		return contentTypes.flatMap((x) => x.compositions ?? []);
	});
	async getContentTypeCompositions() {
		return await this.observe(this.contentTypeCompositions).asPromise();
	}
	async getOwnerContentTypeCompositions() {
		return await this.observe(this.ownerContentTypeCompositions).asPromise();
	}
	readonly #contentTypeContainers = this.#contentTypes.asObservablePart((contentTypes) => {
		return contentTypes.flatMap((x) => x.containers ?? []);
	});
	readonly contentTypeProperties = this.#contentTypes.asObservablePart((contentTypes) => {
		return contentTypes.flatMap((x) => x.properties ?? []);
	});
	async getContentTypeProperties() {
		return await this.observe(this.contentTypeProperties).asPromise();
	}
	readonly contentTypeDataTypeUniques = this.#contentTypes.asObservablePart((contentTypes) => {
		return contentTypes
			.flatMap((x) => x.properties?.map((p) => p.dataType.unique) ?? [])
			.filter(UmbFilterDuplicateStrings);
	});
	readonly contentTypeHasProperties = this.#contentTypes.asObservablePart((contentTypes) => {
		return contentTypes.some((x) => x.properties.length > 0);
	});
	readonly contentTypePropertyAliases = createObservablePart(this.contentTypeProperties, (properties) =>
		properties.map((x) => x.alias),
	);
	readonly contentTypeUniques = this.#contentTypes.asObservablePart((x) => x.map((y) => y.unique));
	readonly contentTypeAliases = this.#contentTypes.asObservablePart((x) => x.map((y) => y.alias));

	readonly variesByCulture = createObservablePart(this.ownerContentType, (x) => x?.variesByCulture);
	readonly variesBySegment = createObservablePart(this.ownerContentType, (x) => x?.variesBySegment);

	#containers: UmbArrayState<UmbPropertyTypeContainerModel> = new UmbArrayState<UmbPropertyTypeContainerModel>(
		[],
		(x) => x.id,
	);
	containerById(id: string) {
		return this.#containers.asObservablePart((x) => x.find((y) => y.id === id));
	}

	constructor(host: UmbControllerHost, typeRepository: UmbDetailRepository<T> | string) {
		super(host);

		if (typeof typeRepository === 'string') {
			this.#observeRepository(typeRepository);
		} else {
			this.#repository = typeRepository;
			this.#initRepositoryResolver?.(typeRepository);
		}

		this.#initRepository.then(() => {
			if (!this.#repository) {
				throw new Error(
					'Content Type Structure Manager failed cause it could not initialize or receive the Content Type Detail Repository.',
				);
			}
			this.#repoManager = new UmbRepositoryDetailsManager(this, typeRepository);
			this.observe(
				this.#repoManager.entries,
				(entries) => {
					// Prevent updating once that are have edited here.
					entries = entries.filter(
						(x) => !(this.#editedTypes.getHasOne(x.unique) && this.#contentTypes.getHasOne(x.unique)),
					);

					this.#contentTypes.append(entries);
				},
				null,
			);
		});

		// Observe all Content Types compositions: [NL]
		this.observe(
			this.contentTypeCompositions,
			(contentTypeCompositions) => {
				this.#loadContentTypeCompositions(contentTypeCompositions);
			},
			null,
		);
		this.observe(
			this.#contentTypeContainers,
			(contentTypeContainers) => {
				this.#containers.setValue(contentTypeContainers);
			},
			null,
		);
	}

	/**
	 * loadType will load the ContentType and all inherited and composed ContentTypes.
	 * This will give us all the structure for properties and containers.
	 * @param {string} unique - The unique of the ContentType to load.
	 * @returns {Promise} - Promise resolved
	 */
	public async loadType(unique: string): Promise<UmbRepositoryResponseWithAsObservable<T>> {
		if (this.#ownerContentTypeUnique === unique) {
			// Its the same, but we do not know if its done loading jet, so we will wait for the load promise to finish. [NL]
			await this.#init;
			return { data: this.getOwnerContentType(), asObservable: () => this.ownerContentType };
		}
		await this.#initRepository;
		this.#clear();
		this.#ownerContentTypeUnique = unique;
		if (!unique) {
			this.#initRejection?.(`Content Type structure manager could not load: ${unique}`);
			return Promise.reject(
				new Error('The unique identifier is missing. A valid unique identifier is required to load the content type.'),
			);
		}
		this.#repoManager!.setUniques([unique]);
		const result = await this.observe(this.#repoManager!.entryByUnique(unique)).asPromise();
		this.#initResolver?.(result);
		await this.#init;
		return { data: result, asObservable: () => this.ownerContentType };
	}

	public async createScaffold(preset?: Partial<T>): Promise<UmbRepositoryResponse<T>> {
		await this.#initRepository;
		this.#clear();

		const repsonse = await this.#repository!.createScaffold(preset);
		const { data } = repsonse;
		if (!data) {
			this.#initRejection?.(`Content Type structure manager could not create scaffold`);
			return { error: repsonse.error };
		}

		this.#ownerContentTypeUnique = data.unique;

		// Add the new content type to the list of content types, this holds our draft state of this scaffold.
		this.#contentTypes.appendOne(data);
		// Make a entry in the repo manager:
		this.#repoManager!.addEntry(data);
		this.#initResolver?.(data);
		return repsonse;
	}

	/**
	 * Save the owner content type. Notice this is for a Content Type that is already stored on the server.
	 * @returns {Promise} - A promise that will be resolved when the content type is saved.
	 */
	public async save(): Promise<T> {
		await this.#initRepository;
		const contentType = this.getOwnerContentType();
		if (!contentType || !contentType.unique) throw new Error('Could not find the Content Type to save');

		const { error, data } = await this.#repository!.save(contentType);
		if (error || !data) {
			throw error?.message ?? 'Repository did not return data after save.';
		}

		// Update state with latest version:
		this.#contentTypes.updateOne(contentType.unique, data);

		// Update entry in the repo manager:
		this.#repoManager!.addEntry(data);
		return data;
	}

	/**
	 * Create the owner content type. Notice this is for a Content Type that is NOT already stored on the server.
	 * @param {string | null} parentUnique - The unique of the parent content type
	 * @returns {Promise} - a promise that is resolved when the content type has been created.
	 */
	public async create(parentUnique: string | null): Promise<T> {
		await this.#initRepository;
		const contentType = this.getOwnerContentType();
		if (!contentType || !contentType.unique) {
			throw new Error('Could not find the Content Type to create');
		}
		const { error, data } = await this.#repository!.create(contentType, parentUnique);
		if (error || !data) {
			throw error?.message ?? 'Repository did not return data after create.';
		}

		// Update state with latest version:
		this.#contentTypes.updateOne(contentType.unique, data);

		// Let the repo manager know about this new unique, so it can be loaded:
		this.#repoManager!.addEntry(data);
		return data;
	}

	async #loadContentTypeCompositions(contentTypeCompositions: T['compositions'] | undefined) {
		const ownerUnique = this.getOwnerContentTypeUnique();
		if (!ownerUnique) return;
		const compositionUniques = contentTypeCompositions?.map((x) => x.contentType.unique) ?? [];
		const newUniques = [ownerUnique, ...compositionUniques];
		this.#contentTypes.filter((x) => newUniques.includes(x.unique));
		this.#repoManager!.setUniques(newUniques);
	}

	/** Public methods for consuming structure: */

	ownerContentTypeObservablePart<R>(mappingFunction: MappingFunction<T | undefined, R>) {
		return createObservablePart(this.ownerContentType, mappingFunction);
	}

	getOwnerContentType() {
		return this.#contentTypes.getValue().find((y) => y.unique === this.#ownerContentTypeUnique);
	}

	getOwnerContentTypeUnique() {
		return this.#ownerContentTypeUnique;
	}

	getVariesByCulture() {
		const ownerContentType = this.getOwnerContentType();
		return ownerContentType?.variesByCulture;
	}
	getVariesBySegment() {
		const ownerContentType = this.getOwnerContentType();
		return ownerContentType?.variesBySegment;
	}

	/**
	 * Figure out if any of the Content Types has a Property.
	 * @returns {boolean} - true if any of the Content Type in this composition has a Property.
	 */
	getHasProperties() {
		return this.#contentTypes.getValue().some((y) => y.properties.length > 0);
	}

	updateOwnerContentType(entry: Partial<T>) {
		this.#editedTypes.appendOne(this.#ownerContentTypeUnique!);
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
	 * @param {string} containerId - The container to ensure exists on the given ContentType.
	 * @param {string} contentTypeUnique - The content type to ensure the container for.
	 * @returns {Promise<UmbPropertyTypeContainerModel | undefined>} - The container found or created for the owner ContentType.
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
		const container = contentType?.containers?.find((x) => x.id === containerId);
		if (!container) {
			return this.cloneContainerTo(containerId, contentTypeUnique);
		}
		return container;
	}

	/**
	 * Clone a container to a specific Content Type.
	 * @param {string} containerId - The container to clone, assuming it does not already exist on the given Content Type.
	 * @param {string} toContentTypeUnique - The content type to clone to.
	 * @returns {Promise<UmbPropertyTypeContainerModel | undefined>} - The container cloned or found for the owner ContentType.
	 */
	async cloneContainerTo(
		containerId: string,
		toContentTypeUnique?: string,
	): Promise<UmbPropertyTypeContainerModel | undefined> {
		await this.#init;
		toContentTypeUnique = toContentTypeUnique ?? this.#ownerContentTypeUnique!;
		this.#editedTypes.appendOne(toContentTypeUnique);

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

		containers.push(clonedContainer);

		this.#contentTypes.updateOne(toContentTypeUnique, { containers } as Partial<T>);

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
				this.#editedTypes.appendOne(contentTypeUnique);
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
	): Promise<UmbPropertyTypeContainerModel> {
		await this.#init;
		contentTypeUnique = contentTypeUnique ?? this.#ownerContentTypeUnique!;
		this.#editedTypes.appendOne(contentTypeUnique);

		if (parentId) {
			const duplicatedParentContainer = await this.ensureContainerOf(parentId, contentTypeUnique);
			if (!duplicatedParentContainer) {
				throw new Error('Parent container for creating a new container could not be found or created');
			}
			parentId = duplicatedParentContainer.id;
		}

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

		this.#contentTypes.updateOne(contentTypeUnique, { containers } as Partial<T>);

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

		this.#contentTypes.updateOne(contentTypeUnique, { containers } as Partial<T>);
	}*/

	makeEmptyContainerName(
		containerId: string,
		containerType: UmbPropertyContainerTypes,
		parentId: string | null = null,
	): string {
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
		this.#editedTypes.appendOne(contentTypeUnique);

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

		const containers: UmbPropertyTypeContainerModel[] = partialUpdateFrozenArray(
			frozenContainers,
			partialUpdate,
			(x) => x.id === containerId,
		);

		// eslint-disable-next-line @typescript-eslint/ban-ts-comment
		// @ts-ignore
		// TODO: fix TS partial complaint
		this.#contentTypes.updateOne(contentTypeUnique, { containers });
	}

	async removeContainer(contentTypeUnique: string | null, containerId: string | null = null) {
		await this.#init;
		contentTypeUnique = contentTypeUnique ?? this.#ownerContentTypeUnique!;
		this.#editedTypes.appendOne(contentTypeUnique);

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

		this.#contentTypes.updateOne(contentTypeUnique, { containers, properties } as Partial<T>);
	}

	async insertProperty(contentTypeUnique: string | null, property: UmbPropertyTypeModel) {
		await this.#init;
		contentTypeUnique = contentTypeUnique ?? this.#ownerContentTypeUnique!;
		this.#editedTypes.appendOne(contentTypeUnique);

		// If we have a container, we need to ensure it exists, and then update the container with the new parent id. [NL]
		if (property.container) {
			this.#contentTypes.mute();
			const container = await this.ensureContainerOf(property.container.id, contentTypeUnique);
			this.#contentTypes.unmute();
			if (!container) {
				throw new Error('Container for inserting property could not be found or created');
			}
			// Unfreeze object, while settings container.id
			property = { ...property, container: { id: container.id } };
		}

		if (property.sortOrder === undefined) {
			property.sortOrder = 0;
		}

		const frozenProperties =
			this.#contentTypes.getValue().find((x) => x.unique === contentTypeUnique)?.properties ?? [];

		const properties = appendToFrozenArray(frozenProperties, property, (x) => x.unique === property.unique);

		this.#contentTypes.updateOne(contentTypeUnique, { properties } as Partial<T>);
	}

	async removeProperty(contentTypeUnique: string | null, propertyUnique: string) {
		await this.#init;
		contentTypeUnique = contentTypeUnique ?? this.#ownerContentTypeUnique!;
		this.#editedTypes.appendOne(contentTypeUnique);

		const frozenProperties =
			this.#contentTypes.getValue().find((x) => x.unique === contentTypeUnique)?.properties ?? [];

		const properties = filterFrozenArray(frozenProperties, (x) => x.unique !== propertyUnique);

		this.#contentTypes.updateOne(contentTypeUnique, { properties } as Partial<T>);
	}

	async updateProperty(
		contentTypeUnique: string | null,
		propertyUnique: string,
		partialUpdate: Partial<UmbPropertyTypeModel>,
	) {
		await this.#init;
		contentTypeUnique = contentTypeUnique ?? this.#ownerContentTypeUnique!;
		this.#editedTypes.appendOne(contentTypeUnique);

		const frozenProperties =
			this.#contentTypes.getValue().find((x) => x.unique === contentTypeUnique)?.properties ?? [];
		const properties = partialUpdateFrozenArray(frozenProperties, partialUpdate, (x) => x.unique === propertyUnique);

		this.#contentTypes.updateOne(contentTypeUnique, { properties } as Partial<T>);
	}

	// TODO: Refactor: These property methods, should maybe be named without structure in their name.
	async propertyStructureById(propertyUnique: string) {
		await this.#init;
		return this.#contentTypes.asObservablePart((docTypes) => {
			for (const docType of docTypes) {
				const foundProp = docType.properties?.find((property) => property.unique === propertyUnique);
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

	async getPropertyStructureById(propertyUnique: string) {
		await this.#init;
		for (const docType of this.#contentTypes.getValue()) {
			const foundProp = docType.properties?.find((property) => property.unique === propertyUnique);
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

	async hasRootContainers(containerType: UmbPropertyContainerTypes) {
		return this.#containers.asObservablePart((data) => {
			return data.filter((x) => x.parent === null && x.type === containerType).length > 0;
		});
	}

	ownerContainersOf(containerType: UmbPropertyContainerTypes, parentId: string | null) {
		return this.ownerContentTypeObservablePart(
			(x) =>
				x?.containers?.filter(
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

	contentTypeOfProperty(propertyId: UmbPropertyTypeUnique) {
		return this.#contentTypes.asObservablePart((contentTypes) =>
			contentTypes.find((contentType) => contentType.properties.some((p) => p.unique === propertyId)),
		);
	}

	#observeRepository(repositoryAlias: string) {
		if (!repositoryAlias) throw new Error('Content Type structure manager must have a repository alias.');

		new UmbExtensionApiInitializer<ManifestRepository<UmbDetailRepository<T>>>(
			this,
			umbExtensionsRegistry,
			repositoryAlias,
			[this._host],
			(permitted, ctrl) => {
				this.#repository = permitted ? ctrl.api : undefined;
				if (this.#repository) {
					this.#initRepositoryResolver?.(this.#repository);
				}
			},
		);
	}

	#clear() {
		this.#contentTypeObservers.forEach((observer) => observer.destroy());
		this.#contentTypeObservers = [];
		this.#containers.setValue([]);
		this.#repoManager?.clear();
		this.#contentTypes.setValue([]);
	}

	public override destroy() {
		this.#contentTypes.destroy();
		this.#containers.destroy();
		super.destroy();
	}
}
