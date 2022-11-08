import { body, defaultResponse, endpoint, response } from '@airtasker/spot';
import { ProblemDetails } from './models';

@endpoint({ method: 'GET', path: '/manifests' })
export class Manifests {
	@response({ status: 200 })
	response(@body body: ManifestsResponse) {}

	@defaultResponse
	default(@body body: ProblemDetails) {}
}

@endpoint({ method: 'GET', path: '/manifests/packages' })
export class ManifestsPackages {
	@response({ status: 200 })
	response(@body body: {}) {}

	@defaultResponse
	default(@body body: ProblemDetails) {}
}

@endpoint({ method: 'GET', path: '/manifests/packages/installed' })
export class ManifestsPackagesInstalled {
	@response({ status: 200 })
	response(@body body: ManifestsPackagesInstalledResponse) {}

	@defaultResponse
	default(@body body: ProblemDetails) {}
}

export interface ManifestsResponse {
	manifests: {}[];
}

export interface ManifestsPackagesInstalledResponse {
	packages: PackageInstalled[];
}

export interface PackageInstalled {
	id: string;
	name: string;
	alias: string;
	version: string;
	hasMigrations: boolean;
	hasPendingMigrations: boolean;
	plans: {}[];
}
