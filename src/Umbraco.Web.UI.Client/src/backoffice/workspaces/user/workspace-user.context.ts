import { UmbWorkspaceNodeContext } from "../shared/workspace-context/workspace-node.context";
import type { UmbUserStore, UmbUserStoreItemType } from "@umbraco-cms/stores/user/user.store";


const DefaultDataTypeData = ({
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
}) as UmbUserStoreItemType;

export class UmbWorkspaceUserContext extends UmbWorkspaceNodeContext<UmbUserStoreItemType, UmbUserStore> {

	constructor(target:HTMLElement, entityKey: string) {
		super(target, DefaultDataTypeData, 'umbUserStore', entityKey, 'user');
	}
}


