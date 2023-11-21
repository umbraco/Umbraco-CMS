import { UmbMemberDetailRepository } from '../repository/index.js';
import type { UmbMemberDetailModel } from '../types.js';
import { UMB_MEMBER_ENTITY_TYPE } from '../entity.js';
import { UMB_MEMBER_WORKSPACE_ALIAS } from './manifests.js';
import {
	UmbSaveableWorkspaceContextInterface,
	UmbEditableWorkspaceContextBase,
} from '@umbraco-cms/backoffice/workspace';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export class UmbMemberWorkspaceContext
	extends UmbEditableWorkspaceContextBase<UmbMemberDetailRepository, UmbMemberDetailModel>
	implements UmbSaveableWorkspaceContextInterface<UmbMemberDetailModel | undefined>
{
	constructor(host: UmbControllerHostElement) {
		super(host, UMB_MEMBER_WORKSPACE_ALIAS, new UmbMemberDetailRepository(host));
	}

	getEntityType(): string {
		return UMB_MEMBER_ENTITY_TYPE;
	}

	getEntityId() {
		return '1234';
	}

	getData() {
		return 'fake' as unknown as UmbMemberDetailModel;
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

export const UMB_MEMBER_WORKSPACE_CONTEXT = new UmbContextToken<
	UmbSaveableWorkspaceContextInterface,
	UmbMemberWorkspaceContext
>(
	'UmbWorkspaceContext',
	'default',
	(context): context is UmbMemberWorkspaceContext => context.getEntityType?.() === UMB_MEMBER_ENTITY_TYPE,
);
