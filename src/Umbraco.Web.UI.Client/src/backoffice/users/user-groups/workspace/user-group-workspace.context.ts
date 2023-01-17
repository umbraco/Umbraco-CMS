import { UmbWorkspaceContentContext } from '../../../shared/components/workspace/workspace-content/workspace-content.context';
import { UMB_USER_STORE_CONTEXT_ALIAS } from '../../users/user.store';
import type { UmbUserGroupStore, UmbUserGroupStoreItemType } from 'src/backoffice/users/user-groups/user-group.store';
import { UmbControllerHostInterface } from '@umbraco-cms/controller';

const DefaultDataTypeData = {
	key: '',
	name: '',
	icon: '',
	type: 'user-group',
	hasChildren: false,
	parentKey: '',
	sections: [],
	permissions: [],
	users: [],
} as UmbUserGroupStoreItemType;

export class UmbWorkspaceUserGroupContext extends UmbWorkspaceContentContext<
	UmbUserGroupStoreItemType,
	UmbUserGroupStore
> {
	constructor(host: UmbControllerHostInterface) {
		super(host, DefaultDataTypeData, UMB_USER_STORE_CONTEXT_ALIAS.toString(), 'userGroup');
	}

	public setPropertyValue(alias: string, value: unknown) {
		throw new Error('setPropertyValue is not implemented for UmbWorkspaceUserGroupContext');
	}
}
