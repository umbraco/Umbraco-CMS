import { MemberTypeService } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { tryExecute } from '@umbraco-cms/backoffice/resources';

/**
 * Export Member Server Data Source
 * @export
 * @class UmbExportMemberTypeServerDataSource
 */
export class UmbExportMemberTypeServerDataSource {
	#host: UmbControllerHost;

	/**
	 * Creates an instance of UmbExportMemberTypeServerDataSource.
	 * @param {UmbControllerHost} host
	 * @memberof UmbExportMemberTypeServerDataSource
	 */
	constructor(host: UmbControllerHost) {
		this.#host = host;
	}

	/**
	 * Export an item for the given id to the destination unique
	 * @param {unique} unique
	 * @returns {*}
	 * @memberof UmbExportMemberTypeServerDataSource
	 */
	async export(unique: string) {
		if (!unique) throw new Error('Unique is missing');

		return tryExecute(this.#host, MemberTypeService.getMemberTypeByIdExport({ path: { id: unique } }));
	}
}
