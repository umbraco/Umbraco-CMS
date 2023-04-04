import { UmbWorkspaceContext } from '../../../shared/components/workspace/workspace-context/workspace-context';
import { UmbMemberRepository } from '../repository/member.repository';
import { UmbEntityWorkspaceContextInterface } from '@umbraco-cms/backoffice/workspace';
import type { MemberDetails } from '@umbraco-cms/backoffice/models';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller';

export class UmbMemberWorkspaceContext
	extends UmbWorkspaceContext<UmbMemberRepository, MemberDetails>
	implements UmbEntityWorkspaceContextInterface<MemberDetails | undefined>
{
	constructor(host: UmbControllerHostElement) {
		super(host, new UmbMemberRepository(host));
	}

	getEntityType(): string {
		return 'member';
	}

	getEntityKey() {
		return '1234';
	}

	getData() {
		return 'fake' as unknown as MemberDetails;
	}

	async save() {
		console.log('save');
	}

	async load(key: string) {
		console.log('load', key);
	}

	public destroy(): void {
		console.log('destroy');
	}
}
