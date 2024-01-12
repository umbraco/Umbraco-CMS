import { UmbMemberGroupDetailRepository } from '../repository/index.js';
import type { UmbMemberGroupDetailModel } from '../types.js';
import { UMB_MEMBER_GROUP_ENTITY_TYPE } from '../entity.js';
import { UMB_MEMBER_GROUP_WORKSPACE_ALIAS } from './manifests.js';
import {
	type UmbSaveableWorkspaceContextInterface,
	UmbEditableWorkspaceContextBase,
} from '@umbraco-cms/backoffice/workspace';
import type { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export class UmbMemberGroupWorkspaceContext
	extends UmbEditableWorkspaceContextBase<UmbMemberGroupDetailRepository, UmbMemberGroupDetailModel>
	implements UmbSaveableWorkspaceContextInterface
{
	constructor(host: UmbControllerHostElement) {
		super(host, UMB_MEMBER_GROUP_WORKSPACE_ALIAS, new UmbMemberGroupDetailRepository(host));
	}

	getEntityType(): string {
		return UMB_MEMBER_GROUP_ENTITY_TYPE;
	}

	getEntityId() {
		return '1234';
	}

	getData() {
		return 'fake' as unknown as UmbMemberGroupDetailModel;
	}

	async save() {
		console.log('save');
	}

	async load(id: string) {
		console.log('load', id);
	}

	public destroy(): void {
		console.log('destroy');
	}
}

export const UMB_MEMBER_GROUP_WORKSPACE_CONTEXT = new UmbContextToken<
	UmbSaveableWorkspaceContextInterface,
	UmbMemberGroupWorkspaceContext
>(
	'UmbWorkspaceContext',
	undefined,
	(context): context is UmbMemberGroupWorkspaceContext => context.getEntityType?.() === UMB_MEMBER_GROUP_ENTITY_TYPE,
);
