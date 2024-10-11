import { installerContextProvider } from '../shared/utils.story-helpers.js';
import type { UmbInstallerUserElement } from './installer-user.element.js';
import type { Meta, StoryFn } from '@storybook/web-components';
import { html } from '@umbraco-cms/backoffice/external/lit';
import './installer-user.element.js';

export default {
	title: 'Apps/Installer/Steps',
	component: 'umb-installer-user',
	id: 'umb-installer-user',
	decorators: [(story) => installerContextProvider(story)],
} as Meta;

export const Step1User: StoryFn<UmbInstallerUserElement> = () => html`<umb-installer-user></umb-installer-user>`;
Step1User.storyName = 'Step 1: User';
