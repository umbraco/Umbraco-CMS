import './installer-database.element';

import { Meta, Story } from '@storybook/web-components';
import { html } from 'lit';
import { rest } from 'msw';

import { installerContextProvider } from '../shared/utils.story-helpers';

import type { UmbInstallerDatabaseElement } from './installer-database.element';
import type { InstallSettingsResponseModel } from '@umbraco-cms/backoffice/backend-api';
export default {
	title: 'Apps/Installer/Steps',
	component: 'umb-installer-database',
	id: 'umb-installer-database',
	decorators: [(story) => installerContextProvider(story)],
} as Meta;

export const Step3Database: Story<UmbInstallerDatabaseElement> = () =>
	html`<umb-installer-database></umb-installer-database>`;
Step3Database.storyName = 'Step 3: Database';

export const Step3DatabasePreconfigured: Story<UmbInstallerDatabaseElement> = () =>
	html`<umb-installer-database></umb-installer-database>`;
Step3DatabasePreconfigured.storyName = 'Step 3: Database (preconfigured)';
Step3DatabasePreconfigured.parameters = {
	msw: {
		handlers: {
			global: null,
			others: [
				rest.get('/umbraco/backoffice/install/settings', (_req, res, ctx) => {
					return res(
						ctx.status(200),
						ctx.json<InstallSettingsResponseModel>({
							user: { consentLevels: [], minCharLength: 2, minNonAlphaNumericLength: 2 },
							databases: [
								{
									id: '1',
									sortOrder: -1,
									displayName: 'SQLite',
									defaultDatabaseName: 'Umbraco',
									providerName: 'Microsoft.Data.SQLite',
									isConfigured: true,
									requiresServer: false,
									serverPlaceholder: undefined,
									requiresCredentials: false,
									supportsIntegratedAuthentication: false,
									requiresConnectionTest: false,
								},
							],
						})
					);
				}),
			],
		},
	},
};
