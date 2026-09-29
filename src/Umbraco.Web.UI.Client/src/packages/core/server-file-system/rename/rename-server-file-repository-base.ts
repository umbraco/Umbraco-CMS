import type { UmbRenameServerFileDataSource, UmbRenameServerFileDataSourceConstructor } from './types.js';
import type { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbDetailStore } from '@umbraco-cms/backoffice/store';
import { UmbRepositoryBase } from '@umbraco-cms/backoffice/repository';
import type { UmbRepositoryResponse } from '@umbraco-cms/backoffice/repository';
import type { UmbEntityModel } from '@umbraco-cms/backoffice/entity';

export abstract class UmbRenameServerFileRepositoryBase<
	DetailModelType extends UmbEntityModel,
> extends UmbRepositoryBase {
	#renameSource: UmbRenameServerFileDataSource<DetailModelType>;
	#detailStoreContextAlias: string | UmbContextToken<UmbDetailStore<DetailModelType>>;

	constructor(
		host: UmbControllerHost,
		detailSource: UmbRenameServerFileDataSourceConstructor<DetailModelType>,
		detailStoreContextAlias: string | UmbContextToken<UmbDetailStore<DetailModelType>>,
	) {
		super(host);
		this.#renameSource = new detailSource(host);
		this.#detailStoreContextAlias = detailStoreContextAlias;
	}

	/**
	 * Rename
	 * @param {string} unique - The unique identifier of the file to rename
	 * @param {string} name - The new name for the file
	 * @returns {Promise<UmbRepositoryResponse<DetailModelType>>} The renamed detail data
	 * @memberof UmbRenameServerFileRepositoryBase
	 */
	async rename(unique: string, name: string): Promise<UmbRepositoryResponse<DetailModelType>> {
		if (!unique) throw new Error('Unique is missing');
		if (!name) throw new Error('Name is missing');

		const { data, error } = await this.#renameSource.rename(unique, name);

		if (data) {
			const detailStore = await this.getContext(this.#detailStoreContextAlias);
			if (!detailStore) throw new Error('Detail store is missing');

			/* When renaming a file the unique changed because it is based on the path/name
			We need to remove the old item and append the new item */
			detailStore.removeItem(unique);
			detailStore.append(data);
		}

		return { data, error };
	}
}
