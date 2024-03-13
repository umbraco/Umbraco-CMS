import type {
	UmbContentTypeModel,
	UmbPropertyContainerTypes,
	UmbPropertyTypeContainerModel,
	UmbPropertyTypeModel,
} from '../types.js';
import type { UmbContentTypePropertyStructureManager } from './content-type-structure-manager.class.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbArrayState } from '@umbraco-cms/backoffice/observable-api';

type UmbPropertyTypeId = UmbPropertyTypeModel['id'];

/**
 * This class is a helper class for managing the structure of containers in a content type.
 * This requires a structure manager {@link UmbContentTypePropertyStructureManager} to manage the structure.
 */
export class UmbContentTypePropertyStructureHelper<T extends UmbContentTypeModel> extends UmbControllerBase {
	#init;
	#initResolver?: (value: unknown) => void;

	#structure?: UmbContentTypePropertyStructureManager<T>;

	private _containerType?: UmbPropertyContainerTypes;
	private _isRoot?: boolean;
	private _containerName?: string;

	#propertyStructure = new UmbArrayState<UmbPropertyTypeModel>([], (x) => x.id);
	readonly propertyStructure = this.#propertyStructure.asObservable();

	constructor(host: UmbControllerHost) {
		super(host);
		this.#init = new Promise((resolve) => {
			this.#initResolver = resolve;
		});
		this.#propertyStructure.sortBy((a, b) => a.sortOrder - b.sortOrder);
	}

	async contentTypes() {
		await this.#init;
		if (!this.#structure) return;
		return this.#structure.contentTypes;
	}

	public setStructureManager(structure: UmbContentTypePropertyStructureManager<T>) {
		if (this.#structure === structure) return;
		if (this.#structure) {
			throw new Error(
				'Structure manager is already set, the helpers are not designed to be re-setup with new managers',
			);
		}
		this.#structure = structure;
		this.#initResolver?.(undefined);
		this.#initResolver = undefined;
		this._observeGroupContainers();
	}

	public getStructureManager() {
		return this.#structure;
	}

	public setContainerType(value?: UmbPropertyContainerTypes) {
		if (this._containerType === value) return;
		this._containerType = value;
		this._observeGroupContainers();
	}
	public getContainerType() {
		return this._containerType;
	}

	public setContainerName(value?: string) {
		if (this._containerName === value) return;
		this._containerName = value;
		this._observeGroupContainers();
	}
	public getContainerName() {
		return this._containerName;
	}

	public setIsRoot(value: boolean) {
		if (this._isRoot === value) return;
		this._isRoot = value;
		this._observeGroupContainers();
	}
	public getIsRoot() {
		return this._isRoot;
	}

	#groupContainers?: Array<UmbPropertyTypeContainerModel>;
	private _observeGroupContainers() {
		if (!this.#structure || !this._containerType) return;

		if (this._isRoot === true) {
			this._observePropertyStructureOf(null);
		} else if (this._containerName !== undefined) {
			this.observe(
				this.#structure.containersByNameAndType(this._containerName, this._containerType),
				(groupContainers) => {
					console.log('group observe', this._containerName);
					if (this.#groupContainers) {
						// We want to remove properties of groups that does not exist anymore: [NL]
						const goneGroupContainers = this.#groupContainers.filter(
							(x) => !groupContainers.some((y) => y.id === x.id),
						);
						console.log('groupContainers', groupContainers);
						console.log('goneGroupContainers', goneGroupContainers);
						const _propertyStructure = this.#propertyStructure
							.getValue()
							.filter((x) => !goneGroupContainers.some((y) => y.id === x.container?.id));
						this.#propertyStructure.setValue(_propertyStructure);
					}

					groupContainers.forEach((group) => this._observePropertyStructureOf(group.id));
					this.#groupContainers = groupContainers;
				},
				'_observeGroupContainers',
			);
		}
	}

	private _observePropertyStructureOf(groupId?: string | null) {
		if (!this.#structure || groupId === undefined) return;

		this.observe(
			this.#structure.propertyStructuresOf(groupId),
			(properties) => {
				// Lets remove the properties that does not exists any longer:
				const _propertyStructure = this.#propertyStructure
					.getValue()
					.filter((x) => !(x.container?.id === groupId && !properties.some((y) => y.id === x.id)));

				// Lets append the properties that does not exists already:
				properties?.forEach((property) => {
					if (!_propertyStructure.find((x) => x.alias === property.alias)) {
						_propertyStructure.push(property);
					}
				});

				// Fire update to subscribers:
				this.#propertyStructure.setValue(_propertyStructure);
			},
			'_observePropertyStructureOfGroup' + groupId,
		);
	}

	async isOwnerProperty(propertyId: UmbPropertyTypeId) {
		await this.#init;
		if (!this.#structure) return;

		return this.#structure.ownerContentTypePart((x) => x?.properties.some((y) => y.id === propertyId));
	}

	// TODO: consider moving this to another class, to separate 'viewer' from 'manipulator':
	/** Manipulate methods: */

	async createPropertyScaffold(ownerId?: string) {
		await this.#init;
		if (!this.#structure) return;

		return await this.#structure.createPropertyScaffold(ownerId);
	}
	/*
		Only used by legacy implementation:
		@deprecated
	*/
	async addProperty(containerId?: string, sortOrder?: number) {
		await this.#init;
		if (!this.#structure) return;

		return await this.#structure.createProperty(null, containerId, sortOrder);
	}

	async insertProperty(property: UmbPropertyTypeModel, sortOrder?: number) {
		await this.#init;
		if (!this.#structure) return false;

		const newProperty = { ...property };
		if (sortOrder) {
			newProperty.sortOrder = sortOrder;
		}

		// TODO: Remove as any when server model has gotten sortOrder:
		await this.#structure.insertProperty(null, newProperty);
		return true;
	}

	async removeProperty(propertyId: UmbPropertyTypeId) {
		await this.#init;
		if (!this.#structure) return false;

		await this.#structure.removeProperty(null, propertyId);
		return true;
	}

	// Takes optional arguments as this is easier for the implementation in the view:
	async partialUpdateProperty(propertyKey?: string, partialUpdate?: Partial<UmbPropertyTypeModel>) {
		await this.#init;
		if (!this.#structure || !propertyKey || !partialUpdate) return;
		return await this.#structure.updateProperty(null, propertyKey, partialUpdate);
	}
}
