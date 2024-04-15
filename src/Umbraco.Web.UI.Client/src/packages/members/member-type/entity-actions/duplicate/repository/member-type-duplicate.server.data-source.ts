import { MemberTypeService } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';
import type { UmbDuplicateDataSource, UmbDuplicateToRequestArgs } from '@umbraco-cms/backoffice/entity-action';

/**
 * Duplicate Document Server Data Source
 * @export
 * @class UmbDuplicateMemberTypeServerDataSource
 */
export class UmbDuplicateMemberTypeServerDataSource implements UmbDuplicateDataSource {
	#host: UmbControllerHost;

	/**
	 * Creates an instance of UmbDuplicateMemberTypeServerDataSource.
	 * @param {UmbControllerHost} host
	 * @memberof UmbDuplicateMemberTypeServerDataSource
	 */
	constructor(host: UmbControllerHost) {
		this.#host = host;
	}

	/**
	 * Duplicate an item for the given id to the destination unique
	 * @param {UmbDuplicateToRequestArgs} args
	 * @return {*}
	 * @memberof UmbDuplicateMemberTypeServerDataSource
	 */
	async duplicateTo(args: UmbDuplicateToRequestArgs) {
		if (!args.unique) throw new Error('Unique is missing');
		if (args.destination.unique === undefined) throw new Error('Destination unique is missing');

		return tryExecuteAndNotify(
			this.#host,
			MemberTypeService.postMemberTypeByIdCopy({
				id: args.unique,
			}),
		);
	}
}
