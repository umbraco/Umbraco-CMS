import { UmbWorkspaceContentContext } from '../../../shared/components/workspace/workspace-content/workspace-content.context';
import type { UmbMediaStore, UmbMediaStoreItemType } from 'src/backoffice/media/media/media.store';
import { UmbControllerHostInterface } from '@umbraco-cms/controller';

const DefaultMediaData = {
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
} as UmbMediaStoreItemType;

export class UmbWorkspaceMediaContext extends UmbWorkspaceContentContext<UmbMediaStoreItemType, UmbMediaStore> {
	constructor(host: UmbControllerHostInterface) {
		super(host, DefaultMediaData, 'umbMediaStore', 'media');
	}

	public setPropertyValue(alias: string, value: unknown) {
		throw new Error("setPropertyValue is not implemented for UmbWorkspaceMediaContext")
	}
}
