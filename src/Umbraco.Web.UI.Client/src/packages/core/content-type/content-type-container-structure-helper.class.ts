import {
	PropertyContainerTypes,
	UmbContentTypePropertyStructureManager,
} from './content-type-structure-manager.class.js';
import { PropertyTypeContainerModelBaseModel } from '@umbraco-cms/backoffice/backend-api';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { UmbArrayState, UmbBooleanState, UmbObserverController } from '@umbraco-cms/backoffice/observable-api';

export class UmbContentTypeContainerStructureHelper {
	#host: UmbControllerHostElement;
	#init;
	#initResolver?: (value: unknown) => void;

	#structure?: UmbContentTypePropertyStructureManager;

	private _ownerType?: PropertyContainerTypes = 'Tab';
	private _childType?: PropertyContainerTypes = 'Group';
	private _isRoot = false;
	private _ownerName?: string;

	// Containers defined in data might be more than actual containers to display as we merge them by name.
	// Direct containers are the containers defining the total of this container(Multiple containers with the same name and type)
	private _directContainers: PropertyTypeContainerModelBaseModel[] = [];
	// Owner containers are containers owned by the owner Document Type (The specific one up for editing)
	private _ownerContainers: PropertyTypeContainerModelBaseModel[] = [];

	// State containing the merged containers (only one pr. name):
	#containers = new UmbArrayState<PropertyTypeContainerModelBaseModel>([], (x) => x.id);
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
		this._observeDirectContainers();
	}

	public setType(value?: PropertyContainerTypes) {
		if (this._ownerType === value) return;
		this._ownerType = value;
		this._observeDirectContainers();
	}
	public getType() {
		return this._ownerType;
	}

	public setContainerChildType(value?: PropertyContainerTypes) {
		if (this._childType === value) return;
		this._childType = value;
		this._observeDirectContainers();
	}
	public getContainerChildType() {
		return this._childType;
	}

	public setName(value?: string) {
		if (this._ownerName === value) return;
		this._ownerName = value;
		this._observeDirectContainers();
	}
	public getName() {
		return this._ownerName;
	}

	public setIsRoot(value: boolean) {
		if (this._isRoot === value) return;
		this._isRoot = value;
		this._observeDirectContainers();
	}
	public getIsRoot() {
		return this._isRoot;
	}

	private _observeDirectContainers() {
		if (!this.#structure || !this._ownerType) return;

		if (this._isRoot) {
			this.#containers.next([]);
			// We cannot have root properties currently, therefor we set it to false:
			this.#hasProperties.next(false);
			this._observeRootContainers();
			new UmbObserverController(
				this.#host,
				this.#structure.ownerContainersOf(this._ownerType),
				(ownerContainers) => {
					this._ownerContainers = ownerContainers || [];
				},
				'_observeOwnerContainers'
			);
		} else if (this._ownerName) {
			new UmbObserverController(
				this.#host,
				this.#structure.containersByNameAndType(this._ownerName, this._ownerType),
				(ownerContainers) => {
					this.#containers.next([]);
					this._ownerContainers = ownerContainers || [];
					this._directContainers = ownerContainers || [];
					if (this._directContainers.length > 0) {
						this._observeChildContainerProperties();
						this._observeChildContainers();
					}
				},
				'_observeOwnerContainers'
			);
		}
	}

	private _observeChildContainerProperties() {
		if (!this.#structure) return;

		this._directContainers.forEach((container) => {
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

		this._directContainers.forEach((container) => {
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

	private _insertGroupContainers = (groupContainers: PropertyTypeContainerModelBaseModel[]) => {
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
	isOwnerContainer(containerId?: string) {
		if (!this.#structure || !containerId) return;

		return this._ownerContainers.find((x) => x.id === containerId) !== undefined;
	}

	/** Manipulate methods: */

	async addContainer(parentContainerId?: string, sortOrder?: number) {
		if (!this.#structure) return;

		await this.#structure.createContainer(null, parentContainerId, this._childType, sortOrder);
	}

	async partialUpdateContainer(
		containerId?: string,
		partialUpdate?: Partial<PropertyTypeContainerModelBaseModel>
	) {
		await this.#init;
		if (!this.#structure || !containerId || !partialUpdate) return;

		return await this.#structure.updateContainer(null, containerId, partialUpdate);
	}
}
