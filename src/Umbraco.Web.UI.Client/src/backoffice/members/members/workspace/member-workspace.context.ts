import { UmbWorkspaceContext } from '../../../shared/components/workspace/workspace-context/workspace-context';
import { UmbEntityWorkspaceContextInterface as UmbEntityWorkspaceContextInterface } from '../../../shared/components/workspace/workspace-context/workspace-entity-context.interface';
import { UmbMemberRepository } from '../repository/member.repository';
import type { MemberDetails } from '@umbraco-cms/backoffice/models';
import { UmbControllerHostInterface } from '@umbraco-cms/backoffice/controller';

export class UmbMemberWorkspaceContext
	extends UmbWorkspaceContext<UmbMemberRepository>
	implements UmbEntityWorkspaceContextInterface<MemberDetails | undefined>
{
	constructor(host: UmbControllerHostInterface) {
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
