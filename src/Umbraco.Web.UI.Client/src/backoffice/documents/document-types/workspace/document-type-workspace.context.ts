import { UmbWorkspaceContentContext } from '../../../shared/components/workspace/workspace-content/workspace-content.context';
import {
	UmbDocumentTypeStore,
	UmbDocumentTypeStoreItemType,
	UMB_DOCUMENT_TYPE_STORE_CONTEXT_TOKEN,
} from 'src/backoffice/documents/document-types/document-type.store';
import { UmbControllerHostInterface } from '@umbraco-cms/controller';

const DefaultDocumentTypeData = {
	key: '',
	name: '',
	icon: '',
	type: '',
	hasChildren: false,
	parentKey: '',
	alias: '',
	properties: [],
} as UmbDocumentTypeStoreItemType;

export class UmbWorkspaceDocumentTypeContext extends UmbWorkspaceContentContext<
	UmbDocumentTypeStoreItemType,
	UmbDocumentTypeStore
> {
	constructor(host: UmbControllerHostInterface) {
		super(host, DefaultDocumentTypeData, UMB_DOCUMENT_TYPE_STORE_CONTEXT_TOKEN.toString(), 'documentType');
	}

	public setPropertyValue(alias: string, value: unknown) {
		throw new Error('setPropertyValue is not implemented for UmbWorkspaceDocumentTypeContext');
	}
}
