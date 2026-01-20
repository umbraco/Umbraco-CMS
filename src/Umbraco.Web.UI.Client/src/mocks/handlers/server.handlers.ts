const { http, HttpResponse } = window.MockServiceWorker;
import { version } from '../../../package.json';
import {
	RuntimeLevelModel,
	RuntimeModeModel,
	type GetServerTroubleshootingResponse,
	type GetServerInformationResponse,
	type GetServerConfigurationResponse,
	type GetServerStatusResponse,
} from '@umbraco-cms/backoffice/external/backend-api';
import { umbracoPath } from '@umbraco-cms/backoffice/utils';

export const serverRunningHandler = http.get(umbracoPath('/server/status'), () => {
	return HttpResponse.json<GetServerStatusResponse>({
		serverStatus: RuntimeLevelModel.RUN,
	});
});

export const serverMustInstallHandler = http.get(umbracoPath('/server/status'), () => {
	return HttpResponse.json<GetServerStatusResponse>({
		serverStatus: RuntimeLevelModel.INSTALL,
	});
});

export const serverMustUpgradeHandler = http.get(umbracoPath('/server/status'), () => {
	return HttpResponse.json<GetServerStatusResponse>({
		serverStatus: RuntimeLevelModel.UPGRADE,
	});
});

export const serverInformationHandlers = [
	http.get(umbracoPath('/server/configuration'), () => {
		return HttpResponse.json<GetServerConfigurationResponse>({
			allowPasswordReset: true,
			versionCheckPeriod: 7, // days
			allowLocalLogin: true,
			umbracoCssPath: '/css',
		});
	}),
	http.get(umbracoPath('/server/information'), () => {
		return HttpResponse.json<GetServerInformationResponse>({
			version,
			assemblyVersion: version,
			baseUtcOffset: '01:00:00',
			runtimeMode: RuntimeModeModel.BACKOFFICE_DEVELOPMENT,
		});
	}),
	http.get(umbracoPath('/server/troubleshooting'), () => {
		return HttpResponse.json<GetServerTroubleshootingResponse>({
			items: [
				{ name: 'Umbraco base url', data: location.origin },
				{ name: 'Mocked server', data: 'true' },
				{ name: 'Umbraco version', data: version },
			],
		});
	}),
];
