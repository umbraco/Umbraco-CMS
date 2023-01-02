import { UmbWorkspaceNodeContext } from "../shared/workspace-context/workspace-node.context";
import type { UmbDocumentStore, UmbDocumentStoreItemType } from "@umbraco-cms/stores/document/document.store";

const DefaultDocumentData = ({
	key: '',
	name: '',
	icon: '',
	type: '',
	hasChildren: false,
	parentKey: '',
	isTrashed: false,
	properties: [
		{
			alias: '',
			label: '',
			description: '',
			dataTypeKey: '',
		},
	],
	data: [
		{
			alias: '',
			value: '',
		},
	],
	variants: [
		{
			name: '',
		},
	],
}) as UmbDocumentStoreItemType;


export class UmbWorkspaceDocumentContext extends UmbWorkspaceNodeContext<UmbDocumentStoreItemType, UmbDocumentStore> {

	constructor(target:HTMLElement, entityKey: string) {
		super(target, DefaultDocumentData, 'umbDocumentStore', entityKey, 'document');
	}

}

