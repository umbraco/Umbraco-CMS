import type {
	UmbManagementApiTreeAncestorsOfRequestArgs,
	UmbManagementApiTreeChildrenOfRequestArgs,
	UmbManagementApiTreeRootItemsRequestArgs,
	UmbManagementApiTreeSiblingsFromRequestArgs,
} from './types.js';
import { isOffsetPaginationRequest, isTargetPaginationRequest } from '@umbraco-cms/backoffice/utils';
import { tryExecute, UmbError, type UmbApiResponse } from '@umbraco-cms/backoffice/resources';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { ReferenceByIdModel } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbEntityUnique } from '@umbraco-cms/backoffice/entity';
import type {
	UmbTreeAncestorsOfRequestArgs,
	UmbTreeChildrenOfRequestArgs,
	UmbTreeRootItemsRequestArgs,
} from '@umbraco-cms/backoffice/tree';

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
	TreeItemType extends { parent?: ReferenceByIdModel | null },
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
	#defaultTakeSize = 50;

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

			const { data: responseData, error: responseError } = await tryExecute(this, this.#getSiblingsFrom(requestArgs));

			if (responseError) {
				return { data: undefined, error: responseError };
			}

			return this.#handleSiblingsResponseData(responseData, null);
		}

		const skip = this.#getSkipFromArgs(args);
		const take = this.#getTakeFromArgs(args);

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

			const { data: responseData, error: responseError } = await tryExecute(this, this.#getSiblingsFrom(requestArgs));

			if (responseError) {
				return { data: undefined, error: responseError };
			}

			return this.#handleSiblingsResponseData(responseData, args.parent.unique);
		}

		const skip = this.#getSkipFromArgs(args);
		const take = this.#getTakeFromArgs(args);

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

	#getSkipFromArgs(args: UmbTreeRootItemsRequestArgs | UmbTreeChildrenOfRequestArgs): number {
		const paging = args.paging;

		if (paging && isOffsetPaginationRequest(paging)) {
			return paging.skip !== undefined ? paging.skip : 0;
		}

		// Including args.skip for backwards compatibility
		return args.skip !== undefined ? args.skip : 0;
	}

	#getTakeFromArgs(args: UmbTreeRootItemsRequestArgs | UmbTreeChildrenOfRequestArgs): number {
		const paging = args.paging;

		if (paging && isOffsetPaginationRequest(paging)) {
			return paging.take !== undefined ? paging.take : this.#defaultTakeSize;
		}

		// Including args.take for backwards compatibility
		return args.take !== undefined ? args.take : this.#defaultTakeSize;
	}

	#getTargetResultHasValidParents(data: Array<TreeItemType> | undefined, parentUnique: string | null): boolean {
		if (!data) {
			return false;
		}
		return data.every((item) => {
			if (item.parent) {
				return item.parent.id === parentUnique;
			} else {
				return parentUnique === null;
			}
		});
	}

	#handleSiblingsResponseData(responseData: SiblingsFromDataResponseType | undefined, parentUnique: UmbEntityUnique) {
		let data = undefined;
		let error = undefined;

		if (responseData) {
			// The siblings endpoint doesn't know about the parent context, so it will return items that may not have the correct parent
			const hasValidParents = this.#getTargetResultHasValidParents(responseData?.items, parentUnique);
			// IF all parents match the request args we return the data
			if (hasValidParents) {
				data = {
					items: responseData.items,
					total: responseData.totalBefore + responseData.items.length + responseData.totalAfter,
					totalBefore: responseData.totalBefore,
					totalAfter: responseData.totalAfter,
				};
				// We have gotten a result where the parent do not match. We return an error that the requested target could not be found for the parent.
				// This could happen if a requested target has been moved to another parent.
			} else {
				error = new UmbError('Target was not found within parent');
			}
		}

		return { data, error };
	}
}
