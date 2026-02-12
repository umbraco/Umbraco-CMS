import type { UmbContentTypeModel, UmbPropertyTypeModel } from '../types.js';
import type { UmbContentTypeStructureManager } from './content-type-structure-manager.class.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbArrayState } from '@umbraco-cms/backoffice/observable-api';

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
	readonly propertyAliases = this.#propertyStructure.asObservablePart((x) => x.map((e) => e.alias));

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
		if (this.#structure === structure || !structure) return;
		if (this.#structure && !structure) {
			throw new Error(
				'Structure manager is already set, the helpers are not designed to be re-setup with new managers',
			);
		}
		this.#structure = structure;
		this.#initResolver?.(undefined);
		this.#initResolver = undefined;
		this.#observeContainer();
	}

	public getStructureManager() {
		return this.#structure;
	}

	public setContainerId(value?: string | null) {
		if (this.#containerId === value) return;
		this.#containerId = value;
		this.#observeContainer();
	}
	public getContainerId() {
		return this.#containerId;
	}

	#observeContainer() {
		// Handle three cases:
		// 1. containerId === undefined → Not set yet, clear properties
		// 2. containerId === null → Root properties (no container)
		// 3. containerId === string → Container-based properties

		if (this.#containerId === undefined) {
			// Not set yet, clear properties
			this.#propertyStructure.setValue([]);
			this.removeUmbControllerByAlias('observeProperties');
			this.removeUmbControllerByAlias('observeContainer');
			return;
		}

		if (this.#containerId === null) {
			// Root properties (no container)
			this.removeUmbControllerByAlias('observeContainer');
			this.observe(
				this.#structure?.rootPropertyStructures(),
				(properties) => {
					this.#propertyStructure.setValue(properties ?? []);
				},
				'observeProperties',
			);
			return;
		}

		// Container-based properties
		this.observe(
			this.#structure?.mergedContainersOfId(this.#containerId),
			(container) => {
				this.observe(
					container ? this.#structure?.propertyStructuresOfGroupIds(container.ids ?? []) : undefined,
					(properties) => {
						this.#propertyStructure.setValue(properties ?? []);
					},
					'observeProperties',
				);
			},
			'observeContainer',
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
