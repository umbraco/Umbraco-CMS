import { UmbWorkspaceContentContext } from '../../../shared/components/workspace/workspace-content/workspace-content.context';
import { UmbMediaDetailStore, UMB_MEDIA_DETAIL_STORE_CONTEXT_TOKEN } from '../../../media/media/media.detail.store';
import { UmbControllerHostInterface } from '@umbraco-cms/controller';
import type { MediaDetails } from '@umbraco-cms/models';

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
} as MediaDetails;

export class UmbWorkspaceMediaContext extends UmbWorkspaceContentContext<MediaDetails, UmbMediaDetailStore> {
	constructor(host: UmbControllerHostInterface) {
		super(host, DefaultMediaData, UMB_MEDIA_DETAIL_STORE_CONTEXT_TOKEN.toString(), 'media');
	}

	public setPropertyValue(alias: string, value: unknown) {
		throw new Error('setPropertyValue is not implemented for UmbWorkspaceMediaContext');
	}
}
