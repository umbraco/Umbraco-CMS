import { UmbWorkspaceContentContext } from '../../../shared/components/workspace/workspace-content/workspace-content.context';
import { isDocumentDetails, STORE_ALIAS as DOCUMENT_STORE_ALIAS } from 'src/backoffice/documents/documents/document.store';
import type { UmbDocumentStore, UmbDocumentStoreItemType } from 'src/backoffice/documents/documents/document.store';
import { UmbControllerHostInterface } from 'src/core/controller/controller-host.mixin';
import type { DocumentDetails } from '@umbraco-cms/models';

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
	constructor(host: UmbControllerHostInterface) {
		super(host, DefaultDocumentData, DOCUMENT_STORE_ALIAS, 'document');
	}

	public setPropertyValue(alias: string, value: unknown) {
		const data = this.getData();
		// TODO: make sure to check that we have a details model:
		// TODO: make a Method to cast
		if(isDocumentDetails(data)) {
			const newDataSet = (data as DocumentDetails).data.map((entry) => {
				if (entry.alias === alias) {
					return {alias: alias, value: value};
				}
				return entry;
			});


			this.update({data: newDataSet} as Partial<UmbDocumentStoreItemType>);
		}
	}

	/*
	concept notes:

	public saveAndPublish() {
		
	}

	public saveAndPreview() {
		
	}
	*/

}