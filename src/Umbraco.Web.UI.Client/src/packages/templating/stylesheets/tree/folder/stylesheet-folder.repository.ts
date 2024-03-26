import { UmbStylesheetFolderServerDataSource } from './stylesheet-folder.server.data-source.js';
import { UmbFolderRepositoryBase } from '@umbraco-cms/backoffice/tree';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbStylesheetFolderRepository extends UmbFolderRepositoryBase {
	constructor(host: UmbControllerHost) {
		super(host, UmbStylesheetFolderServerDataSource);
	}
}

export default UmbStylesheetFolderRepository;
