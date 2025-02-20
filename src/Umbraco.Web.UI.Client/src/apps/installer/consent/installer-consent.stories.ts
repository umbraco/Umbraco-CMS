import { installerContextProvider } from '../shared/utils.story-helpers.js';
import type { UmbInstallerConsentElement } from './installer-consent.element.js';
import type { Meta, StoryFn } from '@storybook/web-components';
import { html } from '@umbraco-cms/backoffice/external/lit';
import './installer-consent.element.js';

export default {
	title: 'Apps/Installer/Steps',
	component: 'umb-installer-consent',
	id: 'umb-installer-consent',
	decorators: [(story) => installerContextProvider(story)],
} as Meta;

export const Step2Telemetry: StoryFn<UmbInstallerConsentElement> = () =>
	html`<umb-installer-consent></umb-installer-consent>`;
Step2Telemetry.storyName = 'Step 2: Telemetry data';
