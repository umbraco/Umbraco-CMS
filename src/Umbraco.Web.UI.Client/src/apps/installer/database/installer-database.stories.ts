import './installer-database.element.js';

import { Meta, Story } from '@storybook/web-components';
import { html } from '@umbraco-cms/backoffice/external/lit';
const { rest } = window.MockServiceWorker;

import { installerContextProvider } from '../shared/utils.story-helpers.js';

import type { UmbInstallerDatabaseElement } from './installer-database.element.js';
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
