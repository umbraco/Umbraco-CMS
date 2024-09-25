import { UmbStylesheetFolderServerDataSource } from './stylesheet-folder.server.data-source.js';
import { UmbDetailRepositoryBase } from '@umbraco-cms/backoffice/repository';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbFolderModel } from '@umbraco-cms/backoffice/tree';

export class UmbStylesheetFolderRepository extends UmbDetailRepositoryBase<
	UmbFolderModel,
	UmbStylesheetFolderServerDataSource
> {
	constructor(host: UmbControllerHost) {
		super(host, UmbStylesheetFolderServerDataSource);
	}
}

export default UmbStylesheetFolderRepository;
