import { Meta } from '@storybook/web-components';
import { html } from '@umbraco-cms/backoffice/external/lit';

import '.';

export default {
	title: 'Apps/Installer',
	component: 'umb-installer',
	id: 'umb-installer',
} as Meta;

export const Installer = () => html`<umb-installer></umb-installer>`;
