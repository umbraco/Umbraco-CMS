import type { UmbDatalistDataSource } from './data-source/types.js';

/**
 *
 * @param dataSource
 */
export function isDatalistDataSource(dataSource: unknown): dataSource is UmbDatalistDataSource {
	return (
		(dataSource as UmbDatalistDataSource).requestOptions !== undefined &&
		(dataSource as UmbDatalistDataSource).requestItems !== undefined
	);
}
