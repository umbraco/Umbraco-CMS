import type { UmbContentTypeModel, UmbPropertyContainerTypes, UmbPropertyTypeContainerModel } from '../types.js';
import type { UmbContentTypePropertyStructureManager } from './content-type-structure-manager.class.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbArrayState, UmbBooleanState } from '@umbraco-cms/backoffice/observable-api';

/**
 * This class is a helper class for managing the structure of containers in a content type.
 * This requires a structure manager {@link UmbContentTypePropertyStructureManager} to manage the structure.
 */
export class UmbContentTypeContainerStructureHelper<T extends UmbContentTypeModel> extends UmbControllerBase {
	#init;
	#initResolver?: (value: unknown) => void;

	#structure?: UmbContentTypePropertyStructureManager<T>;

	private _parentType?: UmbPropertyContainerTypes;
	private _childType?: UmbPropertyContainerTypes = 'Group';
	private _isRoot = false;
	/**
	 * The owner id is the owning container (The container that is begin presented, the container is the parent of the child containers)
	 * If set to null, this helper class will provide containers of the root.
	 */
	private _parentId?: string | null;
	private _parentName?: string;

	// Containers defined in data might be more than actual containers to display as we merge them by name.
	// Direct containers are the containers defining the total of this container(Multiple containers with the same name and type)
	private _parentMatchingContainers: UmbPropertyTypeContainerModel[] = [];
	// Owner containers are containers owned by the owner Content Type (The specific one up for editing)
	private _parentOwnerContainers: UmbPropertyTypeContainerModel[] = [];

	// State containing the merged containers (only one pr. name):
	#containers = new UmbArrayState<UmbPropertyTypeContainerModel>([], (x) => x.id);
	readonly containers = this.#containers.asObservable();

	#hasProperties = new UmbBooleanState(false);
	readonly hasProperties = this.#hasProperties.asObservable();

