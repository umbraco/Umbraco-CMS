import { tryExecute, type UmbApiResponse } from '@umbraco-cms/backoffice/resources';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type {
	UmbTreeAncestorsOfRequestArgs,
	UmbTreeChildrenOfRequestArgs,
	UmbTreeRootItemsRequestArgs,
} from '@umbraco-cms/backoffice/tree';
import { isOffsetPaginationRequest, isTargetPaginationRequest } from '@umbraco-cms/backoffice/utils';

export interface UmbManagementApiTreeDataRequestManagerArgs<
	TreeItemType,
	GetRootItemsDataResponseType extends { items: Array<TreeItemType>; total: number },
	GetChildrenOfDataResponseType extends { items: Array<TreeItemType>; total: number },
	GetAncestorsOfDataResponseType,
	GetSiblingsFromDataResponseType extends { items: Array<TreeItemType>; totalBefore: number; totalAfter: number },
> {
	getRootItems: (args: any) => Promise<UmbApiResponse<{ data?: GetRootItemsDataResponseType }>>;
	getChildrenOf: (args: any) => Promise<UmbApiResponse<{ data?: GetChildrenOfDataResponseType }>>;
	getAncestorsOf: (args: any) => Promise<UmbApiResponse<{ data?: GetAncestorsOfDataResponseType }>>;
	getSiblingsFrom: (args: any) => Promise<UmbApiResponse<{ data?: GetSiblingsFromDataResponseType }>>;
}

export class UmbManagementApiTreeDataRequestManager<
	TreeItemType,
	GetRootItemsDataResponseType extends { items: Array<TreeItemType>; total: number },
	GetChildrenOfDataResponseType extends { items: Array<TreeItemType>; total: number },
	GetAncestorsOfDataResponseType,
	GetSiblingsFromDataResponseType extends { items: Array<TreeItemType>; totalBefore: number; totalAfter: number },
> extends UmbControllerBase {
	#getRootItems;
	#getChildrenOf;
	#getAncestorsOf;
	#getSiblingsFrom;

	constructor(
		host: UmbControllerHost,
		args: UmbManagementApiTreeDataRequestManagerArgs<
			TreeItemType,
			GetRootItemsDataResponseType,
			GetChildrenOfDataResponseType,
			GetAncestorsOfDataResponseType,
			GetSiblingsFromDataResponseType
		>,
	) {
		super(host);
		this.#getRootItems = args.getRootItems;
		this.#getChildrenOf = args.getChildrenOf;
		this.#getAncestorsOf = args.getAncestorsOf;
		this.#getSiblingsFrom = args.getSiblingsFrom;
	}

	async getRootItems(args: UmbTreeRootItemsRequestArgs) {
		const paging = args.paging;

		if (paging && isTargetPaginationRequest(paging)) {
			if (paging.target.unique === null) {
				throw new Error('Target unique cannot be null when using target pagination');
			}

			const { data, error } = await tryExecute(this, this.#getSiblingsFrom(args));

			const mappedData = data
				? {
						items: data.items,
						total: data.totalBefore + data.items.length + data.totalAfter,
						totalBefore: data.totalBefore,
						totalAfter: data.totalAfter,
					}
				: undefined;

			return { data: mappedData, error };
		}

		// Including args.skip + args.take for backwards compatibility
		const skip = paging && isOffsetPaginationRequest(paging) ? paging.skip : args.skip ? args.skip : 0;
		const take = paging && isOffsetPaginationRequest(paging) ? paging.take : args.take ? args.take : 50;

		const { data, error } = await tryExecute(
			this,
			this.#getRootItems({
				...args,
				skip,
				take,
			}),
		);

		const mappedData = data
			? {
					items: data.items,
					total: data.total,
					totalBefore: 0,
					totalAfter: data.total - data.items.length,
				}
			: undefined;

		return { data: mappedData, error };
	}

	async getChildrenOf(args: UmbTreeChildrenOfRequestArgs) {
		if (args.parent.unique === null) {
			return this.getRootItems(args);
		}

		const paging = args.paging;

		if (paging && isTargetPaginationRequest(paging)) {
			if (paging.target.unique === null) {
				throw new Error('Target unique cannot be null when using target pagination');
			}

			const { data, error } = await tryExecute(this, this.#getSiblingsFrom(args));

			const mappedData = data
				? {
						items: data.items,
						total: data.totalBefore + data.items.length + data.totalAfter,
						totalBefore: data.totalBefore,
						totalAfter: data.totalAfter,
					}
				: undefined;

			return { data: mappedData, error };
		}

		// Including args.skip + args.take for backwards compatibility
		const skip = paging && isOffsetPaginationRequest(paging) ? paging.skip : args.skip ? args.skip : 0;
		const take = paging && isOffsetPaginationRequest(paging) ? paging.take : args.take ? args.take : 50;

		const { data, error } = await tryExecute(
			this,
			this.#getChildrenOf({
				...args,
				skip,
				take,
			}),
		);

		const mappedData = data
			? {
					items: data.items,
					total: data.total,
					totalBefore: 0,
					totalAfter: data.total - data.items.length,
				}
			: undefined;

		return { data: mappedData, error };
	}

	async getAncestorsOf(args: UmbTreeAncestorsOfRequestArgs) {
		return tryExecute(this, this.#getAncestorsOf(args));
	}
}
