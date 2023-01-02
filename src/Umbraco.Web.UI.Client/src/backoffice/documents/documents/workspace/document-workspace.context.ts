import { UmbWorkspaceNodeContext } from '../../../core/components/workspace/workspace-context/workspace-node.context';
import type { UmbDocumentStore, UmbDocumentStoreItemType } from 'src/backoffice/documents/documents/document.store';
import { UmbControllerHostInterface } from 'src/core/controller/controller-host.mixin';

const DefaultDocumentData = {
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
} as UmbDocumentStoreItemType;

export class UmbWorkspaceDocumentContext extends UmbWorkspaceNodeContext<UmbDocumentStoreItemType, UmbDocumentStore> {
	constructor(host: UmbControllerHostInterface, entityKey: string) {
		super(host, DefaultDocumentData, 'umbDocumentStore', entityKey, 'document');
	}
}
