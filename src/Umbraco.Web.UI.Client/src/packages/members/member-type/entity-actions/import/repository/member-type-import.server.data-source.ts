import { MemberTypeService, type ImportMemberTypeRequestModel } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { tryExecute } from '@umbraco-cms/backoffice/resources';

/**
 * Member Type Import Server Data Source
 * @Import
 * @class UmbMemberTypeImportServerDataSource
 */
export class UmbMemberTypeImportServerDataSource {
	#host: UmbControllerHost;

	/**
	 * Creates an instance of UmbMemberTypeImportServerDataSource.
	 * @param {UmbControllerHost} host
	 * @memberof UmbMemberTypeImportServerDataSource
	 */
	constructor(host: UmbControllerHost) {
		this.#host = host;
	}

	/**
	 * Import an item for the given id to the destination unique
	 * @param {temporaryUnique} temporaryUnique
	 * @returns {*}
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
