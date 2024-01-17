import { UmbContentTypeModel } from './types.js';
import { UmbDetailRepository } from '@umbraco-cms/backoffice/repository';
import { UmbId } from '@umbraco-cms/backoffice/id';
import type {
	DocumentTypePropertyTypeResponseModel,
	PropertyTypeContainerModelBaseModel,
	PropertyTypeModelBaseModel,
} from '@umbraco-cms/backoffice/backend-api';
import { type UmbControllerHost, type UmbController } from '@umbraco-cms/backoffice/controller-api';
import {
	UmbArrayState,
	MappingFunction,
	partialUpdateFrozenArray,
	appendToFrozenArray,
	filterFrozenArray,
} from '@umbraco-cms/backoffice/observable-api';
import { incrementString } from '@umbraco-cms/backoffice/utils';
import { UmbBaseController } from '@umbraco-cms/backoffice/class-api';

export type PropertyContainerTypes = 'Group' | 'Tab';

// TODO: get this type from the repository, or use some generic type.
// TODO: Make this a controller on its own:
export class UmbContentTypePropertyStructureManager<T extends UmbContentTypeModel> extends UmbBaseController {
	#init!: Promise<unknown>;

	#contentTypeRepository: UmbDetailRepository<T>;

	#ownerContentTypeUnique?: string;
	#contentTypeObservers = new Array<UmbController>();
	#contentTypes = new UmbArrayState<T>([], (x) => x.unique);
	readonly contentTypes = this.#contentTypes.asObservable();
	private readonly _contentTypeContainers = this.#contentTypes.asObservablePart((x) =>
		x.flatMap((x) => x.containers ?? []),
	);

	#containers: UmbArrayState<PropertyTypeContainerModelBaseModel> =
		new UmbArrayState<PropertyTypeContainerModelBaseModel>([], (x) => x.id);

	constructor(host: UmbControllerHost, typeRepository: UmbDetailRepository<T>) {
		super(host);
		this.#contentTypeRepository = typeRepository;

		this.observe(this.contentTypes, (contentTypes) => {
			contentTypes.forEach((contentType) => {
				this._loadContentTypeCompositions(contentType);
			});
		});
		this.observe(this._contentTypeContainers, (contentTypeContainers) => {
			this.#containers.next(contentTypeContainers);
		});
	}

	/**
	 * loadType will load the node type and all inherited and composed types.
	 * This will give us all the structure for properties and containers.
	 */
	public async loadType(unique?: string) {
		this._reset();

		this.#ownerContentTypeUnique = unique;

		const promiseResult = this._loadType(unique);
		this.#init = promiseResult;
		await this.#init;
		return promiseResult;
	}

	public async createScaffold(parentUnique: string | null) {
		this._reset();

		if (parentUnique === undefined) return {};

		const { data } = await this.#contentTypeRepository.createScaffold(parentUnique);
		if (!data) return {};

		this.#ownerContentTypeUnique = data.unique;

		// Add the new content type to the list of content types, this holds our draft state of this scaffold.
		this.#contentTypes.appendOne(data);
		return { data };
	}

	/**
	 * Save the owner content type. Notice this is for a Content Type that is already stored on the server.
	 * @returns
	 */
	public async save() {
		const contentType = this.getOwnerContentType();
		if (!contentType || !contentType.unique) return false;

		const { data } = await this.#contentTypeRepository.save(contentType);
		if (!data) return false;

		// Update state with latest version:
		this.#contentTypes.updateOne(contentType.unique, data);

		return true;
	}

	/**
	 * Create the owner content type. Notice this is for a Content Type that is NOT already stored on the server.
	 * @returns
	 */
	public async create() {
		const contentType = this.getOwnerContentType();
		if (!contentType || !contentType.unique) return false;

		const { data } = await this.#contentTypeRepository.create(contentType);
		if (!data) return false;

		// Update state with latest version:
		this.#contentTypes.updateOne(contentType.unique, data);

		// Start observe the new content type in the store, as we did not do that when it was a scaffold/local-version.
		this._observeContentType(data);

		return true;
	}

	private async _ensureType(unique?: string) {
		if (!unique) return;
		if (this.#contentTypes.getValue().find((x) => x.unique === unique)) return;
		await this._loadType(unique);
	}

	private async _loadType(unique?: string) {
		if (!unique) return {};

		const { data } = await this.#contentTypeRepository.requestByUnique(unique);
		if (!data) return {};

		await this._observeContentType(data);
		return { data };
	}

	private async _observeContentType(data: T) {
		if (!data.unique) return;

		// Load inherited and composed types:
		this._loadContentTypeCompositions(data);

		this.#contentTypeObservers.push(
			this.observe(
				await this.#contentTypeRepository.byUnique(data.unique),
				(docType) => {
					if (docType) {
						// TODO: Handle if there was changes made to the owner document type in this context.
						/*
						possible easy solutions could be to notify user wether they want to update(Discard the changes to accept the new ones).
					 	*/
						this.#contentTypes.appendOne(docType);
					}
				},
				'observeContentType_' + data.unique,
			),
		);
	}

	private async _loadContentTypeCompositions(contentType: T) {
		contentType.compositions?.forEach((composition) => {
			this._ensureType(composition.id);
		});
	}

	/** Public methods for consuming structure: */

	ownerContentType() {
		return this.#contentTypes.asObservablePart((x) => x.find((y) => y.unique === this.#ownerContentTypeUnique));
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
		type: PropertyContainerTypes = 'Group',
		sortOrder?: number,
	) {
		await this.#init;
		contentTypeUnique = contentTypeUnique ?? this.#ownerContentTypeUnique!;

		const container: PropertyTypeContainerModelBaseModel = {
			id: UmbId.new(),
			parentId: parentId ?? null,
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

	async insertContainer(contentTypeUnique: string | null, container: PropertyTypeContainerModelBaseModel) {
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
		contentTypeUnique: string | null,
		containerId: string,
		partialUpdate: Partial<PropertyTypeContainerModelBaseModel>,
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

	async createProperty(contentTypeUnique: string | null, containerId: string | null = null, sortOrder?: number) {
		await this.#init;
		contentTypeUnique = contentTypeUnique ?? this.#ownerContentTypeUnique!;

		const property: PropertyTypeModelBaseModel = this.createPropertyScaffold(containerId, sortOrder);

		const properties = [
			...(this.#contentTypes.getValue().find((x) => x.unique === contentTypeUnique)?.properties ?? []),
		];
		properties.push(property);

		// eslint-disable-next-line @typescript-eslint/ban-ts-comment
		// @ts-ignore
		// TODO: fix TS partial complaint
		this.#contentTypes.updateOne(contentTypeUnique, { properties });

		return property;
	}

	async insertProperty(contentTypeUnique: string | null, property: PropertyTypeModelBaseModel) {
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
		partialUpdate: Partial<PropertyTypeModelBaseModel>,
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
		this.#contentTypes.destroy();
		this.#containers.destroy();
	}
}
