import { PropertyContainerTypes, UmbContentTypePropertyStructureManager } from './content-type-structure-manager.class';
import { PropertyTypeContainerResponseModelBaseModel } from 'src/libs/backend-api';
import { UmbControllerHostElement } from 'src/libs/controller-api';
import { UmbArrayState, UmbBooleanState, UmbObserverController } from 'src/libs/observable-api';

export class UmbContentTypeContainerStructureHelper {
	#host: UmbControllerHostElement;
	#init;
	#initResolver?: (value: unknown) => void;

	#structure?: UmbContentTypePropertyStructureManager;

	private _ownerType?: PropertyContainerTypes = 'Tab';
	private _childType?: PropertyContainerTypes = 'Group';
	private _isRoot = false;
	private _ownerName?: string;
	private _ownerKey?: string;

	// Containers defined in data might be more than actual containers to display as we merge them by name.
	// Owner containers are the containers defining the total of this container(Multiple containers with the same name and type)
	private _ownerContainers: PropertyTypeContainerResponseModelBaseModel[] = [];

	// State containing the merged containers (only one pr. name):
	#containers = new UmbArrayState<PropertyTypeContainerResponseModelBaseModel>([], (x) => x.id);
	readonly containers = this.#containers.asObservable();

	#hasProperties = new UmbBooleanState(false);
	readonly hasProperties = this.#hasProperties.asObservable();

	constructor(host: UmbControllerHostElement) {
		this.#host = host;
		this.#init = new Promise((resolve) => {
			this.#initResolver = resolve;
		});

		this.#containers.sortBy((a, b) => (a.sortOrder || 0) - (b.sortOrder || 0));
	}

	public setStructureManager(structure: UmbContentTypePropertyStructureManager) {
		this.#structure = structure;
		this.#initResolver?.(undefined);
		this.#initResolver = undefined;
		this._observeOwnerContainers();
	}

	public setType(value?: PropertyContainerTypes) {
		if (this._ownerType === value) return;
		this._ownerType = value;
		this._observeOwnerContainers();
	}
	public getType() {
		return this._ownerType;
	}

	public setContainerChildType(value?: PropertyContainerTypes) {
		if (this._childType === value) return;
		this._childType = value;
		this._observeOwnerContainers();
	}
	public getContainerChildType() {
		return this._childType;
	}

	public setName(value?: string) {
		if (this._ownerName === value) return;
		this._ownerName = value;
		this._observeOwnerContainers();
	}
	public getName() {
		return this._ownerName;
	}

	public setIsRoot(value: boolean) {
		if (this._isRoot === value) return;
		this._isRoot = value;
		this._observeOwnerContainers();
	}
	public getIsRoot() {
		return this._isRoot;
	}

	private _observeOwnerContainers() {
		if (!this.#structure) return;

		if (this._isRoot) {
			this.#containers.next([]);
			// We cannot have root properties currently, therefor we set it to false:
			this.#hasProperties.next(false);
			this._observeRootContainers();
		} else if (this._ownerName && this._ownerType) {
			new UmbObserverController(
				this.#host,
				this.#structure.containersByNameAndType(this._ownerName, this._ownerType),
				(ownerContainers) => {
					this.#containers.next([]);
					this._ownerContainers = ownerContainers || [];
					if (this._ownerContainers.length > 0) {
						this._observeOwnerProperties();
						this._observeChildContainers();
					}
				},
				'_observeOwnerContainers'
			);
		}
	}

	private _observeOwnerProperties() {
		if (!this.#structure) return;

		this._ownerContainers.forEach((container) => {
			new UmbObserverController(
				this.#host,
				this.#structure!.hasPropertyStructuresOf(container.id!),
				(hasProperties) => {
					this.#hasProperties.next(hasProperties);
				},
				'_observeOwnerHasProperties_' + container.id
			);
		});
	}

	private _observeChildContainers() {
		if (!this.#structure || !this._ownerName || !this._childType) return;

		this._ownerContainers.forEach((container) => {
			new UmbObserverController(
				this.#host,
				this.#structure!.containersOfParentKey(container.id, this._childType!),
				this._insertGroupContainers,
				'_observeGroupsOf_' + container.id
			);
		});
	}

	private _observeRootContainers() {
		if (!this.#structure || !this._isRoot) return;

		new UmbObserverController(
			this.#host,
			this.#structure.rootContainers(this._childType!),
			(rootContainers) => {
				this.#containers.next([]);
				this._insertGroupContainers(rootContainers);
			},
			'_observeRootContainers'
		);
	}

	private _insertGroupContainers = (groupContainers: PropertyTypeContainerResponseModelBaseModel[]) => {
		groupContainers.forEach((group) => {
			if (group.name !== null && group.name !== undefined) {
				if (!this.#containers.getValue().find((x) => x.name === group.name)) {
					this.#containers.appendOne(group);
				}
			}
		});
	};

	/**
	 * Returns true if the container is an owner container.
	 */
	isOwnerContainer(groupId?: string) {
		if (!this.#structure || !groupId) return;

		return this._ownerContainers.find((x) => x.id === groupId) !== undefined;
	}

	/** Manipulate methods: */

	async addContainer(ownerId?: string, sortOrder?: number) {
		if (!this.#structure) return;

		await this.#structure.createContainer(null, ownerId, this._childType, sortOrder);
	}

	async partialUpdateContainer(groupId?: string, partialUpdate?: Partial<PropertyTypeContainerResponseModelBaseModel>) {
		await this.#init;
		if (!this.#structure || !groupId || !partialUpdate) return;

		return await this.#structure.updateContainer(null, groupId, partialUpdate);
	}
}
