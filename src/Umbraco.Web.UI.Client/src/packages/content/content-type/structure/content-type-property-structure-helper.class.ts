import type {
	UmbContentTypeModel,
	UmbPropertyContainerTypes,
	UmbPropertyTypeContainerModel,
	UmbPropertyTypeModel,
} from '../types.js';
import type { UmbContentTypeStructureManager } from './content-type-structure-manager.class.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbArrayState, mergeObservables } from '@umbraco-cms/backoffice/observable-api';

type UmbPropertyTypeUnique = UmbPropertyTypeModel['unique'];

/**
 * This class is a helper class for managing the structure of containers in a content type.
 * This requires a structure manager {@link UmbContentTypeStructureManager} to manage the structure.
 */
export class UmbContentTypePropertyStructureHelper<T extends UmbContentTypeModel> extends UmbControllerBase {
	#init;
	#initResolver?: (value: unknown) => void;

	#structure?: UmbContentTypeStructureManager<T>;

	#containerId?: string | null;

	// State which holds all the properties of the current container, this is a composition of all properties from the containers that matches our target [NL]
	#propertyStructure = new UmbArrayState<UmbPropertyTypeModel>([], (x) => x.unique);
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

	public setContainerId(value?: string | null) {
		if (this.#containerId === value) return;
		this.#containerId = value;
		this.#observeContainers();
	}
	public getContainerId() {
		return this.#containerId;
	}

	private _containerName?: string;
	private _containerType?: UmbPropertyContainerTypes;
	private _parentName?: string | null;
	private _parentType?: UmbPropertyContainerTypes;

	#containers?: Array<UmbPropertyTypeContainerModel>;
	#observeContainers() {
		if (!this.#structure || this.#containerId === undefined) return;

		if (this.#containerId === null) {
			this.observe(
				this.#structure.propertyStructuresOf(null),
				(properties) => {
					this.#propertyStructure.setValue(properties);
				},
				'observePropertyStructures',
			);
			this.removeUmbControllerByAlias('_observeContainers');
		} else {
			this.observe(
				this.#structure.containerById(this.#containerId),
				(container) => {
					if (container) {
						this._containerName = container.name ?? '';
						this._containerType = container.type;
						if (container.parent) {
							// We have a parent for our main container, so lets observe that one as well: [NL]
							this.observe(
								this.#structure!.containerById(container.parent.id),
								(parent) => {
									if (parent) {
										this._parentName = parent.name ?? '';
										this._parentType = parent.type;
										this.#observeSimilarContainers();
									} else {
										this.removeUmbControllerByAlias('_observeContainers');
										this._parentName = undefined;
										this._parentType = undefined;
									}
								},
								'_observeMainParentContainer',
							);
						} else {
							this.removeUmbControllerByAlias('_observeMainParentContainer');
							this._parentName = null; //In this way we want to look for one without a parent. [NL]
							this._parentType = undefined;
							this.#observeSimilarContainers();
						}
					} else {
						this.removeUmbControllerByAlias('_observeContainers');
						this._containerName = undefined;
						this._containerType = undefined;
						this.#propertyStructure.setValue([]);
					}
				},
				'_observeMainContainer',
			);
		}
	}

	#observeSimilarContainers() {
		if (this._containerName === undefined || !this._containerType || this._parentName === undefined) return;
		this.observe(
			this.#structure!.containersByNameAndTypeAndParent(
				this._containerName,
				this._containerType,
				this._parentName,
				this._parentType,
			),
			(groupContainers) => {
				if (this.#containers) {
					// We want to remove properties of groups that does not exist anymore: [NL]
					const goneGroupContainers = this.#containers.filter((x) => !groupContainers.some((y) => y.id === x.id));
					const _propertyStructure = this.#propertyStructure
						.getValue()
						.filter((x) => !goneGroupContainers.some((y) => y.id === x.container?.id));
					this.#propertyStructure.setValue(_propertyStructure);
				}

				this.observe(
					mergeObservables(
						groupContainers.map((group) => this.#structure!.propertyStructuresOf(group.id)),
						(sources) => {
							return sources.flatMap((x) => x);
						},
					),
					(properties) => {
						this.#propertyStructure.setValue(properties);
					},
					'observePropertyStructures',
				);
				this.#containers = groupContainers;
			},
			'_observeContainers',
		);
	}

	async isOwnerProperty(propertyUnique: UmbPropertyTypeUnique) {
		await this.#init;
		if (!this.#structure) return;

		return this.#structure.ownerContentTypeObservablePart((x) =>
			x?.properties.some((y) => y.unique === propertyUnique),
		);
	}

	async contentTypeOfProperty(propertyUnique: UmbPropertyTypeUnique) {
		await this.#init;
		if (!this.#structure) return;

		return this.#structure.contentTypeOfProperty(propertyUnique);
	}

	// TODO: consider moving this to another class, to separate 'viewer' from 'manipulator':
	/** Manipulate methods: */

	async insertProperty(property: UmbPropertyTypeModel, sortOrder?: number) {
		await this.#init;
		if (!this.#structure) return false;

		const newProperty = { ...property };
		if (sortOrder) {
			newProperty.sortOrder = sortOrder;
		}

		await this.#structure.insertProperty(null, newProperty);
		return true;
	}

	async removeProperty(propertyUnique: UmbPropertyTypeUnique) {
		await this.#init;
		if (!this.#structure) return false;

		await this.#structure.removeProperty(null, propertyUnique);
		return true;
	}

	// Takes optional arguments as this is easier for the implementation in the view:
	async partialUpdateProperty(propertyKey?: string, partialUpdate?: Partial<UmbPropertyTypeModel>) {
		await this.#init;
		if (!this.#structure || !propertyKey || !partialUpdate) return;
		return await this.#structure.updateProperty(null, propertyKey, partialUpdate);
	}
}
