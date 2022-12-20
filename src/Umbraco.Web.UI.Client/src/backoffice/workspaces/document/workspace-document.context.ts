import { Subscription } from "rxjs";
import { UmbWorkspaceContentContext } from "../shared/workspace-content/workspace-content.context";
import { UmbContextConsumer } from "@umbraco-cms/context-api";
import type { DocumentDetails } from "@umbraco-cms/models";
import { UmbDocumentStore } from "@umbraco-cms/stores/document/document.store";
import { UmbNotificationService } from "@umbraco-cms/services";

const DefaultDocumentData = ({
	key: '',
	name: '',
	icon: '',
	type: '',
	hasChildren: false,
	parentKey: '',
	isTrashed: false,
	properties: [
		/*{
			alias: '',
			label: '',
			description: '',
			dataTypeKey: '',
		},*/
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
}) as DocumentDetails;


export class UmbWorkspaceDocumentContext extends UmbWorkspaceContentContext<DocumentDetails> {

	constructor(target:HTMLElement, entityType: string, entityKey: string) {
		super(target, DefaultDocumentData, 'umbDocumentStore', entityType, entityKey);
	}

}

