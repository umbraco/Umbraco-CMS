import '../core/context/context-provider.element';
import '../installer/installer.element';

import { Meta } from '@storybook/web-components';
import { html } from 'lit-html';

export default {
	title: 'Pages/Installer',
	component: 'umb-installer',
	id: 'installer-page',
} as Meta;

export const Installer = () => html`<umb-installer></umb-installer>`;
