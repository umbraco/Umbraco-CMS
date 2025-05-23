import type { UmbInstallerLayoutElement } from './installer-layout.element.js';
import type { Meta, StoryFn } from '@storybook/web-components';
import { html } from '@umbraco-cms/backoffice/external/lit';

import './installer-layout.element.js';

export default {
	title: 'Apps/Installer/Shared',
	component: 'umb-installer-layout',
	id: 'umb-installer-layout',
} as Meta;

export const Layout: StoryFn<UmbInstallerLayoutElement> = () => html`<umb-installer-layout></umb-installer-layout>`;
