import { MemberTypeService } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { tryExecute } from '@umbraco-cms/backoffice/resources';
import type { UmbDataSourceResponse } from '@umbraco-cms/backoffice/repository';

/**
 * Export Member Server Data Source
 * @class UmbExportMemberTypeServerDataSource
 */
export class UmbExportMemberTypeServerDataSource {
	#host: UmbControllerHost;

	/**
	 * Creates an instance of UmbExportMemberTypeServerDataSource.
	 * @param {UmbControllerHost} host - The controller host for this controller to be appended to
	 * @memberof UmbExportMemberTypeServerDataSource
	 */
	constructor(host: UmbControllerHost) {
		this.#host = host;
	}

	/**
	 * Export an item for the given id to the destination unique
	 * @param {unique} unique - The unique identifier of the Member Type to export
	 * @returns {UmbDataSourceResponse<Blob | File>} The exported Member Type file, or an error
	 * @memberof UmbExportMemberTypeServerDataSource
	 */
	async export(unique: string): Promise<UmbDataSourceResponse<Blob | File>> {
		if (!unique) throw new Error('Unique is missing');

		return tryExecute(this.#host, MemberTypeService.getMemberTypeByIdExport({ path: { id: unique } }));
	}
}
