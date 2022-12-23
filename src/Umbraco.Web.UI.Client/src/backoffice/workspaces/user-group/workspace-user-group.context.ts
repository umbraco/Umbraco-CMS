import { UmbWorkspaceNodeContext } from "../shared/workspace-context/workspace-node.context";
import type { UmbUserGroupStore, UmbUserGroupStoreItemType } from "@umbraco-cms/stores/user/user-group.store";

const DefaultDataTypeData = ({
	key: '',
	name: '',
	icon: '',
	type: 'user-group',
	hasChildren: false,
	parentKey: '',
	sections: [],
	permissions: [],
	users: [],
}) as UmbUserGroupStoreItemType;

export class UmbWorkspaceUserGroupContext extends UmbWorkspaceNodeContext<UmbUserGroupStoreItemType, UmbUserGroupStore> {

	constructor(target:HTMLElement, entityKey: string) {
		super(target, DefaultDataTypeData, 'umbUserStore', entityKey, 'userGroup');
	}
}


