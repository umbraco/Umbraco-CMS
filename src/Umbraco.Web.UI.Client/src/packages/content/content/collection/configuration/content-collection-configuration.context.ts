import { umbMapDataTypeToCollectionConfiguration } from './map-data-type-to-collection-configuration.function.js';
import { UMB_CONTENT_COLLECTION_CONFIGURATION_CONTEXT } from './content-collection-configuration.context-token.js';
import { UmbContextBase } from '@umbraco-cms/backoffice/class-api';
import {
	UmbBasicState,
	UmbObjectState,
	UmbStringState,
	mergeObservables,
} from '@umbraco-cms/backoffice/observable-api';
import { UmbDataTypeDetailRepository } from '@umbraco-cms/backoffice/data-type';
import type { UmbCollectionConfiguration } from '@umbraco-cms/backoffice/collection';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbDataTypeDetailModel } from '@umbraco-cms/backoffice/data-type';
import type { UmbEntityUnique } from '@umbraco-cms/backoffice/entity';

const DATA_TYPE_OBSERVER_ALIAS = Symbol();

/**
 * Resolves the configuration of a content collection, for any host that can say which collection it renders, which
 * data type configures it and which entity it is scoped to.
 */
export class UmbContentCollectionConfigurationContext extends UmbContextBase {
	#collectionAlias = new UmbStringState<string | undefined>(undefined);
	/**
	 * The alias of the collection to render, e.g. `Umb.Collection.Document`.
	 * Undefined means the host has no collection concept at all.
	 */
	readonly collectionAlias = this.#collectionAlias.asObservable();

	#dataTypeUnique = new UmbStringState<string | undefined>(undefined);
	/**
	 * The data type configuring the collection, taken from the content type's collection reference.
	 * Undefined means no collection is configured.
	 */
	readonly dataTypeUnique = this.#dataTypeUnique.asObservable();

	#unique = new UmbBasicState<UmbEntityUnique | undefined>(undefined);
	/**
	 * The entity the collection is scoped to. `null` is the root; undefined means there is no subject at all, which is
	 * the case for a content type without a hierarchy.
	 */
	readonly unique = this.#unique.asObservable();

	#dataType = new UmbObjectState<UmbDataTypeDetailModel | undefined>(undefined);
	/**
	 * The resolved configuration data type. Exposed so hosts can derive presentation of their own from it — a workspace
	 * view reads its icon and label from here — without that presentation concern moving into this context.
	 */
	readonly dataType = this.#dataType.asObservable();

	/**
	 * Whether a collection should be rendered at all. True only once both a collection alias and a configuring data type
	 * are known.
	 */
	readonly hasCollection = mergeObservables(
		[this.collectionAlias, this.dataTypeUnique],
		([collectionAlias, dataTypeUnique]) => !!collectionAlias && !!dataTypeUnique,
	);

	/**
	 * The resolved collection configuration, or undefined while it is loading and when no collection is configured.
	 */
	readonly collectionConfig = mergeObservables(
		[this.dataType, this.unique],
		([dataType, unique]): UmbCollectionConfiguration | undefined =>
			dataType ? { ...umbMapDataTypeToCollectionConfiguration(dataType), unique } : undefined,
	);

	#dataTypeDetailRepository = new UmbDataTypeDetailRepository(this);

	/**
	 * @param {UmbControllerHost} host - The controller host providing this context.
	 */
	constructor(host: UmbControllerHost) {
		super(host, UMB_CONTENT_COLLECTION_CONFIGURATION_CONTEXT);

		this.observe(
			this.hasCollection,
			(hasCollection) => this.#observeDataType(hasCollection ? this.getDataTypeUnique() : undefined),
			null,
		);
	}

	/**
	 * Sets the alias of the collection to render.
	 * @param {string | undefined} collectionAlias - The collection extension alias, or undefined when the host has no
	 * collection concept.
	 * @memberof UmbContentCollectionConfigurationContext
	 */
	setCollectionAlias(collectionAlias: string | undefined) {
		this.#collectionAlias.setValue(collectionAlias);
	}

	/**
	 * Returns the alias of the collection to render.
	 * @returns {string | undefined} The collection extension alias.
	 * @memberof UmbContentCollectionConfigurationContext
	 */
	getCollectionAlias(): string | undefined {
		return this.#collectionAlias.getValue();
	}

	/**
	 * Sets the data type configuring the collection.
	 * @param {string | undefined} dataTypeUnique - The unique of the configuring data type, or undefined when no
	 * collection is configured.
	 * @memberof UmbContentCollectionConfigurationContext
	 */
	setDataTypeUnique(dataTypeUnique: string | undefined) {
		this.#dataTypeUnique.setValue(dataTypeUnique);
	}

	/**
	 * Returns the data type configuring the collection.
	 * @returns {string | undefined} The unique of the configuring data type.
	 * @memberof UmbContentCollectionConfigurationContext
	 */
	getDataTypeUnique(): string | undefined {
		return this.#dataTypeUnique.getValue();
	}

	/**
	 * Sets the entity the collection is scoped to.
	 * @param {UmbEntityUnique | undefined} unique - The unique of the entity, `null` for the root, or undefined when
	 * there is no subject.
	 * @memberof UmbContentCollectionConfigurationContext
	 */
	setUnique(unique: UmbEntityUnique | undefined) {
		this.#unique.setValue(unique);
	}

	/**
	 * Returns the entity the collection is scoped to.
	 * @returns {UmbEntityUnique | undefined} The unique of the entity, `null` for the root, or undefined when there is
	 * no subject.
	 * @memberof UmbContentCollectionConfigurationContext
	 */
	getUnique(): UmbEntityUnique | undefined {
		return this.#unique.getValue();
	}

	/**
	 * Returns whether a collection should be rendered at all.
	 * @returns {boolean} True when both a collection alias and a configuring data type are known.
	 * @memberof UmbContentCollectionConfigurationContext
	 */
	getHasCollection(): boolean {
		return !!this.getCollectionAlias() && !!this.getDataTypeUnique();
	}

	/**
	 * Returns the resolved configuration data type.
	 * @returns {UmbDataTypeDetailModel | undefined} The data type, or undefined while it is loading and when no
	 * collection is configured.
	 * @memberof UmbContentCollectionConfigurationContext
	 */
	getDataType(): UmbDataTypeDetailModel | undefined {
		return this.#dataType.getValue();
	}

	async #observeDataType(unique: string | undefined) {
		if (!unique) {
			this.removeUmbControllerByAlias(DATA_TYPE_OBSERVER_ALIAS);
			this.#dataType.setValue(undefined);
			return;
		}

		this.#dataTypeDetailRepository.requestByUnique(unique);
		const observable = await this.#dataTypeDetailRepository.byUnique(unique);

		// The host can re-point this context while the repository resolves, so a superseded request must not install its
		// observer over the current one.
		if (this.getDataTypeUnique() !== unique) return;

		this.observe(observable, (dataType) => this.#dataType.setValue(dataType), DATA_TYPE_OBSERVER_ALIAS);
	}
}
