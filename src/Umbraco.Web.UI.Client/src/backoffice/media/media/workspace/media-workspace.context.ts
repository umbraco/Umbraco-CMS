import { UmbWorkspaceNodeContext } from '../../../shared/components/workspace/workspace-context/workspace-node.context';
import type { UmbMediaStore, UmbMediaStoreItemType } from 'src/backoffice/media/media/media.store';

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

export class UmbWorkspaceMediaContext extends UmbWorkspaceNodeContext<UmbMediaStoreItemType, UmbMediaStore> {
	constructor(target: HTMLElement, entityKey: string) {
		super(target, DefaultMediaData, 'umbMediaStore', entityKey, 'media');
	}
}
