import {
	PropertyContainerTypes,
	UmbContentTypePropertyStructureManager,
} from './content-type-structure-manager.class.js';
import {
	DocumentTypePropertyTypeResponseModel,
	PropertyTypeModelBaseModel,
} from '@umbraco-cms/backoffice/backend-api';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { UmbArrayState, UmbObserverController } from '@umbraco-cms/backoffice/observable-api';

export class UmbContentTypePropertyStructureHelper {
	#host: UmbControllerHostElement;
	#init;
	#initResolver?: (value: unknown) => void;

	#structure?: UmbContentTypePropertyStructureManager;

	private _containerType?: PropertyContainerTypes;
	private _isRoot?: boolean;
	private _containerName?: string;

	#propertyStructure = new UmbArrayState<DocumentTypePropertyTypeResponseModel>([], (x) => x.id);
	readonly propertyStructure = this.#propertyStructure.asObservable();

	constructor(host: UmbControllerHostElement) {
		this.#host = host;
		this.#init = new Promise((resolve) => {
			this.#initResolver = resolve;
		});
		// TODO: Remove as any when sortOrder is implemented:
		this.#propertyStructure.sortBy((a, b) => ((a as any).sortOrder ?? 0) - ((b as any).sortOrder ?? 0));
	}

	public setStructureManager(structure: UmbContentTypePropertyStructureManager) {
		this.#structure = structure;
		this.#initResolver?.(undefined);
		this.#initResolver = undefined;
		this._observeGroupContainers();
	}

	public setContainerType(value?: PropertyContainerTypes) {
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

	private _observeGroupContainers() {
		if (!this.#structure || !this._containerType) return;

		if (this._isRoot === true) {
			this._observePropertyStructureOf(null);
		} else if (this._containerName !== undefined) {
			new UmbObserverController(
				this.#host,
				this.#structure.containersByNameAndType(this._containerName, this._containerType),
				(groupContainers) => {
					groupContainers.forEach((group) => this._observePropertyStructureOf(group.id));
				},
				'_observeGroupContainers'
			);
		}
	}

	private _observePropertyStructureOf(groupId?: string | null) {
		if (!this.#structure || groupId === undefined) return;

		new UmbObserverController(
			this.#host,
			this.#structure.propertyStructuresOf(groupId),
			(properties) => {
				// If this need to be able to remove properties, we need to clean out the ones of this group.id before inserting them:
				const _propertyStructure = this.#propertyStructure.getValue().filter((x) => x.containerId !== groupId);

				properties?.forEach((property) => {
					if (!_propertyStructure.find((x) => x.alias === property.alias)) {
						_propertyStructure.push(property);
					}
				});

				// Fire update to subscribers:
				this.#propertyStructure.next(_propertyStructure);
			},
			'_observePropertyStructureOfGroup' + groupId
		);
	}

	// TODO: consider moving this to another class, to separate 'viewer' from 'manipulator':
	/** Manipulate methods: */

	async addProperty(ownerId?: string, sortOrder?: number) {
		await this.#init;
		if (!this.#structure) return;

		return await this.#structure.createProperty(null, ownerId, sortOrder);
	}

	async insertProperty(property: PropertyTypeModelBaseModel, sortOrder = 0) {
		await this.#init;
		if (!this.#structure) return false;

		const newProperty = { ...property, sortOrder };

		// TODO: Remove as any when server model has gotten sortOrder:
		await this.#structure.insertProperty(null, newProperty);
		return true;
	}

	async removeProperty(propertyId: string) {
		await this.#init;
		if (!this.#structure) return false;

		await this.#structure.removeProperty(null, propertyId);
		return true;
	}

	// Takes optional arguments as this is easier for the implementation in the view:
	async partialUpdateProperty(propertyKey?: string, partialUpdate?: Partial<DocumentTypePropertyTypeResponseModel>) {
		await this.#init;
		if (!this.#structure || !propertyKey || !partialUpdate) return;
		console.log("partial update", propertyKey, partialUpdate);
		return await this.#structure.updateProperty(null, propertyKey, partialUpdate);
	}
}
