import { UmbWorkspaceContentContext } from '../../../shared/components/workspace/workspace-content/workspace-content.context';
import { UMB_DOCUMENT_DETAIL_STORE_CONTEXT_TOKEN } from '../document.detail.store';
import type { UmbDocumentDetailStore } from '../document.detail.store';
import type { UmbControllerHostInterface } from '@umbraco-cms/controller';
import type { DocumentDetails } from '@umbraco-cms/models';
import { appendToFrozenArray } from '@umbraco-cms/observable-api';

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
} as DocumentDetails;

export class UmbWorkspaceDocumentContext extends UmbWorkspaceContentContext<DocumentDetails, UmbDocumentDetailStore> {
	constructor(host: UmbControllerHostInterface) {
		super(host, DefaultDocumentData, UMB_DOCUMENT_DETAIL_STORE_CONTEXT_TOKEN.toString(), 'document');

		console.log("UMB_DOCUMENT_DETAIL_STORE_CONTEXT_TOKEN", UMB_DOCUMENT_DETAIL_STORE_CONTEXT_TOKEN.toString())
	}

	public setPropertyValue(alias: string, value: unknown) {

		// TODO: make sure to check that we have a details model? otherwise fail? 8This can be relevant if we use the same context for tree actions?
		const entry = {alias: alias, value: value};

		const newDataSet = appendToFrozenArray(this._data.getValue().data, entry, x => x.alias);

		this._data.update({data: newDataSet});
	}

	/*
	concept notes:

	public saveAndPublish() {

	}

	public saveAndPreview() {

	}
	*/

}
