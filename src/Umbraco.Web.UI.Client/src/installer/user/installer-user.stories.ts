import { Meta, Story } from '@storybook/web-components';
import { html } from 'lit-html';

import type { UmbInstallerUserElement } from './installer-user.element';
import { installerContextProvider } from '../shared/utils.story-helpers';
import './installer-user.element';

export default {
	title: 'Components/Installer/Steps',
	component: 'umb-installer-user',
	id: 'umb-installer-user',
	decorators: [installerContextProvider],
} as Meta;

export const Step1User: Story<UmbInstallerUserElement> = () => html`<umb-installer-user></umb-installer-user>`;
Step1User.storyName = 'Step 1: User';
Step1User.parameters = {
	actions: {
		handles: ['next'],
	},
};
