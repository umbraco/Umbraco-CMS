import { UMB_MEDIA_PLACEHOLDER_ENTITY_TYPE } from '../entity.js';
import { UMB_EDIT_MEDIA_WORKSPACE_PATH_PATTERN } from '../paths.js';
import { UMB_MEDIA_GRID_COLLECTION_VIEW_ALIAS } from './views/constants.js';
import type { UmbMediaCollectionFilterModel, UmbMediaCollectionItemModel } from './types.js';
import type { UmbFileDropzoneItemStatus } from '@umbraco-cms/backoffice/dropzone';
import { UmbDefaultCollectionContext } from '@umbraco-cms/backoffice/collection';
import type { UmbCollectionConfiguration } from '@umbraco-cms/backoffice/collection';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbArrayState, UmbStringState } from '@umbraco-cms/backoffice/observable-api';
import { UmbDeprecation } from '@umbraco-cms/backoffice/utils';
import {
	UMB_MEDIA_COLLECTION_ORDER_BY_OPTIONS,
	UmbMediaCollectionSortPreferenceRepository,
	mergeMediaCollectionSortPreference,
} from './sort-preference/index.js';
import type { UmbMediaCollectionOrderByOption } from './sort-preference/types.js';

export class UmbMediaCollectionContext extends UmbDefaultCollectionContext<
	UmbMediaCollectionItemModel,
	UmbMediaCollectionFilterModel
> {
	#placeholders = new UmbArrayState<UmbMediaCollectionItemModel>([], (x) => x.unique);
	public readonly placeholders = this.#placeholders.asObservable();

	#orderByOptions = new UmbArrayState<UmbMediaCollectionOrderByOption>(UMB_MEDIA_COLLECTION_ORDER_BY_OPTIONS, (x) => x.unique);
	orderByOptions = this.#orderByOptions.asObservable();

	#activeOrderByOption = new UmbStringState<string | undefined>(undefined);
	activeOrderByOption = this.#activeOrderByOption.asObservable();

	#sortPreferenceRepository = new UmbMediaCollectionSortPreferenceRepository(this);
	#pendingConfig?: UmbCollectionConfiguration;
	#preferenceLoaded = false;

	constructor(host: UmbControllerHost) {
		super(host, UMB_MEDIA_GRID_COLLECTION_VIEW_ALIAS);

		this.#loadSortPreference();

		this.observe(
			this.filter,
			(filter) => {
				const { orderBy, orderDirection } = filter as UmbMediaCollectionFilterModel;
				const option = this.#orderByOptions
					.getValue()
					.find((x) => x.config.orderBy === orderBy && x.config.orderDirection === orderDirection);
				if (option) {
					this.#activeOrderByOption.setValue(option.unique);
				}
			},
			null,
		);
	}

	async #loadSortPreference() {
		const { data } = await this.#sortPreferenceRepository.requestPreference();
		this.#preferenceLoaded = true;

		const config = this.#pendingConfig ?? this.getConfig();
		if (config) {
			super.setConfig(mergeMediaCollectionSortPreference(config, data));
			this.#pendingConfig = undefined;
		}
	}

	override setConfig(config: UmbCollectionConfiguration) {
		if (!this.#preferenceLoaded) {
			this.#pendingConfig = config;
			super.setConfig(config);
			return;
		}

		this.#sortPreferenceRepository.requestPreference().then(({ data }) => {
			super.setConfig(mergeMediaCollectionSortPreference(config, data));
		});
	}

	setActiveOrderByOption(unique: string) {
		const option = this.#orderByOptions.getValue().find((x) => x.unique === unique);
		if (!option) return;

		this.#activeOrderByOption.setValue(unique);
		this.setFilter({ orderBy: option.config.orderBy, orderDirection: option.config.orderDirection });
		this.#sortPreferenceRepository.savePreference(option.config);
	}

	setPlaceholders(partial: Array<{ unique: string; status: UmbFileDropzoneItemStatus; name?: string }>) {
		const items = this._items.getValue();

		const date = new Date();
		const placeholders: Array<UmbMediaCollectionItemModel> = partial
			.filter((placeholder) => !items.find((item) => item.unique === placeholder.unique))
			.map((placeholder) => ({
				updateDate: date,
				createDate: date,
				entityType: UMB_MEDIA_PLACEHOLDER_ENTITY_TYPE,
				flags: [],
				...placeholder,
			}))
			.reverse();
		this.#placeholders.setValue(placeholders);

		this._items.setValue([...placeholders, ...items]);
		this._setTotalItems(placeholders.length + items.length);
	}

	updatePlaceholderStatus(unique: string, status?: UmbFileDropzoneItemStatus) {
		this._items.updateOne(unique, { status });
		this.#placeholders.updateOne(unique, { status });
	}

	updatePlaceholderProgress(unique: string, progress: number) {
		this._items.updateOne(unique, { progress });
		this.#placeholders.updateOne(unique, { progress });
	}

	/**
	 * Requests the collection from the repository.
	 * @returns {Promise<void>}
	 * @deprecated Deprecated since v.17.0.0. Use `loadCollection` instead.
	 * @memberof UmbMediaCollectionContext
	 */
	public override async requestCollection(): Promise<void> {
		new UmbDeprecation({
			removeInVersion: '19.0.0',
			deprecated: 'requestCollection',
			solution: 'Use .loadCollection method instead',
		}).warn();

		return this._requestCollection();
	}

	protected override async _requestCollection() {
		await this._init;

		if (!this._configured) this._configure();

		if (!this._repository) throw new Error(`Missing repository for ${this._manifest}`);

		this._loading.setValue(true);

		const filter = this._filter.getValue();
		const { data } = await this._repository.requestCollection(filter);

		if (data) {
			this.#cleanupPlaceholdersFromCollection(data.items);
			const placeholders = this.#placeholders.getValue();

			this._items.setValue([...placeholders, ...data.items]);
			this._setTotalItems(placeholders.length + data.total);
		}

		this._loading.setValue(false);
	}

	#cleanupPlaceholdersFromCollection(collection: Array<UmbMediaCollectionItemModel>) {
		const placeholderItems = this.#placeholders.getValue();

		const dataSet = new Set(collection.map((item) => item.unique));
		const completedPlaceholders = placeholderItems.filter((item) => dataSet.has(item.unique));
		completedPlaceholders.forEach((placeholder) => {
			this.#placeholders.removeOne(placeholder.unique);
		});
	}

	/**
	 * Returns the href for a specific media collection item.
	 * @param {UmbMediaCollectionItemModel} item - The media item to get the href for.
	 * @returns {Promise<string | undefined>} - The edit workspace href for the media.
	 */
	override async requestItemHref(item: UmbMediaCollectionItemModel): Promise<string | undefined> {
		return `${UMB_EDIT_MEDIA_WORKSPACE_PATH_PATTERN.generateAbsolute({ unique: item.unique })}`;
	}
}

export { UmbMediaCollectionContext as api };
