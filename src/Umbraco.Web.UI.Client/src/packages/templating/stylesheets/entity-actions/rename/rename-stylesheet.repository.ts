import type { UmbStylesheetDetailModel } from '../../types.js';
import { UmbRenameStylesheetServerDataSource } from './rename-stylesheet.server.data-source.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbRenameRepositoryBase } from '@umbraco-cms/backoffice/entity-action';

export class UmbRenameStylesheetRepository extends UmbRenameRepositoryBase<UmbStylesheetDetailModel> {
	constructor(host: UmbControllerHost) {
		super(host, UmbRenameStylesheetServerDataSource);
	}
}

export default UmbRenameStylesheetRepository;
