import { UmbWorkspaceContentContext } from '../../../shared/components/workspace/workspace-content/workspace-content.context';
import type { UmbMediaStore, UmbMediaStoreItemType } from 'src/backoffice/media/media/media.store';
import { UmbControllerHostInterface } from 'src/core/controller/controller-host.mixin';

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
	constructor(host: UmbControllerHostInterface, entityKey: string) {
		super(host, DefaultMediaData, 'umbMediaStore', entityKey, 'media');
	}
}
