import { Meta, Story } from '@storybook/web-components';
import { html } from 'lit-html';
import { rest } from 'msw';

import type { UmbInstallerDatabaseElement } from './installer-database.element';
import type { UmbracoInstaller } from '../../core/models';
import { installerContextProvider } from '../shared/utils.story-helpers';
import './installer-database.element';

export default {
	title: 'Components/Installer/Steps',
	component: 'umb-installer-database',
	id: 'umb-installer-database',
	decorators: [installerContextProvider],
} as Meta;

export const Step3Database: Story<UmbInstallerDatabaseElement> = () =>
	html`<umb-installer-database></umb-installer-database>`;
Step3Database.storyName = 'Step 3: Database';
Step3Database.parameters = {
	actions: {
		handles: ['previous', 'submit'],
	},
};

export const Step3DatabasePreconfigured: Story<UmbInstallerDatabaseElement> = () =>
	html`<umb-installer-database></umb-installer-database>`;
Step3DatabasePreconfigured.storyName = 'Step 3: Database (preconfigured)';
Step3DatabasePreconfigured.parameters = {
	actions: {
		handles: ['previous', 'submit'],
	},
	msw: {
		handlers: {
			global: null,
			others: [
				rest.get('/umbraco/backoffice/install/settings', (_req, res, ctx) => {
					return res(
						ctx.status(200),
						ctx.json<UmbracoInstaller>({
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
									serverPlaceholder: null,
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
