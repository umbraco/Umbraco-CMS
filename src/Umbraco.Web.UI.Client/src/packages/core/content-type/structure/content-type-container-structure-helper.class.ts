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

	private _parentType?: UmbPropertyContainerTypes = 'Tab';
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

	// State containing the merged containers (only one pr. name):
	#mergedContainers = new UmbArrayState<UmbPropertyTypeContainerModel>([], (x) => x.id);
	readonly mergedContainers = this.#mergedContainers.asObservable();

	// Owner containers are containers owned by the owner Content Type (The specific one up for editing)
	private _ownerContainers: UmbPropertyTypeContainerModel[] = [];

	#hasProperties = new UmbBooleanState(false);
	readonly hasProperties = this.#hasProperties.asObservable();

	constructor(host: UmbControllerHost) {
		super(host);
		this.#init = new Promise((resolve) => {
			this.#initResolver = resolve;
		});

		this.#mergedContainers.sortBy((a, b) => (a.sortOrder || 0) - (b.sortOrder || 0));
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
		this.#observeParentAlikeContainers();
	}

	public getStructureManager() {
		return this.#structure;
	}

	public setParentType(value?: UmbPropertyContainerTypes) {
		if (this._parentType === value) return;
		this._parentType = value;
		this.#observeParentAlikeContainers();
	}
	public getParentType() {
		return this._parentType;
	}

	public setContainerChildType(value?: UmbPropertyContainerTypes) {
		if (this._childType === value) return;
		this._childType = value;
		this.#observeParentAlikeContainers();
	}
	public getContainerChildType() {
		return this._childType;
	}

	public setName(value?: string) {
		if (this._parentName === value) return;
		this._parentName = value;
		this.#observeParentAlikeContainers();
	}
	public getName() {
		return this._parentName;
	}

	public setIsRoot(value: boolean) {
		if (this._isRoot === value) return;
		this._isRoot = value;
		this._parentId = null;
		this.#observeParentAlikeContainers();
	}
	public getIsRoot() {
		return this._isRoot;
	}

	public setParentId(value: string | null | undefined) {
		if (this._parentId === value) return;
		this._parentId = value;
		this.#observeParentAlikeContainers();
	}
	public getParentId() {
		return this._parentId;
	}

	#observeParentAlikeContainers() {
		if (!this.#structure) return;

		if (this._isRoot) {
			this.removeControllerByAlias('_observeOwnerContainers');
			this.#mergedContainers.setValue([]);
			//this._observeChildProperties(); // We cannot have root properties currently, therefor we instead just set it to false:
			this.#hasProperties.setValue(false);
			this.#observeRootContainers();
		} else if (this._parentName && this._parentType) {
			this.#mergedContainers.setValue([]);
			this.observe(
				// This only works because we just have two levels, meaning this is the upper level and we want it to merge, so its okay this does not take parent-parent (And further structure) into account: [NL]
				this.#structure.containersByNameAndType(this._parentName, this._parentType),
				(parentContainers) => {
					this._ownerContainers = [];
					this.#mergedContainers.setValue([]);
					// Stop observing a the previous _parentMatchingContainers...
					this._parentMatchingContainers.forEach((container) => {
						this.removeControllerByAlias('_observeParentHasProperties_' + container.id);
						this.removeControllerByAlias('_observeGroupsOf_' + container.id);
					});
					this._parentMatchingContainers = parentContainers ?? [];
					if (this._parentMatchingContainers.length > 0) {
						this.#observeChildProperties();
						this.#observeChildContainers();
					} else {
						// Do some reset:
						this.#hasProperties.setValue(false);
						this.removeControllerByAlias('_observeOwnerContainers');
					}
				},
				'_observeParentContainers',
			);
		}
	}

	#observeChildProperties() {
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

	#observeChildContainers() {
		if (!this.#structure || !this._parentName || !this._childType || !this._parentId) return;

		// TODO: If a owner container is removed, Or suddenly matches name-wise with a inherited container, then we now miss the inherited container,maybe [NL]
		this.observe(
			this.#structure.ownerContainersOf(this._childType, this._parentId),
			(containers) => {
				this._ownerContainers = containers ?? [];
				this.#mergedContainers.setValue(this.#filterNonOwnerContainers(this.#mergedContainers.getValue()));
			},
			'_observeOwnerContainers',
		);

		this._parentMatchingContainers.forEach((parentCon) => {
			this.observe(
				this.#structure!.containersOfParentId(parentCon.id, this._childType!),
				(containers) => {
					// First we will filter out non-owner containers:
					const old = this.#mergedContainers.getValue();
					// Then filter out the containers that are in the new list, either based on id or a match on name & type.
					// Matching on name & type will result in the latest being the one we include, notice will only counts for non-owner containers.
					const oldFiltered = old.filter(
						(x) => !containers.some((y) => y.id === x.id || (y.name === x.name && y.type === x.type)),
					);

					const newFiltered = oldFiltered.concat(containers);

					// Filter out non owners again:
					this.#mergedContainers.setValue(this.#filterNonOwnerContainers(newFiltered));
				},
				'_observeGroupsOf_' + parentCon.id,
			);
		});
	}

	/**
	 * This filters our local containers, so we only have one pr. type and name.
	 * This method is used to ensure we prioritize a Owner Container over the inherited containers.
	 * This method does not ensure that there is only one of each
	 */
	#filterNonOwnerContainers(containers: Array<UmbPropertyTypeContainerModel>) {
		return this._ownerContainers.length > 0
			? containers.filter(
					(anyCon) =>
						!this._ownerContainers.some(
							(ownerCon) =>
								// Then if this is not the owner container but matches one by name & type, then we do not want it.
								ownerCon.id !== anyCon.id && ownerCon.name === anyCon.name && ownerCon.type === anyCon.type,
						),
				)
			: containers;
	}

	#observeRootContainers() {
		if (!this.#structure || !this._isRoot || !this._childType || this._parentId === undefined) return;

		this.observe(
			this.#structure.rootContainers(this._childType),
			(rootContainers) => {
				// Here (When getting root containers) we get containers from all ContentTypes. It also means we need to do an extra filtering to ensure we only get one of each containers. [NL]

				// For that we get the owner containers first (We do not need to observe as this observation will be triggered if one of the owner containers change) [NL]
				this._ownerContainers = this.#structure!.getOwnerContainers(this._childType!, this._parentId!) ?? [];

				// Then we filter out the duplicate containers based on type and name:
				rootContainers = rootContainers.filter(
					(x, i, cons) => i === cons.findIndex((y) => y.name === x.name && y.type === x.type),
				);

				this.#mergedContainers.setValue(this.#filterNonOwnerContainers(rootContainers));
			},
			'_observeRootContainers',
		);
	}

	/**
	 * Returns true if the container is an owner container.
	 */
	isOwnerChildContainer(containerId?: string) {
		if (!this.#structure || !containerId) return;
		return this._ownerContainers.some((x) => x.id === containerId);
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
}
