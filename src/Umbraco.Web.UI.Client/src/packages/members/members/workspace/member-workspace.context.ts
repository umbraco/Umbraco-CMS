import { UmbMemberRepository } from '../repository/member.repository.js';
import type { MemberDetails } from '../types.js';
import { UmbSaveableWorkspaceContextInterface, UmbWorkspaceContext } from '@umbraco-cms/backoffice/workspace';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export class UmbMemberWorkspaceContext
	extends UmbWorkspaceContext<UmbMemberRepository, MemberDetails>
	implements UmbSaveableWorkspaceContextInterface<MemberDetails | undefined>
{
	constructor(host: UmbControllerHostElement) {
		super(host, 'Umb.Workspace.Member', new UmbMemberRepository(host));
	}

	getEntityType(): string {
		return 'member';
	}

	getEntityId() {
		return '1234';
	}

	getData() {
		return 'fake' as unknown as MemberDetails;
	}

	async save() {
		console.log('save');
	}

	async load(id: string) {
		console.log('load', id);
	}

	public destroy(): void {
		console.log('destroy');
	}
}

export const UMB_MEMBER_WORKSPACE_CONTEXT = new UmbContextToken<UmbSaveableWorkspaceContextInterface, UmbMemberWorkspaceContext>(
	'UmbWorkspaceContext',
	(context): context is UmbMemberWorkspaceContext => context.getEntityType?.() === 'member'
);
