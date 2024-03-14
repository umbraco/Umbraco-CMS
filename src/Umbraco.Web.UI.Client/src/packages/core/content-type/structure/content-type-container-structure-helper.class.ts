import type { UmbContentTypeModel, UmbPropertyContainerTypes, UmbPropertyTypeContainerModel } from '../types.js';
import type { UmbContentTypeStructureManager } from './content-type-structure-manager.class.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbArrayState, UmbBooleanState } from '@umbraco-cms/backoffice/observable-api';

/**
 * This class is a helper class for managing the structure of containers in a content type.
 * This requires a structure manager {@link UmbContentTypeStructureManager} to manage the structure.
 */
export class UmbContentTypeContainerStructureHelper<T extends UmbContentTypeModel> extends UmbControllerBase {
	#init;
	#initResolver?: (value: unknown) => void;

	_containerId?: string | null;
	_childType?: UmbPropertyContainerTypes = 'Group';

	#structure?: UmbContentTypeStructureManager<T>;

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

	public setStructureManager(structure: UmbContentTypeStructureManager<T>) {
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
									}
								},
								'_observeMainParentContainer',
							);
						} else {
							this.removeControllerByAlias('_observeMainParentContainer');
							this._parentName = null; //In this way we want to look for one without a parent. [NL]
							this._parentType = undefined;
							this.#observeSimilarContainers();
						}
					} else {
						this.removeControllerByAlias('_observeContainers');
						this._containerName = undefined;
						this._containerType = undefined;
						// TODO: reset has Properties.
						this.#hasProperties.setValue(false);
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
			},
			'_observeContainers',
		);
	}

	#observeRootContainers() {
		if (!this.#structure || !this._childType || !this._containerId === undefined) return;

		this.observe(
			this.#structure.rootContainers(this._childType),
			(rootContainers) => {
				// Here (When getting root containers) we get containers from all ContentTypes. It also means we need to do an extra filtering to ensure we only get one of each containers. [NL]

				// For that we get the owner containers first (We do not need to observe as this observation will be triggered if one of the owner containers change) [NL]
				this._ownerContainers = this.#structure!.getOwnerContainers(this._childType!, this._containerId!) ?? [];
				this.#containers.setValue(rootContainers);
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
	};

	/**
	 * Returns true if the container is an owner container.
	 */
	isOwnerChildContainer(containerId?: string) {
		if (!this.#structure || !containerId) return;
		return this._ownerContainers.some((x) => x.id === containerId);
	}

	containersByNameAndType(name: string, type: UmbPropertyContainerTypes) {
		return this.#containers.asObservablePart((cons) => cons.filter((x) => x.name === name && x.type === type));
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
