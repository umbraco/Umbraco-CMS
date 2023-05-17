import { Meta, Story } from '@storybook/web-components';
import { html } from 'lit';

import { installerContextProvider } from '../shared/utils.story-helpers';
import type { UmbInstallerInstallingElement } from './installer-installing.element';
import './installer-installing.element';

export default {
	title: 'Apps/Installer/Steps',
	component: 'umb-installer-installing',
	id: 'umb-installer-installing',
	decorators: [(story) => installerContextProvider(story)],
} as Meta;

export const Step4Installing: Story<UmbInstallerInstallingElement> = () =>
	html`<umb-installer-installing></umb-installer-installing>`;
Step4Installing.storyName = 'Step 4: Installing';
