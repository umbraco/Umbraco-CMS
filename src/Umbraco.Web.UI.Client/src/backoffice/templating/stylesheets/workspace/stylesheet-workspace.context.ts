import { UmbWorkspaceContext } from '../../../shared/components/workspace/workspace-context/workspace-context';
import { UmbEntityWorkspaceContextInterface as UmbEntityWorkspaceContextInterface } from '../../../shared/components/workspace/workspace-context/workspace-entity-context.interface';
import { UmbStylesheetRepository } from '../repository/stylesheet.repository';
import type { MemberDetails } from '@umbraco-cms/backoffice/models';
import { UmbControllerHostInterface } from '@umbraco-cms/backoffice/controller';

export class UmbStylesheetWorkspaceContext
	extends UmbWorkspaceContext<UmbStylesheetRepository>
	implements UmbEntityWorkspaceContextInterface<MemberDetails | undefined>
{
	constructor(host: UmbControllerHostInterface) {
		super(host, new UmbStylesheetRepository(host));
	}

	getEntityType(): string {
		return 'stylesheet';
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
