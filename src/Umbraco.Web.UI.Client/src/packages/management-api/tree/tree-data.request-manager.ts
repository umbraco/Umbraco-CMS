import { tryExecute, type UmbApiResponse } from '@umbraco-cms/backoffice/resources';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type {
	UmbTreeAncestorsOfRequestArgs,
	UmbTreeChildrenOfRequestArgs,
	UmbTreeRootItemsRequestArgs,
} from '@umbraco-cms/backoffice/tree';

export interface UmbManagementApiTreeDataRequestManagerArgs<
	GetRootItemsDataResponseType,
	GetChildrenOfDataResponseType,
	GetAncestorsOfDataResponseType,
	GetSiblingsFromDataResponseType,
> {
	getRootItems: (args: any) => Promise<UmbApiResponse<{ data?: GetRootItemsDataResponseType }>>;
	getChildrenOf: (
		args: any,
	) => Promise<UmbApiResponse<{ data?: GetRootItemsDataResponseType | GetChildrenOfDataResponseType }>>;
	getAncestorsOf: (args: any) => Promise<UmbApiResponse<{ data?: GetAncestorsOfDataResponseType }>>;
	getSiblingsFrom: (args: any) => Promise<UmbApiResponse<{ data?: GetSiblingsFromDataResponseType }>>;
}

export class UmbManagementApiTreeDataRequestManager<
	GetRootItemsDataResponseType,
	GetChildrenOfDataResponseType,
	GetAncestorsOfDataResponseType,
	GetSiblingsFromDataResponseType,
> extends UmbControllerBase {
	#getRootItems;
	#getChildrenOf;
	#getAncestorsOf;
	#getSiblingsFrom;

	constructor(
		host: UmbControllerHost,
		args: UmbManagementApiTreeDataRequestManagerArgs<
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

	async getRootItems(
		args: UmbTreeRootItemsRequestArgs,
	): Promise<UmbApiResponse<{ data?: GetRootItemsDataResponseType }>> {
		console.log(this.#getSiblingsFrom);
		return tryExecute(this, this.#getRootItems(args));
	}

	async getChildrenOf(
		args: UmbTreeChildrenOfRequestArgs,
	): Promise<UmbApiResponse<{ data?: GetRootItemsDataResponseType | GetChildrenOfDataResponseType }>> {
		console.log(this.#getSiblingsFrom);
		if (args.parent.unique === null) {
			return this.getRootItems(args);
		} else {
			return tryExecute(this, this.#getChildrenOf(args));
		}
	}

	async getAncestorsOf(
		args: UmbTreeAncestorsOfRequestArgs,
	): Promise<UmbApiResponse<{ data?: GetAncestorsOfDataResponseType }>> {
		return tryExecute(this, this.#getAncestorsOf(args));
	}
}
