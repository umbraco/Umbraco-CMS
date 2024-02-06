import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbRepositoryBase } from '@umbraco-cms/backoffice/repository';

export class UmbMemberDetailRepository extends UmbRepositoryBase {
	constructor(host: UmbControllerHost) {
		super(host);
		console.log('UmbMemberDetailRepository');
	}
}
