import { Meta, Story } from '@storybook/web-components';
import { html } from 'lit-html';

import { installerContextProvider } from '../shared/utils.story-helpers';
import type { UmbInstallerConsentElement } from './installer-consent.element';
import './installer-consent.element';

export default {
	title: 'Components/Installer/Steps',
	component: 'umb-installer-consent',
	id: 'umb-installer-consent',
	decorators: [installerContextProvider],
} as Meta;

export const Step2Telemetry: Story<UmbInstallerConsentElement> = () =>
	html`<umb-installer-consent></umb-installer-consent>`;
Step2Telemetry.storyName = 'Step 2: Telemetry data';
Step2Telemetry.parameters = {
	actions: {
		handles: ['previous', 'next'],
	},
};