	constructor(host: UmbControllerHost) {
		super(host);
		this.#init = new Promise((resolve) => {
			this.#initResolver = resolve;
		});

		this.#containers.sortBy((a, b) => (a.sortOrder || 0) - (b.sortOrder || 0));
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
		this._observeParentAlikeContainers();
	}

	public getStructureManager() {
		return this.#structure;
	}

	public setParentType(value?: UmbPropertyContainerTypes) {
		if (this._parentType === value) return;
		this._parentType = value;
		this._observeParentAlikeContainers();
	}
	public getParentType() {
		return this._parentType;
	}

	public setContainerChildType(value?: UmbPropertyContainerTypes) {
		if (this._childType === value) return;
		this._childType = value;
		this._observeParentAlikeContainers();
	}
	public getContainerChildType() {
		return this._childType;
	}

	public setName(value?: string) {
		if (this._parentName === value) return;
		this._parentName = value;
		this._observeParentAlikeContainers();
	}
	public getName() {
		return this._parentName;
	}

	public setIsRoot(value: boolean) {
		if (this._isRoot === value) return;
		this._isRoot = value;
		this._parentId = null;
		this._observeParentAlikeContainers();
	}
	public getIsRoot() {
		return this._isRoot;
	}

	public setParentId(value: string | null | undefined) {
		if (this._parentId === value) return;
		this._parentId = value;
	}
	public getParentId() {
		return this._parentId;
	}

	private _observeParentAlikeContainers() {
		if (!this.#structure) return;

		if (this._isRoot) {
			this.#containers.setValue([]);
			//this._observeChildProperties(); // We cannot have root properties currently, therefor we instead just set it to false:
			this.#hasProperties.setValue(false);
			this._observeRootContainers();
			/*this.observe(
				this.#structure.ownerContainersOf(this._parentType),
				(containers) => {
					this._parentContainers = containers ?? [];
					console.log('root parent containers', this._parentContainers);
					if (this._parentContainers.length !== 1) {
						console.log(
							'!!! We did not just get one parentContainer, I would have expected this, so I have to re-evaluate my understanding.  !!!',
						);
					}
					this._parentAlikeContainers = [];
				},
				'_observeParentContainers',
			);*/
		} else if (this._parentName && this._parentType) {
			this.#containers.setValue([]);
			this.observe(
				this.#structure.containersByNameAndType(this._parentName, this._parentType),
				(parentContainers) => {
					this.#containers.setValue([]);
					this._parentOwnerContainers = parentContainers.filter((x) => x.id === this._parentId) || [];
					this._parentMatchingContainers = parentContainers ?? [];
					if (this._parentMatchingContainers.length > 0) {
						this._observeChildProperties();
						this._observeChildContainers();
					}
				},
				'_observeParentContainers',
			);
		}
	}

	private _observeChildProperties() {
		if (!this.#structure) return;

		this._parentMatchingContainers.forEach((container) => {
			this.observe(
				this.#structure!.hasPropertyStructuresOf(container.id!),
				(hasProperties) => {
					this.#hasProperties.setValue(hasProperties);
				},
				'_observeParentHasProperties_' + container.id,
			);
		});
	}

	private _observeChildContainers() {
		if (!this.#structure || !this._parentName || !this._childType) return;

		this._parentMatchingContainers.forEach((container) => {
			this.observe(
				this.#structure!.containersOfParentKey(container.id, this._childType!),
				(containers) => {
					this.#containers.append(this._insertChildContainers(containers));
				},
				'_observeGroupsOf_' + container.id,
			);
		});
	}

	private _observeRootContainers() {
		if (!this.#structure || !this._isRoot || !this._childType || this._parentId === undefined) return;

		this.observe(
			this.#structure.rootContainers(this._childType),
			(rootContainers) => {
				this.#containers.setValue(this._insertChildContainers(rootContainers));
				this._parentOwnerContainers = this.#structure!.getOwnerContainers(this._childType!, this._parentId!) ?? [];
			},
			'_observeRootContainers',
		);
	}

	private _insertChildContainers(
		childContainers: UmbPropertyTypeContainerModel[],
	): Array<UmbPropertyTypeContainerModel> {
		return childContainers.flatMap((group, i, value) => {
			if (group.name !== null && group.name !== undefined) {
				if (value.find((x) => x.name === group.name)) {
					return group;
				}
			}
			return [];
		});
	}

	/**
	 * Returns true if the container is an owner container.
	 */
	/*
	isOwnerContainer(containerId?: string) {
		if (!this.#structure || !containerId) return;

		return this._parentOwnerContainers.find((x) => x.id === containerId) !== undefined;
	}
	*/

	/**
	 * Returns true if the container is an owner container.
	 */
	isOwnerChildContainer(containerId?: string) {
		if (!this.#structure || !containerId) return;
		return this._parentOwnerContainers.some((x) => x.id === containerId);
	}

	/** Manipulate methods: */

	async insertContainer(container: UmbPropertyTypeContainerModel, sortOrder = 0) {
		await this.#init;
		if (!this.#structure) return false;

		const newContainer = { ...container, sortOrder };

		await this.#structure.insertContainer(null, newContainer);
		return true;
	}

	async addContainer(parentContainerId?: string | null, sortOrder?: number) {
		if (!this.#structure) return;

		await this.#structure.createContainer(null, parentContainerId, this._childType, sortOrder);
	}

	async removeContainer(groupId: string) {
		await this.#init;
		if (!this.#structure) return false;

		await this.#structure.removeContainer(null, groupId);
		return true;
	}

	async partialUpdateContainer(containerId: string, partialUpdate: Partial<UmbPropertyTypeContainerModel>) {
		await this.#init;
		if (!this.#structure || !containerId || !partialUpdate) return;

		return await this.#structure.updateContainer(null, containerId, partialUpdate);
	}

	// TODO: This is only used by legacy implementations:
	// @deprecated
	async _legacy_updateContainerName(containerId: string, containerParentId: string | null, name: string) {
		await this.#init;
		if (!this.#structure) return;

		const newName =
			this.#structure.makeContainerNameUniqueForOwnerContentType(name, this._childType, containerParentId) ?? name;

		return await this.partialUpdateContainer(containerId, { name: newName });
	}
}
