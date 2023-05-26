import { Meta, Story } from '@storybook/web-components';

import { installerContextProvider } from '../shared/utils.story-helpers.js';
import type { UmbInstallerUserElement } from './installer-user.element.js';
import { html } from '@umbraco-cms/backoffice/external/lit';
import './installer-user.element.js';

export default {
	title: 'Apps/Installer/Steps',
	component: 'umb-installer-user',
	id: 'umb-installer-user',
	decorators: [(story) => installerContextProvider(story)],
} as Meta;

export const Step1User: Story<UmbInstallerUserElement> = () => html`<umb-installer-user></umb-installer-user>`;
Step1User.storyName = 'Step 1: User';
