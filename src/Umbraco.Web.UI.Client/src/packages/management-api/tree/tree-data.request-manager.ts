import type {
	UmbManagementApiTreeAncestorsOfRequestArgs,
	UmbManagementApiTreeChildrenOfRequestArgs,
	UmbManagementApiTreeRootItemsRequestArgs,
	UmbManagementApiTreeSiblingsFromRequestArgs,
} from './types.js';
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
	RootItemsRequestArgsType extends UmbManagementApiTreeRootItemsRequestArgs,
	RootItemsDataResponseType extends { items: Array<TreeItemType>; total: number },
	ChildrenOfRequestArgsType extends UmbManagementApiTreeChildrenOfRequestArgs,
	ChildrenOfDataResponseType extends { items: Array<TreeItemType>; total: number },
	AncestorsOfRequestArgsType extends UmbManagementApiTreeAncestorsOfRequestArgs,
	AncestorsOfDataResponseType,
	SiblingsFromRequestArgsType extends UmbManagementApiTreeSiblingsFromRequestArgs,
	SiblingsFromDataResponseType extends { items: Array<TreeItemType>; totalBefore: number; totalAfter: number },
> {
	getRootItems: (args: RootItemsRequestArgsType) => Promise<UmbApiResponse<{ data?: RootItemsDataResponseType }>>;
	getChildrenOf: (args: ChildrenOfRequestArgsType) => Promise<UmbApiResponse<{ data?: ChildrenOfDataResponseType }>>;
	getAncestorsOf: (args: AncestorsOfRequestArgsType) => Promise<UmbApiResponse<{ data?: AncestorsOfDataResponseType }>>;
	getSiblingsFrom: (
		args: SiblingsFromRequestArgsType,
	) => Promise<UmbApiResponse<{ data?: SiblingsFromDataResponseType }>>;
}

export class UmbManagementApiTreeDataRequestManager<
	TreeItemType,
	RootItemsRequestArgsType extends UmbManagementApiTreeRootItemsRequestArgs,
	RootItemsDataResponseType extends { items: Array<TreeItemType>; total: number },
	ChildrenOfRequestArgsType extends UmbManagementApiTreeChildrenOfRequestArgs,
	ChildrenOfDataResponseType extends { items: Array<TreeItemType>; total: number },
	AncestorsOfRequestArgsType extends UmbManagementApiTreeAncestorsOfRequestArgs,
	AncestorsOfDataResponseType,
	SiblingsFromRequestArgsType extends UmbManagementApiTreeSiblingsFromRequestArgs,
	SiblingsFromDataResponseType extends { items: Array<TreeItemType>; totalBefore: number; totalAfter: number },
> extends UmbControllerBase {
	#getRootItems;
	#getChildrenOf;
	#getAncestorsOf;
	#getSiblingsFrom;

	constructor(
		host: UmbControllerHost,
		args: UmbManagementApiTreeDataRequestManagerArgs<
			TreeItemType,
			RootItemsRequestArgsType,
			RootItemsDataResponseType,
			ChildrenOfRequestArgsType,
			ChildrenOfDataResponseType,
			AncestorsOfRequestArgsType,
			AncestorsOfDataResponseType,
			SiblingsFromRequestArgsType,
			SiblingsFromDataResponseType
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
			const targetUnique = paging.target.unique;

			if (targetUnique === null) {
				throw new Error('Target unique cannot be null when using target pagination');
			}

			const requestArgs = {
				...args,
				foldersOnly: args.foldersOnly,
				paging: {
					target: {
						entityType: paging.target.entityType,
						unique: targetUnique,
					},
					takeBefore: paging.takeBefore,
					takeAfter: paging.takeAfter,
				},
			} as SiblingsFromRequestArgsType;

			const { data, error } = await tryExecute(this, this.#getSiblingsFrom(requestArgs));

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

		const requestArgs = {
			...args,
			paging: {
				skip: skip,
				take: take,
			},
		} as RootItemsRequestArgsType;

		const { data, error } = await tryExecute(this, this.#getRootItems(requestArgs));

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
		const parentUnique = args.parent.unique;

		if (parentUnique === null) {
			return this.getRootItems(args);
		}

		const paging = args.paging;

		if (paging && isTargetPaginationRequest(paging)) {
			const targetUnique = paging.target.unique;

			if (targetUnique === null) {
				throw new Error('Target unique cannot be null when using target pagination');
			}

			const requestArgs = {
				...args,
				foldersOnly: args.foldersOnly,
				paging: {
					target: {
						entityType: paging.target.entityType,
						unique: targetUnique,
					},
					takeBefore: paging.takeBefore,
					takeAfter: paging.takeAfter,
				},
			} as unknown as SiblingsFromRequestArgsType;

			const { data, error } = await tryExecute(this, this.#getSiblingsFrom(requestArgs));

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

		const requestArgs = {
			...args,
			parent: {
				entityType: args.parent.entityType,
				unique: parentUnique,
			},
			foldersOnly: args.foldersOnly,
			paging: {
				skip,
				take,
			},
		} as ChildrenOfRequestArgsType;

		const { data, error } = await tryExecute(this, this.#getChildrenOf(requestArgs));

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
		const requestArgs = {
			...args,
			treeItem: args.treeItem,
		} as AncestorsOfRequestArgsType;

		return tryExecute(this, this.#getAncestorsOf(requestArgs));
	}
}
