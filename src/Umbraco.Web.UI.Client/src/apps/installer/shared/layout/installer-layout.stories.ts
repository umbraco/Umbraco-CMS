import { Meta, Story } from '@storybook/web-components';
import type { UmbInstallerLayoutElement } from './installer-layout.element.js';
import { html } from '@umbraco-cms/backoffice/external/lit';

import './installer-layout.element.js';

export default {
	title: 'Apps/Installer/Shared',
	component: 'umb-installer-layout',
	id: 'umb-installer-layout',
} as Meta;

export const Layout: Story<UmbInstallerLayoutElement> = () => html`<umb-installer-layout></umb-installer-layout>`;
