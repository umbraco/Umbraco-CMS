import { MemberTypeService } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { tryExecute } from '@umbraco-cms/backoffice/resources';
import type { UmbDuplicateToDataSource, UmbDuplicateToRequestArgs } from '@umbraco-cms/backoffice/tree';
import type { UmbDataSourceResponse } from '@umbraco-cms/backoffice/repository';

/**
 * Duplicate Member Type Server Data Source
 * @class UmbDuplicateMemberTypeServerDataSource
 */
export class UmbDuplicateMemberTypeServerDataSource implements UmbDuplicateToDataSource {
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
	 * Duplicate an item for the given unique to the destination unique
	 * @param {UmbDuplicateToRequestArgs} args - The arguments for the duplicate operation
	 * @returns {Promise<UmbDataSourceResponse<string>>} The unique of the duplicate, or an error.
	 * @memberof UmbDuplicateMemberTypeServerDataSource
	 */
	async duplicateTo(args: UmbDuplicateToRequestArgs): Promise<UmbDataSourceResponse<string>> {
		if (!args.unique) throw new Error('Unique is missing');
		if (args.destination.unique === undefined) throw new Error('Destination unique is missing');

		const { data, error } = await tryExecute(
			this.#host,
			MemberTypeService.postMemberTypeByIdCopy({
				path: { id: args.unique },
				body: {
					target: args.destination.unique ? { id: args.destination.unique } : null,
				},
			}),
		);

		// The Umb-Generated-Resource interceptor replaces the empty 201 body with the unique of the duplicate.
		return { data: data as string | undefined, error };
	}
}
