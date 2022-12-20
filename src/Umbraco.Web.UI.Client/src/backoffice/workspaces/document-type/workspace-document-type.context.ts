import { UmbWorkspaceContext } from "../shared/workspace-context/workspace.context";
import type { DocumentDetails } from "@umbraco-cms/models";

const DefaultDocumentTypeData = ({
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


export class UmbWorkspaceDocumentContext extends UmbWorkspaceContext<DocumentDetails> {

	constructor(target:HTMLElement, entityType: string, entityKey: string) {
		super(target, DefaultDocumentTypeData);
	}

}

