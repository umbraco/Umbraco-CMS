import { Meta, Story } from '@storybook/web-components';
import { html } from 'lit-html';

import type { UmbInstallerLayoutElement } from './installer-layout.element';
import './installer-layout.element';

export default {
	title: 'Components/Installer/Shared',
	component: 'umb-installer-layout',
	id: 'umb-installer-layout',
} as Meta;

export const Layout: Story<UmbInstallerLayoutElement> = () => html`<umb-installer-layout></umb-installer-layout>`;
