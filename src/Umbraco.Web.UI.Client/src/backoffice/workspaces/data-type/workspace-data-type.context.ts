import { UmbWorkspaceNodeContext } from "../shared/workspace-context/workspace-node.context";
import type { UmbDataTypeStore, UmbDataTypeStoreItemType } from "@umbraco-cms/stores/data-type/data-type.store";

const DefaultDataTypeData = ({
	key: '',
	name: '',
	icon: '',
	type: '',
	hasChildren: false,
	parentKey: '',
	alias: '',
	properties: [],
}) as UmbDataTypeStoreItemType;

export class UmbWorkspaceDataTypeContext extends UmbWorkspaceNodeContext<UmbDataTypeStoreItemType, UmbDataTypeStore> {

	constructor(target:HTMLElement, entityKey: string) {
		super(target, DefaultDataTypeData, 'umbDataTypeStore', entityKey);
	}

}


