import { Meta, Story } from '@storybook/web-components';
import { html } from 'lit';

import { installerContextProvider } from '../shared/utils.story-helpers';
import type { UmbInstallerUserElement } from './installer-user.element';
import './installer-user.element';

export default {
	title: 'Apps/Installer/Steps',
	component: 'umb-installer-user',
	id: 'umb-installer-user',
	decorators: [(story) => installerContextProvider(story)],
} as Meta;

export const Step1User: Story<UmbInstallerUserElement> = () => html`<umb-installer-user></umb-installer-user>`;
Step1User.storyName = 'Step 1: User';
