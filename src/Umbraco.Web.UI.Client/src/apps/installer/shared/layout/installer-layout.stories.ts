import { Meta, Story } from '@storybook/web-components';
import { html } from 'lit';

import type { UmbInstallerLayoutElement } from './installer-layout.element';
import './installer-layout.element';

export default {
	title: 'Apps/Installer/Shared',
	component: 'umb-installer-layout',
	id: 'umb-installer-layout',
} as Meta;

export const Layout: Story<UmbInstallerLayoutElement> = () => html`<umb-installer-layout></umb-installer-layout>`;
