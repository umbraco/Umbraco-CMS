import { MemberTypeService, type ImportMemberTypeRequestModel } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { tryExecute } from '@umbraco-cms/backoffice/resources';
import type { UmbDataSourceErrorResponse } from '@umbraco-cms/backoffice/repository';

/**
 * Member Type Import Server Data Source
 * @class UmbMemberTypeImportServerDataSource
 */
export class UmbMemberTypeImportServerDataSource {
	#host: UmbControllerHost;

	/**
	 * Creates an instance of UmbMemberTypeImportServerDataSource.
	 * @param {UmbControllerHost} host - The controller host for this controller to be appended to
	 * @memberof UmbMemberTypeImportServerDataSource
	 */
	constructor(host: UmbControllerHost) {
		this.#host = host;
	}

	/**
	 * Import an item for the given id to the destination unique
	 * @param {temporaryUnique} temporaryUnique - The unique identifier of the temporary file to import
	 * @returns {UmbDataSourceErrorResponse} An error, if the import failed
	 * @memberof UmbMemberTypeImportServerDataSource
	 */
	async import(temporaryUnique: string) {
		if (!temporaryUnique) throw new Error('Unique is missing');

		const body: ImportMemberTypeRequestModel = {
			file: { id: temporaryUnique },
		};

		return tryExecute(this.#host, MemberTypeService.postMemberTypeImport({ body }));
	}
}
