import { UmbError } from '@umbraco-cms/backoffice/resources';

/**
 * Error returned by a tree repository whose data source cannot resolve tree items by unique.
 * @class UmbTreeItemsNotSupportedError
 * @augments {UmbError}
 */
export class UmbTreeItemsNotSupportedError extends UmbError {
	public override name = 'UmbTreeItemsNotSupportedError';

	constructor(message = 'The tree data source does not support requesting items by unique.') {
		super(message);
	}

	public static isUmbTreeItemsNotSupportedError(error: unknown): error is UmbTreeItemsNotSupportedError {
		return (
			error instanceof UmbTreeItemsNotSupportedError ||
			(error as UmbTreeItemsNotSupportedError | undefined)?.name === 'UmbTreeItemsNotSupportedError'
		);
	}
}
