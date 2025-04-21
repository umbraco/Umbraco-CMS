import type { UmbContentTypeModel, UmbPropertyContainerTypes, UmbPropertyTypeContainerModel } from '../types.js';
import type { UmbContentTypeStructureManager } from './content-type-structure-manager.class.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbController, UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbArrayState } from '@umbraco-cms/backoffice/observable-api';

/**
 * This class is a helper class for managing the structure of containers in a content type.
 * This requires a structure manager {@link UmbContentTypeStructureManager} to manage the structure.
 */
export class UmbContentTypeContainerStructureHelper<T extends UmbContentTypeModel> extends UmbControllerBase {
	#init;
	#initResolver?: (value: unknown) => void;

	#containerId?: string | null;
	#childType?: UmbPropertyContainerTypes = 'Group';

	#structure?: UmbContentTypeStructureManager<T>;

	#containerObservers: Array<UmbController> = [];

	// State containing the all containers defined in the data:
	#childContainers = new UmbArrayState<UmbPropertyTypeContainerModel>([], (x) => x.id);
	readonly containers = this.#childContainers.asObservable();

	// State containing the merged containers (only one pr. name):
	#mergedChildContainers = new UmbArrayState<UmbPropertyTypeContainerModel>([], (x) => x.id);
	readonly mergedContainers = this.#mergedChildContainers.asObservable();

	// Owner containers are containers owned by the owner Content Type (The specific one up for editing)
	#ownerChildContainers: UmbPropertyTypeContainerModel[] = [];

	#hasProperties = new UmbArrayState<{ id: string | null; has: boolean }>([], (x) => x.id);
	readonly hasProperties = this.#hasProperties.asObservablePart((x) => x.some((y) => y.has));

