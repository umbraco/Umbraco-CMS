import { UmbWorkspaceContentContext } from '../../../shared/components/workspace/workspace-content/workspace-content.context';
import {
	UmbDocumentTypeStore,
	UmbDocumentTypeStoreItemType,
} from 'src/backoffice/documents/document-types/document-type.store';
import { UmbControllerHostInterface } from 'src/core/controller/controller-host.mixin';

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
	constructor(host: UmbControllerHostInterface, entityKey: string) {
		super(host, DefaultDocumentTypeData, 'umbDocumentTypeStore', entityKey, 'documentType');
	}
}
