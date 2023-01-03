import { UmbWorkspaceNodeContext } from '../../../shared/components/workspace/workspace-context/workspace-node.context';
import type { UmbUserGroupStore, UmbUserGroupStoreItemType } from 'src/backoffice/users/user-groups/user-group.store';
import { UmbControllerHostInterface } from 'src/core/controller/controller-host.mixin';

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

export class UmbWorkspaceUserGroupContext extends UmbWorkspaceNodeContext<
	UmbUserGroupStoreItemType,
	UmbUserGroupStore
> {
	constructor(host: UmbControllerHostInterface, entityKey: string) {
		super(host, DefaultDataTypeData, 'umbUserStore', entityKey, 'userGroup');
	}
}
