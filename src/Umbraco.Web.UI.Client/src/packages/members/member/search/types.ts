import type { UmbMemberItemModel } from '../repository/index.js';
import type { UmbMemberTypeEntityType } from '@umbraco-cms/backoffice/member-type';
import type { UmbSearchRequestArgs } from '@umbraco-cms/backoffice/search';

export interface UmbMemberSearchItemModel extends UmbMemberItemModel {
	href: string;
}

export interface UmbMemberSearchRequestArgs extends UmbSearchRequestArgs {
	contentTypes?: Array<{ unique: string; entityType: UmbMemberTypeEntityType }>;
}
