import { UmbDocumentWorkspaceContext } from '../../../../documents/documents/workspace/document-workspace.context';
import { PropertyContainerTypes } from './workspace-structure-manager.class';
import { PropertyTypeContainerResponseModelBaseModel } from '@umbraco-cms/backoffice/backend-api';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller';
import { UmbContextConsumerController, UMB_ENTITY_WORKSPACE_CONTEXT } from '@umbraco-cms/backoffice/context-api';
import { ArrayState, BooleanState, UmbObserverController } from '@umbraco-cms/backoffice/observable-api';

export class UmbWorkspaceContainerStructureHelper {
	#host: UmbControllerHostElement;

	#workspaceContext?: UmbDocumentWorkspaceContext;

	private _ownerType?: PropertyContainerTypes = 'Tab';
	private _childType?: PropertyContainerTypes = 'Group';
	private _isRoot = false;
	private _ownerName?: string;
	private _ownerKey?: string;

	// Containers defined in data might be more than actual containers to display as we merge them by name.
	private _ownerContainers: PropertyTypeContainerResponseModelBaseModel[] = [];

	// State containing the merged containers (only one pr. name):
	#containers = new ArrayState<PropertyTypeContainerResponseModelBaseModel>([], (x) => x.key);
	readonly containers = this.#containers.asObservable();

	#hasProperties = new BooleanState(false);
	readonly hasProperties = this.#hasProperties.asObservable();

	constructor(host: UmbControllerHostElement) {
		this.#host = host;

		this.#containers.sortBy((a, b) => (a.sortOrder || 0) - (b.sortOrder || 0));

		new UmbContextConsumerController(host, UMB_ENTITY_WORKSPACE_CONTEXT, (context) => {
			this.#workspaceContext = context as UmbDocumentWorkspaceContext;
			this._observeOwnerContainers();
		});
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
		if (!this.#workspaceContext) return;

		if (this._isRoot) {
			this.#containers.next([]);
			// We cannot have root properties currently, therefor we set it to false:
			this.#hasProperties.next(false);
			this._observeRootContainers();
		} else if (this._ownerName && this._ownerType) {
			new UmbObserverController(
				this.#host,
				this.#workspaceContext.structure.containersByNameAndType(this._ownerName, this._ownerType),
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
		if (!this.#workspaceContext) return;

		this._ownerContainers.forEach((container) => {
			new UmbObserverController(
				this.#host,
				this.#workspaceContext!.structure.hasPropertyStructuresOf(container.key!),
				(hasProperties) => {
					this.#hasProperties.next(hasProperties);
				},
				'_observeOwnerHasProperties_' + container.key
			);
		});
	}

	private _observeChildContainers() {
		if (!this.#workspaceContext || !this._ownerName || !this._childType) return;

		this._ownerContainers.forEach((container) => {
			new UmbObserverController(
				this.#host,
				this.#workspaceContext!.structure.containersOfParentKey(container.key, this._childType!),
				this._insertGroupContainers,
				'_observeGroupsOf_' + container.key
			);
		});
	}

	private _observeRootContainers() {
		if (!this.#workspaceContext || !this._isRoot) return;

		new UmbObserverController(
			this.#host,
			this.#workspaceContext!.structure.rootContainers(this._childType!),
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

	/** Manipulate methods: */

	async addGroup(ownerKey?: string, sortOrder?: number) {
		if (!this.#workspaceContext) return;

		await this.#workspaceContext.structure.createContainer(null, ownerKey, this._childType, sortOrder);
	}
}
