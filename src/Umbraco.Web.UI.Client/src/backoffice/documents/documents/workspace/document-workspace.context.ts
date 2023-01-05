import { UmbWorkspaceContentContext } from '../../../shared/components/workspace/workspace-content/workspace-content.context';
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

export class UmbWorkspaceDocumentContext extends UmbWorkspaceContentContext<UmbDocumentStoreItemType, UmbDocumentStore> {
	constructor(host: UmbControllerHostInterface, entityKey: string) {
		super(host, DefaultDocumentData, 'umbDocumentStore', entityKey, 'document');
	}



	/*
	concept notes:

	public saveAndPublish() {
		
	}

	public saveAndPreview() {
		
	}
	*/

}
