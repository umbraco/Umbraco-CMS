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

	_containerId?: string | null;
	_childType?: UmbPropertyContainerTypes = 'Group';

	#structure?: UmbContentTypePropertyStructureManager<T>;

	// State containing the all containers defined in the data:
	#containers = new UmbArrayState<UmbPropertyTypeContainerModel>([], (x) => x.id);
	readonly containers = this.#containers.asObservable();

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
		this.observe(this.containers, this.#performContainerMerge, null);
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
		this.#observeContainers();
	}

	public getStructureManager() {
		return this.#structure;
	}

	public setIsRoot(value: boolean) {
		if (value === true) {
			this.setContainerId(null);
		}
	}
	public getIsRoot() {
		return this._containerId === null;
	}

	public setContainerId(value: string | null | undefined) {
		if (this._containerId === value) return;
		this._containerId = value;
		this.#observeContainers();
	}
	public getContainerId() {
		return this._containerId;
	}

	public setContainerChildType(value?: UmbPropertyContainerTypes) {
		if (this._childType === value) return;
		this._childType = value;
		this.#observeContainers();
	}
	public getContainerChildType() {
		return this._childType;
	}

	/*
	#observeParentAlikeContainers() {
		if (!this.#structure) return;

		if (this._isRoot) {
			// CLean up:
			this._parentMatchingContainers.forEach((container) => {
				this.removeControllerByAlias('_observeParentHasProperties_' + container.id);
				this.removeControllerByAlias('_observeGroupsOf_' + container.id);
			});
			this._parentMatchingContainers = [];
			this.removeControllerByAlias('_observeOwnerContainers');
			this.#containers.setValue([]);
			this.#mergedContainers.setValue([]);
			//this._observeChildProperties(); // We cannot have root properties currently, therefor we instead just set it to false:
			this.#hasProperties.setValue(false);
			this.#observeRootContainers();
		} else if (this._parentName && this._parentType) {
			this.#containers.setValue([]);
			this.#mergedContainers.setValue([]);
			this.observe(
				// This only works because we just have two levels, meaning this is the upper level and we want it to merge, so its okay this does not take parent-parent (And further structure) into account: [NL]
				this.#structure.containersByNameAndType(this._parentName, this._parentType),
				(parentContainers) => {
					this._ownerContainers = [];
					this.#containers.setValue([]);
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
				this.#containers.append(this._ownerContainers);
				//this.#mergedContainers.setValue(this.#filterNonOwnerContainers(this.#mergedContainers.getValue()));
			},
			'_observeOwnerContainers',
		);

		this._parentMatchingContainers.forEach((parentCon) => {
			this.observe(
				this.#structure!.containersOfParentId(parentCon.id, this._childType!),
				(containers) => {
					// Problem this will never remove a container? [NL]
					this.#containers.append(containers);

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

	*/

	private _containerName?: string;
	private _containerType?: UmbPropertyContainerTypes;
	private _parentName?: string | null;
	private _parentType?: UmbPropertyContainerTypes;

	#observeContainers() {
		if (!this.#structure || this._containerId === undefined) return;

		if (this._containerId === null) {
			this.#observeHasPropertiesOf(null);
			this.#observeRootContainers();
			this.removeControllerByAlias('_observeContainers');
		} else {
			this.observe(
				this.#structure.containerById(this._containerId),
				(container) => {
					if (container) {
						this._containerName = container.name ?? '';
						this._containerType = container.type;
						if (container.parent) {
							// We have a parent for our main container, so lets observe that one as well:
							this.observe(
								this.#structure!.containerById(container.parent.id),
								(parent) => {
									if (parent) {
										this._parentName = parent.name ?? '';
										this._parentType = parent.type;
										this.#observeSimilarContainers();
									} else {
										this.removeControllerByAlias('_observeContainers');
										this._parentName = undefined;
										this._parentType = undefined;
										// TODO: reset has Properties.
										throw new Error('Main parent container does not exist');
									}
								},
								'_observeMainParentContainer',
							);
						} else {
							this._parentName = null; //In this way we want to look for one without a parent. [NL]
							this._parentType = undefined;
							this.removeControllerByAlias('_observeMainParentContainer');
							this.#observeSimilarContainers();
						}
					} else {
						this._containerName = undefined;
						this._containerType = undefined;
						// TODO: reset has Properties.
						this.removeControllerByAlias('_observeContainers');
						throw new Error('Main container does not exist');
					}
				},
				'_observeMainContainer',
			);
		}
	}

	#observeSimilarContainers() {
		if (!this._containerName || !this._containerType || this._parentName === undefined) return;
		this.observe(
			this.#structure!.containersByNameAndTypeAndParent(
				this._containerName,
				this._containerType,
				this._parentName,
				this._parentType,
			),
			(containers) => {
				// We want to remove hasProperties of groups that does not exist anymore.:
				// this.#removeHasPropertiesOfGroup()
				this.#hasProperties.setValue(false);
				this.#containers.setValue([]);

				containers.forEach((container) => {
					this.#observeHasPropertiesOf(container.id);

					this.observe(
						this.#structure!.containersOfParentId(container.id, this._childType!),
						(containers) => {
							// Remove existing containers that are not the parent of the new containers:
							this.#containers.filter((x) => x.parent?.id !== container.id || containers.some((y) => y.id === x.id));

							this.#containers.append(containers);
						},
						'_observeGroupsOf_' + container.id,
					);
				});

				//this._ownerContainers = this.#structure!.getOwnerContainers(this._containerType!, this._containerId!) ?? [];
				//this.#containers.setValue(groupContainers);
			},
			'_observeContainers',
		);
	}

	#observeRootContainers() {
		if (!this.#structure || !this._childType || !this._containerId === undefined) return;

		this.observe(
			this.#structure.rootContainers(this._childType),
			(rootContainers) => {
				console.log('root containers', rootContainers);
				// Here (When getting root containers) we get containers from all ContentTypes. It also means we need to do an extra filtering to ensure we only get one of each containers. [NL]

				// For that we get the owner containers first (We do not need to observe as this observation will be triggered if one of the owner containers change) [NL]
				this._ownerContainers = this.#structure!.getOwnerContainers(this._childType!, this._containerId!) ?? [];
				this.#containers.setValue(rootContainers);
				/*
				// Then we filter out the duplicate containers based on type and name:
				rootContainers = rootContainers.filter(
					(x, i, cons) => i === cons.findIndex((y) => y.name === x.name && y.type === x.type),
				);

				this.#mergedContainers.setValue(this.#filterNonOwnerContainers(rootContainers));
				*/
			},
			'_observeRootContainers',
		);
	}

	#observeHasPropertiesOf(groupId?: string | null) {
		if (!this.#structure || groupId === undefined) return;

		this.observe(
			this.#structure.hasPropertyStructuresOf(groupId),
			(hasProperties) => {
				// TODO: Make this an array/map/state, so we only change the groupId. then hasProperties should be a observablePart that checks the array for true. [NL]
				this.#hasProperties.setValue(hasProperties);
			},
			'_observePropertyStructureOfGroup' + groupId,
		);
	}

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

	#performContainerMerge = (containers: Array<UmbPropertyTypeContainerModel>) => {
		// Remove containers that matches with a owner container:
		let merged = this.#filterNonOwnerContainers(containers);
		// Remove containers of same name and type:
		// This only works cause we are dealing with a single level of containers in this Helper, if we had more levels we would need to be more clever about the parent as well. [NL]
		merged = merged.filter((x, i, cons) => i === cons.findIndex((y) => y.name === x.name && y.type === x.type));
		this.#mergedContainers.setValue(merged);
		console.log('merge', containers, ' > ', merged);
	};

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
