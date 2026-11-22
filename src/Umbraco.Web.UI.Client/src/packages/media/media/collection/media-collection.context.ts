import { UMB_MEDIA_PLACEHOLDER_ENTITY_TYPE } from '../entity.js';
import { UMB_EDIT_MEDIA_WORKSPACE_PATH_PATTERN } from '../paths.js';
import { UMB_MEDIA_GRID_COLLECTION_VIEW_ALIAS } from './views/constants.js';
import type { UmbMediaCollectionFilterModel, UmbMediaCollectionItemModel } from './types.js';
import type { UmbFileDropzoneItemStatus } from '@umbraco-cms/backoffice/dropzone';
import { UmbDefaultCollectionContext } from '@umbraco-cms/backoffice/collection';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UMB_VARIANT_CONTEXT } from '@umbraco-cms/backoffice/variant';
import { UmbArrayState, UmbStringState } from '@umbraco-cms/backoffice/observable-api';
import type { UmbPagedModel } from '@umbraco-cms/backoffice/repository';
export class UmbMediaCollectionContext extends UmbDefaultCollectionContext<
	UmbMediaCollectionItemModel,
	UmbMediaCollectionFilterModel
> {
	#placeholders = new UmbArrayState<UmbMediaCollectionItemModel>([], (x) => x.unique);
	public readonly placeholders = this.#placeholders.asObservable();

	#variantContext?: typeof UMB_VARIANT_CONTEXT.TYPE;
	#displayCulture = new UmbStringState(undefined);
	#displayCultureObservable = this.#displayCulture.asObservable();

	constructor(host: UmbControllerHost) {
		super(host, UMB_MEDIA_GRID_COLLECTION_VIEW_ALIAS);

		this.consumeContext(UMB_VARIANT_CONTEXT, async (variantContext) => {
			this.#variantContext = variantContext;
			this.#observeDisplayCulture();
		});
	}

	#observeDisplayCulture() {
		this.observe(
			this.#variantContext?.displayCulture,
			(displayCulture) => {
				if (!displayCulture) return;
				if (this.#displayCulture.getValue() === displayCulture) return;
				this.#displayCulture.setValue(displayCulture);
				this.setFilter({ orderCulture: displayCulture });
			},
			'umbMediaCollectionDisplayCultureObserver',
		);
	}

	setPlaceholders(partial: Array<{ unique: string; status: UmbFileDropzoneItemStatus; name?: string }>) {
		const items = this._items.getValue();

		// We do not want to set a placeholder which unique already exists in the collection.
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

	protected override async _requestCollection() {
		await this.observe(this.#displayCultureObservable)?.asPromise();
		return super._requestCollection();
	}

	protected override _setCollectionData(data: UmbPagedModel<UmbMediaCollectionItemModel>) {
		this.#cleanupPlaceholdersFromCollection(data.items);
		const placeholders = this.#placeholders.getValue();

		this._items.setValue([...placeholders, ...data.items]);
		this._setTotalItems(placeholders.length + data.total);
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
