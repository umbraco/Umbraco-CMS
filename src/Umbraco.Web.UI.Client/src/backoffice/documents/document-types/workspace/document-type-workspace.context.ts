import { UmbWorkspaceContentContext } from '../../../shared/components/workspace/workspace-content/workspace-content.context';
import {
	UmbDocumentTypeDetailStore,
	UMB_DOCUMENT_TYPE_DETAIL_STORE_CONTEXT_TOKEN,
} from '../document-type.detail.store';
import { UmbControllerHostInterface } from '@umbraco-cms/controller';
import type { DocumentTypeDetails } from '@umbraco-cms/models';

const DefaultDocumentTypeData = {
	key: '',
	name: '',
	icon: '',
	type: '',
	hasChildren: false,
	parentKey: '',
	alias: '',
	properties: [],
} as DocumentTypeDetails;

export class UmbWorkspaceDocumentTypeContext extends UmbWorkspaceContentContext<
	DocumentTypeDetails,
	UmbDocumentTypeDetailStore
> {
	constructor(host: UmbControllerHostInterface) {
		super(host, DefaultDocumentTypeData, UMB_DOCUMENT_TYPE_DETAIL_STORE_CONTEXT_TOKEN.toString(), 'documentType');
	}

	public setPropertyValue(alias: string, value: unknown) {
		throw new Error('setPropertyValue is not implemented for UmbWorkspaceDocumentTypeContext');
	}
}