	constructor(host: UmbControllerHost) {
		super(host);
		this.#init = new Promise((resolve) => {
			this.#initResolver = resolve;
		});

		this.#mergedChildContainers.sortBy((a, b) => (a.sortOrder || 0) - (b.sortOrder || 0));
		this.observe(this.containers, this.#performContainerMerge, null);
	}

	public setStructureManager(structure: UmbContentTypeStructureManager<T> | undefined) {
		if (this.#structure === structure || !structure) return;
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
		return this.#containerId === null;
	}

	public setContainerId(value: string | null | undefined) {
		if (this.#containerId === value) return;
		this.#containerId = value;
		this.#observeContainers();
	}
	public getContainerId() {
		return this.#containerId;
	}

	public setContainerChildType(value?: UmbPropertyContainerTypes) {
		if (this.#childType === value) return;
		this.#childType = value;
		this.#observeContainers();
	}
	public getContainerChildType() {
		return this.#childType;
	}

	#containerName?: string;
	#containerType?: UmbPropertyContainerTypes;
	#parentName?: string | null;
	#parentType?: UmbPropertyContainerTypes;

	#observeContainers() {
		if (!this.#structure || this.#containerId === undefined) return;

		if (this.#containerId === null) {
			this.#observeHasPropertiesOf(null);
			this.#observeRootContainers();
			this.removeUmbControllerByAlias('_observeContainers');
		} else {
			this.observe(
				this.#structure.containerById(this.#containerId),
				(container) => {
					if (container) {
						this.#containerName = container.name ?? '';
						this.#containerType = container.type;
						if (container.parent) {
							// We have a parent for our main container, so lets observe that one as well:
							this.observe(
								this.#structure!.containerById(container.parent.id),
								(parent) => {
									if (parent) {
										this.#parentName = parent.name ?? '';
										this.#parentType = parent.type;
										this.#observeSimilarContainers();
									} else {
										this.removeUmbControllerByAlias('_observeContainers');
										this.#parentName = undefined;
										this.#parentType = undefined;
									}
								},
								'_observeMainParentContainer',
							);
						} else {
							this.removeUmbControllerByAlias('_observeMainParentContainer');
							this.#parentName = null; //In this way we want to look for one without a parent. [NL]
							this.#parentType = undefined;
							this.#observeSimilarContainers();
						}
					} else {
						this.removeUmbControllerByAlias('_observeContainers');
						this.#containerName = undefined;
						this.#containerType = undefined;
						// TODO: reset has Properties.
						this.#hasProperties.setValue([]);
					}
				},
				'_observeMainContainer',
			);
		}
	}

	#observeSimilarContainers() {
		if (this.#containerName === undefined || !this.#containerType || this.#parentName === undefined) return;
		this.observe(
			this.#structure!.containersByNameAndTypeAndParent(
				this.#containerName,
				this.#containerType,
				this.#parentName,
				this.#parentType,
			),
			(containers) => {
				this.#hasProperties.setValue([]);
				this.#childContainers.setValue([]);
				this.#containerObservers.forEach((x) => x.destroy());
				this.#containerObservers = [];

				containers.forEach((container) => {
					this.#observeHasPropertiesOf(container.id);

					this.#containerObservers.push(
						this.observe(
							this.#structure!.containersOfParentId(container.id, this.#childType!),
							(containers) => {
								// get the direct owner containers of this container id: [NL]
								this.#ownerChildContainers =
									this.#structure!.getOwnerContainers(this.#childType!, this.#containerId!) ?? [];

								// Remove existing containers that are not the parent of the new containers: [NL]
								this.#childContainers.filter(
									(x) => x.parent?.id !== container.id || containers.some((y) => y.id === x.id),
								);

								this.#childContainers.append(containers);
							},
							'_observeGroupsOf_' + container.id,
						),
					);
				});
			},
			'_observeContainers',
		);
	}

	#observeRootContainers() {
		if (!this.#structure || !this.#childType || this.#containerId === undefined) return;

		this.observe(
			this.#structure.rootContainers(this.#childType),
			(rootContainers) => {
				// Here (When getting root containers) we get containers from all ContentTypes. It also means we need to do an extra filtering to ensure we only get one of each containers. [NL]

				// For that we get the owner containers first (We do not need to observe as this observation will be triggered if one of the owner containers change) [NL]
				this.#ownerChildContainers = this.#structure!.getOwnerContainers(this.#childType!, this.#containerId!) ?? [];
				this.#childContainers.setValue(rootContainers);
			},
			'_observeRootContainers',
		);
	}

	#observeHasPropertiesOf(groupId?: string | null) {
		if (!this.#structure || groupId === undefined) return;

		this.observe(
			this.#structure.hasPropertyStructuresOf(groupId),
			(hasProperties) => {
				this.#hasProperties.appendOne({ id: groupId, has: hasProperties });
			},
			'_observePropertyStructureOfGroup' + groupId,
		);
	}

	#filterNonOwnerContainers(containers: Array<UmbPropertyTypeContainerModel>) {
		return this.#ownerChildContainers.length > 0
			? containers.filter(
					(anyCon) =>
						!this.#ownerChildContainers.some(
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
		this.#mergedChildContainers.setValue(merged);
	};

	/**
	 * Returns true if the container is an owner container.
	 * @param containerId
	 */
	isOwnerChildContainer(containerId?: string) {
		if (!this.#structure || !containerId) return;
		return this.#ownerChildContainers.some((x) => x.id === containerId);
	}

	getContentTypeOfContainer(containerId?: string) {
		if (!this.#structure || !containerId) return;
		return this.#structure.getContentTypeOfContainer(containerId);
	}

	containersByNameAndType(name: string, type: UmbPropertyContainerTypes) {
		return this.#childContainers.asObservablePart((cons) => cons.filter((x) => x.name === name && x.type === type));
	}

	/** Manipulate methods: */

	/*async insertContainer(container: UmbPropertyTypeContainerModel, sortOrder = 0) {
		await this.#init;
		if (!this.#structure) return false;

		const newContainer = { ...container, sortOrder };

		await this.#structure.insertContainer(null, newContainer);
		return true;
	}*/

	async addContainer(parentContainerId?: string | null, sortOrder?: number) {
		if (!this.#structure) return;

		await this.#structure.createContainer(null, parentContainerId, this.#childType, sortOrder);
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
