import { UmbStylesheetDetailModel } from '../types.js';
import { UmbStylesheetTreeRepository } from '../tree/index.js';
import { UmbStylesheetDetailServerDataSource } from './stylesheet-detail.server.data-source.js';
import { UMB_STYLESHEET_DETAIL_STORE_CONTEXT } from './stylesheet-detail.store.js';
import { type UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { UmbDetailRepositoryBase } from '@umbraco-cms/backoffice/repository';

export class UmbStylesheetDetailRepository extends UmbDetailRepositoryBase<UmbStylesheetDetailModel> {
	#dataSource;

	// TODO: temp solution until it is automated
	#treeRepository = new UmbStylesheetTreeRepository(this);

	constructor(host: UmbControllerHostElement) {
		super(host, UmbStylesheetDetailServerDataSource, UMB_STYLESHEET_DETAIL_STORE_CONTEXT);

		// TODO: figure out how spin up get the correct data source
		this.#dataSource = new UmbStylesheetDetailServerDataSource(this);
	}
}
