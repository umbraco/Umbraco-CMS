import { MemberTypeService } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';
import type { UmbDuplicateDataSource, UmbDuplicateRequestArgs } from '@umbraco-cms/backoffice/entity-action';

/**
 * Duplicate Document Server Data Source
 * @class UmbDuplicateMemberTypeServerDataSource
 */
export class UmbDuplicateMemberTypeServerDataSource implements UmbDuplicateDataSource {
	#host: UmbControllerHost;

	/**
	 * Creates an instance of UmbDuplicateMemberTypeServerDataSource.
	 * @param {UmbControllerHost} host - The controller host for this controller to be appended to
	 * @memberof UmbDuplicateMemberTypeServerDataSource
	 */
	constructor(host: UmbControllerHost) {
		this.#host = host;
	}

	/**
	 * Duplicate an item for the given unique
	 * @param {UmbDuplicateRequestArgs} args
	 * @returns {*}
	 * @memberof UmbDuplicateDataTypeServerDataSource
	 */
	async duplicate(args: UmbDuplicateRequestArgs) {
		if (!args.unique) throw new Error('Unique is missing');

		return tryExecuteAndNotify(
			this.#host,
			MemberTypeService.postMemberTypeByIdCopy({
				id: args.unique,
			}),
		);
	}
}
