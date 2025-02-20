import './installer-database.element.js';

import { installerContextProvider } from '../shared/utils.story-helpers.js';
import type { UmbInstallerDatabaseElement } from './installer-database.element.js';
import type { Meta, StoryFn } from '@storybook/web-components';
import { html } from '@umbraco-cms/backoffice/external/lit';

export default {
	title: 'Apps/Installer/Steps',
	component: 'umb-installer-database',
	id: 'umb-installer-database',
	decorators: [(story) => installerContextProvider(story)],
} as Meta;

export const Step3Database: StoryFn<UmbInstallerDatabaseElement> = () =>
	html`<umb-installer-database></umb-installer-database>`;
Step3Database.storyName = 'Step 3: Database';
