import type {
	UmbContentTypeModel,
	UmbPropertyContainerTypes,
	UmbPropertyTypeContainerModel,
	UmbPropertyTypeModel,
} from '../types.js';
import type { UmbDetailRepository, UmbRepositoryResponse } from '@umbraco-cms/backoffice/repository';
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
import {
	UmbVariantPropertyReadStateManager,
	UmbVariantPropertyWriteStateManager,
} from '@umbraco-cms/backoffice/variant';

type UmbPropertyTypeId = UmbPropertyTypeModel['id'];

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
	#initResolver?: (respoonse: UmbRepositoryResponse<T>) => void;
	#init = new Promise<UmbRepositoryResponse<T>>((resolve) => {
		this.#initResolver = resolve;
	});

	#repository?: UmbDetailRepository<T>;
	#initRepositoryResolver?: () => void;

	#initRepository = new Promise<void>((resolve) => {
		if (this.#repository) {
			resolve();
		} else {
			this.#initRepositoryResolver = resolve;
		}
	});

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

	readonly #contentTypeContainers = this.#contentTypes.asObservablePart((contentTypes) => {
		// Notice this may need to use getValue to avoid resetting it self. [NL]
		return contentTypes.flatMap((x) => x.containers ?? []);
	});
	readonly contentTypeProperties = this.#contentTypes.asObservablePart((contentTypes) => {
		// Notice this may need to use getValue to avoid resetting it self. [NL]
		return contentTypes.flatMap((x) => x.properties ?? []);
	});
	async getContentTypeProperties() {
		return await this.observe(this.contentTypeProperties).asPromise();
	}
	readonly contentTypeDataTypeUniques = this.#contentTypes.asObservablePart((contentTypes) => {
		// Notice this may need to use getValue to avoid resetting it self. [NL]
		return contentTypes
			.flatMap((x) => x.properties?.map((p) => p.dataType.unique) ?? [])
			.filter(UmbFilterDuplicateStrings);
	});
	readonly contentTypeHasProperties = this.#contentTypes.asObservablePart((contentTypes) => {
		// Notice this may need to use getValue to avoid resetting it self. [NL]
		return contentTypes.some((x) => x.properties.length > 0);
	});
	readonly contentTypePropertyAliases = createObservablePart(this.contentTypeProperties, (properties) =>
		properties.map((x) => x.alias),
	);
	readonly contentTypeUniques = this.#contentTypes.asObservablePart((x) => x.map((y) => y.unique));
	readonly contentTypeAliases = this.#contentTypes.asObservablePart((x) => x.map((y) => y.alias));

	readonly variesByCulture = createObservablePart(this.ownerContentType, (x) => x?.variesByCulture);
	readonly variesBySegment = createObservablePart(this.ownerContentType, (x) => x?.variesBySegment);

	public readonly propertyReadState = new UmbVariantPropertyReadStateManager(this);
	public readonly propertyWriteState = new UmbVariantPropertyWriteStateManager(this);

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
			this.#initRepositoryResolver?.();
		}

		// Observe owner content type compositions, as we only allow one level of compositions at this moment. [NL]
		// But, we could support more, we would just need to flatMap all compositions and make sure the entries are unique and then base the observation on that. [NL]
		this.observe(this.ownerContentTypeCompositions, (ownerContentTypeCompositions) => {
			this.#loadContentTypeCompositions(ownerContentTypeCompositions);
		});
		this.observe(this.#contentTypeContainers, (contentTypeContainers) => {
			this.#containers.setValue(contentTypeContainers);
		});
	}

	/**
	 * loadType will load the ContentType and all inherited and composed ContentTypes.
	 * This will give us all the structure for properties and containers.
	 * @param {string} unique - The unique of the ContentType to load.
	 * @returns {Promise} - Promise resolved
	 */
	public async loadType(unique?: string) {
		if (this.#ownerContentTypeUnique === unique) {
			// Its the same, but we do not know if its done loading jet, so we will wait for the load promise to finish. [NL]
			await this.#init;
			return;
		}
		this.#clear();
		this.#ownerContentTypeUnique = unique;
		if (!unique) return;
		const result = await this.#loadType(unique);
		this.#initResolver?.(result);
		return result;
	}

	public async createScaffold(preset?: Partial<T>) {
		await this.#initRepository;
		this.#clear();

		const repsonse = await this.#repository!.createScaffold(preset);
		if (!repsonse.data) return {};

		this.#ownerContentTypeUnique = repsonse.data.unique;

		// Add the new content type to the list of content types, this holds our draft state of this scaffold.
		this.#contentTypes.appendOne(repsonse.data);
		this.#initResolver?.(repsonse);
		return repsonse;
	}

	/**
	 * Save the owner content type. Notice this is for a Content Type that is already stored on the server.
	 * @returns {Promise} - A promise that will be resolved when the content type is saved.
	 */
	public async save() {
		await this.#initRepository;
		const contentType = this.getOwnerContentType();
		if (!contentType || !contentType.unique) throw new Error('Could not find the Content Type to save');

		const { error, data } = await this.#repository!.save(contentType);
		if (error || !data) {
			throw error?.message ?? 'Repository did not return data after save.';
		}

		// Update state with latest version:
		this.#contentTypes.updateOne(contentType.unique, data);

		return data;
	}

	/**
	 * Create the owner content type. Notice this is for a Content Type that is NOT already stored on the server.
	 * @param {string | null} parentUnique - The unique of the parent content type
	 * @returns {Promise} - a promise that is resolved when the content type has been created.
	 */
	public async create(parentUnique: string | null) {
		await this.#initRepository;
		const contentType = this.getOwnerContentType();
		if (!contentType || !contentType.unique) {
			throw new Error('Could not find the Content Type to create');
		}

		const { data } = await this.#repository!.create(contentType, parentUnique);
		if (!data) return Promise.reject();

		// Update state with latest version:
		this.#contentTypes.updateOne(contentType.unique, data);

		// Start observe the new content type in the store, as we did not do that when it was a scaffold/local-version.
		this.#observeContentType(data);
	}

	async #loadContentTypeCompositions(ownerContentTypeCompositions: T['compositions'] | undefined) {
		if (!ownerContentTypeCompositions) {
			// Owner content type was undefined, so we cannot load compositions.
			// But to clean up existing compositions, we set the array to empty to still be able to execute the clean-up code.
			ownerContentTypeCompositions = [];
		}

		const ownerUnique = this.getOwnerContentTypeUnique();
		// Remove content types that does not exist as compositions anymore:
		this.#contentTypes.getValue().forEach((x) => {
			if (
				x.unique !== ownerUnique &&
				!ownerContentTypeCompositions.find((comp) => comp.contentType.unique === x.unique)
			) {
				this.#contentTypeObservers.find((y) => y.controllerAlias === 'observeContentType_' + x.unique)?.destroy();
				this.#contentTypes.removeOne(x.unique);
			}
		});
		ownerContentTypeCompositions.forEach((composition) => {
			this.#ensureType(composition.contentType.unique);
		});
	}

	async #ensureType(unique?: string) {
		if (!unique) return;
		if (this.#contentTypes.getValue().find((x) => x.unique === unique)) return;
		await this.#loadType(unique);
	}

	async #loadType(unique?: string) {
		if (!unique) return {};
		await this.#initRepository;

		// Lets initiate the content type:
		const { data, asObservable } = await this.#repository!.requestByUnique(unique);
		if (!data) return {};

		await this.#observeContentType(data);
		return { data, asObservable };
	}

	async #observeContentType(data: T) {
		if (!data.unique) return;
		await this.#initRepository;

		// Notice we do not store the content type in the store here, cause it will happen shortly after when the observations gets its first initial callback. [NL]

		const ctrl = this.observe(
			// Then lets start observation of the content type:
			await this.#repository!.byUnique(data.unique),
			(docType) => {
				if (docType) {
					this.#contentTypes.appendOne(docType);
				} else {
					// Remove the content type from the store, if it does not exist anymore.
					this.#contentTypes.removeOne(data.unique);
				}
			},
			'observeContentType_' + data.unique,
			// Controller Alias is used to stop observation when no longer needed. [NL]
		);

		this.#contentTypeObservers.push(ctrl);
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
	): Promise<UmbPropertyTypeContainerModel> {
		await this.#init;
		contentTypeUnique = contentTypeUnique ?? this.#ownerContentTypeUnique!;

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

	async insertProperty(contentTypeUnique: string | null, property: UmbPropertyTypeModel) {
		await this.#init;
		contentTypeUnique = contentTypeUnique ?? this.#ownerContentTypeUnique!;

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

	contentTypeOfProperty(propertyId: UmbPropertyTypeId) {
		return this.#contentTypes.asObservablePart((contentTypes) =>
			contentTypes.find((contentType) => contentType.properties.some((p) => p.id === propertyId)),
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
				this.#initRepositoryResolver?.();
			},
		);
	}

	#clear() {
		this.#init = new Promise((resolve) => {
			this.#initResolver = resolve;
		});
		this.#contentTypes.setValue([]);
		this.#contentTypeObservers.forEach((observer) => observer.destroy());
		this.#contentTypeObservers = [];
		this.#contentTypes.setValue([]);
		this.#containers.setValue([]);
	}

	public override destroy() {
		this.#contentTypes.destroy();
		this.#containers.destroy();
		this.propertyReadState.destroy();
		this.propertyWriteState.destroy();
		super.destroy();
	}
}
