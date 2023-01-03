import { UmbWorkspaceNodeContext } from '../../../shared/components/workspace/workspace-context/workspace-node.context';
import type { UmbUserStore, UmbUserStoreItemType } from 'src/backoffice/users/users/user.store';
import { UmbControllerHostInterface } from 'src/core/controller/controller-host.mixin';

const DefaultDataTypeData = {
	key: '',
	name: '',
	icon: '',
	type: 'user',
	hasChildren: false,
	parentKey: '',
	email: '',
	language: '',
	status: 'enabled',
	updateDate: '8/27/2022',
	createDate: '9/19/2022',
	failedLoginAttempts: 0,
	userGroups: [],
	contentStartNodes: [],
	mediaStartNodes: [],
} as UmbUserStoreItemType;

export class UmbWorkspaceUserContext extends UmbWorkspaceNodeContext<UmbUserStoreItemType, UmbUserStore> {
	constructor(host: UmbControllerHostInterface, entityKey: string) {
		super(host, DefaultDataTypeData, 'umbUserStore', entityKey, 'user');
	}
}
