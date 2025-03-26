import { UMB_MEDIA_PLACEHOLDER_ENTITY_TYPE } from '../entity.js';
import { UMB_MEDIA_GRID_COLLECTION_VIEW_ALIAS } from './views/constants.js';
import type { UmbMediaCollectionFilterModel, UmbMediaCollectionItemModel } from './types.js';
import type { UmbFileDropzoneItemStatus } from 'src/packages/dropzone/dropzone/index.js';
import { UmbDefaultCollectionContext } from '@umbraco-cms/backoffice/collection';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbArrayState } from '@umbraco-cms/backoffice/observable-api';
export class UmbMediaCollectionContext extends UmbDefaultCollectionContext<
	UmbMediaCollectionItemModel,
	UmbMediaCollectionFilterModel
> {
	/**
	 * The thumbnail items that are currently displayed in the collection.
	 * @deprecated Use the `<umb-imaging-thumbnail>` element instead.
	 */
	public readonly thumbnailItems = this.items;

	#placeholders = new UmbArrayState<UmbMediaCollectionItemModel>([], (x) => x.unique);
	public readonly placeholders = this.#placeholders.asObservable();

	constructor(host: UmbControllerHost) {
		super(host, UMB_MEDIA_GRID_COLLECTION_VIEW_ALIAS);
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
				...placeholder,
			}))
			.reverse();
		this.#placeholders.setValue(placeholders);

		this._items.setValue([...placeholders, ...items]);
		this._totalItems.setValue(placeholders.length + items.length);
		this.pagination.setTotalItems(placeholders.length + items.length);
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
	 * @memberof UmbCollectionContext
	 */
	public override async requestCollection() {
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
			this._totalItems.setValue(placeholders.length + data.total);
			this.pagination.setTotalItems(placeholders.length + data.total);
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
}

export { UmbMediaCollectionContext as api };
