import { UmbHealthCheckContext } from './health-check.context';
import { HealthCheckGroup, HealthCheckResource } from '@umbraco-cms/backend-api';
import type { ManifestHealthCheck } from '@umbraco-cms/models';

const _getAllGroups = async () => {
	const response = await HealthCheckResource.getHealthCheckGroup({ skip: 0, take: 9999 });
	return response.items;
};
const groups: HealthCheckGroup[] = await _getAllGroups();

const _createManifests = (groups: HealthCheckGroup[]): Array<ManifestHealthCheck> => {
	return groups.map((group) => {
		return {
			type: 'healthCheck',
			alias: `Umb.HealthCheck.${group.name?.replace(/\s+/g, '') || ''}`,
			name: `${group.name} Health Check`,
			weight: 500,
			meta: {
				label: group.name || '',
				api: UmbHealthCheckContext,
			},
		};
	});
};

const healthChecks = _createManifests(groups);
export const manifests = [...healthChecks];
