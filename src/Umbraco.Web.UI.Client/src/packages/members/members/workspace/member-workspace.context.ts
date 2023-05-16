import { UmbMemberRepository } from '../repository/member.repository';
import type { MemberDetails } from '../types';
import { UmbEntityWorkspaceContextInterface, UmbWorkspaceContext } from '@umbraco-cms/backoffice/workspace';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';

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
