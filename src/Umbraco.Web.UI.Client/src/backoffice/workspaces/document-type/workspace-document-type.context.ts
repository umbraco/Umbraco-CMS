import { UmbWorkspaceNodeContext } from "../shared/workspace-context/workspace-node.context";
import { UmbDocumentTypeStore, UmbDocumentTypeStoreItemType } from "@umbraco-cms/stores/document-type/document-type.store";

const DefaultDocumentTypeData = ({
	key: '',
	name: '',
	icon: '',
	type: '',
	hasChildren: false,
	parentKey: '',
	alias: '',
	properties: [],
}) as UmbDocumentTypeStoreItemType;

export class UmbWorkspaceDocumentTypeContext extends UmbWorkspaceNodeContext<UmbDocumentTypeStoreItemType, UmbDocumentTypeStore> {

	constructor(target:HTMLElement, entityKey: string) {
		super(target, DefaultDocumentTypeData, 'umbDocumentTypeStore', entityKey, 'documentType');
	}

}


