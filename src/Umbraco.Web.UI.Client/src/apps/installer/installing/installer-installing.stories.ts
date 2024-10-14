import { installerContextProvider } from '../shared/utils.story-helpers.js';
import type { UmbInstallerInstallingElement } from './installer-installing.element.js';
import type { Meta, StoryFn } from '@storybook/web-components';
import { html } from '@umbraco-cms/backoffice/external/lit';
import './installer-installing.element.js';

export default {
	title: 'Apps/Installer/Steps',
	component: 'umb-installer-installing',
	id: 'umb-installer-installing',
	decorators: [(story) => installerContextProvider(story)],
} as Meta;

export const Step4Installing: StoryFn<UmbInstallerInstallingElement> = () =>
	html`<umb-installer-installing></umb-installer-installing>`;
Step4Installing.storyName = 'Step 4: Installing';
