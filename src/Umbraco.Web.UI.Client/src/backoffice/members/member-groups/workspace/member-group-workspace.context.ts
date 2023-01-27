import type { MemberGroupDetails } from '@umbraco-cms/models';
import { UmbControllerHostInterface } from '@umbraco-cms/controller';
import { UmbWorkspaceContentContext } from '../../../shared/components/workspace/workspace-content/workspace-content.context';
import { UmbMemberGroupDetailStore, UMB_MEMBER_GROUP_DETAIL_STORE_CONTEXT_TOKEN } from '../member-group.detail.store';

const DefaultMemberGroupData = {
	key: '',
	name: '',
	icon: '',
    id: '',
	type: 'member-group',
	hasChildren: false,
} as MemberGroupDetails;

export class UmbWorkspaceMemberGroupContext extends UmbWorkspaceContentContext<
	MemberGroupDetails,
	UmbMemberGroupDetailStore
> {
    
    setPropertyValue(alias: string, value: unknown): void {
        return;    
    }

	constructor(host: UmbControllerHostInterface) {
		super(host, DefaultMemberGroupData, UMB_MEMBER_GROUP_DETAIL_STORE_CONTEXT_TOKEN.toString(), 'memberGroup');
	}
}
